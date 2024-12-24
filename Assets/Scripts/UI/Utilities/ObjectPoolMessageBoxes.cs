using System.Collections.Generic;
using UnityEngine;

internal sealed class ObjectPoolMessageBoxes : MonoBehaviour
{
    [Header("Panel Message Box")]

    [Space]
    [SerializeField]
    private PanelMessageBoxUI messageBox;

    [Space]
    [SerializeField]
    [Range(1, 10)]
    private int messageBoxPoolSize;

    private LinkedList<PanelMessageBoxUI> pooledMessageBoxes;

    internal static ObjectPoolMessageBoxes Instance { get; private set; }

    private void Awake()
    {
        if (ObjectPoolMessageBoxes.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        ObjectPoolMessageBoxes.Instance = this;
    }

    private void Start()
    {
        this.InitializeObjectPool();
    }

    private void InitializeObjectPool()
    {
        if (this.pooledMessageBoxes != null)
            return;

        this.pooledMessageBoxes = new LinkedList<PanelMessageBoxUI>();

        for (int i = 0; i < this.messageBoxPoolSize; ++i)
        {
            PanelMessageBoxUI newMessageBox = GameObject.Instantiate(this.messageBox, this.gameObject.transform.parent.transform);
            this.pooledMessageBoxes.AddLast(newMessageBox);
            newMessageBox.gameObject.SetActive(false);
        }
    }

    internal PanelMessageBoxUI GetPooledObject()
    {
        if (this.pooledMessageBoxes == null)
            this.InitializeObjectPool();

        foreach (PanelMessageBoxUI messageBox in this.pooledMessageBoxes)
        {
            if (messageBox != null && !messageBox.gameObject.activeInHierarchy)
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
