using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Relay;

#if UNITY_EDITOR
using UnityEditor;
#endif

internal sealed class UIManagerMainMenu : MonoBehaviour
{
    #region Loading Tab

    [Space]
    [Header("Loading Tab")]
    [Space]

    [Space]
    [SerializeField] private Canvas loadingTab;

    #region Controls

    [Space]
    [Header("Image Loading")]
    [Space]

    [Space]
    [SerializeField] private Image imageLoading;

    #endregion

    #endregion

    #region Main Menu Tab

    [Space]
    [Header("Main Menu Tab")]
    [Space]

    [SerializeField] private Canvas canvasMainMenuTab;

    #region Controls

    [Space]
    [Header("Controls")]
    [Space]

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

    [Space]
    [SerializeField] private int nicknameMinLength;

    [Space]
    [SerializeField] private int nicknameMaxLength;

    #endregion

    #region Messages

    [Space]
    [Header("Messages")]
    [Space]

    [Space]
    [SerializeField] private string messageNicknameEmpty;

    [Space]
    [SerializeField] private string messageNicknameTooLong;

    [Space]
    [SerializeField] private string messageNicknameTooShort;

    [Space]
    [SerializeField] private string messageConfirmClosingGame;

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

    [Space]
    [SerializeField] private Button buttonCancel;

    [Space]
    [SerializeField] private Button buttonConnect;

    [Space]
    [SerializeField] private TMP_InputField textBoxJoinCode;

    #endregion

    #region Messages

    [Space]
    [SerializeField] private string messageEmptyJoinCode;

    [Space]
    [SerializeField] private string messageWrongJoinCode;

    [Space]
    [SerializeField] private string messageGameCoordinatorIsDown;

    #endregion

    #endregion

    public static UIManagerMainMenu Instance { get; private set; }

    private PanelMessageBoxUI PanelMessageBox { get => PanelMessageBoxUI.Instance; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void OnEnable()
    {
        this.buttonCloseGame.onClick.AddListener(this.HandleButtonCloseGameClicked);
        this.buttonHostLobby.onClick.AddListener(this.HandleButtonHostLobbyClicked);
        this.buttonConnectLobby.onClick.AddListener(this.HandleButtonConnectLobbyClicked);

        this.buttonCancel.onClick.AddListener(this.HandleButtonCancelClicked);
        this.buttonConnect.onClick.AddListener(this.HandleButtonConnectClicked);

        this.PanelMessageBox.ButtonConfirmPanelOKCancelClicked += this.HandleCloseGame;
        this.PanelMessageBox.ButtonConfirmPanelOKClicked += this.HandleClosePanelMessageBox;
        this.PanelMessageBox.ButtonCancelPanelOKCancelClicked += this.HandleClosePanelMessageBox;

        GameCoordinator.LocalInstance.EstablishingConnectionFailed += this.HandleEstablishingConnectionFailed;
    }

    private void OnDisable()
    {
        this.buttonCloseGame.onClick.RemoveListener(this.HandleButtonCloseGameClicked);
        this.buttonHostLobby.onClick.RemoveListener(this.HandleButtonHostLobbyClicked);
        this.buttonConnectLobby.onClick.RemoveListener(this.HandleButtonConnectLobbyClicked);

        this.buttonCancel.onClick.RemoveListener(this.HandleButtonCancelClicked);
        this.buttonConnect.onClick.RemoveListener(this.HandleButtonConnectClicked);

        this.PanelMessageBox.ButtonConfirmPanelOKCancelClicked -= this.HandleCloseGame;
        this.PanelMessageBox.ButtonConfirmPanelOKClicked -= this.HandleClosePanelMessageBox;
        this.PanelMessageBox.ButtonCancelPanelOKCancelClicked -= this.HandleClosePanelMessageBox;

        GameCoordinator.LocalInstance.EstablishingConnectionFailed -= this.HandleEstablishingConnectionFailed;
    }

    #region Validation

    private bool ValidateTextBoxNickname()
    {
        if (String.IsNullOrWhiteSpace(this.textBoxNickname.text))
        {
            this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Warning;
            this.PanelMessageBox.MessageText = this.messageNicknameEmpty;
        }
        else if (this.textBoxNickname.text.Length > this.nicknameMaxLength)
        {
            this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            this.PanelMessageBox.MessageText = this.messageNicknameTooLong;
        }
        else if (this.textBoxNickname.text.Length < this.nicknameMinLength)
        {
            this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            this.PanelMessageBox.MessageText = this.messageNicknameTooShort;
        }
        else
        {
            return true;
        }

        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
        this.PanelMessageBox.Show();
        return false;
    }

    private bool ValidateTextBoxJoinCode()
    {
        if (String.IsNullOrWhiteSpace(this.textBoxJoinCode.text))
        {
            this.PanelMessageBox.MessageText = this.messageEmptyJoinCode;
            this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
            this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Warning;
            this.PanelMessageBox.Show();
            return false;
        }
        else
        {
            return true;
        }
    }

    #endregion

    #region UI Callbacks

    private void HandleCloseGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif

        Application.Quit();
    }

    private void HandleButtonCancelClicked()
    {
        this.canvasMainMenuTab.gameObject.SetActive(true);
        this.canvasConnectionTab.gameObject.SetActive(false);
    }

    private void HandleButtonConnectClicked()
    {
        if (this.ValidateTextBoxJoinCode())
        {
            GameCoordinator.LocalInstance.ConnectLobbyAsync(this.textBoxJoinCode.text);
        }
    }

    private void HandleClosePanelMessageBox()
    {
        this.PanelMessageBox.Hide();
    }

    private void HandleButtonHostLobbyClicked()
    {
        if (this.ValidateTextBoxNickname())
        {
            GameCoordinator.LocalInstance.HostLobbyAsync();
        }
    }

    private void HandleButtonCloseGameClicked()
    {
        this.PanelMessageBox.MessageText = this.messageConfirmClosingGame;
        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OKCancel;
        this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Question;
        this.PanelMessageBox.Show();
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

    #region GameCoordinator Callbacks

    private void HandleEstablishingConnectionFailed(RelayServiceException relayServiceException)
    {
        switch (relayServiceException.Reason)
        {
            case RelayExceptionReason.InvalidRequest:
            case RelayExceptionReason.JoinCodeNotFound:
                this.PanelMessageBox.MessageText = this.messageWrongJoinCode;
                break;
            case RelayExceptionReason.NetworkError:
            case RelayExceptionReason.EntityNotFound:
            case RelayExceptionReason.RegionNotFound:
            case RelayExceptionReason.NoSuitableRelay:
            case RelayExceptionReason.AllocationNotFound:
                this.PanelMessageBox.MessageText = this.messageGameCoordinatorIsDown;
                break;
            default:
                this.PanelMessageBox.MessageText = relayServiceException.Message;
                break;
        }

        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
        this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
        this.PanelMessageBox.Show();
    }

    #endregion
}
