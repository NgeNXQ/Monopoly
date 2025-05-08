using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.UI.Panels.Common;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Game
{
    internal sealed class TileInformationPanel : MonoBehaviour, ICallableAction
    {
        [Header("Visuals")]

        [Space]
        [SerializeField]
        private RectTransform panel;

        [Space]
        [SerializeField]
        private TMP_Text textDescription;

        [Space]
        [Header("Controls")]

        [Space]
        [SerializeField]
        private Button buttonConfirm;

        internal enum DialogResult : byte
        {
            Confirmed
        }

        private Action callback;

        internal static TileInformationPanel Instance { get; private set; }

        internal string DescriptionText
        {
            set => this.textDescription.text = value;
        }

        internal DialogResult PanelDialogResult { get; private set; }

        private void Awake()
        {
            if (TileInformationPanel.Instance != null)
                throw new InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            TileInformationPanel.Instance = this;
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
            this.PanelDialogResult = TileInformationPanel.DialogResult.Confirmed;
            this.callback?.Invoke();
            this.Hide();
        }
    }
}
