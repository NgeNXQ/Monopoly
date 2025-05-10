using System;
using System.Threading.Tasks;
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
        [SerializeField, Header("Initial Tab"), Space]
        private Canvas canvasTabInitial;

        [SerializeField]
        private Button buttonPlayTabInitial;

        [SerializeField, Header("Game Mode Tab"), Space]
        private Canvas canvasTabGameMode;

        [SerializeField]
        private Button buttonBackTabGameMode;

        [SerializeField]
        private Button buttonLobbyRankedTabGameMode;

        [SerializeField]
        private Button buttonLobbyUnrankedTabGameMode;

        [SerializeField, Header("Unranked Lobby Tab"), Space]
        private Canvas canvasTabUnrankedLobby;

        [SerializeField]
        private Button buttonBackTabUnrankedLobby;

        [SerializeField]
        private Button buttonHostTabUnrankedLobby;

        [SerializeField]
        private Button buttonClientTabUnrankedLobby;

        [SerializeField, Header("Unranked Lobby Connection Tab"), Space]
        private Canvas canvasTabUnrankedLobbyConnection;

        [SerializeField]
        private Button buttonBackTabUnrankedLobbyConnection;

        [SerializeField]
        private Button buttonConnectTabUnrankedLobbyConnection;

        [SerializeField]
        private TMP_InputField textBoxCodeTabUnrankedLobbyConnection;

        [Space]
        [Header("Settings Nickname")]

        [Space]
        [SerializeField] private int nicknameMinLength;

        [Space]
        [SerializeField] private int nicknameMaxLength;

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

        internal static UIManagerMainMenu Instance { get; private set; }

        internal string MessageKicked
        {
            get => this.messageKicked;
        }

        internal string MessageDisconnecting
        {
            get => this.messageDisconnecting;
        }

        internal string MessageHostDisconnected
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
            this.canvasTabInitial.gameObject.SetActive(true);
            this.canvasTabGameMode.gameObject.SetActive(false);
            this.canvasTabUnrankedLobby.gameObject.SetActive(false);
            this.canvasTabUnrankedLobbyConnection.gameObject.SetActive(false);

            // this.textBoxNickname.text = PlayerPrefs.GetString(LobbyManager.KEY_PLAYER_NICKNAME);
        }

        private void OnEnable()
        {
            GameCoordinator.Instance.RelayConnectionFailedEvent += this.OnRelayConnectionFailed;
            GameCoordinator.Instance.LobbyConnectionFailedEvent += this.OnLobbyConnectionFailed;

            this.buttonPlayTabInitial.onClick.AddListener(this.OnButtonPlayTabInitialClicked);

            this.buttonBackTabGameMode.onClick.AddListener(this.OnButtonBackTabGameModeClicked);
            this.buttonLobbyRankedTabGameMode.onClick.AddListener(this.OnButtonLobbyRankedTabGameModeClicked);
            this.buttonLobbyUnrankedTabGameMode.onClick.AddListener(this.OnButtonLobbyUnrankedTabGameModeClicked);

            this.buttonBackTabUnrankedLobby.onClick.AddListener(this.OnButtonBackTabUnrankedLobbyClicked);
            this.buttonHostTabUnrankedLobby.onClick.AddListener(this.OnButtonHostTabUnrankedLobbyClicked);
            this.buttonClientTabUnrankedLobby.onClick.AddListener(this.OnButtonClientTabUnrankedLobbyClicked);

            this.buttonBackTabUnrankedLobbyConnection.onClick.AddListener(this.OnButtonBackTabUnrankedLobbyConnectionClicked);
            this.buttonConnectTabUnrankedLobbyConnection.onClick.AddListener(this.OnButtonConnectTabUnrankedLobbyConnectionClicked);
            this.textBoxCodeTabUnrankedLobbyConnection.onValueChanged.AddListener(this.OnTextBoxCodeTabUnrankedLobbyConnectionValueChanged);
        }

        private void OnDisable()
        {
            GameCoordinator.Instance.RelayConnectionFailedEvent -= this.OnRelayConnectionFailed;
            GameCoordinator.Instance.LobbyConnectionFailedEvent -= this.OnLobbyConnectionFailed;

            this.buttonPlayTabInitial.onClick.RemoveListener(this.OnButtonPlayTabInitialClicked);

            this.buttonBackTabGameMode.onClick.RemoveListener(this.OnButtonBackTabGameModeClicked);
            this.buttonLobbyRankedTabGameMode.onClick.RemoveListener(this.OnButtonLobbyRankedTabGameModeClicked);
            this.buttonLobbyUnrankedTabGameMode.onClick.RemoveListener(this.OnButtonLobbyUnrankedTabGameModeClicked);

            this.buttonBackTabUnrankedLobby.onClick.RemoveListener(this.OnButtonBackTabUnrankedLobbyClicked);
            this.buttonHostTabUnrankedLobby.onClick.RemoveListener(this.OnButtonHostTabUnrankedLobbyClicked);
            this.buttonClientTabUnrankedLobby.onClick.RemoveListener(this.OnButtonClientTabUnrankedLobbyClicked);

            this.buttonBackTabUnrankedLobbyConnection.onClick.RemoveListener(this.OnButtonBackTabUnrankedLobbyConnectionClicked);
            this.buttonConnectTabUnrankedLobbyConnection.onClick.RemoveListener(this.OnButtonConnectTabUnrankedLobbyConnectionClicked);
            this.textBoxCodeTabUnrankedLobbyConnection.onValueChanged.RemoveListener(this.OnTextBoxCodeTabUnrankedLobbyConnectionValueChanged);
        }

        private void OnButtonPlayTabInitialClicked()
        {
            this.canvasTabInitial.gameObject.SetActive(false);
            this.canvasTabGameMode.gameObject.SetActive(true);
        }

        private void OnButtonBackTabGameModeClicked()
        {
            this.canvasTabInitial.gameObject.SetActive(true);
            this.canvasTabGameMode.gameObject.SetActive(false);
        }

        private void OnButtonLobbyRankedTabGameModeClicked()
        {

        }

        private void OnButtonLobbyUnrankedTabGameModeClicked()
        {
            this.canvasTabGameMode.gameObject.SetActive(false);
            this.canvasTabUnrankedLobby.gameObject.SetActive(true);
        }

        private void OnButtonBackTabUnrankedLobbyClicked()
        {
            this.canvasTabGameMode.gameObject.SetActive(true);
            this.canvasTabUnrankedLobby.gameObject.SetActive(false);
        }

        private async void OnButtonHostTabUnrankedLobbyClicked()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.None,
                MessageBoxPanel.Icon.Loading,
                this.messageEstablishingConnection
            );

            await GameCoordinator.Instance.HostLobbyAsync();
        }

        private void OnButtonClientTabUnrankedLobbyClicked()
        {
            this.canvasTabUnrankedLobby.gameObject.SetActive(false);
            this.canvasTabUnrankedLobbyConnection.gameObject.SetActive(true);
        }

        private void OnButtonBackTabUnrankedLobbyConnectionClicked()
        {
            this.canvasTabUnrankedLobby.gameObject.SetActive(true);
            this.canvasTabUnrankedLobbyConnection.gameObject.SetActive(false);
        }

        private async void OnButtonConnectTabUnrankedLobbyConnectionClicked()
        {
            if (String.IsNullOrWhiteSpace(this.textBoxCodeTabUnrankedLobbyConnection.text))
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Warning,
                    this.messageEmptyJoinCode
                );
                return;
            }

            if (this.textBoxCodeTabUnrankedLobbyConnection.text.Length != UIManagerMainMenu.JOIN_CODE_LENGTH)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Error,
                    this.messageInvalidLengthJoinCode
                );
                return;
            }

            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.None,
                MessageBoxPanel.Icon.Loading,
                this.messageEstablishingConnection
            );
            // GameCoordinator.Instance.UpdateLocalPlayer(this.textBoxNickname.text);
            await GameCoordinator.Instance.ConnectLobbyAsync(this.textBoxCodeTabUnrankedLobbyConnection.text);
        }

        private void OnTextBoxCodeTabUnrankedLobbyConnectionValueChanged(string value)
        {
            this.textBoxCodeTabUnrankedLobbyConnection.text = value.ToUpper();
        }

        //         private bool ValidateTextBoxNickname()
        //         {
        //             const string pattern = @"[^\S ]";

        //             if (String.IsNullOrWhiteSpace(this.textBoxNickname.text))
        //             {
        //                 UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Warning, this.messageNicknameEmpty);
        //             }
        //             else if (this.textBoxNickname.text.Length > this.nicknameMaxLength)
        //             {
        //                 UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageNicknameTooLong);
        //             }
        //             else if (this.textBoxNickname.text.Length < this.nicknameMinLength)
        //             {
        //                 UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageNicknameTooShort);
        //             }
        //             else if (Regex.IsMatch(this.textBoxNickname.text, pattern))
        //             {
        //                 UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.OK, MessageBoxPanel.Icon.Error, this.messageEnterCorrectNickname);
        //             }
        //             else
        //             {
        //                 return true;
        //             }

        //             return false;
        //         }

        //         private bool ValidateTextBoxJoinCode()
        //         {


        //             return false;
        //         }

        //         private void CallbackCloseGame()
        //         {
        //             if (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult == MessageBoxPanel.DialogResult.OK)
        //             {
        // #if UNITY_EDITOR
        //                 EditorApplication.ExitPlaymode();
        // #else
        //             Application.Quit();
        // #endif
        //             }
        //         }

        //         private void HandleButtonCancelClicked()
        //         {
        //             this.canvasMainMenuTab.gameObject.SetActive(true);
        //             this.canvasConnectionTab.gameObject.SetActive(false);
        //         }

        //         private void HandleButtonCloseGameClicked()
        //         {
        //             UIManagerGlobal.Instance.ShowMessageBox(
        //                 MessageBoxPanel.Type.OKCancel,
        //                 MessageBoxPanel.Icon.Question,
        //                 this.messageConfirmClosingGame,
        //                 this.CallbackCloseGame
        //             );
        //         }

        //         private void HandleButtonConnectLobbyClicked()
        //         {
        //             if (this.ValidateTextBoxNickname())
        //             {
        //                 this.canvasMainMenuTab.gameObject.SetActive(false);
        //                 this.canvasConnectionTab.gameObject.SetActive(true);
        //             }
        //         }

        //         private async void HandleButtonConnectClickedAsync()
        //         {
        //             if (this.ValidateTextBoxJoinCode())
        //             {

        //             }
        //         }

        //         private async void HandleButtonHostLobbyClickedAsync()
        //         {
        //             if (this.ValidateTextBoxNickname())
        //             {
        //                 UIManagerGlobal.Instance.ShowMessageBox(MessageBoxPanel.Type.None, MessageBoxPanel.Icon.Loading, this.messageEstablishingConnection);

        //                 GameCoordinator.Instance.UpdateLocalPlayer(this.textBoxNickname.text);

        //                 await GameCoordinator.Instance.HostLobbyAsync();
        //             }
        //         }

        private void OnRelayConnectionFailed(RelayServiceException relayServiceException)
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

        private void OnLobbyConnectionFailed(LobbyServiceException lobbyServiceException)
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
