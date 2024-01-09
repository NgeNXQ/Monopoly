using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

internal sealed class UIManagerMainMenu : MonoBehaviour
{
    #region Setup

    #region Main Menu Tab

    [Header("Main Menu Tab")]

    [Space]
    [SerializeField] private Canvas canvasMainMenuTab;

    #region Controls

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField] private TMP_InputField textBoxNickname;

    [Space]
    [SerializeField] private Button buttonConnectLobby;

    [Space]
    [SerializeField] private Button buttonHostLobby;

    [Space]
    [SerializeField] private Button buttonCloseGame;

    #endregion

    #region Nickname settings

    [Space]
    [Header("Settings Nickname")]

    [Space]
    [SerializeField] private int nicknameMinLength;

    [Space]
    [SerializeField] private int nicknameMaxLength;

    #endregion

    #endregion

    #region Connection Tab

    [Space]
    [Header("Connection Tab")]

    [Space]
    [SerializeField] private Canvas canvasConnectionTab;

    #region Controls

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField] private Button buttonCancel;

    [Space]
    [SerializeField] private Button buttonConnect;

    [Space]
    [SerializeField] private TMP_InputField textBoxJoinCode;

    #endregion

    #endregion

    #region Messages

    [Space]
    [Header("Messages")]
    
    #region General

    [Space]
    [Header("General")]

    [Space]
    [SerializeField] private string messageConfirmClosingGame;

    [Space]
    [SerializeField] private string messageEstablishingConnection;

    #endregion

    #region Code Validation

    [Space]
    [Header("Code Validation")]

    [Space]
    [SerializeField] private string messageEmptyJoinCode;

    [Space]
    [SerializeField] private string messageInvalidLengthJoinCode;



    #endregion

    #region Nickname Validation

    [Space]
    [Header("Nickname Validation")]

    [Space]
    [SerializeField] private string messageNicknameEmpty;

    [Space]
    [SerializeField] private string messageNicknameTooLong;

    [Space]
    [SerializeField] private string messageNicknameTooShort;

    [Space]
    [SerializeField] private string messageEnterCorrectNickname;

    #endregion

    #region Establishing Connection

    [Space]
    [Header("Establishing Connection")]
    
    [Space]
    [SerializeField] private string messageKicked;

    [Space]
    [SerializeField] private string messageLobbyIsFull;

    [Space]
    [SerializeField] private string messageDisconnecting;

    [Space]
    [SerializeField] private string messageInvalidJoinCode;

    [Space]
    [SerializeField] private string messageHostDisconnected;

    [Space]
    [SerializeField] private string messageFailedToJoinLobby;

    [Space]
    [SerializeField] private string messageGameCoordinatorIsDown;

    #endregion

    #endregion

    #endregion

    private const int JOIN_CODE_LENGTH = 6;

    public static UIManagerMainMenu Instance { get; private set; }

    public string MessageKicked 
    {
        get => this.messageKicked;
    }

    public string MessageDisconnecting 
    {
        get => this.messageDisconnecting;
    }

    public string MessageHostDisconnected 
    {
        get => this.messageHostDisconnected;
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        this.textBoxNickname.text = PlayerPrefs.GetString(LobbyManager.KEY_PLAYER_NICKNAME);
    }

    private void OnEnable()
    {
        this.buttonCancel.onClick.AddListener(this.HandleButtonCancelClicked);
        this.buttonConnect.onClick.AddListener(this.HandleButtonConnectClickedAsync);

        this.buttonCloseGame.onClick.AddListener(this.HandleButtonCloseGameClicked);
        this.buttonHostLobby.onClick.AddListener(this.HandleButtonHostLobbyClickedAsync);
        this.buttonConnectLobby.onClick.AddListener(this.HandleButtonConnectLobbyClicked);
        
        GameCoordinator.Instance.OnEstablishingConnectionRelayFailed += this.HandleEstablishingConnectionRelayFailed;
        GameCoordinator.Instance.OnEstablishingConnectionLobbyFailed += this.HandleEstablishingConnectionLobbyFailed;
    }

    private void OnDisable()
    {
        this.buttonCancel.onClick.RemoveListener(this.HandleButtonCancelClicked);
        this.buttonConnect.onClick.RemoveListener(this.HandleButtonConnectClickedAsync);

        this.buttonCloseGame.onClick.RemoveListener(this.HandleButtonCloseGameClicked);
        this.buttonHostLobby.onClick.RemoveListener(this.HandleButtonHostLobbyClickedAsync);
        this.buttonConnectLobby.onClick.RemoveListener(this.HandleButtonConnectLobbyClicked);

        GameCoordinator.Instance.OnEstablishingConnectionRelayFailed -= this.HandleEstablishingConnectionRelayFailed;
        GameCoordinator.Instance.OnEstablishingConnectionLobbyFailed -= this.HandleEstablishingConnectionLobbyFailed;
    }

    #region Validation

    private bool ValidateTextBoxNickname()
    {
        const string pattern = @"[^\S ]";

        if (String.IsNullOrWhiteSpace(this.textBoxNickname.text))
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageNicknameEmpty, PanelMessageBoxUI.Icon.Warning);
        }
        else if (this.textBoxNickname.text.Length > this.nicknameMaxLength)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageNicknameTooLong, PanelMessageBoxUI.Icon.Error);
        }
        else if (this.textBoxNickname.text.Length < this.nicknameMinLength)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageNicknameTooShort, PanelMessageBoxUI.Icon.Error);
        }
        else if (Regex.IsMatch(this.textBoxNickname.text, pattern))
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageEnterCorrectNickname, PanelMessageBoxUI.Icon.Error);
        }
        else
        {
            return true;
        }

        return false;
    }

    private bool ValidateTextBoxJoinCode()
    {
        if (String.IsNullOrWhiteSpace(this.textBoxJoinCode.text))
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageEmptyJoinCode, PanelMessageBoxUI.Icon.Warning);
        }
        else if (this.textBoxJoinCode.text.Length != UIManagerMainMenu.JOIN_CODE_LENGTH)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageInvalidLengthJoinCode, PanelMessageBoxUI.Icon.Error);
        }
        else
        {
            return true;
        }

        return false;
    }

    #endregion

    #region GUI Callbacks

    #region General Callbacks

    private void CallbackCloseGame()
    {
        if (UIManagerGlobal.Instance.LastMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }

    private void HandleButtonCancelClicked()
    {
        this.canvasMainMenuTab.gameObject.SetActive(true);
        this.canvasConnectionTab.gameObject.SetActive(false);
    }

    private void HandleButtonCloseGameClicked()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OKCancel, this.messageConfirmClosingGame, PanelMessageBoxUI.Icon.Question, this.CallbackCloseGame);
    }

    private void HandleButtonConnectLobbyClicked()
    {
        if (this.ValidateTextBoxNickname())
        {
            this.canvasMainMenuTab.gameObject.SetActive(false);
            this.canvasConnectionTab.gameObject.SetActive(true);
        }
    }

    #endregion

    #region Connection Callbacks
    
    private async void HandleButtonConnectClickedAsync()
    {
        if (this.ValidateTextBoxJoinCode())
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, this.messageEstablishingConnection, PanelMessageBoxUI.Icon.Loading);

            GameCoordinator.Instance.UpdateLocalPlayer(this.textBoxNickname.text);

            await GameCoordinator.Instance.ConnectLobbyAsync(this.textBoxJoinCode.text);
        }
    }

    private async void HandleButtonHostLobbyClickedAsync()
    {
        if (this.ValidateTextBoxNickname())
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, this.messageEstablishingConnection, PanelMessageBoxUI.Icon.Loading);

            GameCoordinator.Instance.UpdateLocalPlayer(this.textBoxNickname.text);

            await GameCoordinator.Instance.HostLobbyAsync();
        }
    }

    #endregion

    #endregion

    #region GameCoordinator Callbacks

    private void HandleEstablishingConnectionRelayFailed(RelayServiceException relayServiceException)
    {
        switch (relayServiceException.Reason)
        {
            case RelayExceptionReason.InvalidRequest:
            case RelayExceptionReason.JoinCodeNotFound:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageInvalidJoinCode, PanelMessageBoxUI.Icon.Error);
                break;
            case RelayExceptionReason.NetworkError:
            case RelayExceptionReason.EntityNotFound:
            case RelayExceptionReason.RegionNotFound:
            case RelayExceptionReason.NoSuitableRelay:
            case RelayExceptionReason.AllocationNotFound:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageGameCoordinatorIsDown, PanelMessageBoxUI.Icon.Error);
                break;
            default:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, relayServiceException.Message, PanelMessageBoxUI.Icon.Error);
                break;
        }
    }

    private void HandleEstablishingConnectionLobbyFailed(LobbyServiceException lobbyServiceException)
    {
        switch (lobbyServiceException.Reason)
        {
            case LobbyExceptionReason.LobbyFull:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageLobbyIsFull, PanelMessageBoxUI.Icon.Error);
                break;
            case LobbyExceptionReason.LobbyNotFound:
            case LobbyExceptionReason.InvalidJoinCode:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageInvalidJoinCode, PanelMessageBoxUI.Icon.Error);
                break;
            case LobbyExceptionReason.LobbyConflict:
            case LobbyExceptionReason.LobbyAlreadyExists:
            case LobbyExceptionReason.LobbyEventServiceConnectionError:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageFailedToJoinLobby, PanelMessageBoxUI.Icon.Error);
                break;
            default:
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, lobbyServiceException.Message, PanelMessageBoxUI.Icon.Error);
                break;
        }
    }

    #endregion
}
