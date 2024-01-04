using UnityEngine;
using System.Collections.Generic;

internal sealed class ObjectPoolPanelPlayerLobby : MonoBehaviour
{
    #region Setup

    #region Parent Object

    [Header("Parent Object")]

    [Space]
    [SerializeField] private Canvas canvasParent;

    #endregion

    #region Panel Message Box

    [Space]
    [Header("Panel Player Lobby")]

    [Space]
    [SerializeField] private PanelPlayerLobbyUI panelPlayerLobby;

    #endregion

    #endregion

    private LinkedList<PanelPlayerLobbyUI> pooledPanelsPlayer;

    public static ObjectPoolPanelPlayerLobby Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");
        }

        Instance = this;
    }

    private void Start()
    {
        this.InitializeObjectPool();
    }

    private void InitializeObjectPool()
    {
        if (this.pooledPanelsPlayer != null)
        {
            return;
        }

        this.pooledPanelsPlayer = new LinkedList<PanelPlayerLobbyUI>();

        for (int i = 0; i < LobbyManager.MAX_PLAYERS; ++i)
        {
            PanelPlayerLobbyUI newPanelPlayer = GameObject.Instantiate(this.panelPlayerLobby, this.canvasParent.gameObject.transform);
            this.pooledPanelsPlayer.AddLast(newPanelPlayer);
            newPanelPlayer.gameObject.SetActive(false);
        }

        GameCoordinator.Instance.UpdateInitializedObjects(this.GetType());
    }

    public PanelPlayerLobbyUI GetPooledObject()
    {
        if (this.pooledPanelsPlayer == null)
        {
            this.InitializeObjectPool();
        }

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
