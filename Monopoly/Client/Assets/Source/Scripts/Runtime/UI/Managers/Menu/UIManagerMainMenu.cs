using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Unity.Services.Lobbies.Models.Extensions;
using Monopoly.Domain.DTOs.Api;

namespace Monopoly.Client.Runtime.UI.Managers.Menu
{
    internal sealed class UIManagerMainMenu : MonoBehaviour
    {
        [SerializeField, Header("Initial Tab"), Space]
        private Canvas canvasTabInitial;

        [SerializeField]
        private Button buttonPlayTabInitial;

        [SerializeField]
        private Button buttonProfileTabInitial;

        [SerializeField, Header("Profile Tab"), Space]
        private Canvas canvasTabProfile;

        [SerializeField]
        private Button buttonBackTabProfile;

        [SerializeField]
        private TMP_Text textLabelTrophiesTabProfile;

        [SerializeField]
        private TMP_InputField textBoxNicknameTabProfile;

        [SerializeField, Header("Game Mode Tab"), Space]
        private Canvas canvasTabGameMode;

        [SerializeField]
        private Button buttonBackTabGameMode;

        [SerializeField]
        private Button buttonPublicLobbyTabGameMode;

        [SerializeField]
        private Button buttonPrivateLobbyTabGameMode;

        [SerializeField, Header("Public Lobby Tab"), Space]
        private Canvas canvasTabPublicLobby;

        [SerializeField]
        private Button buttonBackTabPublicLobby;

        [SerializeField]
        private Button buttonHostTabPublicLobby;

        [SerializeField]
        private Button buttonSearchTabPublicLobby;

        [SerializeField, Header("Private Lobby Tab"), Space]
        private Canvas canvasTabPrivateLobby;

        [SerializeField]
        private Button buttonBackTabPrivateLobby;

        [SerializeField]
        private Button buttonHostTabPrivateLobby;

        [SerializeField]
        private Button buttonClientTabPrivateLobby;

        [SerializeField, Header("Private Lobby Connection Tab"), Space]
        private Canvas canvasTabPrivateLobbyConnection;

        [SerializeField]
        private Button buttonBackTabPrivateLobbyConnection;

        [SerializeField]
        private Button buttonConnectTabPrivateLobbyConnection;

        [SerializeField]
        private TMP_InputField textBoxCodeTabPrivateLobbyConnection;

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
            this.canvasTabProfile.gameObject.SetActive(false);
            this.canvasTabGameMode.gameObject.SetActive(false);
            this.canvasTabPrivateLobby.gameObject.SetActive(false);
            this.canvasTabPrivateLobbyConnection.gameObject.SetActive(false);

            // this.textBoxNickname.text = PlayerPrefs.GetString(LobbyManager.KEY_PLAYER_NICKNAME);
        }

        private void OnEnable()
        {
            GameCoordinator.Instance.RelayConnectionFailedEvent += this.OnRelayConnectionFailed;
            GameCoordinator.Instance.LobbyConnectionFailedEvent += this.OnLobbyConnectionFailed;

            this.buttonPlayTabInitial.onClick.AddListener(this.OnButtonPlayTabInitialClicked);
            this.buttonProfileTabInitial.onClick.AddListener(this.OnButtonProfileTabInitialClicked);

            this.buttonBackTabProfile.onClick.AddListener(this.OnButtonBackTabProfileClicked);

            this.buttonBackTabGameMode.onClick.AddListener(this.OnButtonBackTabGameModeClicked);
            this.buttonPublicLobbyTabGameMode.onClick.AddListener(this.OnButtonPublicLobbyTabGameModeClicked);
            this.buttonPrivateLobbyTabGameMode.onClick.AddListener(this.OnButtonPrivateLobbyTabGameModeClicked);

            this.buttonBackTabPublicLobby.onClick.AddListener(this.OnButtonBackTabPublicLobbyClicked);
            this.buttonHostTabPublicLobby.onClick.AddListener(this.OnButtonHostTabPublicLobbyClicked);
            this.buttonSearchTabPublicLobby.onClick.AddListener(this.OnButtonSearchTabPublicLobbyClicked);

            this.buttonBackTabPrivateLobby.onClick.AddListener(this.OnButtonBackTabPrivateLobbyClicked);
            this.buttonClientTabPrivateLobby.onClick.AddListener(this.OnButtonClientTabPrivateLobbyClicked);
            this.buttonHostTabPrivateLobby.onClick.AddListener(this.OnButtonHostTabPrivateLobbyClicked);

            this.buttonBackTabPrivateLobbyConnection.onClick.AddListener(this.OnButtonBackTabPrivateLobbyConnectionClicked);
            this.buttonConnectTabPrivateLobbyConnection.onClick.AddListener(this.OnButtonConnectTabPrivateLobbyConnectionClicked);
            this.textBoxCodeTabPrivateLobbyConnection.onValueChanged.AddListener(this.OnTextBoxCodeTabPrivateLobbyConnectionValueChanged);
        }

