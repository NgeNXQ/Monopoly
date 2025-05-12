using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Gameplay.Layout;
using Monopoly.Client.Runtime.Game.Serializables;
using Monopoly.Client.Runtime.UI.Panels.Common;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Gameplay
{
    internal sealed class TradeReceiverPanel : MonoBehaviour, ICallableAction
    {
        [SerializeField]
        private RectTransform panel;

        [SerializeField]
        private Image imageSender;

        [SerializeField]
        private Image imageReceiver;

        [SerializeField]
        private TMP_Text labelSenderOffer;

        [SerializeField]
        private TMP_Text labelReceiverOffer;

        [SerializeField]
        private TMP_Text labelSenderNickname;

        [SerializeField]
        private TMP_Text labelReceiverNickname;

        [SerializeField]
        private Button buttonAccept;

        [SerializeField]
        private Button buttonDecline;

        internal enum DialogResult : byte
        {
            Accept,
            Decline
        }

        private Action callback;

        internal static TradeReceiverPanel Instance { get; private set; }

        internal TradeCredentialsSerializable Credentials
        {
            set
            {
                this.labelSenderNickname.text = GameManager.Instance.GetPawnController(value.SenderNetworkIndex).Nickname;
                this.labelReceiverNickname.text = GameManager.Instance.GetPawnController(value.ReceiverNetworkIndex).Nickname;
                this.labelSenderOffer.text = value.SenderBalanceAmount.ToString();
                this.labelReceiverOffer.text = value.ReceiverBalanceAmount.ToString();

                if (value.SenderNodeIndex != TradeCredentialsSerializable.PLACEHOLDER)
                {
                    this.imageSender.gameObject.SetActive(true);
                    // this.imageSender.sprite = MonopolyBoard.Instance.GetTileByIndex(value.SenderNodeIndex).TileSprite;
                }

                if (value.ReceiverNodeIndex != TradeCredentialsSerializable.PLACEHOLDER)
                {
                    this.imageReceiver.gameObject.SetActive(true);
                    // this.imageReceiver.sprite = MonopolyBoard.Instance.GetTileByIndex(value.ReceiverNodeIndex).TileSprite; ;
                }
            }
        }

        internal DialogResult PanelDialogResult { get; private set; }

        private void Awake()
        {
            if (TradeReceiverPanel.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            TradeReceiverPanel.Instance = this;
        }

        private void OnEnable()
        {
            this.buttonAccept.onClick.AddListener(this.OnButtonAcceptClicked);
            this.buttonDecline.onClick.AddListener(this.OnButtonDeclineClicked);
        }

        private void OnDisable()
        {
            this.buttonAccept.onClick.RemoveListener(this.OnButtonAcceptClicked);
            this.buttonDecline.onClick.RemoveListener(this.OnButtonDeclineClicked);
        }

        public void Show(Action actionCallback)
        {
            this.callback = actionCallback;
            this.panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.labelSenderNickname.text = String.Empty;
            this.labelReceiverNickname.text = String.Empty;

            this.labelSenderOffer.text = String.Empty;
            this.labelReceiverOffer.text = String.Empty;

            this.imageSender.sprite = null;
            this.imageSender.gameObject.SetActive(false);
            this.imageReceiver.sprite = null;
            this.imageReceiver.gameObject.SetActive(false);

            this.callback = null;
            this.panel.gameObject.SetActive(false);
        }

        private void OnButtonAcceptClicked()
        {
            this.PanelDialogResult = TradeReceiverPanel.DialogResult.Accept;
            this.callback?.Invoke();
        }

        private void OnButtonDeclineClicked()
        {
            this.PanelDialogResult = TradeReceiverPanel.DialogResult.Decline;
            this.callback?.Invoke();
        }
    }
}
