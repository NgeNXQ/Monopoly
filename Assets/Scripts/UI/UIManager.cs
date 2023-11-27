using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;

public sealed class UIManager : NetworkBehaviour
{
    [Space]
    [Header("Panel Players Info")]
    [Space]

    [SerializeField] private NetworkObject panelPlayersInfo;

    [SerializeField] private NetworkObject playerInfo;

    [Space]
    [Header("Button roll dices")]
    [Space]

    [SerializeField] private Button buttonRoll;

    [Space]
    [Header("Panel Information")]
    [Space]

    [SerializeField] private RectTransform panelInformation;

    [SerializeField] private Image imagePanelInformation;

    [SerializeField] private TMP_Text textPanelInformation;

    [SerializeField] private Button buttonPanelInformation;

    [Space]
    [Header("Panel Offer")]
    [Space]

    [SerializeField] private RectTransform panelOffer;

    [SerializeField] private Image imagePanelOffer;

    [SerializeField] private TMP_Text textPanelOffer;

    [SerializeField] private Button buttonAcceptPanelOffer;

    [SerializeField] private Button buttonDeclinePanelOffer;

    //[Space]
    //[Header("Panel Fee")]
    //[Space]

    //[SerializeField] private RectTransform panelFee;

    //[SerializeField] private Image imagePanelFee;

    //[SerializeField] private TMP_Text textPanelFee;

    //[SerializeField] private Button buttonPayPanelFee;

    //[Space]
    //[Header("Panel Offer")]
    //[Space]

    //[SerializeField] private RectTransform panelOffer;

    //[SerializeField] private Image imagepanelOffer;

    //[SerializeField] private TMP_Text textpanelOffer;

    //[SerializeField] private Button buttonBuyPanelOffer;

    //[SerializeField] private Button buttonCancelPanelOffer;

    //public UnityAction OnButtonRollDicesPressed { get; set; }

    public delegate void ButtonClickHandler();

    public event ButtonClickHandler OnButtonRollDicesClicked;

    public event ButtonClickHandler OnButtonAcceptPanelOffer;

    public event ButtonClickHandler OnButtonDeclinePanelOffer;

    public static UIManager Instance { get; private set; }

    public enum UIControl : byte
    {
        PanelPay,
        PanelOffer,
        ButtonRollDices,
        PanelInformation,
    }

    private void Awake()
    {
        Instance = this;

        //this.buttonRoll.onClick.AddListener(this.HandleButtonRollDicesClicked);
        //this.buttonAcceptPanelOffer.onClick.AddListener(this.HandleButtonAcceptPanelOfferClicked);
        //this.buttonDeclinePanelOffer.onClick.AddListener(this.HandleButtonDeclinePanelOfferClicked);
    }

    public void ShowControl(UIControl control)
    {
        switch (control)
        {
            case UIControl.PanelOffer:
                this.panelOffer?.gameObject.SetActive(true);
                break;
            case UIControl.ButtonRollDices:
                this.buttonRoll?.gameObject.SetActive(true);
                break;
            case UIControl.PanelInformation:
                this.panelInformation?.gameObject.SetActive(true);
                break;
        }
    }

    public void HideControl(UIControl control)
    {
        switch (control)
        {
            case UIControl.PanelOffer:
                this.panelOffer?.gameObject.SetActive(false);
                break;
            case UIControl.ButtonRollDices:
                this.buttonRoll?.gameObject.SetActive(false);
                break;
            case UIControl.PanelInformation:
                this.panelInformation?.gameObject.SetActive(false);
                break;
        }
    }

    public void AddPlayer(FixedString32Bytes nickname, Color color)
    {
        UIPlayerInfo info = NetworkObject.Instantiate(this.playerInfo, this.panelPlayersInfo.transform).GetComponent<UIPlayerInfo>();
        info.SetUpPlayerInfo(nickname, color);
    }

    public void SetUpPanelOffer(Sprite sprite, string description)
    {
        this.imagePanelOffer.sprite = sprite;
        this.textPanelOffer.text = description;
    }

    private void HandleButtonRollDicesClicked() => this.OnButtonRollDicesClicked?.Invoke();

    private void HandleButtonAcceptPanelOfferClicked() => this.OnButtonAcceptPanelOffer?.Invoke();

    private void HandleButtonDeclinePanelOfferClicked() => this.OnButtonDeclinePanelOffer?.Invoke();

    private void OnDestroy()
    {
        //this.buttonRoll.onClick.RemoveListener(this.HandleButtonRollDicesClicked);
    }
}
