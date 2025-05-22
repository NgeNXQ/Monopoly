using System;
using UnityEngine;
using Monopoly.Client.Shared.Pools;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;

namespace Monopoly.Client.Runtime.UI.Pools
{
    internal sealed class MessageBoxViewsPool : ObjectPool<MessageBoxView>
    {
        [SerializeField]
        private MessageBoxView viewMessageBox;

        [SerializeField, Range(1, 10)]
        private int messageBoxPoolSize;

        internal static MessageBoxViewsPool Instance { get; private set; }

        private void Awake()
        {
            if (MessageBoxViewsPool.Instance != null)
                throw new TypeInitializationException(nameof(MessageBoxViewsPool), new ApplicationException($"Singleton has already been initialized."));

            MessageBoxViewsPool.Instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < this.messageBoxPoolSize; ++i)
            {
                MessageBoxView newMessageBox = GameObject.Instantiate(this.viewMessageBox, base.gameObject.transform.parent.transform);
                newMessageBox.gameObject.SetActive(false);
                base.Append(newMessageBox);
            }
        }
    }
}
