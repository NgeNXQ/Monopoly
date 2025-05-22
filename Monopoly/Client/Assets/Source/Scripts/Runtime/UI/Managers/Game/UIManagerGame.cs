using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.Game.Models;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Gameplay.Boards;
using Monopoly.Client.Runtime.Game.Gameplay.Groups;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
// using Monopoly.Client.Runtime.UI.Panels.Concrete.Gameplay;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Concrete;

namespace Monopoly.Client.Runtime.UI.Managers.Game
{
    internal sealed class UIManagerGame : NetworkBehaviour
    {
        [SerializeField]
        private char currency;

        [SerializeField]
        private Image imageDiePlaceholderLeft;

        [SerializeField]
        private Image imageDiePlaceholderRight;

        [SerializeField]
        private Sprite[] spriteDieFaces = new Sprite[6];

        [SerializeField, Range(0.0f, 10.0f)]
        private float diceScreenTime = 1.0f;

        [SerializeField]
        private Button buttonRollDice;

        [SerializeField]
        private Button buttonDisconnect;

        [SerializeField]
        private TilePaymentView viewTilePayment;

        [SerializeField]
        private TileProposalView viewTileProposal;

        [SerializeField]
        private TileManagementView viewTileManagement;

        [SerializeField]
        private CardPaymentView viewCardPayment;

        [SerializeField]
        private CardInformationView viewCardInformation;

        [SerializeField]
        private TradeSenderView viewTradeSender;

        [SerializeField]
        private TradeReceiverView viewTradeReceiver;

        [SerializeField]
        private RectTransform panelPlayersContainer;

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
        internal TilePaymentView ViewTilePayment => this.viewTilePayment;
        internal TileProposalView ViewTileProposal => this.viewTileProposal;
        internal TileManagementView ViewTileManagement => this.viewTileManagement;
        internal CardPaymentView ViewCardPayment => this.viewCardPayment;
        internal CardInformationView ViewCardInformation => this.viewCardInformation;
        internal RectTransform PanelPlayersContainer => this.panelPlayersContainer;
        internal TradeSenderView ViewTradeSender => this.viewTradeSender;
        internal TradeReceiverView ViewTradeReceiver => this.viewTradeReceiver;

        // internal CardPaymentView ViewCardPayment => CardPaymentView.Instance;
        // internal TradeSenderView ViewTradeSender => TradeSenderView.Instance;
        // internal TradeReceiverView ViewTradeReceiver => TradeReceiverView.Instance;
        // internal TileManagementView ViewTileManagement => TileManagementView.Instance;
        // internal TileInformationView ViewTileInformation => TileInformationView.Instance;

