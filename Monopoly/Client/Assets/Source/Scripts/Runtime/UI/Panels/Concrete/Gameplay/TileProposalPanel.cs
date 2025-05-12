using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.UI.Panels.Common;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Gameplay
{
    internal sealed class TileProposalPanel : MonoBehaviour, ICallableAction
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
        private Image imageMonopolyType;

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
            Accepted,
            Declined
        }

        private Action callback;

        internal static TileProposalPanel Instance { get; private set; }

        internal string PriceText
        {
            set => this.textPrice.text = value;
        }

        internal Sprite PictureSprite
        {
            set => this.imagePicture.sprite = value;
        }

        internal Color MonopolyTypeColor
        {
            set => this.imageMonopolyType.color = value;
        }

        internal DialogResult PanelDialogResult { get; private set; }

        private void Awake()
        {
            if (TileProposalPanel.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            TileProposalPanel.Instance = this;
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
            this.PanelDialogResult = TileProposalPanel.DialogResult.Accepted;
            this.callback?.Invoke();
        }

        private void OnButtonDeclineClicked()
        {
            this.PanelDialogResult = TileProposalPanel.DialogResult.Declined;
            this.callback?.Invoke();
        }
    }
}
