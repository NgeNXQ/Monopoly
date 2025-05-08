using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Runtime.Game.Core;
using Monopoly.Client.Runtime.Game.Serializables;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Controllers.Concrete;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Game;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;

namespace Monopoly.Client.Runtime.UI.Managers
{
    internal sealed class UIManagerGame : NetworkBehaviour
    {
        [Header("Currency")]

        [Space]
        [SerializeField]
        private char currency;

        [Space]
        [Header("Button Roll Dice")]

        [Space]
        [SerializeField]
        private Button buttonRollDice;

        [Space]
        [Header("Images \"Dices\"")]

        [Space]
        [SerializeField]
        private Image imageDiePlaceholder1;

        [Space]
        [SerializeField]
        private Image imageDiePlaceholder2;

        [Space]
        [SerializeField]
        private Sprite[] spriteDieFaces = new Sprite[6];

        [Space]
        [Header("Dice screen time")]

        [Space]
        [SerializeField]
        [Range(0.0f, 10.0f)]
        private float diceScreenTime = 1.0f;

        [Space]
        [Header("Button Disconnect")]

        [Space]
        [SerializeField]
        private Button buttonDisconnect;

        [Space]
        [Header("Panels Players List")]

        [Space]
        [SerializeField]
        private Canvas canvasPlayersList;

        [Space]
        [Header("Messages")]

        [Space]
        [SerializeField]
        private string messageWon;

        [Space]
        [SerializeField]
        private string messageBankrupt;

        [Space]
        [SerializeField]
        private string messageSentJail;

        [Space]
        [SerializeField]
        private string messageAlreadyBuilt;

        [Space]
        [SerializeField]
        private string messageDisconnecting;

        [Space]
        [SerializeField]
        private string messageTradeAccepted;

        [Space]
        [SerializeField]
        private string messageTradeDeclined;

        [Space]
        [SerializeField]
        private string messageHostDisconnected;


        [Space]
        [SerializeField]
        private string messageConfirmSurrender;

        [Space]
        [SerializeField]
        private string messageInsufficientFunds;

        [Space]
        [SerializeField]
        private string messageLimitedTradesCount;

        [Space]
        [SerializeField]
        private string messageWaitingOtherPlayers;

        [Space]
        [SerializeField]
        private string messagePlayersFailedToLoad;

        [Space]
        [SerializeField]
        private string messageCannotUpgradeMaxLevel;

        [Space]
        [SerializeField]
        private string messageWrongTradeCredentials;

        [Space]
        [SerializeField]
        private string messageCannotDowngradeMinLevel;

        [Space]
        [SerializeField]
        private string messageOnlyEvenBuildingAllowed;

        [Space]
        [SerializeField]
        private string messageCannotUpgradeNotProperty;

        [Space]
        [SerializeField]
        private string messageCompleteMonopolyRequired;

        internal string MessageWon => this.messageWon;
        internal string MessageBankrupt => this.messageBankrupt;
        internal string MessageSentJail => this.messageSentJail;
        internal string MessageAlreadyBuilt => this.messageAlreadyBuilt;
        internal string MessageTradeAccepted => this.messageTradeAccepted;
        internal string MessageTradeDeclined => this.messageTradeDeclined;
        internal string MessageHostDisconnected => this.messageHostDisconnected;
        internal string MessageConfirmSurrender => this.messageConfirmSurrender;
        internal string MessageInsufficientFunds => this.messageInsufficientFunds;
        internal string MessageLimitedTradesCount => this.messageLimitedTradesCount;
        internal string MessageWaitingOtherPlayers => this.messageWaitingOtherPlayers;
        internal string MessagePlayersFailedToLoad => this.messagePlayersFailedToLoad;
        internal string MessageWrongTradeCredentials => this.messageWrongTradeCredentials;
        internal string MessageCannotUpgradeMaxLevel => this.messageCannotUpgradeMaxLevel;
        internal string MessageCannotDowngradeMinLevel => this.messageCannotDowngradeMinLevel;
        internal string MessageOnlyEvenBuildingAllowed => this.messageOnlyEvenBuildingAllowed;
        internal string MessageCannotUpgradeNotProperty => this.messageCannotUpgradeNotProperty;
        internal string MessageCompleteMonopolyRequired => this.messageCompleteMonopolyRequired;

