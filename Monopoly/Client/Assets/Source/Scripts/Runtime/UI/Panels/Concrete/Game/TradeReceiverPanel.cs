using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.Game.Core;
using Monopoly.Client.Runtime.Game.Serializables;
using Monopoly.Client.Runtime.UI.Panels.Common;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Game
{
    internal sealed class TradeReceiverPanel : MonoBehaviour, ICallableAction
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
        private TMP_Text labelSenderOffer;

        [Space]
        [SerializeField]
        private TMP_Text labelReceiverOffer;

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
        private Button buttonAccept;

        [Space]
        [SerializeField]
        private Button buttonDecline;

        internal enum DialogResult : byte
        {
            Accept,
            Decline
        }

        private Action callback;

        internal static TradeReceiverPanel Instance { get; private set; }

        internal TradeCredentials Credentials
        {
            set
            {
                this.labelSenderNickname.text = GameManager.Instance.GetPawnController(value.SenderNetworkIndex).Nickname;
                this.labelReceiverNickname.text = GameManager.Instance.GetPawnController(value.ReceiverNetworkIndex).Nickname;
                this.labelSenderOffer.text = value.SenderBalanceAmount.ToString();
                this.labelReceiverOffer.text = value.ReceiverBalanceAmount.ToString();

                if (value.SenderNodeIndex != TradeCredentials.PLACEHOLDER)
                {
                    this.imageSender.gameObject.SetActive(true);
                    this.imageSender.sprite = MonopolyBoard.Instance.GetNodeByIndex(value.SenderNodeIndex).TileSprite;
                }

                if (value.ReceiverNodeIndex != TradeCredentials.PLACEHOLDER)
                {
                    this.imageReceiver.gameObject.SetActive(true);
                    this.imageReceiver.sprite = MonopolyBoard.Instance.GetNodeByIndex(value.ReceiverNodeIndex).TileSprite; ;
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
