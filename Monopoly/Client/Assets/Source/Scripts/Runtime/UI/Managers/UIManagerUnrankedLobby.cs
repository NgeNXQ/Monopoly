using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Lobby;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;
using Monopoly.Client.Runtime.UI.Utilities.Pools.Concrete;

namespace Monopoly.Client.Runtime.UI.Managers
{
    internal sealed class UIManagerUnrankedLobby : MonoBehaviour
    {
        [Header("Shared Visuals")]

        [Space]
        [SerializeField]
        private TMP_Text labelJoinCode;

        [Space]
        [SerializeField]
        private Canvas canvaslPlayersList;

        [Space]
        [SerializeField]
        private PlayerUnrankedLobbyPanel panelPlayer;

        [Space]
        [Header("Host Controls")]

        [Space]
        [SerializeField]
        private Canvas canvasHost;

        [Space]
        [SerializeField]
        private Button buttonStartGame;

        [Space]
        [SerializeField]
        private Button buttonDisconnectHost;

        [Space]
        [Header("Client Controls")]

        [Space]
        [SerializeField]
        private Canvas canvasClient;

        [Space]
        [SerializeField]
        private Button buttonDisconnectClient;

        [Space]
        [Header("Messages")]

        [Space]
        [SerializeField]
        private string messageKicked;

        [Space]
        [SerializeField]
        private string messageLoadingGame;

        [Space]
        [SerializeField]
        private string messagePendingGame;

        [Space]
        [SerializeField]
        private string messageFailedToLoad;

        [Space]
        [SerializeField]
        private string messageDisconnecting;

        [Space]
        [SerializeField]
        private string messageTooFewPlayers;

        [Space]
        [SerializeField]
        private string messageFailedToConnect;

        [Space]
        [SerializeField]
        private string messageConfirmStartGame;

        [Space]
        [SerializeField]
        private string messageHostDisconnected;

        [Space]
        [SerializeField]
        private string messageCannotKickNotHost;

        [Space]
        [SerializeField]
        private string messageConfirmDisconnect;

        [Space]
        [SerializeField]
        private string messageConfirmKickPlayer;

        [Space]
        [SerializeField]
        private string messageCannotKickYourself;

        [Space]
        [SerializeField]
        private string messageNotAllPlayersLoaded;

        [Space]
        [SerializeField]
        private string messageCannotKickPlayerAlreadyLeft;

        internal static UIManagerUnrankedLobby Instance { get; private set; }

        internal string MessageKicked
        {
            get => this.messageKicked;
        }

        internal string MessagePendingGame
        {
            get => this.messagePendingGame;
        }

        internal string MessageFailedToLoad
        {
            get => this.messageFailedToLoad;
        }

        internal string MessageDisconnecting
        {
            get => this.messageDisconnecting;
        }

        internal string MessageTooFewPlayers
        {
            get => this.messageTooFewPlayers;
        }

        internal string MessageFailedToConnect
        {
            get => this.messageFailedToConnect;
        }

        internal string MessageHostDisconnected
        {
            get => this.messageHostDisconnected;
        }

        internal string MessageCannotKickNotHost
        {
            get => this.messageCannotKickNotHost;
        }

        internal string MessageConfirmKickPlayer
        {
            get => this.messageConfirmKickPlayer;
        }

        internal string MessageCannotKickYourself
        {
            get => this.messageCannotKickYourself;
        }

        internal string MessageNotAllPlayersLoaded
        {
            get => this.messageNotAllPlayersLoaded;
        }

        internal string MessageCannotKickPlayerAlreadyLeft
        {
            get => this.messageCannotKickPlayerAlreadyLeft;
        }

        private void Awake()
        {
            if (UIManagerUnrankedLobby.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            UIManagerUnrankedLobby.Instance = this;
        }

        private void Start()
        {
            this.labelJoinCode.text = LobbyManager.Instance.JoinCode;

            GameCoordinator.Instance.UpdateInitializedObjects(this.GetType());
        }

        private void OnEnable()
        {
            this.buttonStartGame.onClick.AddListener(this.HandleButtonStartGameClicked);
            this.buttonDisconnectHost.onClick.AddListener(this.HandleButtonDisconnectClicked);
            this.buttonDisconnectClient.onClick.AddListener(this.HandleButtonDisconnectClicked);

            LobbyManager.Instance.OnGameLobbyLoaded += this.HandleGameLobbyLoaded;
            LobbyManager.Instance.OnMonopolyGameFailedToLoad += this.HandleMonopolyGameFailedToLoad;

            LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerLeft += this.HandlePlayerLeft;
            LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerJoined += this.HandlePlayerJoined;
        }

        private void OnDisable()
        {
            this.buttonStartGame.onClick.RemoveListener(this.HandleButtonStartGameClicked);
            this.buttonDisconnectHost.onClick.RemoveListener(this.HandleButtonDisconnectClicked);
            this.buttonDisconnectClient.onClick.RemoveListener(this.HandleButtonDisconnectClicked);

            LobbyManager.Instance.OnGameLobbyLoaded -= this.HandleGameLobbyLoaded;
            LobbyManager.Instance.OnMonopolyGameFailedToLoad -= this.HandleMonopolyGameFailedToLoad;

            LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerLeft -= this.HandlePlayerLeft;
            LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerJoined -= this.HandlePlayerJoined;
        }

        private void AddPlayerToList(Player player)
        {
            PlayerUnrankedLobbyPanel newPanel = UnrankedLobbyPlayerPanelsPool.Instance?.GetInactiveObject();
            newPanel.PlayerNickname = player.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
            newPanel.name = player.Id;
        }

        private void RemovePlayerFromList(int playerIndex)
        {
            this.canvaslPlayersList.transform.GetChild(playerIndex).gameObject.SetActive(false);
        }

        private void InitializePlayersList(List<Player> players)
        {
            foreach (Player player in players)
            {
                this.AddPlayerToList(player);
            }
        }

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
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.OK,
                MessageBoxPanel.Icon.Error,
                UIManagerGame.Instance.MessagePlayersFailedToLoad
            );
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

        private void CallbackButtonStartGame()
        {
            if (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult == MessageBoxPanel.DialogResult.OK)
            {
                LobbyManager.Instance.StartGame();
            }
        }

        private void HandleButtonStartGameClicked()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.OKCancel,
                MessageBoxPanel.Icon.Question,
                this.messageConfirmStartGame,
                this.CallbackButtonStartGame
            );
        }

        private void HandleButtonDisconnectClicked()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.OKCancel,
                MessageBoxPanel.Icon.Question,
                this.messageConfirmDisconnect,
                this.CallbackButtonDisconnectAsync
            );
        }

        private async void CallbackButtonDisconnectAsync()
        {
            if (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult == MessageBoxPanel.DialogResult.OK)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.None,
                    MessageBoxPanel.Icon.Loading,
                    this.messageDisconnecting
                );

                await LobbyManager.Instance.DisconnectFromLobbyAsync();
            }
        }
    }
}