        internal static UIManagerGame Instance { get; private set; }

        internal Action ButtonRollDiceClicked;

        internal char Currency => this.currency;
        internal Canvas CanvasPlayersList => this.canvasPlayersList;
        internal TradeSenderPanel PanelTradeSender => TradeSenderPanel.Instance;
        internal TilePaymentPanel PanelTilePayment => TilePaymentPanel.Instance;
        internal TileProposalPanel PanelTileProposal => TileProposalPanel.Instance;
        internal TradeReceiverPanel PanelTradeReceiver => TradeReceiverPanel.Instance;
        internal TileManagementPanel PanelTileManagement => TileManagementPanel.Instance;
        internal TileInformationPanel PanelTileInformation => TileInformationPanel.Instance;
        internal ChanceCardPaymentPanel PanelChanceCardPayment => ChanceCardPaymentPanel.Instance;

        private void Awake()
        {
            if (UIManagerGame.Instance != null)
                throw new InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            UIManagerGame.Instance = this;
        }

        private void Start()
        {
            GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
        }

        private void OnEnable()
        {
            this.buttonRollDice.onClick.AddListener(this.OnButtonRollDiceClicked);
            this.buttonDisconnect.onClick.AddListener(this.OnButtonDisconnectClickedAsync);
        }

        private void OnDisable()
        {
            this.buttonRollDice.onClick.RemoveListener(this.OnButtonRollDiceClicked);
            this.buttonDisconnect.onClick.RemoveListener(this.OnButtonDisconnectClickedAsync);
        }

