using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;
using System.Collections;

public sealed class UIManager : NetworkBehaviour
{
    #region Visuals

    [Space]
    [Header("Panel Players Info")]
    [Space]

    [SerializeField] private NetworkObject playerInfo;

    [SerializeField] private NetworkObject panelPlayersInfo;

    [Space]
    [Header("Button roll dices")]
    [Space]

    [SerializeField] private Button buttonRollDices;

    [Space]
    [Header("Images Dices")]
    [Space]

    [SerializeField] private Image imageDicePlaceholder1;

    [SerializeField] private Image imageDicePlaceholder2;

    [SerializeField] private Image imageDice1;

    [SerializeField] private Image imageDice2;

    [SerializeField] private Image imageDice3;

    [SerializeField] private Image imageDice4;

    [SerializeField] private Image imageDice5;

    [SerializeField] private Image imageDice6;

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

    [Space]
    [Header("Panel Payment")]
    [Space]

    [SerializeField] private RectTransform panelPayment;

    [SerializeField] private Image imagePanelPayment;

    [SerializeField] private TMP_Text textPanelPayment;

    [SerializeField] private Button buttonPanelPayment;

    [Space]
    [Header("Panel Monopoly Node")]
    [Space]

    [SerializeField] private RectTransform panelMonopolyNode;

    //[SerializeField] private Image imagePanelPayment;

    //[SerializeField] private TMP_Text textPanelPayment;

    //[SerializeField] private Button buttonPanelPayment;

    #endregion

    public delegate void ButtonClickHandler();

    public event ButtonClickHandler OnButtonRollDicesClicked;

    public event ButtonClickHandler OnButtonPanelPaymentClicked;

    public event ButtonClickHandler OnButtonPanelInformationClicked;

    public event ButtonClickHandler OnButtonAcceptPanelOfferClicked;

    public event ButtonClickHandler OnButtonDeclinePanelOfferClicked;

    public static UIManager Instance { get; private set; }

    public enum UIControl : byte
    {
        PanelTrade,
        PanelOffer,
        PanelWarning,
        PanelPayment,
        ButtonRollDices,
        PanelInformation,
        PanelMonopolyNode
    }

    private void Awake() => Instance = this;

    private void OnEnable()
    {
        this.buttonRollDices.onClick.AddListener(this.HandleButtonRollDicesClicked);
        this.buttonPanelPayment.onClick.AddListener(this.HandleButtonPanelPaymentClicked);
        this.buttonPanelInformation.onClick.AddListener(this.HandleButtonPanelInformationClicked);
        this.buttonAcceptPanelOffer.onClick.AddListener(this.HandleButtonAcceptPanelOfferClicked);
        this.buttonDeclinePanelOffer.onClick.AddListener(this.HandleButtonDeclinePanelOfferClicked);
    }

    private void OnDisable()
    {
        this.buttonRollDices.onClick.RemoveListener(this.HandleButtonRollDicesClicked);
        this.buttonPanelPayment.onClick.RemoveListener(this.HandleButtonPanelPaymentClicked);
        this.buttonPanelInformation.onClick.RemoveListener(this.HandleButtonPanelInformationClicked);
        this.buttonAcceptPanelOffer.onClick.RemoveListener(this.HandleButtonAcceptPanelOfferClicked);
        this.buttonDeclinePanelOffer.onClick.RemoveListener(this.HandleButtonDeclinePanelOfferClicked);
    }

    public void WaitForPlayerInput(bool condition)
    {
        this.StartCoroutine(WaitPlayerInputCoroutine());

        IEnumerator WaitPlayerInputCoroutine()
        {
            yield return new WaitUntil(() => condition);
        }
    }

    public void SetControlVisibility(UIControl control, bool state)
    {
        switch (control)
        {
            case UIControl.PanelOffer:
                this.panelOffer?.gameObject.SetActive(state);
                break;
            case UIControl.PanelPayment:
                this.panelPayment?.gameObject.SetActive(state);
                break;
            case UIControl.ButtonRollDices:
                this.buttonRollDices?.gameObject.SetActive(state);
                break;
            case UIControl.PanelInformation:
                this.panelInformation?.gameObject.SetActive(state);
                break;
            case UIControl.PanelMonopolyNode:
                this.panelMonopolyNode?.gameObject.SetActive(state);
                break;
        }
    }

    public void AddPlayer(FixedString32Bytes nickname, Color color)
    {
        UIPlayerInfo info = NetworkObject.Instantiate(this.playerInfo, this.panelPlayersInfo.transform).GetComponent<UIPlayerInfo>();
        info.SetUpPlayerInfo(nickname, color);
    }

    public void SetUpPanel(UIControl control, MonopolyNode node)
    {
        switch (control)
        {
            case UIControl.PanelOffer:
                this.imagePanelOffer.sprite = node.NodeSprite;
                break;
            case UIControl.PanelPayment:
                this.imagePanelPayment.sprite = node.NodeSprite;
                break;
            case UIControl.PanelInformation:
                this.imagePanelInformation.sprite = node.NodeSprite;
                break;
            default:
                throw new System.ArgumentException($"{nameof(control)} is not a Panel.");
        }
    }

    private void HandleButtonRollDicesClicked() => this.OnButtonRollDicesClicked?.Invoke();

    private void HandleButtonPanelPaymentClicked() => this.OnButtonPanelPaymentClicked?.Invoke();

    private void HandleButtonPanelInformationClicked() => this.OnButtonPanelInformationClicked?.Invoke();

    private void HandleButtonAcceptPanelOfferClicked() => this.OnButtonAcceptPanelOfferClicked?.Invoke();

    private void HandleButtonDeclinePanelOfferClicked() => this.OnButtonDeclinePanelOfferClicked?.Invoke();
}
