using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Relay;

internal sealed class UIManagerMainMenu : MonoBehaviour
{
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
    [SerializeField] private Button buttonHostLobby;

    [Space]
    [SerializeField] private Button buttonConnectLobby;

    [Space]
    [SerializeField] private TMP_InputField textBoxNickname;

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

    private int NicknameMinLength { get => this.nicknameMinLength; }

    private int NicknameMaxLength { get => this.nicknameMaxLength; }

    private string MessageNicknameEmpty { get => this.messageNicknameEmpty; }

    private string MessageNicknameTooLong { get => this.messageNicknameTooLong; }

    private string MessageNicknameTooShort { get => this.messageNicknameTooShort; }

    private string MessageEmptyJoinCode { get => this.messageEmptyJoinCode; }

    private string MessageWrongJoinCode { get => this.messageWrongJoinCode; }

    private string MessageGameCoordinatorIsDown { get => this.messageGameCoordinatorIsDown; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void OnEnable()
    {
        this.PanelMessageBox.ButtonConfirmClicked += ClosePanelMessageBoxCallback;

        this.buttonCancel.onClick.AddListener(this.ButtonCancelClickedCallback);
        this.buttonConnect.onClick.AddListener(this.ButtonConnectClickedCallback);
        this.buttonHostLobby.onClick.AddListener(this.ButtonHostLobbyClickedCallback);
        this.buttonConnectLobby.onClick.AddListener(this.ButtonConnectLobbyClickedCallback);

        GameCoordinator.LocalInstance.EstablishingConnectionFailed += HandleEstablishingConnectionFailed;
    }

    private void OnDisable()
    {
        this.PanelMessageBox.ButtonConfirmClicked -= ClosePanelMessageBoxCallback;

        this.buttonCancel.onClick.RemoveListener(this.ButtonCancelClickedCallback);
        this.buttonConnect.onClick.RemoveListener(this.ButtonConnectClickedCallback);
        this.buttonHostLobby.onClick.RemoveListener(this.ButtonHostLobbyClickedCallback);
        this.buttonConnectLobby.onClick.RemoveListener(this.ButtonConnectLobbyClickedCallback);

        GameCoordinator.LocalInstance.EstablishingConnectionFailed -= HandleEstablishingConnectionFailed;
    }

    private bool ValidateTextBoxNickname()
    {
        if (String.IsNullOrWhiteSpace(this.textBoxNickname.text))
        {
            this.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Warning;
            this.PanelMessageBox.MessageText = this.MessageNicknameEmpty;
        }
        else if (this.textBoxNickname.text.Length > this.NicknameMaxLength)
        {
            this.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Error;
            this.PanelMessageBox.MessageText = this.MessageNicknameTooLong;
        }
        else if (this.textBoxNickname.text.Length < this.NicknameMinLength)
        {
            this.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Error;
            this.PanelMessageBox.MessageText = this.MessageNicknameTooShort;
        }
        else
        {
            return true;
        }

        this.PanelMessageBox.Show();
        return false;
    }

    private bool ValidateTextBoxJoinCode()
    {
        if (String.IsNullOrWhiteSpace(this.textBoxJoinCode.text))
        {
            this.PanelMessageBox.MessageText = this.MessageEmptyJoinCode;
            this.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Warning;
            this.PanelMessageBox.Show();
            return false;
        }
        else
        {
            return true;
        }
    }

    private void ButtonCancelClickedCallback()
    {
        this.canvasMainMenuTab.gameObject.SetActive(true);
        this.canvasConnectionTab.gameObject.SetActive(false);
    }

    private void ButtonConnectClickedCallback()
    {
        if (ValidateTextBoxJoinCode())
        {
            GameCoordinator.LocalInstance.ConnectLobbyAsync(this.textBoxJoinCode.text);
        }
    }

    private void ButtonHostLobbyClickedCallback()
    {
        if (this.ValidateTextBoxNickname())
        {
            try
            {
                GameCoordinator.LocalInstance.HostLobbyAsync();
            }
            catch (RelayServiceException relayServiceException)
            {
                switch (relayServiceException.Reason)
                {
                    case RelayExceptionReason.RegionNotFound:
                    case RelayExceptionReason.NoSuitableRelay:
                    case RelayExceptionReason.AllocationNotFound:
                        this.PanelMessageBox.MessageText = this.MessageGameCoordinatorIsDown;
                        break;
                    default:
                        this.PanelMessageBox.MessageText = relayServiceException.Message;
                        break;
                }

                this.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Error;
                this.PanelMessageBox.Show();
            }
        }
    }

    private void ButtonConnectLobbyClickedCallback()
    {
        if (this.ValidateTextBoxNickname())
        {
            this.canvasMainMenuTab.gameObject.SetActive(false);
            this.canvasConnectionTab.gameObject.SetActive(true);
        }
    }

    private void ClosePanelMessageBoxCallback()
    {
        this.PanelMessageBox.Hide();
    }

    private void HandleEstablishingConnectionFailed(RelayServiceException relayServiceException)
    {
        switch (relayServiceException.Reason)
        {
            case RelayExceptionReason.InvalidRequest:
            case RelayExceptionReason.JoinCodeNotFound:
                this.PanelMessageBox.MessageText = this.MessageWrongJoinCode;
                break;
            case RelayExceptionReason.NetworkError:
            case RelayExceptionReason.EntityNotFound:
            case RelayExceptionReason.RegionNotFound:
            case RelayExceptionReason.NoSuitableRelay:
            case RelayExceptionReason.AllocationNotFound:
                this.PanelMessageBox.MessageText = this.MessageGameCoordinatorIsDown;
                break;
            default:
                this.PanelMessageBox.MessageText = relayServiceException.Message;
                break;
        }

        this.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Error;
        this.PanelMessageBox.Show();
    }
}
