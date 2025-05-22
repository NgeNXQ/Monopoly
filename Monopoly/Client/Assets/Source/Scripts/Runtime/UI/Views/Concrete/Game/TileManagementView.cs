using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.UI.Views.Common;

namespace Monopoly.Client.Runtime.UI.Views.Concrete.Game
{
    internal sealed class TileManagementView : MonoBehaviour, IViewCallableAction
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
        private Button buttonUpgrade;

        [SerializeField]
        private Button buttonDowngrade;

        internal enum DialogResult : byte
        {
            Upgrade,
            Downgrade
        }

        internal static TileManagementView Instance { get; private set; }

        private Action callback;

        internal DialogResult PanelDialogResult { get; private set; }

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

        private void Awake()
        {
            if (TileManagementView.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            TileManagementView.Instance = this;
        }

        private void OnEnable()
        {
            this.buttonUpgrade.onClick.AddListener(this.OnButtonUpgradeClicked);
            this.buttonDowngrade.onClick.AddListener(this.OnButtonDowngradeClicked);
        }

        private void OnDisable()
        {
            this.buttonUpgrade.onClick.RemoveListener(this.OnButtonUpgradeClicked);
            this.buttonDowngrade.onClick.RemoveListener(this.OnButtonDowngradeClicked);
        }

        public void Show(Action actionCallback = null)
        {
            this.panel.gameObject.SetActive(true);
            this.callback = actionCallback;
        }

        public void Hide()
        {
            this.panel.gameObject.SetActive(false);
        }

        private void OnButtonUpgradeClicked()
        {
            this.PanelDialogResult = TileManagementView.DialogResult.Upgrade;
            this.callback?.Invoke();
        }

        private void OnButtonDowngradeClicked()
        {
            this.PanelDialogResult = TileManagementView.DialogResult.Downgrade;
            this.callback?.Invoke();
        }
    }
}
