using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

internal sealed class LobbyManager : MonoBehaviour
{
    private const float LOBBY_UPTIME = 25.0F;

    public const int MIN_PLAYERS = 1;

    public const int MAX_PLAYERS = 5;

    public const float LOBBY_REFRESH_RATE = 1.0f;

    public const float LOBBY_LOADING_TIMEOUT = 1.0F;

    public const string KEY_PLAYER_SCENE = "Scene";

    public const string KEY_PLAYER_NICKNAME = "Nickname";

    public const string KEY_LOBBY_STATE = "State";

    public const string LOBBY_STATE_GAME = "Game";

    public const string LOBBY_STATE_LOBBY = "Lobby";

    public const string LOBBY_STATE_PENDING = "Waiting";

    public const string LOBBY_STATE_LOADING = "Loading";

    public const string LOBBY_STATE_RETURNING = "Returning";

    private bool hasLeft;

    private bool hasLoaded;

    private string lobbyName 
    {
        get => $"LOBBY_{this.JoinCode}"; 
    }

    private ILobbyEvents localLobbyEvents;

    private QueryLobbiesOptions queryCurrentLobby 
    {
        get
        {
            return new QueryLobbiesOptions() 
            {
                Filters = new List<QueryFilter>()
                {
                    new QueryFilter(QueryFilter.FieldOptions.Name, this.JoinCode, QueryFilter.OpOptions.CONTAINS)
                }
            };
        }
    }

    private readonly SemaphoreSlim lobbyRefreshSemaphore = new SemaphoreSlim(1, 1);

    public static LobbyManager Instance { get; private set; }

    public Action OnGameLobbyLoaded;

    public Action OnMonopolyGameLoaded;

    public Action OnGameLobbyFailedToLoad;

    public Action OnMonopolyGameFailedToLoad;

    public bool IsHost { get; private set; }

    public string JoinCode { get; private set; }

    public Lobby LocalLobby { get; private set; }

    public bool HavePlayersLoaded { get; private set; }

    public LobbyEventCallbacks LocalLobbyEventCallbacks { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        this.LocalLobbyEventCallbacks = new LobbyEventCallbacks();
        
        this.OnGameLobbyLoaded += this.HandleGameLobbyLoadedAsync;
        this.OnMonopolyGameLoaded += this.HandleMonopolyGameLoadedAsync;
        this.OnGameLobbyFailedToLoad += this.HandleGameLobbyFailedToLoadAsync;
        this.OnMonopolyGameFailedToLoad += this.HandleMonopolyGameFailedToLoadAsync;

        this.LocalLobbyEventCallbacks.DataChanged += this.HandleDataChanged;
        this.LocalLobbyEventCallbacks.LobbyDeleted += this.HandleLobbyDeleted;
        this.LocalLobbyEventCallbacks.PlayerLeft += this.HandlePlayerLeftAsync;
        this.LocalLobbyEventCallbacks.PlayerJoined += this.HandlePlayerJoinedAsync;
        this.LocalLobbyEventCallbacks.KickedFromLobby += this.HandleKickedFromLobbyAsync;

        this.LocalLobbyEventCallbacks.PlayerDataChanged += this.HandlePlayerDataChangedAsync;
    }

    private void OnDisable()
    {
        this.LocalLobbyEventCallbacks = new LobbyEventCallbacks();

        this.OnGameLobbyLoaded -= this.HandleGameLobbyLoadedAsync;
        this.OnMonopolyGameLoaded -= this.HandleMonopolyGameLoadedAsync;
        this.OnGameLobbyFailedToLoad -= this.HandleGameLobbyFailedToLoadAsync;
        this.OnMonopolyGameFailedToLoad -= this.HandleMonopolyGameFailedToLoadAsync;

        this.LocalLobbyEventCallbacks.DataChanged -= this.HandleDataChanged;
        this.LocalLobbyEventCallbacks.LobbyDeleted -= this.HandleLobbyDeleted;
        this.LocalLobbyEventCallbacks.PlayerLeft -= this.HandlePlayerLeftAsync;
        this.LocalLobbyEventCallbacks.PlayerJoined -= this.HandlePlayerJoinedAsync;
        this.LocalLobbyEventCallbacks.KickedFromLobby -= this.HandleKickedFromLobbyAsync;

        this.LocalLobbyEventCallbacks.PlayerDataChanged -= this.HandlePlayerDataChangedAsync;
    }

