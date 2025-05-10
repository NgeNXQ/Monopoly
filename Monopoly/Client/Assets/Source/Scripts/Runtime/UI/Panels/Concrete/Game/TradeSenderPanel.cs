using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Monopoly.Client.Runtime.Game.Core;
using Monopoly.Client.Runtime.Game.Board;
using Monopoly.Client.Runtime.Game.Serializables;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.UI.Panels.Common;
using Monopoly.Client.Runtime.Game.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Game
{
    internal sealed class TradeSenderPanel : MonoBehaviour, ICallableAction, IPointerClickHandler
    {
        [Header("Visuals")]

        [Space]
        [SerializeField]
        private RectTransform panel;

        [Space]
        [SerializeField]
        private Image imageSender;

        [Space]
        [SerializeField]
        private Image imageReceiver;

        [Space]
        [SerializeField]
        private TMP_Text labelSenderNickname;

        [Space]
        [SerializeField]
        private TMP_Text labelReceiverNickname;

        [Space]
        [Header("Controls")]

        [Space]
        [SerializeField]
        private Button buttonSendOffer;

        [Space]
        [SerializeField]
        private Button buttonCancelOffer;

        [Space]
        [SerializeField]
        private TMP_InputField textBoxSenderBalanceAmount;

        [Space]
        [SerializeField]
        private TMP_InputField textBoxReceiverBalanceAmount;

        internal enum DialogResult : byte
        {
            Offer,
            Cancel
        }

        private Action callback;
        private PawnController sender;
        private PawnController receiver;
        private PropertyMonopolyTile senderTile;
        private PropertyMonopolyTile receiverTile;

        internal static TradeSenderPanel Instance { get; private set; }

        internal DialogResult PanelDialogResult { get; private set; }

        internal int senderBalanceAmount => this.textBoxSenderBalanceAmount.text.Length == 0 ? 0 : Int32.Parse(this.textBoxSenderBalanceAmount.text);
        internal int receiverBalanceAmount => this.textBoxReceiverBalanceAmount.text.Length == 0 ? 0 : Int32.Parse(this.textBoxReceiverBalanceAmount.text);

        internal PropertyMonopolyTile SenderTile
        {
            get => this.senderTile;
            set
            {
                this.senderTile = value;
                this.imageSender.gameObject.SetActive(value != null);
                // this.imageSender.sprite = this.senderTile?.TileSprite;
            }
        }

        internal PropertyMonopolyTile ReceiverTile
        {
            get => this.receiverTile;
            set
            {
                this.receiverTile = value;
                this.imageReceiver.gameObject.SetActive(value != null);
                // this.imageReceiver.sprite = this.receiverTile?.TileSprite;
            }
        }

        internal PawnController Sender
        {
            get => this.sender;
            set
            {
                this.sender = value;
                this.labelSenderNickname.text = this.sender.Nickname;
            }
        }

        internal PawnController Receiver
        {
            get => this.receiver;
            set
            {
                this.receiver = value;
                this.labelReceiverNickname.text = this.receiver.Nickname;
            }
        }

        internal TradeCredentials Credentials
        {
            get
            {
                int senderNetworkIndex = this.Sender.NetworkIndex;
                int receiverNetworkIndex = this.Receiver.NetworkIndex;
                int clampedSenderBalanceAmount = this.senderBalanceAmount;
                int clampedReceiverBalanceAmount = this.receiverBalanceAmount;
                int senderNodeIndex = this.SenderTile == null ? TradeCredentials.PLACEHOLDER : MonopolyBoard.Instance.GetIndexOfTile(this.SenderTile);
                int receiverNodeIndex = this.ReceiverTile == null ? TradeCredentials.PLACEHOLDER : MonopolyBoard.Instance.GetIndexOfTile(this.ReceiverTile);

                if (this.senderBalanceAmount > GameManager.Instance.GetPawnController(senderNetworkIndex).Balance.Value)
                    clampedSenderBalanceAmount = GameManager.Instance.GetPawnController(senderNetworkIndex).Balance.Value;
                else if (this.senderBalanceAmount < 0)
                    clampedSenderBalanceAmount = 0;

                if (this.receiverBalanceAmount > GameManager.Instance.GetPawnController(receiverNetworkIndex).Balance.Value)
                    clampedReceiverBalanceAmount = GameManager.Instance.GetPawnController(receiverNetworkIndex).Balance.Value;
                else if (this.receiverBalanceAmount < 0)
                    clampedReceiverBalanceAmount = 0;

                TradeCredentials tradeCredentials = new TradeCredentials()
                {
                    SenderNetworkIndex = senderNetworkIndex,
                    ReceiverNetworkIndex = receiverNetworkIndex,
                    SenderNodeIndex = senderNodeIndex,
                    ReceiverNodeIndex = receiverNodeIndex,
                    SenderBalanceAmount = clampedSenderBalanceAmount,
                    ReceiverBalanceAmount = clampedReceiverBalanceAmount,
                };

                return tradeCredentials;
            }
        }

        private void Awake()
        {
            if (TradeSenderPanel.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            TradeSenderPanel.Instance = this;
        }

        private void OnEnable()
        {
            this.buttonSendOffer.onClick.AddListener(this.OnButtonSendOfferClicked);
            this.buttonCancelOffer.onClick.AddListener(this.OnButtonCancelOfferClicked);
        }

        private void OnDisable()
        {
            this.buttonSendOffer.onClick.RemoveListener(this.OnButtonSendOfferClicked);
            this.buttonCancelOffer.onClick.RemoveListener(this.OnButtonCancelOfferClicked);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == this.imageSender.gameObject)
                this.SenderTile = null;
            else if (eventData.pointerCurrentRaycast.gameObject == this.imageReceiver.gameObject)
                this.ReceiverTile = null;
        }

        public void Show(Action actionCallback)
        {
            this.callback = actionCallback;

            this.imageSender.gameObject.SetActive(false);
            this.imageReceiver.gameObject.SetActive(false);
            this.textBoxSenderBalanceAmount.text = String.Empty;
            this.textBoxReceiverBalanceAmount.text = String.Empty;

            this.panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.imageSender.sprite = null;
            this.imageReceiver.sprite = null;
            this.labelSenderNickname.text = String.Empty;
            this.labelReceiverNickname.text = String.Empty;
            this.textBoxSenderBalanceAmount.text = String.Empty;
            this.textBoxReceiverBalanceAmount.text = String.Empty;

            this.callback = null;
            this.panel.gameObject.SetActive(false);
        }

        private void OnButtonSendOfferClicked()
        {
            this.PanelDialogResult = TradeSenderPanel.DialogResult.Offer;
            this.callback?.Invoke();
        }

        private void OnButtonCancelOfferClicked()
        {
            this.PanelDialogResult = TradeSenderPanel.DialogResult.Cancel;
            this.callback?.Invoke();
            this.Hide();
        }
    }
}
