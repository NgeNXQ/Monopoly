﻿using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelPaymentChanceUI : MonoBehaviour, IActionControlUI
{
    #region Setup

    #region Visuals

    [Header("Visuals")]

    [Space]
    [SerializeField] private RectTransform panel;

    [Space]
    [SerializeField] private TMP_Text textDescription;

    #endregion

    #region Controls

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField] private Button buttonConfirm;

    #endregion

    #endregion

    public enum DialogResult : byte
    {
        Confirmed
    }

    private Action callback;

    public static PanelPaymentChanceUI Instance { get; private set; }

    public string DescriptionText 
    { 
        set => this.textDescription.text = value; 
    }

    public DialogResult PaymentDialogResult { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");
        }

        Instance = this;
    }

    private void OnEnable()
    {
        this.buttonConfirm.onClick.AddListener(this.HandleButtonConfirmClicked);
    }

    private void OnDisable()
    {
        this.buttonConfirm.onClick.RemoveListener(this.HandleButtonConfirmClicked);
    }
    
    public void Show(Action actionCallback = null)
    {
        this.callback = actionCallback;

        this.panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.callback = null;

        this.panel.gameObject.SetActive(false);
    }

    private void HandleButtonConfirmClicked()
    {
        this.PaymentDialogResult = PanelPaymentChanceUI.DialogResult.Confirmed;

        this.callback?.Invoke();
    }
}