    private async void OnDestroy()
    {
        this.StopAllCoroutines();

        if (this.LocalLobby != null)
        {
            await this.DisconnectFromLobbyAsync();
        }

        this.lobbyRefreshSemaphore.Dispose();
    }

    #region Start & End Game

    public async void StartGameAsync()
    {
        this.HavePlayersLoaded = this.LocalLobby.Players.All(player => player.Data[LobbyManager.KEY_PLAYER_SCENE].Value.Equals(GameCoordinator.Instance.ActiveScene.ToString(), StringComparison.Ordinal));

        if (!this.HavePlayersLoaded)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance.MessageNotAllPlayersLoaded, PanelMessageBoxUI.Icon.Warning);
            return;
        }

        if (this.LocalLobby.Players.Count < LobbyManager.MIN_PLAYERS)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance.MessageTooFewPlayers, PanelMessageBoxUI.Icon.Warning);
            return;
        }

        this.LocalLobby.Data[LobbyManager.KEY_LOBBY_STATE] = new DataObject(DataObject.VisibilityOptions.Member, LobbyManager.LOBBY_STATE_LOADING);

        UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
        {
            IsPrivate = true,
            Data = this.LocalLobby.Data
        };

        await Lobbies.Instance.UpdateLobbyAsync(this.LocalLobby.Id, updateLobbyOptions);
        
        GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.MonopolyGame);
    }

    public void EndGame()
    {
        GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.GameLobby);
    }

    #endregion

    #region Loading Callbacks

    private async void HandleGameLobbyLoadedAsync()
    {
        this.hasLoaded = true;

        if (this.IsHost)
        {
            this.LocalLobby.Data[LobbyManager.KEY_LOBBY_STATE] = new DataObject(DataObject.VisibilityOptions.Member, LobbyManager.LOBBY_STATE_LOBBY);

            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                IsPrivate = false,
                Data = this.LocalLobby.Data
            };

            this.LocalLobby = await LobbyService.Instance.UpdateLobbyAsync(this.LocalLobby.Id, updateLobbyOptions);
        }

        this.UpdateLocalPlayerData();
    }

    private async void HandleMonopolyGameLoadedAsync()
    {
        this.hasLoaded = true;

        GameCoordinator.Instance.LocalPlayer.Data[LobbyManager.KEY_PLAYER_SCENE].Value = GameCoordinator.MonopolyScene.MonopolyGame.ToString();

        UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
        {
            Data = GameCoordinator.Instance.LocalPlayer.Data
        };

        await Lobbies.Instance.UpdatePlayerAsync(LobbyManager.Instance.LocalLobby.Id, GameCoordinator.Instance.LocalPlayer.Id, updatePlayerOptions);
    }

    private async void HandleGameLobbyFailedToLoadAsync()
    {
        await this.DisconnectFromLobbyAsync();
    }

    private async void HandleMonopolyGameFailedToLoadAsync()
    {
        if (this.IsHost)
        {
            GameCoordinator.Instance.LocalPlayer.Data[LobbyManager.KEY_PLAYER_SCENE].Value = GameCoordinator.MonopolyScene.MonopolyGame.ToString();

            this.LocalLobby.Data[LobbyManager.KEY_LOBBY_STATE] = new DataObject(DataObject.VisibilityOptions.Member, LobbyManager.LOBBY_STATE_PENDING);

            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                IsPrivate = true,
                Data = this.LocalLobby.Data
            };

            UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
            {
                Data = GameCoordinator.Instance.LocalPlayer.Data
            };

            await Lobbies.Instance.UpdateLobbyAsync(this.LocalLobby.Id, updateLobbyOptions);

            await Lobbies.Instance.UpdatePlayerAsync(this.LocalLobby.Id, GameCoordinator.Instance.LocalPlayer.Id, updatePlayerOptions);

            GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.MonopolyGame);
        }
        else
        {
            await this.DisconnectFromLobbyAsync();
        }
    }

    #endregion

    #region Handle Lobby Deleted

    private void HandleLobbyDeleted()
    {
        if (!this.IsHost)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance.MessageHostDisconnected, PanelMessageBoxUI.Icon.Error, this.CallbackLobbyDeletedAsync);
        }
    }

    private async void CallbackLobbyDeletedAsync()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerGameLobby.Instance.MessageDisconnecting, PanelMessageBoxUI.Icon.Loading);

        await this.LeaveLobby();
    }

    #endregion

    #region Lobby Ping & Load & Update

    public async void UpdateLocalPlayerData()
    {
        GameCoordinator.Instance.LocalPlayer.Data[LobbyManager.KEY_PLAYER_SCENE] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GameCoordinator.Instance.ActiveScene.ToString());

        UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
        {
            Data = GameCoordinator.Instance.LocalPlayer.Data
        };

        this.LocalLobby = await LobbyService.Instance.UpdatePlayerAsync(this.LocalLobby.Id, GameCoordinator.Instance.LocalPlayer.Id, updatePlayerOptions);
    }

    //public IEnumerator LoadLobbyCoroutine()
    //{
    //    yield return new WaitForSeconds(LobbyManager.PLAYER_LOADING_TIMEOUT);

    //    if (!this.hasLoaded)
    //    {
    //        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMainMenu.Instance.MessageFailedToLoad, PanelMessageBoxUI.Icon.Error);

    //        this.OnGameLobbyFailedToLoad?.Invoke();
    //    }
    //}

    //public IEnumerator LoadGameCoroutine()
    //{
    //    yield return new WaitForSeconds(LobbyManager.PLAYER_LOADING_TIMEOUT);

    //    if (!this.hasLoaded)
    //    {
    //        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance.MessageFailedToConnect, PanelMessageBoxUI.Icon.Error);

    //        this.OnMonopolyGameFailedToLoad?.Invoke();
    //    }
    //}

    private IEnumerator PingLobbyCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(LobbyManager.LOBBY_UPTIME);

        while (this.LocalLobby != null)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(this.LocalLobby.Id);
            yield return waitForSeconds;
        }
    }

    private async void HandleDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> changedData)
    {
        await this.RefreshLobbyAsync();

        switch (this.LocalLobby.Data[LobbyManager.KEY_LOBBY_STATE].Value)
        {
            case LobbyManager.LOBBY_STATE_PENDING:
                {
                    UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerGameLobby.Instance.MessagePendingGame, PanelMessageBoxUI.Icon.Loading);
                }
                break;
            case LobbyManager.LOBBY_STATE_LOADING:
                {
                    UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerMonopolyGame.Instance.MessageWaitingOtherPlayers ?? UIManagerGameLobby.Instance.MessagePendingGame, PanelMessageBoxUI.Icon.Loading);
                }
                break;
            case LobbyManager.LOBBY_STATE_RETURNING:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerMonopolyGame.Instance.MessagePlayersFailedToLoad ?? UIManagerGameLobby.Instance.MessageFailedToLoad, PanelMessageBoxUI.Icon.Loading);
                break;
        }
    }

    private async void HandlePlayerLeftAsync(List<int> leftPlayer)
    {
        await this.RefreshLobbyAsync();
    }

    private async void HandlePlayerJoinedAsync(List<LobbyPlayerJoined> joinedPlayer)
    {
        await this.RefreshLobbyAsync();
    }

    private async void HandlePlayerDataChangedAsync(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> changedPlayer)
    {
        await this.RefreshLobbyAsync();
    }

    private async Task RefreshLobbyAsync()
    {
        await this.lobbyRefreshSemaphore.WaitAsync();

        try
        {
            await Awaitable.WaitForSecondsAsync(LobbyManager.LOBBY_REFRESH_RATE);
            this.LocalLobby = await Lobbies.Instance.GetLobbyAsync(this.LocalLobby.Id);
        }
        catch
        {
            Debug.Log("Caught");
        }
        finally
        {
            this.lobbyRefreshSemaphore.Release();
        }
    }

    #endregion

    #region Host & Connect & Disconnect & Kick

    private async Task LeaveLobby()
    {
        if (this != null)
        {
            await this.localLobbyEvents.UnsubscribeAsync();
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            await GameCoordinator.Instance.LoadSceneAsync(GameCoordinator.MonopolyScene.MainMenu);
        }

        if (!this.hasLeft)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance?.MessageKicked ?? UIManagerMainMenu.Instance.MessageKicked, PanelMessageBoxUI.Icon.Error);
        }

        this.IsHost = false;
        this.hasLeft = false;
        this.LocalLobby = null;
        this.hasLoaded = false;
    }

    public async Task DisconnectFromLobbyAsync()
    {
        this.hasLeft = true;

        if (this.IsHost)
        {
            this.StopCoroutine(this.PingLobbyCoroutine());
            await LobbyService.Instance.DeleteLobbyAsync(this.LocalLobby.Id);
        }
        else
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(this.queryCurrentLobby);

            if (queryResponse.Results.Count > 0)
            {
                await LobbyService.Instance.RemovePlayerAsync(this.LocalLobby.Id, GameCoordinator.Instance.LocalPlayer.Id);
            }
        }

        await this.LeaveLobby();
    }

    private async void HandleKickedFromLobbyAsync()
    {
        if (!this.IsHost)
        {
            await this.LeaveLobby();
        }
    }

    public async Task HostLobbyAsync(string relayCode, CancellationToken cancellationToken)
    {
        this.IsHost = true;
        this.hasLeft = false;
        this.hasLoaded = false;
        this.JoinCode = relayCode;

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions()
        {
            Player = GameCoordinator.Instance.LocalPlayer,

            Data = new Dictionary<string, DataObject>()
            {
                { LobbyManager.KEY_LOBBY_STATE, new DataObject(DataObject.VisibilityOptions.Member, LobbyManager.LOBBY_STATE_LOBBY) }
            }
        };

        try
        {
            NetworkManager.Singleton?.StartHost();
            this.LocalLobby = await Task.Run(() => LobbyService.Instance.CreateLobbyAsync(this.lobbyName, LobbyManager.MAX_PLAYERS, lobbyOptions), cancellationToken);
            this.localLobbyEvents = await Task.Run(() => LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks), cancellationToken);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }
        catch (OperationCanceledException operationCanceledException)
        {
            throw operationCanceledException;
        }

        if (this != null)
        {
            this.StartCoroutine(this.PingLobbyCoroutine());
            await GameCoordinator.Instance.LoadSceneAsync(GameCoordinator.MonopolyScene.GameLobby);
        }
    }

    public async Task ConnectLobbyAsync(string joinCode, CancellationToken cancellationToken)
    {
        this.IsHost = false;
        this.hasLeft = false;
        this.hasLoaded = false;
        this.JoinCode = joinCode;

        JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions()
        {
            Player = GameCoordinator.Instance.LocalPlayer
        };

        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(this.queryCurrentLobby);

            //if (queryResponse.Results.Count < 1)
            //{
            //    throw new LobbyServiceException(LobbyExceptionReason.InvalidJoinCode, "The game has not found.");
            //}
            //else
            //{
                NetworkManager.Singleton?.StartClient();
                this.LocalLobby = await Task.Run(() => LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results.FirstOrDefault().Id, joinOptions), cancellationToken);
                this.localLobbyEvents = await Task.Run(() => LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks), cancellationToken); 
            //}
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }
        catch (OperationCanceledException operationCanceledException)
        {
            throw operationCanceledException;
        }
    }

    public async Task KickFromLobbyAsync(string playerId)
    {
        NetworkManager.Singleton?.DisconnectClient((ushort)NetworkManager.Singleton?.ConnectedClientsIds.LastOrDefault());
        await LobbyService.Instance.RemovePlayerAsync(this.LocalLobby.Id, playerId);
    }

    #endregion
}