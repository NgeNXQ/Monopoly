using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class PanelTradeOfferUI : MonoBehaviour, IActionControlUI, IPointerClickHandler
{
    #region Setup

    #region Visuals

    [Header("Visuals")]

    [Space]
    [SerializeField] private RectTransform panel;
    
    [Space]
    [SerializeField] private Image imageThis;

    [Space]
    [SerializeField] private Image imageOther;

    [Space]
    [SerializeField] private TMP_Text textThisPlayerNickname;

    [Space]
    [SerializeField] private TMP_Text textOtherPlayerNickname;

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

    private Action callback;

    public static PanelTradeOfferUI Instance { get; private set; }

    public Sprite ThisSprite 
    {
        set
        {
            this.imageThis.sprite = value;
            this.imageThis.gameObject.SetActive(true);
        }
    }

    public Sprite OtherSprite 
    {
        set
        {
            this.imageOther.sprite = value;
            this.imageOther.gameObject.SetActive(true);
        }
    }

    public string ThisNicknameText 
    {
        set => this.textThisPlayerNickname.text = value;
    }

    public string OtherThisNicknameText 
    {
        set => this.textOtherPlayerNickname.text = value;
    }

    public int ThisOffer
    {
        get
        {
            Debug.Log("Here");

            return this.textBoxThisOffer.text.Length == 0 ? 0 : Int32.Parse(this.textBoxThisOffer.text);
        }
    }

    public int OtherOffer
    {
        get
        {
            Debug.Log("Here");

            return this.textBoxOtherOffer.text.Length == 0 ? 0 : Int32.Parse(this.textBoxOtherOffer.text);
        }
    }

    public int ThisNodeIndex { get; set; }

    public int OtherNodeIndex { get; set; }

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

        this.imageThis.sprite = null;
        this.imageOther.sprite = null;

        this.imageThis.gameObject.SetActive(false);
        this.imageOther.gameObject.SetActive(false);

        this.callback = actionCallback;

        this.panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.callback = null;

        this.ThisNodeIndex = -1;
        this.OtherNodeIndex = -1;

        this.imageThis.sprite = null;
        this.imageOther.sprite = null;

        this.textBoxThisOffer.text = String.Empty;
        this.textBoxOtherOffer.text = String.Empty;

        this.textThisPlayerNickname.text = String.Empty;
        this.textOtherPlayerNickname.text = String.Empty;

        this.panel.gameObject.SetActive(false);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == this.imageThis.gameObject)
        {
            this.ThisNodeIndex = -1;
            this.imageThis.gameObject.SetActive(false);
        }
        else if (eventData.pointerCurrentRaycast.gameObject == this.imageOther.gameObject)
        {
            this.OtherNodeIndex = -1;
            this.imageOther.gameObject.SetActive(false);
        }
    }
}