        internal void ShowPanelTileInformation(string descriptionText, Action callback = default)
        {
            this.PanelTileInformation.DescriptionText = descriptionText;
            this.PanelTileInformation.Show(callback);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void ShowPanelInfoServerRpc(string descriptionText, ServerRpcParams serverRpcParams)
        {
            this.ShowPanelInfoClientRpc(descriptionText, GameManager.Instance.TargetAllClientsExcludingCurrentPlayer);
        }

        [ClientRpc]
        private void ShowPanelInfoClientRpc(string descriptionText, ClientRpcParams clientRpcParams)
        {
            this.ShowPanelTileInformation(descriptionText);
        }

        internal void ShowPanelTileProposal(MonopolyTile tile, Action callback)
        {
            this.PanelTileProposal.PictureSprite = tile.TileSprite;
            this.PanelTileProposal.MonopolyTypeColor = tile.AffiliatedMonopoly.ColorOfSet;
            this.PanelTileProposal.PriceText = $"{this.Currency} {tile.PricePurchase}";
            this.PanelTileProposal.Show(callback);
        }

        internal void HidePanelNodeOffer()
        {
            this.PanelTileProposal.Hide();
        }

        internal void ShowPanelTradeSender(PawnController sender, PawnController receiver, Action callback)
        {
            this.PanelTradeSender.Sender = sender;
            this.PanelTradeSender.Receiver = receiver;
            this.PanelTradeSender.Show(callback);
        }

        internal void HidePanelTradeSender()
        {
            this.PanelTradeSender.Hide();
        }

        internal void ShowPanelTradeReceiver(TradeCredentials credentials, Action callback)
        {
            this.PanelTradeReceiver.Credentials = credentials;
            this.PanelTradeReceiver.Show(callback);
        }

        internal void HidePanelTradeReceiver()
        {
            this.PanelTradeReceiver.Hide();
        }

        internal void ShowPanelTileManagement(MonopolyTile tile, Action callback)
        {
            this.PanelTileManagement.PictureSprite = tile.TileSprite;
            this.PanelTileManagement.MonopolyColor = tile.AffiliatedMonopoly.ColorOfSet;

            if (PlayerPawnController.LocalInstance.SelectedTile.TileType == MonopolyTile.Type.Property)
            {
                if (PlayerPawnController.LocalInstance.SelectedTile.Level == 0)
                    this.PanelTileManagement.PriceText = $"- {this.Currency} {tile.PricePurchase}";
                else if (PlayerPawnController.LocalInstance.SelectedTile.Level == 1)
                    this.PanelTileManagement.PriceText = $"- {this.Currency} {tile.PriceUpgrade}\n+ {this.Currency} {tile.PricePurchase}";
                else
                    this.PanelTileManagement.PriceText = $"+- {this.Currency} {tile.PriceUpgrade}";
            }
            else
            {
                if (PlayerPawnController.LocalInstance.SelectedTile.Level == 0)
                    this.PanelTileManagement.PriceText = $"- {this.Currency} {tile.PricePurchase}";
                else
                    this.PanelTileManagement.PriceText = $"+ {this.Currency} {tile.PricePurchase}";
            }

            this.PanelTileManagement.Show(callback);
        }

        internal void HidePanelTileManagement()
        {
            this.PanelTileManagement.Hide();
        }

        internal void ShowPanelChancePayment(string descriptionText, Action callback)
        {
            this.PanelChanceCardPayment.DescriptionText = descriptionText;
            this.PanelChanceCardPayment.Show(callback);
        }

        internal void HidePanelChancePayment()
        {
            this.PanelChanceCardPayment.Hide();
        }

        internal void ShowPanelTilePayment(MonopolyTile tile, Action callback)
        {
            this.PanelTilePayment.PictureSprite = tile.TileSprite;
            this.PanelTilePayment.MonopolyColor = tile.AffiliatedMonopoly.ColorOfSet;
            this.PanelTilePayment.PriceText = $"- {this.Currency} {tile.PriceRent}";

            this.PanelTilePayment.Show(callback);
        }

        internal void HidePanelTilePayment()
        {
            this.PanelTilePayment.Hide();
        }

        internal void ShowButtonRollDice()
        {
            this.buttonRollDice.gameObject.SetActive(true);
        }

        internal void HideButtonRollDice()
        {
            this.buttonRollDice.gameObject.SetActive(false);
        }

        private void OnButtonRollDiceClicked()
        {
            this.buttonRollDice.gameObject.SetActive(false);
            this.ButtonRollDiceClicked?.Invoke();
        }

        internal void ShowButtonDisconnect()
        {
            this.buttonDisconnect.gameObject.SetActive(true);
        }

        private async void OnButtonDisconnectClickedAsync()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.None,
                MessageBoxPanel.Icon.Loading,
                this.messageDisconnecting
            );

            await LobbyManager.Instance.DisconnectFromLobbyAsync();
        }

        internal void ShowDiceAnimation()
        {
            this.ShowDiceAnimationAsync();
            this.ShowDiceAnimationServerRpc(GameManager.Instance.SenderLocalClient);
        }

        private async void ShowDiceAnimationAsync()
        {
            this.imageDiePlaceholder1.gameObject.SetActive(true);
            this.imageDiePlaceholder2.gameObject.SetActive(true);

            this.imageDiePlaceholder1.sprite = this.spriteDieFaces[GameManager.Instance.FirstDieValue - 1];
            this.imageDiePlaceholder2.sprite = this.spriteDieFaces[GameManager.Instance.SecondDieValue - 1];

            await Awaitable.WaitForSecondsAsync(this.diceScreenTime);

            this.imageDiePlaceholder1.gameObject.SetActive(false);
            this.imageDiePlaceholder2.gameObject.SetActive(false);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ShowDiceAnimationServerRpc(ServerRpcParams serverRpcParams)
        {
            this.ShowDiceAnimationClientRpc(GameManager.Instance.TargetAllClientsExcludingCurrentPlayer);
        }

        [ClientRpc]
        private void ShowDiceAnimationClientRpc(ClientRpcParams clientRpcParams)
        {
            this.ShowDiceAnimationAsync();
        }
    }
}