        private void OnDisable()
        {
            GameCoordinator.Instance.RelayConnectionFailedEvent -= this.OnRelayConnectionFailed;
            GameCoordinator.Instance.LobbyConnectionFailedEvent -= this.OnLobbyConnectionFailed;

            this.buttonPlayTabInitial.onClick.RemoveListener(this.OnButtonPlayTabInitialClicked);
            this.buttonProfileTabInitial.onClick.RemoveListener(this.OnButtonProfileTabInitialClicked);

            this.buttonBackTabProfile.onClick.RemoveListener(this.OnButtonBackTabProfileClicked);

            this.buttonBackTabGameMode.onClick.RemoveListener(this.OnButtonBackTabGameModeClicked);
            this.buttonPublicLobbyTabGameMode.onClick.RemoveListener(this.OnButtonPublicLobbyTabGameModeClicked);
            this.buttonPrivateLobbyTabGameMode.onClick.RemoveListener(this.OnButtonPrivateLobbyTabGameModeClicked);

            this.buttonBackTabPublicLobby.onClick.RemoveListener(this.OnButtonBackTabPublicLobbyClicked);
            this.buttonHostTabPublicLobby.onClick.RemoveListener(this.OnButtonHostTabPublicLobbyClicked);
            this.buttonSearchTabPublicLobby.onClick.RemoveListener(this.OnButtonSearchTabPublicLobbyClicked);

            this.buttonBackTabPrivateLobby.onClick.RemoveListener(this.OnButtonBackTabPrivateLobbyClicked);
            this.buttonClientTabPrivateLobby.onClick.RemoveListener(this.OnButtonClientTabPrivateLobbyClicked);
            this.buttonHostTabPrivateLobby.onClick.RemoveListener(this.OnButtonHostTabPrivateLobbyClicked);

            this.buttonBackTabPrivateLobbyConnection.onClick.RemoveListener(this.OnButtonBackTabPrivateLobbyConnectionClicked);
            this.buttonConnectTabPrivateLobbyConnection.onClick.RemoveListener(this.OnButtonConnectTabPrivateLobbyConnectionClicked);
            this.textBoxCodeTabPrivateLobbyConnection.onValueChanged.RemoveListener(this.OnTextBoxCodeTabPrivateLobbyConnectionValueChanged);
        }

        private void OnButtonPlayTabInitialClicked()
        {
            this.canvasTabInitial.gameObject.SetActive(false);
            this.canvasTabGameMode.gameObject.SetActive(true);
        }

        private async void OnButtonProfileTabInitialClicked()
        {
            this.canvasTabProfile.gameObject.SetActive(true);
            this.canvasTabInitial.gameObject.SetActive(false);

            string accountGetResponse = await GameCoordinator.Instance.ServiceAccount.GetAccount();
            ApiResponse<AccountPayload> parsedAccountResponse = JsonConvert.DeserializeObject<ApiResponse<AccountPayload>>(accountGetResponse);

            //  LocalPlayer.GetData(GameCoordinator.KEY_PLAYER_DATA_NICKNAME);
            // GameCoordinator.Instance.LocalPlayer.GetData(GameCoordinator.KEY_PLAYER_DATA_TROPHIES);

            this.textBoxNicknameTabProfile.text = parsedAccountResponse.Payload.Nickname;
            this.textLabelTrophiesTabProfile.text = parsedAccountResponse.Payload.Trophies;
        }

        private async void OnButtonBackTabProfileClicked()
        {
            this.canvasTabInitial.gameObject.SetActive(true);
            this.canvasTabProfile.gameObject.SetActive(false);

            await GameCoordinator.Instance.UpdateLocalPlayerNickname(this.textBoxNicknameTabProfile.text);
        }

        private void OnButtonBackTabGameModeClicked()
        {
            this.canvasTabInitial.gameObject.SetActive(true);
            this.canvasTabGameMode.gameObject.SetActive(false);
        }

        private void OnButtonPublicLobbyTabGameModeClicked()
        {
            this.canvasTabGameMode.gameObject.SetActive(false);
            this.canvasTabPublicLobby.gameObject.SetActive(true);
        }

        private void OnButtonPrivateLobbyTabGameModeClicked()
        {
            this.canvasTabGameMode.gameObject.SetActive(false);
            this.canvasTabPrivateLobby.gameObject.SetActive(true);
        }

