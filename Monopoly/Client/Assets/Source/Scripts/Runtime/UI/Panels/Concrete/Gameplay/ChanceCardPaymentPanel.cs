using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.UI.Panels.Common;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Gameplay
{
    internal sealed class ChanceCardPaymentPanel : MonoBehaviour, ICallableAction
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

        internal static ChanceCardPaymentPanel Instance { get; private set; }

        internal string DescriptionText
        {
            set => this.textDescription.text = value;
        }

        internal DialogResult PanelDialogResult { get; private set; }

        private void Awake()
        {
            if (ChanceCardPaymentPanel.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            ChanceCardPaymentPanel.Instance = this;
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
            this.PanelDialogResult = ChanceCardPaymentPanel.DialogResult.Confirmed;
            this.callback?.Invoke();
        }
    }
}
