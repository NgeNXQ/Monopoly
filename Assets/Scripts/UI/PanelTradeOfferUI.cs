using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class PanelTradeOfferUI : MonoBehaviour, IActionControlUI
{
    #region Setup

    #region Visuals

    [Header("Visuals")]

    [Space]
    [SerializeField] private RectTransform panel;
    
    [Space]
    [SerializeField] private Image[] imagesThis;

    [Space]
    [SerializeField] private Image[] imagesOther;

    [Space]
    [SerializeField] private TMP_Text textPlayerNickname;

    #endregion

    #region Controls

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField] private Button buttonOffer;

    [Space]
    [SerializeField] private Button buttonCancel;

    [Space]
    [SerializeField] private TMP_InputField textBoxThisOffer;

    [Space]
    [SerializeField] private TMP_InputField textBoxOtherOffer;

    #endregion

    #endregion

    public enum DialogResult : byte
    {
        Offer,
        Cancel
    }

    private int thisIndex;

    private int otherIndex;

    private Action callback;

    //private int[] 

    public static PanelTradeOfferUI Instance { get; private set; }

    public Sprite ThisSprite 
    {
        set
        {
            this.imagesThis[this.thisIndex].sprite = value;
            this.thisIndex = (this.thisIndex + 1) % this.imagesThis.Length;
        }
    }

    public Sprite OtherSprite 
    {
        set
        {
            this.imagesOther[this.otherIndex].sprite = value;
            this.otherIndex = (this.otherIndex + 1) % this.imagesOther.Length;
        }
    }

    public string NicknameText 
    {
        set => this.textPlayerNickname.text = value;
    }

    public DialogResult TradeOfferDialogResult { get; private set; }

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
        this.buttonOffer.onClick.AddListener(this.HandleButtonOfferClicked);
        this.buttonCancel.onClick.AddListener(this.HandleButtonCancelClicked);
    }

    private void OnDisable()
    {
        this.buttonOffer.onClick.RemoveListener(this.HandleButtonOfferClicked);
        this.buttonCancel.onClick.RemoveListener(this.HandleButtonCancelClicked);
    }

    public void Show(Action actionCallback)
    {
        this.textBoxThisOffer.text = String.Empty;
        this.textBoxOtherOffer.text = String.Empty;

        for (int i = 0; i < this.imagesThis.Length; ++i)
        {
            this.imagesThis[i].sprite = null;
            this.imagesOther[i].sprite = null;
        }

        this.callback = actionCallback;

        this.panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.callback = null;

        this.panel.gameObject.SetActive(false);

        for (int i = 0; i < this.imagesThis.Length; ++i)
        {
            this.imagesThis[i].sprite = null;
            this.imagesOther[i].sprite = null;
        }

        this.textBoxThisOffer.text = String.Empty;
        this.textBoxOtherOffer.text = String.Empty;

        this.textPlayerNickname.text = String.Empty;
    }

    private void HandleButtonOfferClicked()
    {
        this.TradeOfferDialogResult = PanelTradeOfferUI.DialogResult.Offer;

        this.callback?.Invoke();
    }

    private void HandleButtonCancelClicked()
    {
        this.TradeOfferDialogResult = PanelTradeOfferUI.DialogResult.Cancel;

        this.callback?.Invoke();
    }

    public bool ValidateTextBoxBalance(TMP_InputField textBox)
    {
        if (Int32.TryParse(textBox.text, out _))
        {
            return true;
        }
        else
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageOnlyNumbersAllowed, PanelMessageBoxUI.Icon.Warning);
            return false;
        }
    }
}
