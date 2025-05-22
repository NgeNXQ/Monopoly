using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.Game.Models;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.UI.Views.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Boards;

namespace Monopoly.Client.Runtime.UI.Views.Concrete.Game
{
    internal sealed class TradeReceiverView : MonoBehaviour, IViewCallableAction
    {
        [SerializeField]
        private RectTransform panel;

        [SerializeField]
        private Image imagePropertySender;

        [SerializeField]
        private Image imagePropertyReceiver;

        [SerializeField]
        private TMP_Text labelSenderBalance;

        [SerializeField]
        private TMP_Text labelReceiverBalance;

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

        internal static TradeReceiverView Instance { get; private set; }

        internal TradeCredentialsSerializable Credentials
        {
            set
            {
                this.labelSenderBalance.text = value.SenderBalanceAmount.ToString();
                this.labelReceiverBalance.text = value.ReceiverBalanceAmount.ToString();
                this.labelSenderNickname.text = GameManager.Instance.GetPawnController(value.SenderPawnId).Nickname;
                this.labelReceiverNickname.text = GameManager.Instance.GetPawnController(value.ReceiverPawnId).Nickname;

                if (value.SenderTileIndex != TradeCredentialsSerializable.PLACEHOLDER)
                {
                    this.imagePropertySender.gameObject.SetActive(true);
                    this.imagePropertySender.sprite = Board.Instance.GetTileByIndex(value.SenderTileIndex).SpritePicture;
                }

                if (value.ReceiverTileIndex != TradeCredentialsSerializable.PLACEHOLDER)
                {
                    this.imagePropertyReceiver.gameObject.SetActive(true);
                    this.imagePropertyReceiver.sprite = Board.Instance.GetTileByIndex(value.ReceiverTileIndex).SpritePicture;
                }
            }
        }

        internal DialogResult ViewDialogResult { get; private set; }

        private void Awake()
        {
            if (TradeReceiverView.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            TradeReceiverView.Instance = this;
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

            this.labelSenderBalance.text = String.Empty;
            this.labelReceiverBalance.text = String.Empty;

            this.imagePropertySender.sprite = null;
            this.imagePropertySender.gameObject.SetActive(false);
            this.imagePropertyReceiver.sprite = null;
            this.imagePropertyReceiver.gameObject.SetActive(false);

            this.callback = null;
            this.panel.gameObject.SetActive(false);
        }

        private void OnButtonAcceptClicked()
        {
            this.ViewDialogResult = TradeReceiverView.DialogResult.Accept;
            this.callback?.Invoke();
        }

        private void OnButtonDeclineClicked()
        {
            this.ViewDialogResult = TradeReceiverView.DialogResult.Decline;
            this.callback?.Invoke();
        }
    }
}
