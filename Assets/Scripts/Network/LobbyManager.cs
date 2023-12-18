using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;

internal sealed class LobbyManager : MonoBehaviour
{
    private const float LOBBY_UPTIME = 25.0f;

    public const string KEY_PLAYER_NICKNAME = "Nickname";

    private LobbyEventCallbacks lobbyEventCallbacks;

    private string lobbyName { get => $"LOBBY_{this.JoinCode}"; }

    public static LobbyManager LocalInstance { get; private set; }

    public Action GameLobbyLoaded;

    //public Action HostDisconnected;

    //public Action<ClientRpcParams> ClientConnected;

    //public Action<ServerRpcParams> ClientConnected;

    //public Action<ClientRpcParams> ClientDisconnected;

    //public Action<ClientRpcParams> ClientDisconnected;

    public string JoinCode { get; private set; }

    public Player LocalPlayer { get; private set; }

    public Lobby CurrentLobby { get; private set; }

    private void Awake()
    {
        if (LocalInstance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        LocalInstance = this;
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        this.GameLobbyLoaded += this.HandleLocalLobbyLoaded;

        this.lobbyEventCallbacks = new LobbyEventCallbacks();

        this.lobbyEventCallbacks.LobbyDeleted += this.HandleLobbyDeleted;
        this.lobbyEventCallbacks.PlayerJoined += this.HandlePlayerJoined;
        this.lobbyEventCallbacks.KickedFromLobby += this.HandlePlayerDisconnected;
    }

    private void OnDisable()
    {
        this.GameLobbyLoaded -= this.HandleLocalLobbyLoaded;

        this.lobbyEventCallbacks = new LobbyEventCallbacks();

        this.lobbyEventCallbacks.LobbyDeleted -= this.HandleLobbyDeleted;
        this.lobbyEventCallbacks.PlayerJoined -= this.HandlePlayerJoined;
        this.lobbyEventCallbacks.KickedFromLobby -= this.HandlePlayerDisconnected;
    }

    #region UI Callbacks

    public void StartGame()
    {
        GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.MonopolyGame);
    }

    private void HandleCancelDisconnectClicked()
    {
        UIManagerLobby.Instance.PanelMessageBox.Hide();
    }

    #endregion

    #region Lobby Connect & Disconnect

    public async Task DisconnectLobby()
    {
        Debug.Log(this.LocalPlayer.Id);
        Debug.Log(this.CurrentLobby.Id);
        Debug.Log(this.CurrentLobby.HostId);

        Debug.Log("1");

        if (this.LocalPlayer.Id == this.CurrentLobby.HostId)
        {
            Debug.Log("2");
            await LobbyService.Instance.DeleteLobbyAsync(this.CurrentLobby.Id);
        }
        else
        {
            Debug.Log("3");
            await LobbyService.Instance.RemovePlayerAsync(this.CurrentLobby.Id, this.LocalPlayer.Id);
        }

        Debug.Log("4");
        this.CurrentLobby = null;
        NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);

        Debug.Log("5");

        Debug.Log(NetworkManager.Singleton.IsClient);
        Debug.Log(NetworkManager.Singleton.IsHost);
    }

    public async Task HostLobby(Player player, string joinCode)
    {
        this.JoinCode = joinCode;
        this.LocalPlayer = player;

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = false,
        };

        try
        {
            this.CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(this.lobbyName, GameCoordinator.MAX_PLAYERS, lobbyOptions);

            ILobbyEvents currentLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.CurrentLobby.Id, this.lobbyEventCallbacks);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }

        this.StartCoroutine(this.PingLobbyCoroutine());

        GameCoordinator.Instance.LoadScene(GameCoordinator.MonopolyScene.GameLobby);
    }

    public async Task ConnectLobby(Player player, string joinCode)
    {
        this.JoinCode = joinCode;
        this.LocalPlayer = player;

        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.Name, joinCode, QueryFilter.OpOptions.CONTAINS)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            this.CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results.First().Id, new JoinLobbyByIdOptions() { Player = player });

            ILobbyEvents currentLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.CurrentLobby.Id, this.lobbyEventCallbacks);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            throw lobbyServiceException;
        }
    }

    private IEnumerator PingLobbyCoroutine()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(LobbyManager.LOBBY_UPTIME);

        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(this.CurrentLobby.Id);
            yield return waitForSeconds;
        }
    }

    #endregion

    #region Connect & Disconnect Callbacks

    private async void HandleLobbyDeleted()
    {
        Debug.Log("In HandleLobbyDeleted 1");

        if (this.LocalPlayer.Id != this.CurrentLobby.HostId)
        {
            Debug.Log("In HandleLobbyDeleted 2");

            await this.DisconnectLobby();
        }
    }

    private void HandlePlayerJoined(List<LobbyPlayerJoined> joinedPlayer)
    {
        UIManagerLobby.Instance.UpdatePlayersList(joinedPlayer.First().Player);
    }

    private void HandlePlayerDisconnected()
    {
        //UIManagerLobby.Instance.PanelMessageBox.ButtonCancelPanelOKCancelClicked -= this.HandleCancelDisconnectClicked;
        //UIManagerLobby.Instance.PanelMessageBox.ButtonConfirmPanelOKCancelClicked -= this.HandleConfirmDisconnectClicked;

        Debug.Log("Disconnected");
    }

    private void HandleLocalLobbyLoaded()
    {
        //UIManagerLobby.Instance.PanelMessageBox.ButtonCancelPanelOKCancelClicked += this.HandleCancelDisconnectClicked;
        //UIManagerLobby.Instance.PanelMessageBox.ButtonConfirmPanelOKCancelClicked += this.HandleConfirmDisconnectClicked;

        UIManagerLobby.Instance.ShowPlayerControls(this.LocalPlayer);
        UIManagerLobby.Instance.FillPlayersList(this.CurrentLobby.Players);
    }

    #endregion

    private void Update()
    {
        if (this.CurrentLobby != null)
        {
            Debug.Log(this.CurrentLobby.LastUpdated);
        }


    }
}
