using UnityEngine;
using System.Collections.Generic;

internal sealed class ObjectPoolMessageBoxes : MonoBehaviour
{
    #region Setup

    #region Panel Message Box

    [Header("Panel Message Box")]

    [Space]
    [SerializeField] private PanelMessageBoxUI messageBox;

    [Space]
    [SerializeField] [Range(1, 10)] private int messageBoxPoolSize;

    #endregion

    #endregion

    private LinkedList<PanelMessageBoxUI> pooledMessageBoxes;

    public static ObjectPoolMessageBoxes Instance { get; private set; }

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
        if (this.pooledMessageBoxes != null)
        {
            return;
        }

        this.pooledMessageBoxes = new LinkedList<PanelMessageBoxUI>();

        for (int i = 0; i < this.messageBoxPoolSize; ++i)
        {
            PanelMessageBoxUI newMessageBox = GameObject.Instantiate(this.messageBox, this.gameObject.transform.parent.transform);
            this.pooledMessageBoxes.AddLast(newMessageBox);
            newMessageBox.gameObject.SetActive(false);
        }
    }

    public PanelMessageBoxUI GetPooledObject()
    {
        if (this.pooledMessageBoxes == null)
        {
            this.InitializeObjectPool();
        }

        foreach (PanelMessageBoxUI messageBox in this.pooledMessageBoxes)
        {
            if ((bool)!messageBox.gameObject?.activeInHierarchy)
            {
                messageBox.gameObject.SetActive(true);
                return messageBox;
            }
        }

        PanelMessageBoxUI newMessageBox = GameObject.Instantiate(this.messageBox, this.gameObject.transform.parent.transform);
        this.pooledMessageBoxes.AddLast(newMessageBox);
        newMessageBox.gameObject.SetActive(true);

        return newMessageBox;
    }
}
