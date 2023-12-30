using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

internal sealed class UIManagerGameLobby : MonoBehaviour
{
    #region Setup

    #region Shared Visuals

    [Header("Shared Visuals")]

    [Space]
    [SerializeField] private TMP_Text labelJoinCode;

    [Space]
    [SerializeField] private Canvas canvaslPlayersList;

    [Space]
    [SerializeField] private PanelPlayerLobbyUI panelPlayerLobby;

    #endregion

    #region Shared Controls

    [Header("Shared Controls")]

    [Space]
    [SerializeField] private Button buttonDisconnect;

    #endregion

    #region Host Controls

    [Header("Host Controls")]

    [Space]
    [SerializeField] private Canvas canvasHost;

    [Space]
    [SerializeField] private Button buttonStartGame;

    #endregion

    #region Client Controls

    [Header("Client Controls")]

    [Space]
    [SerializeField] private Canvas canvasClient;

    [Space]
    [SerializeField] private Button buttonReady;

    #endregion

    #region Messages

    [Header("Messages")]

    [Space]
    [SerializeField] private string messageLoadingGame;

    [Space]
    [SerializeField] private string messagePendingGame;

    [Space]
    [SerializeField] private string messageFailedToLoad;

    [Space]
    [SerializeField] private string messageDisconnecting;

    [Space]
    [SerializeField] private string messageTooFewPlayers;

    [Space]
    [SerializeField] private string messageFailedToConnect;

    [Space]
    [SerializeField] private string messageConfirmStartGame;

    [Space]
    [SerializeField] private string messageHostDisconnected;

    [Space]
    [SerializeField] private string messageCannotKickNotHost;

    [Space]
    [SerializeField] private string messageConfirmDisconnect;

    [Space]
    [SerializeField] private string messageConfirmKickPlayer;

    [Space]
    [SerializeField] private string messageCannotKickYourself;

    [Space]
    [SerializeField] private string messageNotAllPlayersLoaded;

    [Space]
    [SerializeField] private string messageCannotKickPlayerAlreadyLeft;

    #endregion

    #endregion

    public static UIManagerGameLobby Instance { get; private set; }

    public string MessagePendingGame 
    {
        get => this.messagePendingGame;
    }

    public string MessageFailedToLoad 
    {
        get => this.messageFailedToLoad;
    }

    public string MessageDisconnecting 
    {
        get => this.messageDisconnecting;
    }

    public string MessageTooFewPlayers 
    {
        get => this.messageTooFewPlayers;
    }

    public string MessageFailedToConnect 
    {
        get => this.messageFailedToConnect;
    }

    public string MessageHostDisconnected 
    { 
        get => this.messageHostDisconnected; 
    }

    public string MessageCannotKickNotHost 
    {
        get => this.messageCannotKickNotHost;
    }

    public string MessageConfirmKickPlayer 
    {
        get => this.messageConfirmKickPlayer;
    }

    public string MessageCannotKickYourself 
    {
        get => this.messageCannotKickYourself;
    }

    public string MessageNotAllPlayersLoaded 
    {
        get => this.messageNotAllPlayersLoaded;
    }

    public string MessageCannotKickPlayerAlreadyLeft 
    {
        get => this.messageCannotKickPlayerAlreadyLeft;
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        this.labelJoinCode.text = LobbyManager.Instance.JoinCode;
    }

    private void OnEnable()
    {
        this.buttonStartGame.onClick.AddListener(this.HandleButtonStartGame);
        this.buttonDisconnect.onClick.AddListener(this.HandleButtonDisconnectClicked);

        LobbyManager.Instance.OnGameLobbyLoaded += this.HandleGameLobbyLoaded;
        LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerLeft += this.HandlePlayerLeft;
        LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerJoined += this.HandlePlayerJoined;
        LobbyManager.Instance.OnMonopolyGameFailedToLoad += this.HandleMonopolyGameFailedToLoad;
    }

    private void OnDisable()
    {
        this.buttonStartGame.onClick.RemoveListener(this.HandleButtonStartGame);
        this.buttonDisconnect.onClick.RemoveListener(this.HandleButtonDisconnectClicked);

        LobbyManager.Instance.OnGameLobbyLoaded -= this.HandleGameLobbyLoaded;
        LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerLeft -= this.HandlePlayerLeft;
        LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerJoined -= this.HandlePlayerJoined;
        LobbyManager.Instance.OnMonopolyGameFailedToLoad -= this.HandleMonopolyGameFailedToLoad;
    }

    #region Updating GUI

    private void AddPlayerToList(Player player)
    {
        PanelPlayerLobbyUI newPanelPlayer = GameObject.Instantiate(this.panelPlayerLobby, this.canvaslPlayersList.transform);
        newPanelPlayer.PlayerNickname = player.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
        newPanelPlayer.name = player.Id;
    }

    private void RemovePlayerFromList(int playerIndex)
    {
        GameObject.Destroy(this.canvaslPlayersList.transform.GetChild(playerIndex).gameObject);
    }

    private void InitializePlayersList(List<Player> players)
    {
        foreach (Player player in players)
        {
            this.panelPlayerLobby.PlayerNickname = player.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;

            PanelPlayerLobbyUI newPanelPlayer = GameObject.Instantiate(this.panelPlayerLobby, this.canvaslPlayersList.transform);
            newPanelPlayer.name = player.Id;
        }
    }

    #endregion

    #region Lobby Callbacks

    private void HandleGameLobbyLoaded()
    {
        if (LobbyManager.Instance.IsHost)
        {
            this.canvasHost.gameObject.SetActive(true);
        }
        else
        {
            this.canvasClient.gameObject.SetActive(true);
        }

        this.InitializePlayersList(LobbyManager.Instance.LocalLobby.Players);
    }

    private void HandleMonopolyGameFailedToLoad()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessagePlayersFailedToLoad, PanelMessageBoxUI.Icon.Error);
    }

    private void HandlePlayerLeft(List<int> leftPlayers)
    {
        foreach (int playerIndex in leftPlayers)
        {
            this.RemovePlayerFromList(playerIndex);
        }
    }

    private void HandlePlayerJoined(List<LobbyPlayerJoined> joinedPlayers)
    {
        foreach (LobbyPlayerJoined newPlayer in joinedPlayers)
        {
            this.AddPlayerToList(newPlayer.Player);
        }
    }

    #endregion

    #region Button Start Game

    private void HandleButtonStartGame()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OKCancel, this.messageConfirmStartGame, PanelMessageBoxUI.Icon.Question, this.CallbackButtonStartGame);
    }

    private void CallbackButtonStartGame()
    {
        if (UIManagerGlobal.Instance.LastMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
        {
            LobbyManager.Instance.StartGameAsync();
        }
    }

    #endregion

    #region Button Disconnect

    private void HandleButtonDisconnectClicked()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OKCancel, this.messageConfirmDisconnect, PanelMessageBoxUI.Icon.Question, this.CallbackButtonDisconnectAsync);
    }

    private async void CallbackButtonDisconnectAsync()
    {
        if (UIManagerGlobal.Instance.LastMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, this.messageDisconnecting, PanelMessageBoxUI.Icon.Loading);

            await LobbyManager.Instance.DisconnectFromLobbyAsync();
        }
    }

    #endregion
}
