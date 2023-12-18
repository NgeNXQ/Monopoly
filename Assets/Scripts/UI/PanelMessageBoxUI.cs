using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelMessageBoxUI : MonoBehaviour, IControlUI, IButtonHandlerUI
{
    #region Visuals

    #region Shared Visuals

    [Space]
    [Header("Shared Visuals")]
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

    public static PanelMessageBoxUI Instance { get; private set; }

    public event IButtonHandlerUI.ButtonClickedEventHandler ButtonConfirmPanelOKClicked;

    public event IButtonHandlerUI.ButtonClickedEventHandler ButtonConfirmPanelOKCancelClicked;

    public event IButtonHandlerUI.ButtonClickedEventHandler ButtonCancelPanelOKCancelClicked;

    public Type MessageBoxType { get; set; }

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

    public string MessageText 
    {
        set => this.textMessage.text = value; 
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void OnEnable()
    {
        this.buttonConfirmPanelOK.onClick.AddListener(this.HandleButtonConfirmPanelOKClicked);
        this.buttonConfirmPanelOKCancel.onClick.AddListener(this.HandleButtonConfirmPanelOKCancelClicked);
        this.buttonCancelPanelOKCancel.onClick.AddListener(this.HandleButtonCancelPanelOKCancelClicked);
    }

    private void OnDisable()
    {
        this.buttonConfirmPanelOK.onClick.RemoveListener(this.HandleButtonConfirmPanelOKClicked);
        this.buttonConfirmPanelOKCancel.onClick.RemoveListener(this.HandleButtonConfirmPanelOKCancelClicked);
        this.buttonCancelPanelOKCancel.onClick.RemoveListener(this.HandleButtonCancelPanelOKCancelClicked);
    }

    public void Show()
    {
        this.panelTemplate.gameObject.SetActive(true);

        switch (this.MessageBoxType)
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

        switch (this.MessageBoxType)
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

    private void HandleButtonConfirmPanelOKClicked() => this.ButtonConfirmPanelOKClicked?.Invoke();

    private void HandleButtonConfirmPanelOKCancelClicked() => this.ButtonConfirmPanelOKCancelClicked?.Invoke();

    private void HandleButtonCancelPanelOKCancelClicked() => this.ButtonCancelPanelOKCancelClicked?.Invoke();
}
