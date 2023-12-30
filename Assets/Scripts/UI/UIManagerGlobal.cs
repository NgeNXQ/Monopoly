using System;
using UnityEngine;
using System.Collections.Generic;

internal sealed class UIManagerGlobal : MonoBehaviour
{
    #region Setup

    #region Object Pool Message Boxes

    [Header("Object Pool Message Boxes")]

    [Space]
    [SerializeField] private ObjectPoolMessageBoxes objectPoolMessageBoxes;

    #endregion

    #region Messages

    [Header("Messages")]

    [Space]
    [SerializeField] private string messageKicked;

    #endregion

    #endregion

    private TouchScreenKeyboard keyboard;

    private Stack<PanelMessageBoxUI> activeMessageBoxes;

    public static UIManagerGlobal Instance { get; private set; }

    public string MessageKicked 
    {
        get => this.messageKicked;
    }

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

        PanelMessageBoxUI messageBox = this.objectPoolMessageBoxes.GetPooledObject();

        messageBox.MessageBoxType = type;
        messageBox.MessageBoxIcon = icon;
        messageBox.MessageBoxText = text;
        
        if (actionCallback != null)
        {
            messageBox.Show(actionCallback);
            this.activeMessageBoxes.Push(messageBox);
        }
        else if (stateCallback != null) 
        {
            messageBox.Show(stateCallback);
            this.activeMessageBoxes.Push(messageBox);
        }
        else
        {
            messageBox.Show();
        }
    }

    public void ShowTouchKeyboard(TouchScreenKeyboardType touchScreenKeyboardType)
    {
        throw new NotImplementedException();
        //this.keyboard = TouchScreenKeyboard.Open(String.Empty, touchScreenKeyboardType);
    }

    public void HideTouchKeyboard()
    {
        throw new NotImplementedException();
        //this.keyboard = TouchScreenKeyboard.Open(String.Empty, touchScreenKeyboardType);
    }
}
