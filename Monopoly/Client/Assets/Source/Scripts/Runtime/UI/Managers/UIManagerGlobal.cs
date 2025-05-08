using System;
using System.Collections.Generic;
using UnityEngine;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;
using Monopoly.Client.Runtime.UI.Utilities.Pools.Concrete;

namespace Monopoly.Client.Runtime.UI.Managers
{
    internal sealed class UIManagerGlobal : MonoBehaviour
    {
        private Stack<MessageBoxPanel> activeMessageBoxes;

        internal static UIManagerGlobal Instance { get; private set; }

        internal MessageBoxPanel TopMessageBox
        {
            get
            {
                if (this.activeMessageBoxes.TryPop(out MessageBoxPanel lastMessageBox))
                    return lastMessageBox;
                else
                    throw new InvalidOperationException($"No active instances of {nameof(MessageBoxPanel)}.");
            }
        }

        private void Awake()
        {
            if (UIManagerGlobal.Instance != null)
                throw new TypeInitializationException(nameof(GUIManagerBootstrap), new ApplicationException($"Singleton has already been initialized."));

            UIManagerGlobal.Instance = this;
            GameObject.DontDestroyOnLoad(this);
        }

        private void Start()
        {
            this.activeMessageBoxes = new Stack<MessageBoxPanel>();
        }

        private void OnDestroy()
        {
            this.activeMessageBoxes.Clear();
        }

        public void ShowMessageBox(MessageBoxPanel.Type type, MessageBoxPanel.Icon icon, string text)
        {
            if (this.activeMessageBoxes.TryPeek(out MessageBoxPanel lastMessageBox))
            {
                if (lastMessageBox.MessageBoxType == MessageBoxPanel.Type.None)
                {
                    lastMessageBox.Hide();
                    this.activeMessageBoxes.Pop();
                }
            }

            MessageBoxPanel messageBox = MessageBoxPanelsPool.Instance?.GetInactiveObject();

            messageBox.MessageBoxType = type;
            messageBox.MessageBoxIcon = icon;
            messageBox.MessageBoxText = text;

            this.activeMessageBoxes.Push(messageBox);
            messageBox.Show();
        }

        public void ShowMessageBox(MessageBoxPanel.Type type, MessageBoxPanel.Icon icon, string text, Action actionHandler = default)
        {
            if (this.activeMessageBoxes.TryPeek(out MessageBoxPanel lastMessageBox))
            {
                if (lastMessageBox.MessageBoxType == MessageBoxPanel.Type.None)
                {
                    lastMessageBox.Hide();
                    this.activeMessageBoxes.Pop();
                }
            }

            MessageBoxPanel messageBox = MessageBoxPanelsPool.Instance?.GetInactiveObject();

            messageBox.MessageBoxType = type;
            messageBox.MessageBoxIcon = icon;
            messageBox.MessageBoxText = text;

            this.activeMessageBoxes.Push(messageBox);
            messageBox.Show(actionHandler != null ? actionHandler : null);
        }

        public void ShowMessageBox(MessageBoxPanel.Type type, MessageBoxPanel.Icon icon, string text, Func<bool> stateHandler = default)
        {
            if (this.activeMessageBoxes.TryPeek(out MessageBoxPanel lastMessageBox))
            {
                if (lastMessageBox.MessageBoxType == MessageBoxPanel.Type.None)
                {
                    lastMessageBox.Hide();
                    this.activeMessageBoxes.Pop();
                }
            }

            MessageBoxPanel messageBox = MessageBoxPanelsPool.Instance?.GetInactiveObject();

            messageBox.MessageBoxType = type;
            messageBox.MessageBoxIcon = icon;
            messageBox.MessageBoxText = text;

            this.activeMessageBoxes.Push(messageBox);
            messageBox.Show(stateHandler != null ? stateHandler : null);
        }
    }
}