        private void OnButtonBackTabPublicLobbyClicked()
        {
            this.canvasTabGameMode.gameObject.SetActive(true);
            this.canvasTabPublicLobby.gameObject.SetActive(false);
        }

        private async void OnButtonHostTabPublicLobbyClicked()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxView.Type.None,
                MessageBoxView.Icon.Loading,
                this.messageEstablishingConnection
            );

            await GameCoordinator.Instance.HostPublicLobbyAsync();
        }

        private async void OnButtonSearchTabPublicLobbyClicked()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxView.Type.None,
                MessageBoxView.Icon.Loading,
                this.messageEstablishingConnection
            );

            await GameCoordinator.Instance.FindLobbyAsync();
        }

        private void OnButtonBackTabPrivateLobbyClicked()
        {
            this.canvasTabGameMode.gameObject.SetActive(true);
            this.canvasTabPrivateLobby.gameObject.SetActive(false);
        }

        private async void OnButtonHostTabPrivateLobbyClicked()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxView.Type.None,
                MessageBoxView.Icon.Loading,
                this.messageEstablishingConnection
            );

            await GameCoordinator.Instance.HostPrivateLobbyAsync();
        }

        private void OnButtonClientTabPrivateLobbyClicked()
        {
            this.canvasTabPrivateLobby.gameObject.SetActive(false);
            this.canvasTabPrivateLobbyConnection.gameObject.SetActive(true);
        }

        private void OnButtonBackTabPrivateLobbyConnectionClicked()
        {
            this.canvasTabPrivateLobby.gameObject.SetActive(true);
            this.canvasTabPrivateLobbyConnection.gameObject.SetActive(false);
        }

        private async void OnButtonConnectTabPrivateLobbyConnectionClicked()
        {
            if (String.IsNullOrWhiteSpace(this.textBoxCodeTabPrivateLobbyConnection.text))
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Warning,
                    this.messageEmptyJoinCode
                );
                return;
            }

            if (this.textBoxCodeTabPrivateLobbyConnection.text.Length != UIManagerMainMenu.JOIN_CODE_LENGTH)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Error,
                    this.messageInvalidLengthJoinCode
                );
                return;
            }

            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxView.Type.None,
                MessageBoxView.Icon.Loading,
                this.messageEstablishingConnection
            );

            await GameCoordinator.Instance.JoinLobbyAsync(this.textBoxCodeTabPrivateLobbyConnection.text);
        }

        private void OnTextBoxCodeTabPrivateLobbyConnectionValueChanged(string value)
        {
            this.textBoxCodeTabPrivateLobbyConnection.text = value.ToUpper();
        }

        private void OnRelayConnectionFailed(RelayServiceException relayServiceException)
        {
            switch (relayServiceException.Reason)
            {
                case RelayExceptionReason.InvalidRequest:
                case RelayExceptionReason.JoinCodeNotFound:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxView.Type.OK, MessageBoxView.Icon.Error, this.messageInvalidJoinCode);
                    break;
                case RelayExceptionReason.NetworkError:
                case RelayExceptionReason.EntityNotFound:
                case RelayExceptionReason.RegionNotFound:
                case RelayExceptionReason.NoSuitableRelay:
                case RelayExceptionReason.AllocationNotFound:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxView.Type.OK, MessageBoxView.Icon.Error, this.messageGameCoordinatorIsDown);
                    break;
                default:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxView.Type.OK, MessageBoxView.Icon.Error, relayServiceException.Message);
                    break;
            }
        }

        private void OnLobbyConnectionFailed(LobbyServiceException lobbyServiceException)
        {
            switch (lobbyServiceException.Reason)
            {
                case LobbyExceptionReason.LobbyFull:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxView.Type.OK, MessageBoxView.Icon.Error, this.messageLobbyIsFull);
                    break;
                case LobbyExceptionReason.LobbyNotFound:
                case LobbyExceptionReason.InvalidJoinCode:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxView.Type.OK, MessageBoxView.Icon.Error, this.messageInvalidJoinCode);
                    break;
                case LobbyExceptionReason.LobbyConflict:
                case LobbyExceptionReason.LobbyAlreadyExists:
                case LobbyExceptionReason.LobbyEventServiceConnectionError:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxView.Type.OK, MessageBoxView.Icon.Error, this.messageFailedToJoinLobby);
                    break;
                default:
                    UIManagerGlobal.Instance.ShowMessageBox(MessageBoxView.Type.OK, MessageBoxView.Icon.Error, lobbyServiceException.Message);
                    break;
            }
        }
    }
}
