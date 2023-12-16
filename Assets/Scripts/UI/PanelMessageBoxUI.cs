using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelMessageBoxUI : MonoBehaviour, IControlUI, IButtonHandlerUI
{
    #region Visuals

    #region Panel OK

    [Space]
    [Header("Panel OK")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelOK;

    [Space]
    [SerializeField] private Image imageIconPanelOK;

    [Space]
    [SerializeField] private TMP_Text textMessagePanelOK;

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
    [SerializeField] private Image imageIconPanelOKCancel;

    [Space]
    [SerializeField] private TMP_Text textMessagePanelOKCancel;

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
    [SerializeField] private Sprite spriteQuestion;

    #endregion  

    #endregion

    public enum Type : byte
    {
        OK,
        OKCancel
    }

    public enum Icon : byte
    {
        Error,
        Trophy,
        Warning,
        Question
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
                case Icon.Error:
                    this.imageIconPanelOK.sprite = this.imageIconPanelOKCancel.sprite = this.spriteError;
                    break;
                case Icon.Trophy:
                    this.imageIconPanelOK.sprite = this.imageIconPanelOKCancel.sprite = this.spriteTrophy;
                    break;
                case Icon.Warning:
                    this.imageIconPanelOK.sprite = this.imageIconPanelOKCancel.sprite = this.spriteWarning;
                    break;
                case Icon.Question:
                    this.imageIconPanelOK.sprite = this.imageIconPanelOKCancel.sprite = this.spriteQuestion;
                    break;
            }
        }
    }

    public string MessageText 
    { 
        set => this.textMessagePanelOK.text = this.textMessagePanelOKCancel.text = value; 
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
        switch (this.MessageBoxType)
        {
            case Type.OK:
                this.panelOK.gameObject.SetActive(false);
                break;
            case Type.OKCancel:
                this.panelOKCancel.gameObject.SetActive(false);
                break;
        }
    }

    private void HandleButtonConfirmPanelOKClicked() => this.ButtonConfirmPanelOKClicked?.Invoke();

    private void HandleButtonConfirmPanelOKCancelClicked() => this.ButtonConfirmPanelOKCancelClicked?.Invoke();

    private void HandleButtonCancelPanelOKCancelClicked() => this.ButtonCancelPanelOKCancelClicked?.Invoke();
}
