using System;
using UnityEngine;
using Monopoly.Client.Shared.Pools;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Lobby;

namespace Monopoly.Client.Runtime.UI.Pools
{
    internal sealed class UnrankedLobbyPlayerPanelsPool : ObjectPool<PlayerUnrankedLobbyPanel>
    {
        [SerializeField]
        private Canvas canvasParent;

        [SerializeField]
        private PlayerUnrankedLobbyPanel panelPlayer;

        internal static UnrankedLobbyPlayerPanelsPool Instance { get; private set; }

        private void Awake()
        {
            if (UnrankedLobbyPlayerPanelsPool.Instance != null)
                throw new TypeInitializationException(nameof(UnrankedLobbyPlayerPanelsPool), new ApplicationException($"Singleton has already been initialized."));

            UnrankedLobbyPlayerPanelsPool.Instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < LobbyManager.MAX_PLAYERS; ++i)
            {
                PlayerUnrankedLobbyPanel newPanel = GameObject.Instantiate(this.panelPlayer, base.gameObject.transform.parent.transform);
                newPanel.gameObject.SetActive(false);
                base.Append(newPanel);
            }
        }
    }
}
