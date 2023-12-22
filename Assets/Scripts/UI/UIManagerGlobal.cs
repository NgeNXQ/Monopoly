using System;
using UnityEngine;

internal sealed class UIManagerGlobal : MonoBehaviour
{
    private TouchScreenKeyboard keyboard;

    public static UIManagerGlobal Instance { get; private set; }

    public PanelMessageBoxUI PanelMessageBox { get => PanelMessageBoxUI.Instance; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
        GameObject.DontDestroyOnLoad(this);
    }

    public void ShowTouchKeyboard(TouchScreenKeyboardType touchScreenKeyboardType)
    {
        this.keyboard = TouchScreenKeyboard.Open(String.Empty, touchScreenKeyboardType);
    }
}
