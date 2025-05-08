using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Monopoly.Client.Runtime.UI.Managers
{
    internal sealed class UIManagerMainMenu : MonoBehaviour
    {
        [Header("Main Menu Tab")]

        [Space]
        [SerializeField] private Canvas canvasMainMenuTab;

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

        [Space]
        [Header("Settings Nickname")]

        [Space]
        [SerializeField] private int nicknameMinLength;

        [Space]
        [SerializeField] private int nicknameMaxLength;

        [Space]
        [Header("Connection Tab")]

        [Space]
        [SerializeField] private Canvas canvasConnectionTab;

        [Space]
        [Header("Controls")]

        [Space]
        [SerializeField] private Button buttonCancel;

        [Space]
        [SerializeField] private Button buttonConnect;

        [Space]
        [SerializeField] private TMP_InputField textBoxJoinCode;

        [Space]
        [Header("Messages")]

        [Space]
        [Header("General")]

        [Space]
        [SerializeField] private string messageConfirmClosingGame;

        [Space]
        [SerializeField] private string messageEstablishingConnection;

        [Space]
        [Header("Code Validation")]

        [Space]
        [SerializeField] private string messageEmptyJoinCode;

        [Space]
        [SerializeField] private string messageInvalidLengthJoinCode;

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
            if (UIManagerMainMenu.Instance != null)
                throw new TypeInitializationException(nameof(UIManagerMainMenu), new ApplicationException($"Singleton has already been initialized."));

            UIManagerMainMenu.Instance = this;
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

        private bool ValidateTextBoxNickname()
        {
            const string pattern = @"[^\S ]";

            if (String.IsNullOrWhiteSpace(this.textBoxNickname.text))
            {
                UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Warning, this.messageNicknameEmpty);
            }
            else if (this.textBoxNickname.text.Length > this.nicknameMaxLength)
            {
                UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageNicknameTooLong);
            }
            else if (this.textBoxNickname.text.Length < this.nicknameMinLength)
            {
                UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageNicknameTooShort);
            }
            else if (Regex.IsMatch(this.textBoxNickname.text, pattern))
            {
                UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageEnterCorrectNickname);
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
                UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Warning, this.messageEmptyJoinCode);
            }
            else if (this.textBoxJoinCode.text.Length != UIManagerMainMenu.JOIN_CODE_LENGTH)
            {
                UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageInvalidLengthJoinCode);
            }
            else
            {
                return true;
            }

            return false;
        }

        private void CallbackCloseGame()
        {
            if (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult == MessageBoxPanel.DialogResult.OK)
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
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.OKCancel,
                MessageBoxPanel.Icon.Question,
                this.messageConfirmClosingGame,
                this.CallbackCloseGame
            );
        }

        private void HandleButtonConnectLobbyClicked()
        {
            if (this.ValidateTextBoxNickname())
            {
                this.canvasMainMenuTab.gameObject.SetActive(false);
                this.canvasConnectionTab.gameObject.SetActive(true);
            }
        }

        private async void HandleButtonConnectClickedAsync()
        {
            if (this.ValidateTextBoxJoinCode())
            {
                UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.None, MessageBoxPanel.Icon.Loading, this.messageEstablishingConnection);

                GameCoordinator.Instance.UpdateLocalPlayer(this.textBoxNickname.text);

                await GameCoordinator.Instance.ConnectLobbyAsync(this.textBoxJoinCode.text);
            }
        }

        private async void HandleButtonHostLobbyClickedAsync()
        {
            if (this.ValidateTextBoxNickname())
            {
                UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.None, MessageBoxPanel.Icon.Loading, this.messageEstablishingConnection);

                GameCoordinator.Instance.UpdateLocalPlayer(this.textBoxNickname.text);

                await GameCoordinator.Instance.HostLobbyAsync();
            }
        }

        private void HandleEstablishingConnectionRelayFailed(RelayServiceException relayServiceException)
        {
            switch (relayServiceException.Reason)
            {
                case RelayExceptionReason.InvalidRequest:
                case RelayExceptionReason.JoinCodeNotFound:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageInvalidJoinCode);
                    break;
                case RelayExceptionReason.NetworkError:
                case RelayExceptionReason.EntityNotFound:
                case RelayExceptionReason.RegionNotFound:
                case RelayExceptionReason.NoSuitableRelay:
                case RelayExceptionReason.AllocationNotFound:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageGameCoordinatorIsDown);
                    break;
                default:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, relayServiceException.Message);
                    break;
            }
        }

        private void HandleEstablishingConnectionLobbyFailed(LobbyServiceException lobbyServiceException)
        {
            switch (lobbyServiceException.Reason)
            {
                case LobbyExceptionReason.LobbyFull:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageLobbyIsFull);
                    break;
                case LobbyExceptionReason.LobbyNotFound:
                case LobbyExceptionReason.InvalidJoinCode:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageInvalidJoinCode);
                    break;
                case LobbyExceptionReason.LobbyConflict:
                case LobbyExceptionReason.LobbyAlreadyExists:
                case LobbyExceptionReason.LobbyEventServiceConnectionError:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageFailedToJoinLobby);
                    break;
                default:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, lobbyServiceException.Message);
                    break;
            }
        }
    }
}
