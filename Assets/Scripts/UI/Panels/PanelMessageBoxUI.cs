using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

internal sealed class PanelMessageBoxUI : MonoBehaviour, IControlUI, IActionControlUI, IStateControlUI
{
    [Header("Panel Template")]

    [Space]
    [SerializeField]
    private Canvas panelTemplate;

    [Space]
    [SerializeField]
    private Image imageIcon;

    [Space]
    [SerializeField]
    private TMP_Text textMessage;

    [Header("Panel OK")]

    [Space]
    [SerializeField]
    private RectTransform panelOK;

    [Space]
    [SerializeField]
    private Button buttonConfirmPanelOK;

    [Header("Panel OK/Cancel")]

    [Space]
    [SerializeField]
    private RectTransform panelOKCancel;

    [Space]
    [SerializeField]
    private Button buttonConfirmPanelOKCancel;

    [Space]
    [SerializeField]
    private Button buttonCancelPanelOKCancel;

    [Header("Assets")]

    [Space]
    [SerializeField]
    private Sprite spriteError;

    [Space]
    [SerializeField]
    private Sprite spriteTrophy;

    [Space]
    [SerializeField]
    private Sprite spriteWarning;

    [Space]
    [SerializeField]
    private Sprite spriteLoading;

    [Space]
    [SerializeField]
    private Sprite spriteQuestion;

    [Space]
    [SerializeField]
    private Sprite spriteSuccess;

    [Space]
    [SerializeField]
    private Sprite spriteFailure;

    internal enum Type : byte
    {
        OK,
        None,
        OKCancel,
    }

    internal enum Icon : byte
    {
        None,
        Error,
        Trophy,
        Warning,
        Loading,
        Question,
        Success,
        Failure
    }

    internal enum DialogResult : byte
    {
        OK,
        None,
        Cancel
    }

    private Type messageBoxType;
    private Action actionCallback;
    private Func<bool> stateCallback;

    internal DialogResult PanelDialogResult { get; private set; }

    internal string MessageBoxText
    {
        set => this.textMessage.text = value;
    }

    internal Type MessageBoxType
    {
        get => this.messageBoxType;
        set => this.messageBoxType = value;
    }

    internal Icon MessageBoxIcon
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
                case Icon.Success:
                    this.imageIcon.sprite = this.spriteSuccess;
                    break;
                case Icon.Failure:
                    this.imageIcon.sprite = this.spriteFailure;
                    break;
            }
        }
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += this.OnActiveSceneChanged;

        this.buttonConfirmPanelOK.onClick.AddListener(this.OnButtonOkClicked);
        this.buttonConfirmPanelOKCancel.onClick.AddListener(this.OnButtonOkClicked);
        this.buttonCancelPanelOKCancel.onClick.AddListener(this.OnButtonCancelClicked);
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= this.OnActiveSceneChanged;

        this.buttonConfirmPanelOK.onClick.RemoveListener(this.OnButtonOkClicked);
        this.buttonConfirmPanelOKCancel.onClick.RemoveListener(this.OnButtonOkClicked);
        this.buttonCancelPanelOKCancel.onClick.RemoveListener(this.OnButtonCancelClicked);
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
                        this.StartCoroutine(this.WaitStateCoroutine());
                }
                break;
            case Type.OKCancel:
                this.panelOKCancel.gameObject.SetActive(true);
                break;
        }
    }

    private IEnumerator WaitStateCoroutine()
    {
        yield return new WaitUntil(this.stateCallback);
        this.Hide();
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

    private void OnButtonOkClicked()
    {
        this.PanelDialogResult = PanelMessageBoxUI.DialogResult.OK;
        this.actionCallback?.Invoke();
        this.Hide();
    }

    private void OnButtonCancelClicked()
    {
        this.PanelDialogResult = PanelMessageBoxUI.DialogResult.Cancel;
        this.actionCallback?.Invoke();
        this.Hide();
    }

    private void OnActiveSceneChanged(Scene previousActiveScene, Scene newActiveScene)
    {
        if (this.messageBoxType == PanelMessageBoxUI.Type.None)
            this.Hide();
    }
}
