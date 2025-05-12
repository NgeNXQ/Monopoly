using System;
using UnityEngine;
using Monopoly.Client.Shared.Pools;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;

namespace Monopoly.Client.Runtime.UI.Pools
{
    internal sealed class MessageBoxPanelsPool : ObjectPool<MessageBoxPanel>
    {
        [SerializeField]
        private MessageBoxPanel panelMessageBox;

        [SerializeField, Range(1, 10)]
        private int messageBoxPoolSize;

        internal static MessageBoxPanelsPool Instance { get; private set; }

        private void Awake()
        {
            if (MessageBoxPanelsPool.Instance != null)
                throw new TypeInitializationException(nameof(MessageBoxPanelsPool), new ApplicationException($"Singleton has already been initialized."));

            MessageBoxPanelsPool.Instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < this.messageBoxPoolSize; ++i)
            {
                MessageBoxPanel newMessageBox = GameObject.Instantiate(this.panelMessageBox, base.gameObject.transform.parent.transform);
                newMessageBox.gameObject.SetActive(false);
                base.Append(newMessageBox);
            }
        }
    }
}
