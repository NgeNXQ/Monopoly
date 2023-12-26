using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

internal sealed class LobbyManager : MonoBehaviour
{
    private const float LOBBY_UPTIME = 25.0F;

    public const float LOBBY_REFRESH_RATE = 1.0f;

    public const float PLAYER_LOADING_TIMEOUT = 15.0F;

    public const string KEY_PLAYER_NICKNAME = "PlayerNickname";

    public const string KEY_PLAYER_ACTIVE_SCENE = "ActiveScene";

    private bool hasLeft;

    private bool hasLoaded;

    private string lobbyName 
    {
        get => $"LOBBY_{this.JoinCode}"; 
    }

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

        this.LocalLobbyEventCallbacks.LobbyDeleted -= this.HandleLobbyDeleted;
        this.LocalLobbyEventCallbacks.PlayerLeft -= this.HandlePlayerLeftAsync;
        this.LocalLobbyEventCallbacks.PlayerJoined -= this.HandlePlayerJoinedAsync;
        this.LocalLobbyEventCallbacks.KickedFromLobby -= this.HandleKickedFromLobbyAsync;

        this.LocalLobbyEventCallbacks.PlayerDataChanged -= this.HandlePlayerDataChangedAsync;
    }

    private async void OnDestroy()
    {
        await this.LeaveLobby();
    }

    #region Start & End Game

    public void StartGame()
    {
        if (this.HavePlayersLoaded) 
        {
            GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.MonopolyGame);
        }
        else
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance.MessageFailedToLoad, PanelMessageBoxUI.Icon.Warning);
        }
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
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                IsPrivate = false
            };

            this.LocalLobby = await LobbyService.Instance.UpdateLobbyAsync(this.LocalLobby.Id, updateLobbyOptions);
        }

        this.StopCoroutine(this.LoadLobbyCoroutine());
    }

    private async void HandleMonopolyGameLoadedAsync()
    {
        GameCoordinator.Instance.LocalPlayer.Data[LobbyManager.KEY_PLAYER_ACTIVE_SCENE].Value = GameCoordinator.MonopolyScene.MonopolyGame.ToString();

        UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
        {
            Data = GameCoordinator.Instance.LocalPlayer.Data
        };

        await Lobbies.Instance.UpdatePlayerAsync(LobbyManager.Instance.LocalLobby.Id, GameCoordinator.Instance.LocalPlayer.Id, updatePlayerOptions);

        if (this.IsHost)
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                IsPrivate = true
            };

            this.LocalLobby = await LobbyService.Instance.UpdateLobbyAsync(this.LocalLobby.Id, updateLobbyOptions);
        }
    }

    private async void HandleGameLobbyFailedToLoadAsync()
    {
        this.hasLoaded = false;
        this.StopCoroutine(this.LoadLobbyCoroutine());
        await this.DisconnectFromLobbyAsync();
    }

    #endregion

    #region Handle Lobby Deleted

    private void HandleLobbyDeleted()
    {
        if (this.LocalLobby != null)
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

    private IEnumerator LoadLobbyCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(LobbyManager.PLAYER_LOADING_TIMEOUT);

        yield return waitForSeconds;

        if (!this.hasLoaded)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance.MessageFailedToLoad, PanelMessageBoxUI.Icon.Error);

            this.OnGameLobbyFailedToLoad?.Invoke();
        }
    }

    private IEnumerator PingLobbyCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(LobbyManager.LOBBY_UPTIME);

        while (this.LocalLobby != null)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(this.LocalLobby.Id);
            yield return waitForSeconds;
        }
    }

    private async void HandlePlayerLeftAsync(List<int> leftPlayer)
    {
        await Awaitable.WaitForSecondsAsync(LobbyManager.LOBBY_REFRESH_RATE);

        this.LocalLobby = await Lobbies.Instance.GetLobbyAsync(this.LocalLobby.Id);
    }

    private async void HandlePlayerJoinedAsync(List<LobbyPlayerJoined> joinedPlayer)
    {
        await Awaitable.WaitForSecondsAsync(LobbyManager.LOBBY_REFRESH_RATE);

        this.LocalLobby = await Lobbies.Instance.GetLobbyAsync(this.LocalLobby.Id);
    }

    private async void HandlePlayerDataChangedAsync(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> changedPlayer)
    {
        if (this.IsHost)
        {
            await Awaitable.WaitForSecondsAsync(LobbyManager.LOBBY_REFRESH_RATE);

            this.LocalLobby = await Lobbies.Instance.GetLobbyAsync(this.LocalLobby.Id);

            this.HavePlayersLoaded = this.LocalLobby.Players.All(player => player.Data[LobbyManager.KEY_PLAYER_ACTIVE_SCENE].Value.Equals(GameCoordinator.Instance.ActiveScene.ToString(), StringComparison.Ordinal));
        }
    }

    #endregion

    #region Host & Connect & Disconnect & Kick

    private async Task LeaveLobby()
    {
        this.IsHost = false;
        this.hasLoaded = false;
        this.LocalLobby = null;

        this.StopCoroutine(this.LoadLobbyCoroutine());

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            await GameCoordinator.Instance.LoadSceneAsync(GameCoordinator.MonopolyScene.MainMenu);
        }

        if (!this.hasLeft)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance?.MessageKicked ?? UIManagerMainMenu.Instance.MessageKicked, PanelMessageBoxUI.Icon.Error);
        }

        this.hasLeft = false;
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
        await this.LeaveLobby();
    }

    public async Task HostLobbyAsync(string relayCode)
    {
        this.IsHost = true;
        this.hasLeft = false;
        this.JoinCode = relayCode;

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions()
        {
            IsPrivate = false,
            Player = GameCoordinator.Instance.LocalPlayer
        };

        try
        {
            NetworkManager.Singleton?.StartHost();
            this.LocalLobby = await LobbyService.Instance.CreateLobbyAsync(this.lobbyName, GameCoordinator.MAX_PLAYERS, lobbyOptions);
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }

        if (this != null)
        {
            this.StartCoroutine(this.PingLobbyCoroutine());
            this.StartCoroutine(this.LoadLobbyCoroutine());
            await GameCoordinator.Instance.LoadSceneAsync(GameCoordinator.MonopolyScene.GameLobby);
        }
    }

    public async Task ConnectLobbyAsync(string joinCode)
    {
        this.IsHost = false;
        this.hasLeft = false;
        this.JoinCode = joinCode;

        JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions()
        {
            Player = GameCoordinator.Instance.LocalPlayer
        };

        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(this.queryCurrentLobby);

            if (queryResponse.Results.Count == 0)
            {
                throw new LobbyServiceException(LobbyExceptionReason.InvalidJoinCode, "The game has not found.");
            }
            else
            {
                NetworkManager.Singleton?.StartClient();
                this.LocalLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results.FirstOrDefault().Id, joinOptions);
                await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);
            }
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }

        if (this != null)
        {
            this.StartCoroutine(this.LoadLobbyCoroutine());
        }
    }

    public async Task KickFromLobbyAsync(string playerId)
    {
        NetworkManager.Singleton?.DisconnectClient((ushort)NetworkManager.Singleton?.ConnectedClientsIds.LastOrDefault());
        await LobbyService.Instance.RemovePlayerAsync(this.LocalLobby.Id, playerId);
    }

    #endregion
}