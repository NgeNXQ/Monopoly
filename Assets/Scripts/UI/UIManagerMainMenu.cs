using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Relay;
using Unity.Services.Lobbies;

#if UNITY_EDITOR
using UnityEditor;
#endif

internal sealed class UIManagerMainMenu : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]

    #region Main Menu Tab

    [Header("Main Menu Tab")]

    [Space]
    [SerializeField] private Canvas canvasMainMenuTab;

    #region Controls

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

    [Header("Controls")]

    [Space]
    [SerializeField] private Button buttonCancel;

    [Space]
    [SerializeField] private Button buttonConnect;

    [Space]
    [SerializeField] private TMP_InputField textBoxJoinCode;

    #endregion

    #region Join Code settings

    [Header("Settings Join Code")]

    [Space]
    [SerializeField] private int joinCodeLength;

    #endregion

    #endregion

    #region Messages

    [Space]
    [Header("Messages")]

    #region General Information

    [Header("General Information")]

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
    [SerializeField] private string messageConfirmClosingGame;

    #endregion

    #region Establishing Connection

    [Space]
    [Header("Establishing Connection")]

    [Space]
    [SerializeField] private string messageLobbyIsFull;

    [Space]
    [SerializeField] private string messageInvalidJoinCode;

    [Space]
    [SerializeField] private string messageFailedToJoinLobby;

    [Space]
    [SerializeField] private string messageGameCoordinatorIsDown;

    #endregion

    #endregion

    #endregion

    public static UIManagerMainMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        this.textBoxNickname.text = PlayerPrefs.GetString(GameCoordinator.KEY_NICKNAME_PLAYER_PREFS);
    }

    private void OnEnable()
    {
        this.buttonCancel.onClick.AddListener(this.HandleButtonCancelClicked);
        this.buttonConnect.onClick.AddListener(this.HandleButtonConnectClicked);

        this.buttonCloseGame.onClick.AddListener(this.HandleButtonCloseGameClicked);
        this.buttonHostLobby.onClick.AddListener(this.HandleButtonHostLobbyClicked);
        this.buttonConnectLobby.onClick.AddListener(this.HandleButtonConnectLobbyClicked);

        GameCoordinator.Instance.OnEstablishingConnectionRelayFailed += this.HandleEstablishingConnectionRelayFailed;
        GameCoordinator.Instance.OnEstablishingConnectionLobbyFailed += this.HandleEstablishingConnectionLobbyFailed;
    }

    private void OnDisable()
    {
        this.buttonCancel.onClick.RemoveListener(this.HandleButtonCancelClicked);
        this.buttonConnect.onClick.RemoveListener(this.HandleButtonConnectClicked);

        this.buttonCloseGame.onClick.RemoveListener(this.HandleButtonCloseGameClicked);
        this.buttonHostLobby.onClick.RemoveListener(this.HandleButtonHostLobbyClicked);
        this.buttonConnectLobby.onClick.RemoveListener(this.HandleButtonConnectLobbyClicked);

        GameCoordinator.Instance.OnEstablishingConnectionRelayFailed -= this.HandleEstablishingConnectionRelayFailed;
        GameCoordinator.Instance.OnEstablishingConnectionLobbyFailed -= this.HandleEstablishingConnectionLobbyFailed;
    }

    #region Validation

    private bool ValidateTextBoxNickname()
    {
        if (String.IsNullOrWhiteSpace(this.textBoxNickname.text))
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Warning;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageNicknameEmpty;
        }
        else if (this.textBoxNickname.text.Length > this.nicknameMaxLength)
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageNicknameTooLong;
        }
        else if (this.textBoxNickname.text.Length < this.nicknameMinLength)
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageNicknameTooShort;
        }
        else
        {
            return true;
        }

        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
        UIManagerGlobal.Instance.PanelMessageBox.Show(null);
        return false;
    }

    private bool ValidateTextBoxJoinCode()
    {
        if (String.IsNullOrWhiteSpace(this.textBoxJoinCode.text))
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Warning;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageEmptyJoinCode;
        }
        else if (this.textBoxJoinCode.text.Length != this.joinCodeLength)
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageInvalidLengthJoinCode;
        }
        else
        {
            return true;
        }

        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
        UIManagerGlobal.Instance.PanelMessageBox.Show(null);
        return false;
    }

    #endregion

    #region GUI Callbacks

    #region General Callbacks

    private void InvokeCloseGameCallback()
    {
        if (UIManagerGlobal.Instance.PanelMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
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
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageConfirmClosingGame;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OKCancel;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Question;
        UIManagerGlobal.Instance.PanelMessageBox.Show(this.InvokeCloseGameCallback);
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

    private async void HandleButtonConnectClicked()
    {
        if (this.ValidateTextBoxJoinCode())
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageEstablishingConnection;
            UIManagerGlobal.Instance.PanelMessageBox.Show(null);

            GameCoordinator.Instance.UpdateLocalPlayer(this.textBoxNickname.text);

            await GameCoordinator.Instance.ConnectLobbyAsync(this.textBoxJoinCode.text);
        }
    }

    private async void HandleButtonHostLobbyClicked()
    {
        if (this.ValidateTextBoxNickname())
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageEstablishingConnection;
            UIManagerGlobal.Instance.PanelMessageBox.Show(null);

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
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageInvalidJoinCode;
                break;
            case RelayExceptionReason.NetworkError:
            case RelayExceptionReason.EntityNotFound:
            case RelayExceptionReason.RegionNotFound:
            case RelayExceptionReason.NoSuitableRelay:
            case RelayExceptionReason.AllocationNotFound:
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageGameCoordinatorIsDown;
                break;
            default:
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = relayServiceException.Message;
                break;
        }

        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
        UIManagerGlobal.Instance.PanelMessageBox.Show(null);
    }

    private void HandleEstablishingConnectionLobbyFailed(LobbyServiceException lobbyServiceException)
    {
        switch (lobbyServiceException.Reason)
        {
            case LobbyExceptionReason.LobbyFull:
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageLobbyIsFull;
                break;
            case LobbyExceptionReason.LobbyNotFound:
            case LobbyExceptionReason.InvalidJoinCode:
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageInvalidJoinCode;
                break;
            case LobbyExceptionReason.LobbyConflict:
            case LobbyExceptionReason.LobbyAlreadyExists:
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageFailedToJoinLobby;
                break;
            default:
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = lobbyServiceException.Message;
                break;
        }

        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
        UIManagerGlobal.Instance.PanelMessageBox.Show(null);
    }

    #endregion
}
