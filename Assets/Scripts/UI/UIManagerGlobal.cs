using System;
using UnityEngine;
using System.Collections.Generic;

internal sealed class UIManagerGlobal : MonoBehaviour
{
    private TouchScreenKeyboard keyboard;

    private Stack<PanelMessageBoxUI> activeMessageBoxes;

    public static UIManagerGlobal Instance { get; private set; }

    public PanelMessageBoxUI LastMessageBox 
    {
        get
        {
            if (this.activeMessageBoxes.TryPop(out PanelMessageBoxUI lastMessageBox))
            {
                return lastMessageBox;
            }
            else
            {
                throw new System.InvalidOperationException($"No active instances of {nameof(PanelMessageBoxUI)}.");
            }
        }
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

    private void OnDestroy()
    {
        this.activeMessageBoxes.Clear();
    }

    public void ShowMessageBox(PanelMessageBoxUI.Type type, string text, PanelMessageBoxUI.Icon icon = default, Action actionCallback = default, Func<bool> stateCallback = default)
    {
        if (actionCallback != null && stateCallback != null)
        {
            throw new System.ArgumentException($"{nameof(PanelMessageBoxUI)} must have only 1 callback.");
        }

        if (this.activeMessageBoxes.TryPeek(out PanelMessageBoxUI lastMessageBox))
        {
            if (lastMessageBox.MessageBoxType == PanelMessageBoxUI.Type.None)
            {
                lastMessageBox.Hide();
                this.activeMessageBoxes.Pop();
            }
        }

        PanelMessageBoxUI messageBox = ObjectPoolMessageBoxes.Instance?.GetPooledObject();

        messageBox.MessageBoxType = type;
        messageBox.MessageBoxIcon = icon;
        messageBox.MessageBoxText = text;

        this.activeMessageBoxes.Push(messageBox);

        if (actionCallback != null)
        {
            messageBox.Show(actionCallback);
        }
        else if (stateCallback != null)
        {
            messageBox.Show(stateCallback);
        }
        else
        {
            messageBox.Show();
        }
    }
}
