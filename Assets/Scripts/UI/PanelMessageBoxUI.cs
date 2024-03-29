﻿using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

internal sealed class PanelMessageBoxUI : MonoBehaviour, IControlUI, IActionControlUI, IStateControlUI
{
    #region Setup

    #region Panel Template

    [Header("Panel Template")]

    [Space]
    [SerializeField] private Canvas panelTemplate;

    [Space]
    [SerializeField] private Image imageIcon;

    [Space]
    [SerializeField] private TMP_Text textMessage;

    #endregion

    #region Panel OK

    [Header("Panel OK")]

    [Space]
    [SerializeField] private RectTransform panelOK;

    [Space]
    [SerializeField] private Button buttonConfirmPanelOK;

    #endregion

    #region Panel OK/Cancel

    [Header("Panel OK/Cancel")]

    [Space]
    [SerializeField] private RectTransform panelOKCancel;

    [Space]
    [SerializeField] private Button buttonConfirmPanelOKCancel;

    [Space]
    [SerializeField] private Button buttonCancelPanelOKCancel;

    #endregion

    #region Assets

    [Header("Assets")]

    [Space]
    [SerializeField] private Sprite spriteError;

    [Space]
    [SerializeField] private Sprite spriteTrophy;

    [Space]
    [SerializeField] private Sprite spriteWarning;

    [Space]
    [SerializeField] private Sprite spriteLoading;

    [Space]
    [SerializeField] private Sprite spriteQuestion;

    #endregion  

    #endregion

    public enum Type : byte
    {
        OK,
        None,
        OKCancel,
    }

    public enum Icon : byte
    {
        None,
        Error,
        Trophy,
        Warning,
        Loading,
        Question,
    }

    public enum DialogResult : byte
    {
        OK,
        None,
        Cancel
    }

    private Type messageBoxType;

    private Action actionCallback;

    private Func<bool> stateCallback;
    
    private DialogResult messageBoxDialogResult;
    
    public Type MessageBoxType 
    {
        get => this.messageBoxType;
        set => this.messageBoxType = value;
    }

    public Icon MessageBoxIcon 
    {
        set
        {
            switch (value)
            {
                case Icon.None:
                    this.imageIcon.sprite = null;
                    break;
                case Icon.Error:
                    this.imageIcon.sprite = this.spriteError;
                    break;
                case Icon.Trophy:
                    this.imageIcon.sprite = this.spriteTrophy;
                    break;
                case Icon.Warning:
                    this.imageIcon.sprite = this.spriteWarning;
                    break;
                case Icon.Loading:
                    this.imageIcon.sprite = this.spriteLoading;
                    break;
                case Icon.Question:
                    this.imageIcon.sprite = this.spriteQuestion;
                    break;
            }
        }
    }

    public string MessageBoxText 
    {
        set => this.textMessage.text = value;
    }

    public DialogResult MessageBoxDialogResult 
    {
        get => this.messageBoxDialogResult;
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += this.HandleActiveSceneChanged;

        this.buttonConfirmPanelOK.onClick.AddListener(this.HandleButtonOKClicked);
        this.buttonConfirmPanelOKCancel.onClick.AddListener(this.HandleButtonOKClicked);
        this.buttonCancelPanelOKCancel.onClick.AddListener(this.HandleButtonCancelClicked);
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= this.HandleActiveSceneChanged;

        this.buttonConfirmPanelOK.onClick.RemoveListener(this.HandleButtonOKClicked);
        this.buttonConfirmPanelOKCancel.onClick.RemoveListener(this.HandleButtonOKClicked);
        this.buttonCancelPanelOKCancel.onClick.RemoveListener(this.HandleButtonCancelClicked);
    }

    public void Show()
    {
        this.panelTemplate.gameObject.SetActive(true);

        switch (this.messageBoxType)
        {
            case Type.OK:
                this.panelOK.gameObject.SetActive(true);
                break;
            case Type.None:
                {
                    if (this.stateCallback != null)
                    {
                        this.StartCoroutine(this.WaitStateCoroutine());
                    }
                }
                break;
            case Type.OKCancel:
                this.panelOKCancel.gameObject.SetActive(true);
                break;
        }
    }

    public void Hide()
    {
        this.stateCallback = null;
        this.actionCallback = null;

        this.panelTemplate.gameObject.SetActive(false);

        switch (this.messageBoxType)
        {
            case Type.OK:
                this.panelOK.gameObject.SetActive(false);
                break;
            case Type.OKCancel:
                this.panelOKCancel.gameObject.SetActive(false);
                break;
        }
    }

    public void Show(Action actionCallback = null)
    {
        this.actionCallback = actionCallback;

        this.Show();
    }

    public void Show(Func<bool> stateCallback = null)
    {
        this.stateCallback = stateCallback;

        this.Show();
    }
    
    private void HandleButtonOKClicked()
    {
        this.messageBoxDialogResult = PanelMessageBoxUI.DialogResult.OK;

        this.actionCallback?.Invoke();

        this.Hide();
    }

    private void HandleButtonCancelClicked()
    {
        this.messageBoxDialogResult = PanelMessageBoxUI.DialogResult.Cancel;

        this.actionCallback?.Invoke();

        this.Hide();
    }

    private IEnumerator WaitStateCoroutine()
    {
        yield return new WaitUntil(this.stateCallback);

        this.Hide();
    }

    private void HandleActiveSceneChanged(Scene previousActiveScene, Scene newActiveScene)
    {
        if (this.messageBoxType == PanelMessageBoxUI.Type.None)
        {
            this.Hide();
        }
    }
}
