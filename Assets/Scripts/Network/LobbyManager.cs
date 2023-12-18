using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;

internal sealed class LobbyManager : NetworkBehaviour
{
    private const float LOBBY_UPTIME = 25.0f;

    public const string KEY_PLAYER_NICKNAME = "Nickname";

    private Player localPlayer;

    private string lobbyName { get => $"LOBBY_{this.JoinCode}"; }

    public static LobbyManager LocalInstance { get; private set; }

    public Action ClientLoaded;

    public Action HostDisconnected;

    public Action<ClientRpcParams> ClientConnected;

    public Action<ClientRpcParams> ClientDisconnected;

    public string JoinCode { get; private set; }

    public Lobby CurrentLobby { get; private set; }

    public Player LastConnectedPlayer { get => this.CurrentLobby.Players.Last(); }

    public IReadOnlyList<Player> ConnectedPlayers { get => this.CurrentLobby.Players; }

    private void Awake()
    {
        if (LocalInstance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        LocalInstance = this;
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        //this.ClientConnected += this.HandleClientConnected;
        //this.ClientDisconnected += this.HandleClientDisconnected;

        UIManagerLobby.Instance.PanelMessageBox.ButtonCancelPanelOKCancelClicked += this.HandleCancelDisconnectClicked;
        UIManagerLobby.Instance.PanelMessageBox.ButtonConfirmPanelOKCancelClicked += this.HandleConfirmDisconnectClicked;
    }

    private void OnDisable()
    {
        if (GameCoordinator.Instance.ActiveScene == GameCoordinator.MonopolyScene.GameLobby)
        {
            //this.ClientConnected -= this.HandleClientConnected;
            //this.ClientDisconnected -= this.HandleClientDisconnected;

            UIManagerLobby.Instance.PanelMessageBox.ButtonCancelPanelOKCancelClicked -= this.HandleCancelDisconnectClicked;
            UIManagerLobby.Instance.PanelMessageBox.ButtonConfirmPanelOKCancelClicked -= this.HandleConfirmDisconnectClicked;
        }
    }

    public async Task DisconnectLobby()
    {
        //this.ClientDisconnected?.Invoke();
        await LobbyService.Instance.RemovePlayerAsync(this.CurrentLobby.Id, this.localPlayer.Id);
    }

    public async Task HostLobby(Player player, string joinCode)
    {
        this.JoinCode = joinCode;
        this.localPlayer = player;

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = false,
        };

        try
        {
            this.CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(this.lobbyName, GameCoordinator.MAX_PLAYERS, lobbyOptions);
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
        this.localPlayer = player;

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

    #region Connect & Disconnect Callbacks

    private void HandleClientConnected()
    {
        
    }

    private void HandleClientDisconnected()
    {
        GameCoordinator.Instance.LoadScene(GameCoordinator.MonopolyScene.MainMenu);
    }

    #endregion

    #region UI Disconnect Callbacks

    private void HandleCancelDisconnectClicked()
    {
        UIManagerLobby.Instance.PanelMessageBox.Hide();
    }

    private async void HandleConfirmDisconnectClicked()
    {
        await this.DisconnectLobby();
    }

    #endregion
}
