using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.UI.Views.Common;

namespace Monopoly.Client.Runtime.UI.Views.Concrete.Game
{
    internal sealed class TilePaymentView : MonoBehaviour, IViewCallableAction
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
        private Button buttonConfirm;

        internal enum DialogResult : byte
        {
            Confirmed
        }

        private Action callback;

        internal static TilePaymentView Instance { get; private set; }

        internal string PriceText
        {
            set => this.textPrice.text = value;
        }

        internal Color MonopolyColor
        {
            set => this.imageMonopoly.color = value;
        }

        internal Sprite PictureSprite
        {
            set => this.imagePicture.sprite = value;
        }

        internal DialogResult PanelDialogResult { get; private set; }

        private void Awake()
        {
            if (TilePaymentView.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            TilePaymentView.Instance = this;
        }

        private void OnEnable()
        {
            this.buttonConfirm.onClick.AddListener(this.OnButtonConfirmClicked);
        }

        private void OnDisable()
        {
            this.buttonConfirm.onClick.RemoveListener(this.OnButtonConfirmClicked);
        }

        public void Show(Action actionCallback = null)
        {
            this.callback = actionCallback;
            this.panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.callback = null;
            this.panel.gameObject.SetActive(false);
        }

        private void OnButtonConfirmClicked()
        {
            this.PanelDialogResult = TilePaymentView.DialogResult.Confirmed;
            this.callback?.Invoke();
        }
    }
}
