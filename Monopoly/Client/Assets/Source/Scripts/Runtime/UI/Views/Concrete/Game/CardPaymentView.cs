using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.UI.Views.Common;

namespace Monopoly.Client.Runtime.UI.Views.Concrete.Game
{
    internal sealed class CardPaymentView : MonoBehaviour, IViewCallableAction
    {
        [SerializeField]
        private RectTransform panel;

        [SerializeField]
        private Button buttonConfirm;

        [SerializeField]
        private TMP_Text textDescription;

        internal enum DialogResult : byte
        {
            Confirmed
        }

        private Action callback;

        internal static CardPaymentView Instance { get; private set; }

        internal string DescriptionText
        {
            set => this.textDescription.text = value;
        }

        internal DialogResult PanelDialogResult { get; private set; }

        private void Awake()
        {
            if (CardPaymentView.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            CardPaymentView.Instance = this;
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
            this.PanelDialogResult = CardPaymentView.DialogResult.Confirmed;
            this.callback?.Invoke();
        }
    }
}
