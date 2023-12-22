using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
//using Unity.Netcode;

internal sealed class UIManagerLobby : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]
    [Space]

    #region Host Controls

    [Space]
    [Header("Host Controls")]
    [Space]

    [Space]
    [SerializeField] private Canvas canvasHost;

    [Space]
    [SerializeField] private Button buttonStartGame;

    #endregion

    #region Client Controls

    [Space]
    [Header("Client Controls")]
    [Space]

    [Space]
    [SerializeField] private Canvas canvasClient;

    [Space]
    [SerializeField] private Button buttonReady;

    #endregion

    #region Shared Visuals

    [Space]
    [Header("Shared Visuals")]
    [Space]

    [Space]
    [SerializeField] private TMP_Text labelJoinCode;

    [Space]
    [SerializeField] private Canvas canvaslPlayersList;

    [Space]
    [SerializeField] private PanelPlayerLobbyUI panelPlayerLobby;

    #endregion

    #region Shared Controls

    [Space]
    [Header("Shared Controls")]
    [Space]

    [Space]
    [SerializeField] private Button buttonDisconnect;

    #endregion

    #region Messages

    [Space]
    [Header("Messages")]
    [Space]

    [Space]
    [SerializeField] private string messageKicked;

    [Space]
    [SerializeField] private string messageLoadingGame;

    [Space]
    [SerializeField] private string messageFailedToLoad;

    [Space]
    [SerializeField] private string messageDisconnecting;

    [Space]
    [SerializeField] private string messageTooManyRequests;

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
    [SerializeField] private string messageCannotKickPlayerAlreadyLeft;

    #endregion

    #endregion

    public static UIManagerLobby Instance { get; private set; }

    public string MessageKicked 
    {
        get => this.messageKicked;
    }

    public string MessageFailedToLoad 
    {
        get => this.messageFailedToLoad;
    }

    public string MessageDisconnecting 
    {
        get => this.messageDisconnecting;
    }

    public string MessageTooManyRequests 
    {
        get => this.messageTooManyRequests;
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
        LobbyManager.Instance.OnGameLobbyLoaded += this.HandleGameLobbyLoaded;
        LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerLeft += this.HandlePlayerLeft;
        LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerJoined += this.HandlePlayerJoined;

        this.buttonStartGame.onClick.AddListener(this.HandleButtonStartGame);
        this.buttonDisconnect.onClick.AddListener(this.HandleButtonDisconnectClicked);
    }

    private void OnDisable()
    {
        LobbyManager.Instance.OnGameLobbyLoaded -= this.HandleGameLobbyLoaded;
        LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerLeft -= this.HandlePlayerLeft;
        LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerJoined -= this.HandlePlayerJoined;

        this.buttonStartGame.onClick.RemoveListener(this.HandleButtonStartGame);
        this.buttonDisconnect.onClick.RemoveListener(this.HandleButtonDisconnectClicked);
    }

    #region Updating GUI

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

    private void HandlePlayerLeft(List<int> leftPlayer)
    {
        this.RemovePlayerFromList(leftPlayer.FirstOrDefault());
    }

    private void HandlePlayerJoined(List<LobbyPlayerJoined> joinedPlayer)
    {
        this.AddPlayerToList(joinedPlayer.LastOrDefault().Player);
    }

    private void AddPlayerToList(Player player)
    {
        Debug.Log(LobbyManager.Instance.LocalLobby.Players.Count);
        Debug.Log(LobbyManager.Instance.LocalLobby.Players.Last().Id);

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

    #region Button Start Game

    private void HandleButtonStartGame()
    {
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OKCancel;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Question;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageConfirmStartGame;
        UIManagerGlobal.Instance.PanelMessageBox.Show(this.InvokeButtonStartGameCallback);
    }

    private void InvokeButtonStartGameCallback()
    {
        if (UIManagerGlobal.Instance.PanelMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK) 
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageLoadingGame;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
            UIManagerGlobal.Instance.PanelMessageBox.Show(null);

            LobbyManager.Instance.StartGame();
        }
    }

    #endregion

    #region Button Disconnect

    private void HandleButtonDisconnectClicked()
    {
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OKCancel;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Question;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageConfirmDisconnect;
        UIManagerGlobal.Instance.PanelMessageBox.Show(this.InvokeButtonDisconnectCallback);
    }

    private async void InvokeButtonDisconnectCallback()
    {
        if (UIManagerGlobal.Instance.PanelMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerLobby.Instance.MessageDisconnecting;
            UIManagerGlobal.Instance.PanelMessageBox.Show(null);

            await LobbyManager.Instance.DisconnectLobby();
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("LobbyManager: " + LobbyManager.Instance.LocalLobby.Players.Count);
            Debug.Log("NetworkManager: " + NetworkManager.Singleton.ConnectedClients.Count);
        }
    }

    #endregion
}
