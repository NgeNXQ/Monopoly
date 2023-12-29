using System;
using UnityEngine;
using System.Collections.Generic;

internal sealed class UIManagerGlobal : MonoBehaviour
{
    #region Setup

    #region Object Pool UI Manager Global 

    [Header("Object Pool UI Manager Global")]

    [Space]
    [SerializeField] private ObjectPoolUIManagerGlobal objectPoolMessageBox;

    #endregion

    #endregion

    private TouchScreenKeyboard keyboard;

    private Stack<PanelMessageBoxUI> activeMessageBoxes;

    public static UIManagerGlobal Instance { get; private set; }

    public PanelMessageBoxUI LastMessageBox 
    { 
        get => this.activeMessageBoxes.Pop();
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
        GameObject.DontDestroyOnLoad(this);
    }

    private void Start()
    {
        this.activeMessageBoxes = new Stack<PanelMessageBoxUI>();
    }

    public void ShowMessageBox(PanelMessageBoxUI.Type type, string text, PanelMessageBoxUI.Icon icon = default, IControlUI.ButtonClickedCallback callback = default, Func<bool> stateCallback = default)
    {
        PanelMessageBoxUI messageBox = this.objectPoolMessageBox.GetPooledObject();

        messageBox.MessageBoxType = type;
        messageBox.MessageBoxIcon = icon;
        messageBox.MessageBoxText = text;

        if (callback != null)
        {
            this.activeMessageBoxes.Push(messageBox);
        }

        messageBox.Show(callback, stateCallback);
    }

    public void ShowTouchKeyboard(TouchScreenKeyboardType touchScreenKeyboardType)
    {
        this.keyboard = TouchScreenKeyboard.Open(String.Empty, touchScreenKeyboardType);
    }
}
