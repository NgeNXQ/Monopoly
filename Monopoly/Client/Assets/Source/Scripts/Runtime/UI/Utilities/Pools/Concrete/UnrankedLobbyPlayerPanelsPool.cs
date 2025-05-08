using System;
using UnityEngine;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Lobby;
using Monopoly.Client.Runtime.UI.Utilities.Pools.Common;

namespace Monopoly.Client.Runtime.UI.Utilities.Pools.Concrete
{
    internal sealed class UnrankedLobbyPlayerPanelsPool : ObjectPool<PlayerUnrankedLobbyPanel>
    {
        [Header("Parent Object")]

        [Space]
        [SerializeField] private Canvas canvasParent;

        [Space]
        [Header("Panel Player Lobby")]

        [Space]
        [SerializeField] private PlayerUnrankedLobbyPanel panelPlayer;

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
                PlayerUnrankedLobbyPanel newPanel = GameObject.Instantiate(this.panelPlayer, this.gameObject.transform.parent.transform);
                newPanel.gameObject.SetActive(false);
                base.Append(newPanel);
            }
        }
    }
}
