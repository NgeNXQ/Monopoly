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
    private const float LOBBY_UPTIME = 10.0f;

    public const string KEY_GAME_STATUS = "GameStatus";

    public const string GAME_STATUS_PENDING = "Pending";

    public const string GAME_STATUS_STARTED = "Started";

    public const string KEY_PLAYER_NICKNAME = "PlayerNickname";

    public const string KEY_PLAYER_STATUS = "PlayerStatus";

    public const string PLAYER_STATUS_READY = "Ready";

    public const string PLAYER_STATUS_NOT_READY = "NotReady";

    private string lobbyName 
    {
        get => $"LOBBY_{this.JoinCode}"; 
    }

    private bool isLobbyActive
    {
        get => this.CurrentLobby != null;
    }

    //private Player localPlayer;

    private bool isLocalPlayerHost
    {
        get => GameCoordinator.Instance.LocalPlayer?.Id == this.CurrentLobby?.HostId; 
    }

    private LobbyEventCallbacks lobbyEventCallbacks;

    public static LobbyManager Instance { get; private set; }

    public Action OnGameLobbyLoaded;

    //public Action HostDisconnected;

    //public Action<ClientRpcParams> ClientConnected;

    //public Action<ServerRpcParams> ClientConnected;

    //public Action<ClientRpcParams> ClientDisconnected;

    //public Action<ClientRpcParams> ClientDisconnected;

    public string JoinCode { get; private set; }

    public Lobby CurrentLobby { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        this.lobbyEventCallbacks = new LobbyEventCallbacks();

        this.OnGameLobbyLoaded += this.HandleGameLobbyLoaded;

        this.lobbyEventCallbacks.PlayerLeft += this.HandlePlayerLeft;
        this.lobbyEventCallbacks.DataChanged += this.HandleDataChanged;
        this.lobbyEventCallbacks.LobbyDeleted += this.HandleLobbyDeleted;
        this.lobbyEventCallbacks.PlayerJoined += this.HandlePlayerJoined;
    }

    private void OnDisable()
    {
        this.lobbyEventCallbacks = new LobbyEventCallbacks();

        this.OnGameLobbyLoaded -= this.HandleGameLobbyLoaded;

        this.lobbyEventCallbacks.PlayerLeft -= this.HandlePlayerLeft;
        this.lobbyEventCallbacks.DataChanged -= this.HandleDataChanged;
        this.lobbyEventCallbacks.LobbyDeleted -= this.HandleLobbyDeleted;
        this.lobbyEventCallbacks.PlayerJoined -= this.HandlePlayerJoined;
    }

    #region Connect & Disconnect

    public void StartGame()
    {
        GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.MonopolyGame);
    }

    public async Task DisconnectLobby()
    {
        if (this.isLocalPlayerHost) 
        {
            this.StopCoroutine(this.PingLobbyCoroutine());
            await LobbyService.Instance.DeleteLobbyAsync(this.CurrentLobby.Id);
        }
        else
        {
            Lobby foundLobby = await this.SelectLobbyByJoinCode(this.JoinCode);

            if (foundLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(this.CurrentLobby.Id, GameCoordinator.Instance.LocalPlayer.Id);
            }
            else
            {
                UIManagerLobby.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
                UIManagerLobby.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
                UIManagerLobby.Instance.PanelMessageBox.MessageText = UIManagerLobby.Instance.MessageHostDisconnected;
                UIManagerLobby.Instance.PanelMessageBox.Show(this.InvokeDisconnectLobbyCallback);
            }
        }

        this.CurrentLobby = null;

        NetworkManager.Singleton.Shutdown();

        await GameCoordinator.Instance.LoadScene(GameCoordinator.MonopolyScene.MainMenu);
    }

    public async Task HostLobby(string relayCode)
    {
        this.JoinCode = relayCode;

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions()
        {
            IsPrivate = false,
            Player = GameCoordinator.Instance.LocalPlayer,
            Data = new Dictionary<string, DataObject>()
            {
                { LobbyManager.KEY_GAME_STATUS, new DataObject(DataObject.VisibilityOptions.Public, LobbyManager.GAME_STATUS_PENDING) }
            }
        };

        try
        {
            this.CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(this.lobbyName, GameCoordinator.MAX_PLAYERS, lobbyOptions);

            await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.CurrentLobby.Id, this.lobbyEventCallbacks);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }

        this.StartCoroutine(this.PingLobbyCoroutine());

        await GameCoordinator.Instance.LoadScene(GameCoordinator.MonopolyScene.GameLobby);
    }

    public async Task ConnectLobby(string joinCode)
    {
        this.JoinCode = joinCode;

        JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions()
        {
            Player = GameCoordinator.Instance.LocalPlayer
        };

        try
        {
            Lobby foundLobby = await this.SelectLobbyByJoinCode(joinCode);

            Debug.Log(foundLobby.Name);

            this.CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(foundLobby.Id, joinOptions);

            await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.CurrentLobby.Id, this.lobbyEventCallbacks);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }
    }

    #endregion

    #region Callbacks Connect & Disconnect 

    private void HandleDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> changedData)
    {

    }

    private void HandleGameLobbyLoaded()
    {
        if (this.isLocalPlayerHost)
        {
            UIManagerLobby.Instance.ShowHostControls();
        }
        else
        {
            UIManagerLobby.Instance.ShowClientControls();
        }

        UIManagerLobby.Instance.InitializePlayersList(this.CurrentLobby.Players);
    }

    private async void HandleLobbyDeleted()
    {
        if (!this.isLocalPlayerHost)
        {
            await this.DisconnectLobby();
        }
    }

    private void HandlePlayerLeft(List<int> leftPlayers)
    {
        UIManagerLobby.Instance.RemovePlayerFromList(leftPlayers.First());
    }

    private void HandlePlayerJoined(List<LobbyPlayerJoined> joinedPlayer)
    {
        Debug.Log(joinedPlayer.Last().Player.Id);
        Debug.Log(joinedPlayer.First().Player.Id);

        UIManagerLobby.Instance.AddPlayerToList(joinedPlayer.Last().Player);
    }

    #endregion

    private void InvokeDisconnectLobbyCallback()
    {

    }

    private IEnumerator PingLobbyCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(LobbyManager.LOBBY_UPTIME);

        while (this.isLobbyActive)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(this.CurrentLobby.Id);
            yield return waitForSeconds;
        }
    }

    private async Task<Lobby> SelectLobbyByJoinCode(string joinCode)
    {
        QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
        {
            Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.Name, joinCode, QueryFilter.OpOptions.CONTAINS)
            }
        };

        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

        if (queryResponse.Results.Count == 1)
        {
            return queryResponse.Results.FirstOrDefault();
        }
        else
        {
            return null;
        }
    }
}
