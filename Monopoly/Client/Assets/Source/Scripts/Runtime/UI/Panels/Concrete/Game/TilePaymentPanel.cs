using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.UI.Panels.Common;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Game
{
    internal sealed class TilePaymentPanel : MonoBehaviour, ICallableAction
    {
        [Header("Visuals")]

        [Space]
        [SerializeField]
        private RectTransform panel;

        [Space]
        [SerializeField]
        private TMP_Text textPrice;

        [Space]
        [SerializeField]
        private Image imagePicture;

        [Space]
        [SerializeField]
        private Image imageMonopoly;

        [Space]
        [Header("Controls")]

        [Space]
        [SerializeField] private Button buttonConfirm;

        internal enum DialogResult : byte
        {
            Confirmed
        }

        private Action callback;

        internal static TilePaymentPanel Instance { get; private set; }

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
            if (TilePaymentPanel.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            TilePaymentPanel.Instance = this;
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
            this.PanelDialogResult = TilePaymentPanel.DialogResult.Confirmed;
            this.callback?.Invoke();
        }
    }
}
