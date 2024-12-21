using System;
using System.Collections.Generic;
using UnityEngine;

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
                return lastMessageBox;
            else
                throw new System.InvalidOperationException($"No active instances of {nameof(PanelMessageBoxUI)}.");
        }
    }

    private void Awake()
    {
        if (UIManagerGlobal.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        UIManagerGlobal.Instance = this;
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

    public void ShowMessageBox(PanelMessageBoxUI.Type type, string text, PanelMessageBoxUI.Icon icon = default)
    {
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
        messageBox.Show();
    }

    public void ShowMessageBox(PanelMessageBoxUI.Type type, string text, PanelMessageBoxUI.Icon icon = default, Action actionCallback = default)
    {
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
        messageBox.Show(actionCallback != null ? actionCallback : null);
    }

    public void ShowMessageBox(PanelMessageBoxUI.Type type, string text, PanelMessageBoxUI.Icon icon = default, Func<bool> stateCallback = default)
    {
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
        messageBox.Show(stateCallback != null ? stateCallback : null);
    }
}
