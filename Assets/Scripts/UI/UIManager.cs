using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;
using System.Collections;

public sealed class UIManager : NetworkBehaviour
{
    #region In-editor Setup (Visuals & Logic)

    #region Dice

    [Space]
    [Header("Button  \"Roll Dice\"")]
    [Space]

    [Space]
    [SerializeField] private Button buttonRollDice;

    [Space]
    [Header("Images \"Dices\"")]
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

    #endregion

    #region Panel Info

    [Space]
    [Header("Panel \"Info\"")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelInfo;

    [Space]
    [SerializeField] private Image imagePanelInfo;

    [Space]
    [SerializeField] private TMP_Text textPanelInfo;

    [Space]
    [SerializeField] private Button buttonConfirmPanelInfo;

    #endregion

    #region Panel Offer

    [Space]
    [Header("Panel \"Offer\"")]
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

    #endregion

    #region Panel Payment

    [Space]
    [Header("Panel \"Payment\"")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelPayment;

    [Space]
    [SerializeField] private Image imagePanelPayment;

    [Space]
    [SerializeField] private TMP_Text textPanelPayment;

    [Space]
    [SerializeField] private Button buttonPayPanelPayment;

    #endregion

    #region Panel Players

    [Space]
    [Header("Panel \"Players\"")]
    [Space]

    [Space]
    [SerializeField] private NetworkObject playerInfo;

    [Space]
    [SerializeField] private NetworkObject panelPlayers;

    #endregion

    #region Panel Message

    [Space]
    [Header("Panel \"Message\"")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelMessage;

    [Space]
    [SerializeField] private TMP_Text textPanelMessage;

    [Space]
    [SerializeField] private Button buttonConfirmPanelMessage;

    #endregion

    #region Panel Monopoly Node

    [Space]
    [Header("Panel \"Monopoly Node\"")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panelMonopolyNode;

    [Space]
    [SerializeField] private Image imagePanelMonopolyNode;

    [Space]
    [SerializeField] private TMP_Text textPanelMonopolyNode;

    [Space]
    [SerializeField] private Button buttonUpgradePanelMonopolyNode;

    [Space]
    [SerializeField] private Button buttonDowngradePanelMonopolyNode;

    #endregion

    #endregion

    #region Events

    public delegate void ButtonClickHandler();

    public event ButtonClickHandler OnButtonRollDiceClicked;

    public event ButtonClickHandler OnButtonPayPanelPaymentClicked;

    public event ButtonClickHandler OnButtonConfirmPanelInfoClicked;

    public event ButtonClickHandler OnButtonAcceptPanelOfferClicked;

    public event ButtonClickHandler OnButtonDeclinePanelOfferClicked;

    public event ButtonClickHandler OnButtonConfirmPanelMessageClicked;

    public event ButtonClickHandler OnButtonUpgradePanelMonopolyNodeClicked;

    public event ButtonClickHandler OnButtonDowngradePanelMonopolyNodeClicked;

    #endregion

    public static UIManager Instance { get; private set; }

    public enum UIControl : byte
    {
        PanelInfo,
        PanelTrade,
        PanelOffer,
        PanelWarning,
        PanelPayment,
        PanelMessage,
        ButtonRollDice,
        PanelMonopolyNode
    }

    private void Awake() => Instance = this;

    private void OnEnable()
    {
        this.buttonRollDice.onClick.AddListener(this.HandleButtonRollDiceClicked);
        this.buttonPayPanelPayment.onClick.AddListener(this.HandleButtonPayPanelPaymentClicked);
        this.buttonConfirmPanelInfo.onClick.AddListener(this.HandleButtonConfirmPanelInfoClicked);
        this.buttonAcceptPanelOffer.onClick.AddListener(this.HandleButtonAcceptPanelOfferClicked);
        this.buttonDeclinePanelOffer.onClick.AddListener(this.HandleButtonDeclinePanelOfferClicked);
        this.buttonConfirmPanelMessage.onClick.AddListener(this.HandleButtonConfirmPanelMessageClicked);
        this.buttonUpgradePanelMonopolyNode.onClick.AddListener(this.HandleButtonUpgradeMonopolyNodeClicked);
        this.buttonDowngradePanelMonopolyNode.onClick.AddListener(this.HandleButtonDowngradeMonopolyNodeClicked);
    }

    private void OnDisable()
    {
        this.buttonRollDice.onClick.RemoveListener(this.HandleButtonRollDiceClicked);
        this.buttonPayPanelPayment.onClick.RemoveListener(this.HandleButtonPayPanelPaymentClicked);
        this.buttonConfirmPanelInfo.onClick.RemoveListener(this.HandleButtonConfirmPanelInfoClicked);
        this.buttonAcceptPanelOffer.onClick.RemoveListener(this.HandleButtonAcceptPanelOfferClicked);
        this.buttonDeclinePanelOffer.onClick.RemoveListener(this.HandleButtonDeclinePanelOfferClicked);
        this.buttonConfirmPanelMessage.onClick.RemoveListener(this.HandleButtonConfirmPanelMessageClicked);
        this.buttonUpgradePanelMonopolyNode.onClick.RemoveListener(this.HandleButtonUpgradeMonopolyNodeClicked);
        this.buttonDowngradePanelMonopolyNode.onClick.RemoveListener(this.HandleButtonDowngradeMonopolyNodeClicked);
    }

    public void WaitPlayerInput(bool condition)
    {
        this.StartCoroutine(WaitPlayerInputCoroutine());

        IEnumerator WaitPlayerInputCoroutine()
        {
            yield return new WaitUntil(() => condition);
        }
    }

    public void AddPlayer(FixedString32Bytes nickname, Color color)
    {
        UIPlayerInfo info = NetworkObject.Instantiate(this.playerInfo, this.panelPlayers.transform).GetComponent<UIPlayerInfo>();
        info.SetUpPlayerInfo(nickname, color);
    }

    public void SetControlState(UIControl control, bool visability, MonopolyNode node = null)
    {
        switch (control)
        {
            case UIControl.PanelInfo:
                {
                    this.panelInfo.gameObject.SetActive(visability);

                    if (node != null)
                    {
                        this.imagePanelInfo.sprite = node.NodeSprite;
                    }
                }
                break;
            case UIControl.PanelOffer:
                {
                    this.panelOffer.gameObject.SetActive(visability);

                    if (node != null)
                    {
                        this.imagePanelOffer.sprite = node.NodeSprite;
                        this.textPanelOffer.text = $"₴ {node?.Price}";
                    }
                }
                break;
            case UIControl.PanelPayment:
                {
                    this.panelPayment.gameObject.SetActive(visability);
                    
                    if (node != null)
                    {
                        this.imagePanelPayment.sprite = node.NodeSprite;
                    }
                }
                break;
            case UIControl.ButtonRollDice:
                this.buttonRollDice.gameObject.SetActive(visability);
                break;
            case UIControl.PanelMonopolyNode:
                {
                    this.panelMonopolyNode.gameObject.SetActive(visability);

                    if (node != null)
                    {
                        this.imagePanelMonopolyNode.sprite = node.NodeSprite;
                    }
                }
                break;
            default:
                throw new System.ArgumentException($"{nameof(control)} is not a Panel.");
        }
    }

    #region Dice

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

    [ServerRpc(RequireOwnership = false)]
    public void SyncShowDiceServerRpc() => this.SyncShowDiceClientRpc(GameManager.Instance.ClientParamsAllPlayers);

    [ClientRpc]
    private void SyncShowDiceClientRpc(ClientRpcParams clientRpcParams) => this.ShowDice();

    #endregion

    private void HandleButtonRollDiceClicked() => this.OnButtonRollDiceClicked?.Invoke();

    private void HandleButtonPayPanelPaymentClicked() => this.OnButtonPayPanelPaymentClicked?.Invoke();

    private void HandleButtonConfirmPanelInfoClicked() => this.OnButtonConfirmPanelInfoClicked?.Invoke();

    private void HandleButtonAcceptPanelOfferClicked() => this.OnButtonAcceptPanelOfferClicked?.Invoke();

    private void HandleButtonDeclinePanelOfferClicked() => this.OnButtonDeclinePanelOfferClicked?.Invoke();

    private void HandleButtonConfirmPanelMessageClicked() => this.OnButtonConfirmPanelMessageClicked?.Invoke();

    private void HandleButtonUpgradeMonopolyNodeClicked() => this.OnButtonUpgradePanelMonopolyNodeClicked?.Invoke();

    private void HandleButtonDowngradeMonopolyNodeClicked() => this.OnButtonDowngradePanelMonopolyNodeClicked?.Invoke();
}
