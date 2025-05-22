using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Monopoly.Client.Runtime.UI.Views.Common;
using Monopoly.Client.Runtime.Game.Models;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Gameplay.Boards;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;

namespace Monopoly.Client.Runtime.UI.Views.Concrete.Game
{
    internal sealed class TradeSenderView : MonoBehaviour, IViewCallableAction, IPointerClickHandler
    {
        [SerializeField]
        private RectTransform panel;

        [SerializeField]
        private Image imageSender;

        [SerializeField]
        private Image imageReceiver;

        [SerializeField]
        private TMP_Text labelSenderNickname;

        [SerializeField]
        private TMP_Text labelReceiverNickname;

        [SerializeField]
        private Button buttonSendOffer;

        [SerializeField]
        private Button buttonCancelOffer;

        [SerializeField]
        private TMP_InputField textBoxSenderBalance;

        [SerializeField]
        private TMP_InputField textBoxReceiverBalance;

        internal enum DialogResult : byte
        {
            Offer,
            Cancel
        }

        private Action callback;
        private PawnController sender;
        private PawnController receiver;
        private PropertyTile senderTile;
        private PropertyTile receiverTile;

        internal static TradeSenderView Instance { get; private set; }

        private int senderBalanceAmount => this.textBoxSenderBalance.text.Length == 0 ? 0 : Int32.Parse(this.textBoxSenderBalance.text);
        private int receiverBalanceAmount => this.textBoxReceiverBalance.text.Length == 0 ? 0 : Int32.Parse(this.textBoxReceiverBalance.text);

        internal DialogResult ViewDialogResult { get; private set; }

        internal PropertyTile SenderTile
        {
            get => this.senderTile;
            set
            {
                this.senderTile = value;
                this.imageSender.gameObject.SetActive(value != null);
                this.imageSender.sprite = this.senderTile?.SpritePicture;
            }
        }

        internal PropertyTile ReceiverTile
        {
            get => this.receiverTile;
            set
            {
                this.receiverTile = value;
                this.imageReceiver.gameObject.SetActive(value != null);
                this.imageReceiver.sprite = this.receiverTile?.SpritePicture;
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

        internal TradeCredentialsSerializable Credentials
        {
            get
            {
                int senderNetworkIndex = this.Sender.PawnId;
                int receiverNetworkIndex = this.Receiver.PawnId;
                int clampedSenderBalanceAmount = this.senderBalanceAmount;
                int clampedReceiverBalanceAmount = this.receiverBalanceAmount;
                int senderNodeIndex = this.SenderTile == null ? TradeCredentialsSerializable.PLACEHOLDER : Board.Instance.GetIndexOfTile(this.SenderTile);
                int receiverNodeIndex = this.ReceiverTile == null ? TradeCredentialsSerializable.PLACEHOLDER : Board.Instance.GetIndexOfTile(this.ReceiverTile);

                if (this.senderBalanceAmount > GameManager.Instance.GetPawnController(senderNetworkIndex).Balance.Value)
                    clampedSenderBalanceAmount = GameManager.Instance.GetPawnController(senderNetworkIndex).Balance.Value;
                else if (this.senderBalanceAmount < 0)
                    clampedSenderBalanceAmount = 0;

                if (this.receiverBalanceAmount > GameManager.Instance.GetPawnController(receiverNetworkIndex).Balance.Value)
                    clampedReceiverBalanceAmount = GameManager.Instance.GetPawnController(receiverNetworkIndex).Balance.Value;
                else if (this.receiverBalanceAmount < 0)
                    clampedReceiverBalanceAmount = 0;

                TradeCredentialsSerializable tradeCredentials = new TradeCredentialsSerializable()
                {
                    SenderPawnId = senderNetworkIndex,
                    ReceiverPawnId = receiverNetworkIndex,
                    SenderTileIndex = senderNodeIndex,
                    ReceiverTileIndex = receiverNodeIndex,
                    SenderBalanceAmount = clampedSenderBalanceAmount,
                    ReceiverBalanceAmount = clampedReceiverBalanceAmount,
                };

                return tradeCredentials;
            }
        }

        private void Awake()
        {
            if (TradeSenderView.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            TradeSenderView.Instance = this;
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
            this.textBoxSenderBalance.text = String.Empty;
            this.textBoxReceiverBalance.text = String.Empty;

            this.panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.imageSender.sprite = null;
            this.imageReceiver.sprite = null;
            this.labelSenderNickname.text = String.Empty;
            this.labelReceiverNickname.text = String.Empty;

            this.callback = null;
            this.panel.gameObject.SetActive(false);
        }

        private void OnButtonSendOfferClicked()
        {
            this.ViewDialogResult = TradeSenderView.DialogResult.Offer;
            this.callback?.Invoke();
        }

        private void OnButtonCancelOfferClicked()
        {
            this.ViewDialogResult = TradeSenderView.DialogResult.Cancel;
            this.callback?.Invoke();
            this.Hide();
        }
    }
}
