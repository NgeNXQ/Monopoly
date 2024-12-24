using System.Collections.Generic;
using UnityEngine;

internal sealed class ObjectPoolPanelPlayerLobby : MonoBehaviour
{
    [Header("Parent Object")]

    [Space]
    [SerializeField] private Canvas canvasParent;

    [Space]
    [Header("Panel Player Lobby")]

    [Space]
    [SerializeField] private PanelPlayerLobbyUI panelPlayerLobby;

    private LinkedList<PanelPlayerLobbyUI> pooledPanelsPlayer;

    internal static ObjectPoolPanelPlayerLobby Instance { get; private set; }

    private void Awake()
    {
        if (ObjectPoolPanelPlayerLobby.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        ObjectPoolPanelPlayerLobby.Instance = this;
    }

    private void Start()
    {
        this.InitializeObjectPool();
    }

    private void InitializeObjectPool()
    {
        if (this.pooledPanelsPlayer != null)
            return;

        this.pooledPanelsPlayer = new LinkedList<PanelPlayerLobbyUI>();

        for (int i = 0; i < LobbyManager.MAX_PLAYERS; ++i)
        {
            PanelPlayerLobbyUI newPanelPlayer = GameObject.Instantiate(this.panelPlayerLobby, this.canvasParent.gameObject.transform);
            this.pooledPanelsPlayer.AddLast(newPanelPlayer);
            newPanelPlayer.gameObject.SetActive(false);
        }

        GameCoordinator.Instance.UpdateInitializedObjects(this.GetType());
    }

    internal PanelPlayerLobbyUI GetPooledObject()
    {
        if (this.pooledPanelsPlayer == null)
            this.InitializeObjectPool();

        foreach (PanelPlayerLobbyUI panelPlayer in this.pooledPanelsPlayer)
        {
            if (!panelPlayer.gameObject.activeInHierarchy)
            {
                panelPlayer.gameObject.SetActive(true);
                return panelPlayer;
            }
        }

        throw new System.InvalidOperationException($"Unauthorized access to {nameof(LobbyManager)}!");
    }
}
