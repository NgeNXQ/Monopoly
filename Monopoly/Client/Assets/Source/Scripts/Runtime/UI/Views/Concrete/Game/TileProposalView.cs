using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.UI.Views.Common;

namespace Monopoly.Client.Runtime.UI.Views.Concrete.Game
{
    internal sealed class TileProposalView : MonoBehaviour, IViewCallableAction
    {
        [SerializeField]
        private RectTransform panel;

        [SerializeField]
        private TMP_Text textPrice;

        [SerializeField]
        private Image imagePicture;

        [SerializeField]
        private Image imageMonopoly;

        [SerializeField]
        private Button buttonAccept;

        [SerializeField]
        private Button buttonDecline;

        internal enum UserDialogResult : byte
        {
            Accepted,
            Declined
        }

        private Action callback;

        internal string PriceText
        {
            set => this.textPrice.text = value;
        }

        internal Sprite PictureSprite
        {
            set => this.imagePicture.sprite = value;
        }

        internal Color MonopolyColor
        {
            set => this.imageMonopoly.color = value;
        }

        internal UserDialogResult DialogResult { get; private set; }

        private void Start()
        {
            this.buttonAccept.onClick.AddListener(this.OnButtonAcceptClicked);
            this.buttonDecline.onClick.AddListener(this.OnButtonDeclineClicked);
        }

        private void OnDestroy()
        {
            this.buttonAccept.onClick.RemoveListener(this.OnButtonAcceptClicked);
            this.buttonDecline.onClick.RemoveListener(this.OnButtonDeclineClicked);
        }

        public void Show(Action actionCallback = default)
        {
            this.callback = actionCallback;
            this.panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.callback = null;
            this.panel.gameObject.SetActive(false);
        }

        private void OnButtonAcceptClicked()
        {
            this.DialogResult = TileProposalView.UserDialogResult.Accepted;
            this.callback?.Invoke();
        }

        private void OnButtonDeclineClicked()
        {
            this.DialogResult = TileProposalView.UserDialogResult.Declined;
            this.callback?.Invoke();
        }
    }
}
