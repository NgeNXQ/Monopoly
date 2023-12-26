using UnityEngine;
using System.Collections.Generic;

internal sealed class ObjectPoolUIManagerGlobal : MonoBehaviour
{
    #region Setup

    #region UI Manager Global

    [Header("UI Manager Global")]

    [Space]
    [SerializeField] private UIManagerGlobal globalUIManager;

    #endregion

    #region Panel Message Box

    [Header("Panel Message Box")]

    [Space]
    [SerializeField] private PanelMessageBoxUI messageBox;

    [Space]
    [SerializeField] [Range(1, 10)] private int messageBoxPoolSize;

    #endregion

    #endregion

    private LinkedList<PanelMessageBoxUI> pooledMessageBoxes;

    public static ObjectPoolUIManagerGlobal Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
        GameObject.DontDestroyOnLoad(this);
    }

    private void Start()
    {
        this.pooledMessageBoxes =  new LinkedList<PanelMessageBoxUI>();

        for (int i = 0; i < this.messageBoxPoolSize; ++i)
        {
            PanelMessageBoxUI newMessageBox = GameObject.Instantiate(this.messageBox, this.globalUIManager.transform);
            this.pooledMessageBoxes.AddLast(newMessageBox);
            newMessageBox.gameObject.SetActive(false);
        }
    }

    public PanelMessageBoxUI GetPooledObject()
    {
        foreach (PanelMessageBoxUI messageBox in this.pooledMessageBoxes)
        {
            if (!messageBox.gameObject.activeInHierarchy)
            {
                return messageBox;
            }
        }

        PanelMessageBoxUI newMessageBox = GameObject.Instantiate(this.messageBox, this.globalUIManager.transform);
        this.pooledMessageBoxes.AddLast(newMessageBox);
        newMessageBox.gameObject.SetActive(false);

        return newMessageBox;
    }
}
