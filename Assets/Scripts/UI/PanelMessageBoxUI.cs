using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelMessageBoxUI : MonoBehaviour, IControlUI
{
    #region Setup

    [Space]
    [Header("Setup")]
    [Space]

    #region Panel Template

    [Space]
    [Header("Panel Template")]
    [Space]

    [Space]
    [SerializeField] private Canvas panelTemplate;

    [Space]
    [SerializeField] private Image imageIcon;

    [Space]
    [SerializeField] private TMP_Text textMessage;

    #endregion

    #region Panel OK

    [Space]
    [Header("Panel OK")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelOK;

    [Space]
    [SerializeField] private Button buttonConfirmPanelOK;

    #endregion

    #region Panel OK/Cancel

    [Space]
    [Header("Panel OK/Cancel")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelOKCancel;

    [Space]
    [SerializeField] private Button buttonConfirmPanelOKCancel;

    [Space]
    [SerializeField] private Button buttonCancelPanelOKCancel;

    #endregion

    #region Assets

    [Space]
    [Header("Assets")]
    [Space]

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

    private bool isShown;

    private Type messageBoxType;

    private DialogResult messageBoxDialogResult;

    private IControlUI.ButtonClickedCallback callback;

    public static PanelMessageBoxUI Instance { get; private set; }

    public string MessageText
    {
        set => this.textMessage.text = value;
    }

    public Type MessageBoxType
    {
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

    public DialogResult MessageBoxDialogResult 
    {
        get => this.messageBoxDialogResult;
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void OnEnable()
    {
        this.buttonConfirmPanelOK.onClick.AddListener(this.HandleButtonOKClicked);
        this.buttonConfirmPanelOKCancel.onClick.AddListener(this.HandleButtonOKClicked);
        this.buttonCancelPanelOKCancel.onClick.AddListener(this.HandleButtonCancelClicked);
    }

    private void OnDisable()
    {
        this.buttonConfirmPanelOK.onClick.RemoveListener(this.HandleButtonOKClicked);
        this.buttonConfirmPanelOKCancel.onClick.RemoveListener(this.HandleButtonOKClicked);
        this.buttonCancelPanelOKCancel.onClick.RemoveListener(this.HandleButtonCancelClicked);
    }

    public void Show(PanelMessageBoxUI.Type type, string message, PanelMessageBoxUI.Icon icon = PanelMessageBoxUI.Icon.None, IControlUI.ButtonClickedCallback callback = null)
    {
        this.isShown = true;

        this.MessageBoxType = type;
        this.MessageText = message;
        this.MessageBoxIcon = icon;

        this.Show(callback);
    }

    public void Show(IControlUI.ButtonClickedCallback callback)
    {
        this.callback = callback;
        this.panelTemplate.gameObject.SetActive(true);

        switch (this.messageBoxType)
        {
            case Type.OK:
                this.panelOK.gameObject.SetActive(true);
                break;
            case Type.OKCancel:
                this.panelOKCancel.gameObject.SetActive(true);
                break;
        }
    }

    public void Hide()
    {
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

        this.MessageText = String.Empty;
        this.MessageBoxIcon = PanelMessageBoxUI.Icon.None;
        this.MessageBoxType = PanelMessageBoxUI.Type.None;
    }

    private void HandleButtonOKClicked()
    {
        this.Hide();
        this.messageBoxDialogResult = PanelMessageBoxUI.DialogResult.OK;

        this.callback?.Invoke();
    }

    private void HandleButtonCancelClicked()
    {
        this.Hide();
        this.messageBoxDialogResult = PanelMessageBoxUI.DialogResult.Cancel;
        
        this.callback?.Invoke();
    }
}
