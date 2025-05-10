using System;
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
        [SerializeField, Header("Lobby Code"), Space]
        private TMP_Text textLabelCode;

        [SerializeField, Header("Players List"), Space]
        private Canvas canvasPlayersList;

        [SerializeField, Header("Host's Canvas"), Space]
        private Canvas canvasHost;

        [SerializeField]
        private Button buttonStartHost;

        [SerializeField]
        private Button buttonDisconnectHost;

        [SerializeField, Header("Client's Canvas"), Space]
        private Canvas canvasClient;

        [SerializeField]
        private Button buttonDisconnectClient;

        [SerializeField, Header("Lobby Player's Panel"), Space]
        private PlayerUnrankedLobbyPanel panelPlayerLobby;

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
                throw new TypeInitializationException(nameof(UIManagerUnrankedLobby), new ApplicationException($"Singleton has already been initialized."));

            UIManagerUnrankedLobby.Instance = this;
        }

        private void Start()
        {
            if (LobbyManager.Instance.IsHost)
                this.canvasHost.gameObject.SetActive(true);
            else
                this.canvasClient.gameObject.SetActive(true);

            this.textLabelCode.text = LobbyManager.Instance.JoinCode;
            this.InitializePlayersList(LobbyManager.Instance.LocalLobby.Players);

            // this.InitializePlayersList(LobbyManager.Instance.LocalLobby.Players);
            // GameCoordinator.Instance.UpdateInitializedObjects(this.GetType());
        }

        private void OnEnable()
        {
            this.buttonStartHost.onClick.AddListener(this.OnButtonStartHostClicked);
            this.buttonDisconnectHost.onClick.AddListener(this.OnButtonDisconnectClicked);
            this.buttonDisconnectClient.onClick.AddListener(this.OnButtonDisconnectClicked);

            // LobbyManager.Instance.GameLobbyLoadedEvent += this.OnGameLobbyLoaded;
            LobbyManager.Instance.MonopolyGameFailedToLoadEvent += this.OnMonopolyGameFailedToLoad;

            LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerLeft += this.OnPlayerLeft;
            LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerJoined += this.OnPlayerJoined;
        }

        private void OnDisable()
        {
            this.buttonStartHost.onClick.RemoveListener(this.OnButtonStartHostClicked);
            this.buttonDisconnectHost.onClick.RemoveListener(this.OnButtonDisconnectClicked);
            this.buttonDisconnectClient.onClick.RemoveListener(this.OnButtonDisconnectClicked);

            // LobbyManager.Instance.GameLobbyLoadedEvent -= this.OnGameLobbyLoaded;
            LobbyManager.Instance.MonopolyGameFailedToLoadEvent -= this.OnMonopolyGameFailedToLoad;

            LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerLeft -= this.OnPlayerLeft;
            LobbyManager.Instance.LocalLobbyEventCallbacks.PlayerJoined -= this.OnPlayerJoined;
        }

        // private void OnGameLobbyLoaded()
        // {

        // }

        private void InitializePlayersList(List<Player> players)
        {
            foreach (Player player in players)
                this.AddPlayerToList(player);
        }

        private void AddPlayerToList(Player player)
        {
            PlayerUnrankedLobbyPanel newPanel = UnrankedLobbyPlayerPanelsPool.Instance?.GetInactiveObject();
            newPanel.PlayerNickname = player.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
            newPanel.name = player.Id;
        }

        private void OnMonopolyGameFailedToLoad()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.OK,
                MessageBoxPanel.Icon.Error,
                UIManagerGame.Instance.MessagePlayersFailedToLoad
            );
        }

        private void OnPlayerLeft(List<int> leftPlayers)
        {
            foreach (int playerIndex in leftPlayers)
                this.RemovePlayerFromList(playerIndex);
        }

        private void RemovePlayerFromList(int playerIndex)
        {
            this.canvasPlayersList.transform.GetChild(playerIndex).gameObject.SetActive(false);
        }

        private void OnPlayerJoined(List<LobbyPlayerJoined> joinedPlayers)
        {
            foreach (LobbyPlayerJoined newPlayer in joinedPlayers)
                this.AddPlayerToList(newPlayer.Player);
        }

        private void OnButtonStartHostClicked()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.OKCancel,
                MessageBoxPanel.Icon.Question,
                this.messageConfirmStartGame,
                this.CallbackButtonStartGame
            );
        }

        private void CallbackButtonStartGame()
        {
            if (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult == MessageBoxPanel.DialogResult.OK)
                LobbyManager.Instance.StartGame();
        }

        private void OnButtonDisconnectClicked()
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
