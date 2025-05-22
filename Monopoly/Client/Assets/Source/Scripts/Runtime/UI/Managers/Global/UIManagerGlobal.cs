using System;
using System.Collections.Generic;
using UnityEngine;
using Monopoly.Client.Runtime.UI.Pools;
using Monopoly.Client.Runtime.UI.Managers.Bootstrap;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;

namespace Monopoly.Client.Runtime.UI.Managers.Global
{
    internal sealed class UIManagerGlobal : MonoBehaviour
    {
        private Stack<MessageBoxView> activeMessageBoxes;

        internal static UIManagerGlobal Instance { get; private set; }

        internal MessageBoxView TopMessageBox
        {
            get
            {
                if (this.activeMessageBoxes.TryPop(out MessageBoxView lastMessageBox))
                    return lastMessageBox;
                else
                    throw new InvalidOperationException($"No active instances of {nameof(MessageBoxView)}.");
            }
        }

        private void Awake()
        {
            if (UIManagerGlobal.Instance != null)
                throw new TypeInitializationException(nameof(UIManagerBootstrap), new ApplicationException($"Singleton has already been initialized."));

            UIManagerGlobal.Instance = this;
            GameObject.DontDestroyOnLoad(this);
        }

        private void Start()
        {
            this.activeMessageBoxes = new Stack<MessageBoxView>();
        }

        private void OnDestroy()
        {
            this.activeMessageBoxes.Clear();
        }

        public void ShowMessageBox(MessageBoxView.Type type, MessageBoxView.Icon icon, string text)
        {
            if (this.activeMessageBoxes.TryPeek(out MessageBoxView lastMessageBox))
            {
                if (lastMessageBox.MessageBoxType == MessageBoxView.Type.None)
                {
                    lastMessageBox.Hide();
                    this.activeMessageBoxes.Pop();
                }
            }

            MessageBoxView messageBox = MessageBoxViewsPool.Instance?.GetInactiveObject();

            messageBox.MessageBoxType = type;
            messageBox.MessageBoxIcon = icon;
            messageBox.MessageBoxText = text;

            this.activeMessageBoxes.Push(messageBox);
            messageBox.Show();
        }

        public void ShowMessageBox(MessageBoxView.Type type, MessageBoxView.Icon icon, string text, Action actionHandler = default)
        {
            if (this.activeMessageBoxes.TryPeek(out MessageBoxView lastMessageBox))
            {
                if (lastMessageBox.MessageBoxType == MessageBoxView.Type.None)
                {
                    lastMessageBox.Hide();
                    this.activeMessageBoxes.Pop();
                }
            }

            MessageBoxView messageBox = MessageBoxViewsPool.Instance?.GetInactiveObject();

            messageBox.MessageBoxType = type;
            messageBox.MessageBoxIcon = icon;
            messageBox.MessageBoxText = text;

            this.activeMessageBoxes.Push(messageBox);
            messageBox.Show(actionHandler != null ? actionHandler : null);
        }

        public void ShowMessageBox(MessageBoxView.Type type, MessageBoxView.Icon icon, string text, Func<bool> stateHandler = default)
        {
            if (this.activeMessageBoxes.TryPeek(out MessageBoxView lastMessageBox))
            {
                if (lastMessageBox.MessageBoxType == MessageBoxView.Type.None)
                {
                    lastMessageBox.Hide();
                    this.activeMessageBoxes.Pop();
                }
            }

            MessageBoxView messageBox = MessageBoxViewsPool.Instance?.GetInactiveObject();

            messageBox.MessageBoxType = type;
            messageBox.MessageBoxIcon = icon;
            messageBox.MessageBoxText = text;

            this.activeMessageBoxes.Push(messageBox);
            messageBox.Show(stateHandler != null ? stateHandler : null);
        }
    }
}
