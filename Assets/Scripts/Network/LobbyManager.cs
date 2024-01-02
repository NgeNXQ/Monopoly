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
    private const float LOBBY_UPTIME = 25.0f;

    public const int MIN_PLAYERS = 1;

    public const int MAX_PLAYERS = 5;

    public const float LOBBY_LOADING_TIMEOUT = 15.0f;

    public const string KEY_PLAYER_SCENE = "Scene";

    public const string KEY_PLAYER_NICKNAME = "Nickname";

    public const string KEY_LOBBY_STATE = "State";

    public const string LOBBY_STATE_GAME = "Game";

    public const string LOBBY_STATE_LOBBY = "Lobby";

    public const string LOBBY_STATE_LOADING = "Loading";

    public const string LOBBY_STATE_PENDING = "Waiting";

    public const string LOBBY_STATE_RETURNING = "Returning";
    
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

    public static LobbyManager Instance { get; private set; }

    public Action OnGameLobbyLoaded;

    public Action OnMonopolyGameLoaded;

    public Action OnGameLobbyFailedToLoad;

    public Action OnMonopolyGameFailedToLoad;

    public Lobby LocalLobby { get; private set; }

    public bool IsHost { get; private set; }

    public string JoinCode { get; private set; }

    public bool HasHostLeft { get; private set; }

    public bool HasLocalPlayerLeft { get; private set; }
    
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
        
        this.OnGameLobbyLoaded += this.HandleGameLobbyLoaded;
        this.OnMonopolyGameLoaded += this.HandleMonopolyGameLoaded;
        this.OnGameLobbyFailedToLoad += this.HandleGameLobbyFailedToLoadAsync;
        this.OnMonopolyGameFailedToLoad += this.HandleMonopolyGameFailedToLoad;

        this.LocalLobbyEventCallbacks.PlayerLeft += this.HandlePlayerLeft;
        this.LocalLobbyEventCallbacks.DataChanged += this.HandleDataChanged;
        this.LocalLobbyEventCallbacks.LobbyDeleted += this.HandleLobbyDeleted;
        this.LocalLobbyEventCallbacks.PlayerJoined += this.HandlePlayerJoined;
        this.LocalLobbyEventCallbacks.PlayerDataChanged += this.HandlePlayerDataChanged;
        this.LocalLobbyEventCallbacks.KickedFromLobby += this.HandleKickedFromLobbyAsync;
    }

    private void OnDisable()
    {
        this.LocalLobbyEventCallbacks = new LobbyEventCallbacks();

        this.OnGameLobbyLoaded -= this.HandleGameLobbyLoaded;
        this.OnMonopolyGameLoaded -= this.HandleMonopolyGameLoaded;
        this.OnGameLobbyFailedToLoad -= this.HandleGameLobbyFailedToLoadAsync;
        this.OnMonopolyGameFailedToLoad -= this.HandleMonopolyGameFailedToLoad;

        this.LocalLobbyEventCallbacks.PlayerLeft -= this.HandlePlayerLeft;
        this.LocalLobbyEventCallbacks.DataChanged -= this.HandleDataChanged;
        this.LocalLobbyEventCallbacks.LobbyDeleted -= this.HandleLobbyDeleted;
        this.LocalLobbyEventCallbacks.PlayerJoined -= this.HandlePlayerJoined;
        this.LocalLobbyEventCallbacks.PlayerDataChanged -= this.HandlePlayerDataChanged;
        this.LocalLobbyEventCallbacks.KickedFromLobby -= this.HandleKickedFromLobbyAsync;
    }

    private async void OnDestroy()
    {
        this.StopAllCoroutines();

        if (this.LocalLobby != null)
        {
            await this.DisconnectFromLobbyAsync();
        }
    }

    #region Start & End Game

    public void StartGameAsync()
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

        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerGameLobby.Instance.MessagePendingGame, PanelMessageBoxUI.Icon.Loading);

        this.UpdateLocalLobbyData(LobbyManager.LOBBY_STATE_LOADING, true);

        GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.MonopolyGame);
    }

    public void EndGame()
    {
        GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.GameLobby);
    }

    #endregion

    #region Lobby API

    private async Task LeaveLobbyAsync()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerGameLobby.Instance?.MessageDisconnecting ?? UIManagerMainMenu.Instance?.MessageDisconnecting, PanelMessageBoxUI.Icon.Loading);

        NetworkManager.Singleton?.Shutdown();

        if (this != null)
        {
            await this.localLobbyEvents?.UnsubscribeAsync();
        }

        await GameCoordinator.Instance?.LoadSceneAsync(GameCoordinator.MonopolyScene.MainMenu);

        if (!this.IsHost)
        {
            if (this.HasHostLeft)
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance?.MessageHostDisconnected ?? UIManagerMainMenu.Instance?.MessageHostDisconnected, PanelMessageBoxUI.Icon.Error);
            }
            else if (!this.HasLocalPlayerLeft)
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance?.MessageKicked ?? UIManagerMainMenu.Instance?.MessageKicked, PanelMessageBoxUI.Icon.Error);
            }
        }

        this.IsHost = false;
        this.LocalLobby = null;
        this.HasHostLeft = false;
        this.HasLocalPlayerLeft = false;
    }

    public async Task DisconnectFromLobbyAsync()
    {
        this.HasLocalPlayerLeft = true;

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
    }
    
    public async Task HostLobbyAsync(string relayCode)
    {
        this.IsHost = true;
        this.HasHostLeft = false;
        this.JoinCode = relayCode;
        this.HasLocalPlayerLeft = false;

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
            this.LocalLobby = await LobbyService.Instance.CreateLobbyAsync(this.lobbyName, LobbyManager.MAX_PLAYERS, lobbyOptions);
            this.localLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);

            NetworkManager.Singleton?.StartHost();
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }

        if (this != null)
        {
            this.StartCoroutine(this.PingLobbyCoroutine());
            await GameCoordinator.Instance.LoadSceneAsync(GameCoordinator.MonopolyScene.GameLobby);
        }
    }

    public async Task ConnectLobbyAsync(string joinCode)
    {
        this.IsHost = false;
        this.HasHostLeft = false;
        this.JoinCode = joinCode;
        this.HasLocalPlayerLeft = false;

        JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions()
        {
            Player = GameCoordinator.Instance.LocalPlayer
        };

        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(this.queryCurrentLobby);
            this.LocalLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results.FirstOrDefault().Id, joinOptions);
            this.localLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);

            NetworkManager.Singleton?.StartClient();
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }
    }

    public async Task KickFromLobbyAsync(string playerId)
    {
        await LobbyService.Instance.RemovePlayerAsync(this.LocalLobby.Id, playerId);
    }

    #endregion

    #region Lobby Ping

    private IEnumerator PingLobbyCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(LobbyManager.LOBBY_UPTIME);

        while (this.LocalLobby != null)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(this.LocalLobby?.Id);
            yield return waitForSeconds;
        }
    }

    #endregion

    #region Lobby Update

    public async void UpdateLocalPlayerData()
    {
        GameCoordinator.Instance.LocalPlayer.Data[LobbyManager.KEY_PLAYER_SCENE] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GameCoordinator.Instance.ActiveScene.ToString());

        UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
        {
            Data = GameCoordinator.Instance.LocalPlayer.Data
        };

        this.LocalLobby = await LobbyService.Instance.UpdatePlayerAsync(this.LocalLobby.Id, GameCoordinator.Instance.LocalPlayer.Id, updatePlayerOptions);
    }

    public async void UpdateLocalLobbyData(string lobbyState, bool isPrivate = true)
    {
        this.LocalLobby.Data[LobbyManager.KEY_LOBBY_STATE] = new DataObject(DataObject.VisibilityOptions.Member, lobbyState);

        UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
        {
            IsPrivate = isPrivate,
            Data = this.LocalLobby.Data
        };

        await Lobbies.Instance.UpdateLobbyAsync(this.LocalLobby.Id, updateLobbyOptions);
    }

    #endregion

    #region Lobby Callbacks

    private void HandleLobbyDeleted()
    {
        this.HasHostLeft = true;
    }

    private async void HandleKickedFromLobbyAsync()
    {
        await this.LeaveLobbyAsync();
    }

    private void HandlePlayerLeft(List<int> leftPlayers)
    {
        foreach (int playerIndex in leftPlayers)
        {
            this.LocalLobby.Players.RemoveAt(playerIndex);
        }
    }

    private void HandlePlayerJoined(List<LobbyPlayerJoined> joinedPlayers)
    {
        foreach (LobbyPlayerJoined newPlayer in joinedPlayers)
        {
            this.LocalLobby.Players.Add(newPlayer.Player);
        }
    }

    private void HandleDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> changedLobbyData)
    {
        foreach (string key in changedLobbyData.Keys)
        {
            this.LocalLobby.Data[key] = changedLobbyData[key].Value;
        }

        switch (this.LocalLobby.Data[LobbyManager.KEY_LOBBY_STATE].Value)
        {
            case LobbyManager.LOBBY_STATE_LOADING:
            case LobbyManager.LOBBY_STATE_PENDING:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerGameLobby.Instance?.MessagePendingGame ?? UIManagerMonopolyGame.Instance?.MessageWaitingOtherPlayers, PanelMessageBoxUI.Icon.Loading, stateCallback: () => this.HavePlayersLoaded);
                break;
            case LobbyManager.LOBBY_STATE_RETURNING:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerGameLobby.Instance?.MessageFailedToConnect ?? UIManagerMonopolyGame.Instance?.MessagePlayersFailedToLoad, PanelMessageBoxUI.Icon.Loading);
                break;
        }
    }

    private void HandlePlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> changedPlayerData)
    {
        foreach (int playerIndex in changedPlayerData.Keys)
        {
            foreach (string key in changedPlayerData[playerIndex].Keys)
            {
                this.LocalLobby.Players[playerIndex].Data[key] = changedPlayerData[playerIndex][key].Value;
            }
        }

        this.HavePlayersLoaded = this.LocalLobby.Players.All(player => player.Data[LobbyManager.KEY_PLAYER_SCENE].Value.Equals(GameCoordinator.Instance.ActiveScene.ToString(), StringComparison.Ordinal));
    }

    #endregion

    #region Loading Callbacks

    private void HandleGameLobbyLoaded()
    {
        if (this.IsHost)
        {
            this.UpdateLocalLobbyData(LobbyManager.LOBBY_STATE_LOBBY, false);
        }

        this.UpdateLocalPlayerData();
    }

    private void HandleMonopolyGameLoaded()
    {
        this.UpdateLocalPlayerData();
    }

    private void HandleMonopolyGameFailedToLoad()
    {
        if (this.IsHost)
        {
            this.UpdateLocalLobbyData(LobbyManager.LOBBY_STATE_RETURNING, true);

            GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.GameLobby);
        }
    }

    private async void HandleGameLobbyFailedToLoadAsync()
    {
        await this.DisconnectFromLobbyAsync();
    }

    #endregion
}