        private void Awake()
        {
            if (UIManagerGame.Instance != null)
                throw new InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            UIManagerGame.Instance = this;
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

        internal void HideAllControls()
        {
            this.HideButtonRollDice();
            this.ViewTradeSender.Hide();
            this.ViewCardPayment.Hide();
            this.ViewTilePayment.Hide();
            this.ViewTileProposal.Hide();
            this.ViewTradeReceiver.Hide();
            this.ViewTileManagement.Hide();
            this.ViewCardInformation.Hide();
        }

        internal void ShowViewTilePayment(PropertyTile tile, Action callback)
        {
            this.ViewTilePayment.PictureSprite = tile.SpritePicture;
            this.ViewTilePayment.MonopolyColor = tile.Monopoly.ColorGroup;
            this.ViewTilePayment.PriceText = $"{this.Currency}{tile.PriceRenting}";

            this.ViewTilePayment.Show(callback);
        }

        internal void HideViewTilePayment()
        {
            this.ViewTilePayment.Hide();
        }

        internal void ShowViewTileProposal(PropertyTile tile, Action callback)
        {
            this.ViewTileProposal.PictureSprite = tile.SpritePicture;
            this.ViewTileProposal.MonopolyColor = tile.Monopoly.ColorGroup;
            this.ViewTileProposal.PriceText = $"{this.Currency}{tile.PricePurchase}";

            this.ViewTileProposal.Show(callback);
        }

        internal void HideViewTileProposal()
        {
            this.ViewTileProposal.Hide();
        }

        internal void ShowViewTileManagement(PropertyTile tile, Action callback)
        {
            this.ViewTileManagement.PictureSprite = tile.SpritePicture;
            this.ViewTileManagement.MonopolyColor = tile.Monopoly.ColorGroup;

            switch (tile.Level)
            {
                case PropertyTile.LEVEL_MORTGAGE:
                    this.ViewTileManagement.PriceText = $"+ {this.Currency} {tile.PricePurchase}";
                    break;
                case PropertyTile.LEVEL_DEFAULT:
                    this.ViewTileManagement.PriceText = $"- {this.Currency} {tile.PricePurchase}\n+ {this.Currency} {tile.PricePurchase}";
                    break;
                default:
                    this.ViewTileManagement.PriceText = $"+- {this.Currency} {tile.PricePurchase}";
                    break;
            }

            this.ViewTileManagement.Show(callback);
        }

        internal void HideViewTileManagement()
        {
            this.ViewTileManagement.Hide();
        }

        internal void ShowViewCardPayment(string description, Action callback)
        {
            this.ViewCardPayment.DescriptionText = description;

            this.ViewCardPayment.Show(callback);
        }

        internal void HideViewCardPayment()
        {
            this.ViewCardPayment.Hide();
        }

        internal void ShowViewCardInformation(string description, Action callback)
        {
            this.ViewCardInformation.DescriptionText = description;

            this.ViewCardInformation.Show(callback);
        }

        internal void HideViewCardInformation()
        {
            this.ViewCardInformation.Hide();
        }

        internal void ShowCardInformationToEveryoneNotMe(string description)
        {
            this.ShowCardInformationEveryoneNotMeRemotelyRpc(description);
        }

        [Rpc(SendTo.Server)]
        private void ShowCardInformationEveryoneNotMeRemotelyRpc(string description)
        {
            this.ShowCardInformationEveryoneNotMeClientRpc(description);
        }

        [Rpc(SendTo.Everyone)]
        private void ShowCardInformationEveryoneNotMeClientRpc(string description)
        {
            if (PlayerPawnController.LocalInstance == GameManager.Instance.CurrentPawn)
                return;

            this.ViewCardInformation.DescriptionText = description;

            this.ViewCardInformation.Show(() => this.ViewCardInformation.Hide());
        }

        internal void ShowViewTradeSender(PawnController sender, PawnController receiver, Action callback)
        {
            this.ViewTradeSender.Sender = sender;
            this.ViewTradeSender.Receiver = receiver;

            this.ViewTradeSender.Show(callback);
        }

        internal void HideViewTradeSender()
        {
            this.ViewTradeSender.Hide();
        }

        internal void ShowViewTradeReceiver(TradeCredentialsSerializable credentials, Action callback)
        {
            this.ViewTradeReceiver.Credentials = credentials;

            this.ViewTradeReceiver.Show(callback);
        }

        internal void HideViewTradeReceiver()
        {
            this.ViewTradeReceiver.Hide();
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
                MessageBoxView.Type.None,
                MessageBoxView.Icon.Loading,
                this.messageDisconnecting
            );

            await LobbyManager.Instance.DisconnectFromLobbyAsync();
        }

        internal void ShowDiceAnimation()
        {
            this.ShowDiceAnimationRemotelyRpc();
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void ShowDiceAnimationRemotelyRpc()
        {
            this.ShowDiceAnimationLocally();
        }

        private async void ShowDiceAnimationLocally()
        {
            if (this == null)
                return;

            this.imageDiePlaceholderLeft.gameObject.SetActive(true);
            this.imageDiePlaceholderRight.gameObject.SetActive(true);

            this.imageDiePlaceholderLeft.sprite = this.spriteDieFaces[GameManager.Instance.FirstDieValue - 1];
            this.imageDiePlaceholderRight.sprite = this.spriteDieFaces[GameManager.Instance.SecondDieValue - 1];

            await Awaitable.WaitForSecondsAsync(this.diceScreenTime);

            this.imageDiePlaceholderLeft.gameObject.SetActive(false);
            this.imageDiePlaceholderRight.gameObject.SetActive(false);
        }
    }
}
