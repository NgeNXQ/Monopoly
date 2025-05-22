using System;
using UnityEngine;
using Monopoly.Client.Shared.Pools;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.UI.Views.Concrete.Lobby;

namespace Monopoly.Client.Runtime.UI.Pools
{
    internal sealed class PrivateLobbyPlayerViewsPool : ObjectPool<PlayerUnrankedLobbyView>
    {
        [SerializeField]
        private Canvas canvasParent;

        [SerializeField]
        private PlayerUnrankedLobbyView viewPlayer;

        internal static PrivateLobbyPlayerViewsPool Instance { get; private set; }

        private void Awake()
        {
            if (PrivateLobbyPlayerViewsPool.Instance != null)
                throw new TypeInitializationException(nameof(PrivateLobbyPlayerViewsPool), new ApplicationException($"Singleton has already been initialized."));

            PrivateLobbyPlayerViewsPool.Instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < LobbyManager.MAX_PLAYERS; ++i)
            {
                PlayerUnrankedLobbyView newPlayerLobby = GameObject.Instantiate(this.viewPlayer, base.gameObject.transform.parent.transform);
                newPlayerLobby.gameObject.SetActive(false);
                base.Append(newPlayerLobby);
            }
        }
    }
}
