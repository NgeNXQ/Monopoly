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

    private const float LOBBY_REFRESH_TIME = 1.0F;

    public const float PLAYER_LOADING_TIMEOUT = 0.1F;

    public const string KEY_PLAYER_NICKNAME = "PlayerNickname";

    public const string KEY_PLAYER_ACTIVE_SCENE = "ActiveScene";

    private bool hasLeft;

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

    public bool ArePlayersLoaded 
    {
        get
        {
            return this.LocalLobby.Players.All(player => player.Data[LobbyManager.KEY_PLAYER_ACTIVE_SCENE].Value.Equals(GameCoordinator.Instance.ActiveScene.ToString(), StringComparison.Ordinal));
        }
    }

    public bool IsHost { get; private set; }

    public List<Player> PlayersFailedToLoad 
    {
        get
        {
            return this.LocalLobby.Players.Where(player => player.Data[LobbyManager.KEY_PLAYER_ACTIVE_SCENE].Value != GameCoordinator.MonopolyScene.MonopolyGame.ToString()).ToList();
        }
    }

    public Action OnGameLobbyLoaded;

    public Action OnMonopolyGameLoaded;

    public string JoinCode { get; private set; }

    public Lobby LocalLobby { get; private set; }

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

        this.OnMonopolyGameLoaded += this.HandleMonopolyGameLoaded;

        this.LocalLobbyEventCallbacks.PlayerLeft += this.HandlePlayerLeft;
        this.LocalLobbyEventCallbacks.PlayerJoined += this.HandlePlayerJoined;
        this.LocalLobbyEventCallbacks.LobbyDeleted += this.HandleLobbyDeleted;
        this.LocalLobbyEventCallbacks.KickedFromLobby += this.HandleKickedFromLobby;
    }

    private void OnDisable()
    {
        this.LocalLobbyEventCallbacks = new LobbyEventCallbacks();

        this.OnMonopolyGameLoaded -= this.HandleMonopolyGameLoaded;

        this.LocalLobbyEventCallbacks.PlayerLeft -= this.HandlePlayerLeft;
        this.LocalLobbyEventCallbacks.PlayerJoined -= this.HandlePlayerJoined;
        this.LocalLobbyEventCallbacks.LobbyDeleted -= this.HandleLobbyDeleted;
        this.LocalLobbyEventCallbacks.KickedFromLobby += this.HandleKickedFromLobby;
    }

    #region Start Game & End Game

    public void StartGame()
    {
        GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.MonopolyGame);
    }

    public void EndGame()
    {
        GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.GameLobby);
    }

    #endregion

    #region Loading Callbacks

    private async void HandleMonopolyGameLoaded()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                IsPrivate = true
            };

            this.LocalLobby = await LobbyService.Instance.UpdateLobbyAsync(this.LocalLobby.Id, updateLobbyOptions);
        }
    }

    #endregion

    #region Lobby Ping & Update

    private IEnumerator PingLobbyCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(LobbyManager.LOBBY_UPTIME);

        while (this.LocalLobby != null)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(this.LocalLobby.Id);
            yield return waitForSeconds;
        }
    }

    private async Task InvokeUpdateLobbyCallback()
    {
        try
        {
            this.LocalLobby = await Lobbies.Instance.GetLobbyAsync(this.LocalLobby.Id);
        }
        catch
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Warning; 
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerLobby.Instance.MessageTooManyRequests;
            UIManagerGlobal.Instance.PanelMessageBox.Show(null);

            await Awaitable.WaitForSecondsAsync(LobbyManager.LOBBY_REFRESH_TIME);

            this.LocalLobby = await Lobbies.Instance.GetLobbyAsync(this.LocalLobby.Id);
        }
    }

    private async void HandlePlayerLeft(List<int> leftPlayer)
    {
        await this.InvokeUpdateLobbyCallback();
    }

    private async void HandlePlayerJoined(List<LobbyPlayerJoined> joinedPlayer)
    {
        await this.InvokeUpdateLobbyCallback();
    }

    #endregion

    #region Handle Lobby Deleted

    private void HandleLobbyDeleted()
    {
        if (!this.IsHost)
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerLobby.Instance.MessageHostDisconnected;
            UIManagerGlobal.Instance.PanelMessageBox.Show(this.InvokeHandleLobbyDeletedCallback);
        }

        this.IsHost = false;
    }

    private async void InvokeHandleLobbyDeletedCallback()
    {
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerLobby.Instance.MessageDisconnecting;
        UIManagerGlobal.Instance.PanelMessageBox.Show(null);

        await this.DisconnectLobby();
    }

    #endregion

    #region Handle Kicked From Lobby

    private void HandleKickedFromLobby()
    {
        Debug.Log("In HandleKickedFromLobby 1");

        if (!this.hasLeft)
        {
            Debug.Log("In HandleKickedFromLobby 2");

            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerLobby.Instance.MessageKicked;
            UIManagerGlobal.Instance.PanelMessageBox.Show(this.InvokeKickedFromLobbyCallback);
        }
    }

    private async void InvokeKickedFromLobbyCallback()
    {
        this.LocalLobby = null;

        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerLobby.Instance.MessageDisconnecting;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            await GameCoordinator.Instance.LoadScene(GameCoordinator.MonopolyScene.MainMenu);
        }
    }

    #endregion 

    #region Host & Connect & Disconnect & Kick

    public async Task DisconnectLobby()
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

        this.LocalLobby = null;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            await GameCoordinator.Instance.LoadScene(GameCoordinator.MonopolyScene.MainMenu);
        }
    }

    public async Task HostLobby(string relayCode)
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
            await GameCoordinator.Instance.LoadScene(GameCoordinator.MonopolyScene.GameLobby);
        }
    }

    public async Task ConnectLobby(string joinCode)
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
    }

    public async Task KickFromLobby(string playerId)
    {
        await LobbyService.Instance.RemovePlayerAsync(this.LocalLobby.Id, playerId);
        NetworkManager.Singleton?.DisconnectClient((ushort)NetworkManager.Singleton?.ConnectedClientsIds.LastOrDefault());
    }

    #endregion
}