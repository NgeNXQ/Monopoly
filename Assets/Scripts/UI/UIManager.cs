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

    [Space]
    [SerializeField] private NetworkObject playerInfo;

    [Space]
    [SerializeField] private NetworkObject panelPlayersInfo;

    [Space]
    [Header("Button roll dice")]
    [Space]

    [Space]
    [SerializeField] private Button buttonRollDice;

    [Space]
    [Header("Images Dices")]
    [Space]

    [Space]
    [SerializeField] private Image imageDiePlaceholder1;

    [Space]
    [SerializeField] private Image imageDiePlaceholder2;

    [Space]
    [SerializeField] private Sprite[] spriteDieFaces = new Sprite[6];

    [Header("Dice screen time")]

    [Space]
    [SerializeField][Range(0.0f, 10.0f)] private float diceScreenTime = 1.0f;

    [Space]
    [Header("Panel Information")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelInformation;

    [Space]
    [SerializeField] private Image imagePanelInformation;

    [Space]
    [SerializeField] private TMP_Text textPanelInformation;

    [Space]
    [SerializeField] private Button buttonPanelInformation;

    [Space]
    [Header("Panel Offer")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelOffer;

    [Space]
    [SerializeField] private Image imagePanelOffer;

    [Space]
    [SerializeField] private TMP_Text textPanelOffer;

    [Space]
    [SerializeField] private Button buttonAcceptPanelOffer;

    [Space]
    [SerializeField] private Button buttonDeclinePanelOffer;

    [Space]
    [Header("Panel Payment")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelPayment;

    [Space]
    [SerializeField] private Image imagePanelPayment;

    [Space]
    [SerializeField] private TMP_Text textPanelPayment;

    [Space]
    [SerializeField] private Button buttonPanelPayment;

    [Space]
    [Header("Panel Monopoly Node")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelMonopolyNode;

    //[SerializeField] private Image imagePanelPayment;

    //[SerializeField] private TMP_Text textPanelPayment;

    //[SerializeField] private Button buttonPanelPayment;

    #endregion

    public delegate void ButtonClickHandler();

    public event ButtonClickHandler OnButtonRollDiceClicked;

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
        ButtonRollDice,
        PanelInformation,
        PanelMonopolyNode
    }

    private void Awake() => Instance = this;

    private void OnEnable()
    {
        this.buttonRollDice.onClick.AddListener(this.HandlebuttonRollDiceClicked);
        this.buttonPanelPayment.onClick.AddListener(this.HandleButtonPanelPaymentClicked);
        this.buttonPanelInformation.onClick.AddListener(this.HandleButtonPanelInformationClicked);
        this.buttonAcceptPanelOffer.onClick.AddListener(this.HandleButtonAcceptPanelOfferClicked);
        this.buttonDeclinePanelOffer.onClick.AddListener(this.HandleButtonDeclinePanelOfferClicked);
    }

    private void OnDisable()
    {
        this.buttonRollDice.onClick.RemoveListener(this.HandlebuttonRollDiceClicked);
        this.buttonPanelPayment.onClick.RemoveListener(this.HandleButtonPanelPaymentClicked);
        this.buttonPanelInformation.onClick.RemoveListener(this.HandleButtonPanelInformationClicked);
        this.buttonAcceptPanelOffer.onClick.RemoveListener(this.HandleButtonAcceptPanelOfferClicked);
        this.buttonDeclinePanelOffer.onClick.RemoveListener(this.HandleButtonDeclinePanelOfferClicked);
    }

    public void WaitPlayerInput(bool condition)
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
            case UIControl.ButtonRollDice:
                this.buttonRollDice?.gameObject.SetActive(state);
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

    public void ShowDice()
    {
        this.imageDiePlaceholder1.gameObject.SetActive(true);
        this.imageDiePlaceholder2.gameObject.SetActive(true);

        this.imageDiePlaceholder1.sprite = this.spriteDieFaces[GameManager.Instance.FirstDieValue - 1];
        this.imageDiePlaceholder2.sprite = this.spriteDieFaces[GameManager.Instance.SecondDieValue - 1];

        this.StartCoroutine(ShowDiceCoroutine());

        IEnumerator ShowDiceCoroutine()
        {
            yield return new WaitForSeconds(this.diceScreenTime);

            this.imageDiePlaceholder1.gameObject.SetActive(false);
            this.imageDiePlaceholder2.gameObject.SetActive(false);
        }
    }

    private void HandlebuttonRollDiceClicked() => this.OnButtonRollDiceClicked?.Invoke();

    private void HandleButtonPanelPaymentClicked() => this.OnButtonPanelPaymentClicked?.Invoke();

    private void HandleButtonPanelInformationClicked() => this.OnButtonPanelInformationClicked?.Invoke();

    private void HandleButtonAcceptPanelOfferClicked() => this.OnButtonAcceptPanelOfferClicked?.Invoke();

    private void HandleButtonDeclinePanelOfferClicked() => this.OnButtonDeclinePanelOfferClicked?.Invoke();
}
