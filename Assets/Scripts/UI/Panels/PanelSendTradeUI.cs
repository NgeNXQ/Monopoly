using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

internal sealed class PanelSendTradeUI : MonoBehaviour, IActionControlUI, IPointerClickHandler
{
    [Header("Visuals")]

    [Space]
    [SerializeField]
    private RectTransform panel;

    [Space]
    [SerializeField]
    private Image imageSender;

    [Space]
    [SerializeField]
    private Image imageReceiver;

    [Space]
    [SerializeField]
    private TMP_Text labelSenderNickname;

    [Space]
    [SerializeField]
    private TMP_Text labelReceiverNickname;

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField]
    private Button buttonSendOffer;

    [Space]
    [SerializeField]
    private Button buttonCancelOffer;

    [Space]
    [SerializeField]
    private TMP_InputField textBoxSenderBalanceAmount;

    [Space]
    [SerializeField]
    private TMP_InputField textBoxReceiverBalanceAmount;

    internal enum DialogResult : byte
    {
        Offer,
        Cancel
    }

    private Action callback;
    private PawnController sender;
    private PawnController receiver;
    private MonopolyNode senderNode;
    private MonopolyNode receiverNode;

    internal static PanelSendTradeUI Instance { get; private set; }

    internal DialogResult PanelDialogResult { get; private set; }

    internal int senderBalanceAmount => this.textBoxSenderBalanceAmount.text.Length == 0 ? 0 : Int32.Parse(this.textBoxSenderBalanceAmount.text);
    internal int receiverBalanceAmount => this.textBoxReceiverBalanceAmount.text.Length == 0 ? 0 : Int32.Parse(this.textBoxReceiverBalanceAmount.text);

    internal MonopolyNode SenderNode
    {
        get => this.senderNode;
        set
        {
            this.senderNode = value;
            this.imageSender.gameObject.SetActive(value != null);
            this.imageSender.sprite = this.senderNode?.NodeSprite;
        }
    }

    internal MonopolyNode ReceiverNode
    {
        get => this.receiverNode;
        set
        {
            this.receiverNode = value;
            this.imageReceiver.gameObject.SetActive(value != null);
            this.imageReceiver.sprite = this.receiverNode?.NodeSprite;
        }
    }

    internal PawnController Sender
    {
        get => this.sender;
        set
        {
            this.sender = value;
            this.labelSenderNickname.text = this.sender.Nickname;
        }
    }

    internal PawnController Receiver
    {
        get => this.receiver;
        set
        {
            this.receiver = value;
            this.labelReceiverNickname.text = this.receiver.Nickname;
        }
    }

    internal TradeCredentials Credentials
    {
        get
        {
            int senderNetworkIndex = this.Sender.NetworkIndex;
            int receiverNetworkIndex = this.Receiver.NetworkIndex;
            int clampedSenderBalanceAmount = this.senderBalanceAmount;
            int clampedReceiverBalanceAmount = this.receiverBalanceAmount;
            int senderNodeIndex = this.SenderNode == null ? TradeCredentials.PLACEHOLDER : MonopolyBoard.Instance.GetIndexOfNode(this.SenderNode);
            int receiverNodeIndex = this.ReceiverNode == null ? TradeCredentials.PLACEHOLDER : MonopolyBoard.Instance.GetIndexOfNode(this.ReceiverNode);

            if (this.senderBalanceAmount > GameManager.Instance.GetPawnController(senderNetworkIndex).Balance.Value)
                clampedSenderBalanceAmount = GameManager.Instance.GetPawnController(senderNetworkIndex).Balance.Value;
            else if (this.senderBalanceAmount < 0)
                clampedSenderBalanceAmount = 0;

            if (this.receiverBalanceAmount > GameManager.Instance.GetPawnController(receiverNetworkIndex).Balance.Value)
                clampedReceiverBalanceAmount = GameManager.Instance.GetPawnController(receiverNetworkIndex).Balance.Value;
            else if (this.receiverBalanceAmount < 0)
                clampedReceiverBalanceAmount = 0;

            TradeCredentials tradeCredentials = new TradeCredentials()
            {
                SenderNetworkIndex = senderNetworkIndex,
                ReceiverNetworkIndex = receiverNetworkIndex,
                SenderNodeIndex = senderNodeIndex,
                ReceiverNodeIndex = receiverNodeIndex,
                SenderBalanceAmount = clampedSenderBalanceAmount,
                ReceiverBalanceAmount = clampedReceiverBalanceAmount,
            };

            return tradeCredentials;
        }
    }

    private void Awake()
    {
        if (PanelSendTradeUI.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        PanelSendTradeUI.Instance = this;
    }

    private void OnEnable()
    {
        this.buttonSendOffer.onClick.AddListener(this.OnButtonSendOfferClicked);
        this.buttonCancelOffer.onClick.AddListener(this.OnButtonCancelOfferClicked);
    }

    private void OnDisable()
    {
        this.buttonSendOffer.onClick.RemoveListener(this.OnButtonSendOfferClicked);
        this.buttonCancelOffer.onClick.RemoveListener(this.OnButtonCancelOfferClicked);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == this.imageSender.gameObject)
            this.SenderNode = null;
        else if (eventData.pointerCurrentRaycast.gameObject == this.imageReceiver.gameObject)
            this.ReceiverNode = null;
    }

    public void Show(Action actionCallback)
    {
        this.callback = actionCallback;

        this.imageSender.gameObject.SetActive(false);
        this.imageReceiver.gameObject.SetActive(false);
        this.textBoxSenderBalanceAmount.text = String.Empty;
        this.textBoxReceiverBalanceAmount.text = String.Empty;

        this.panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.imageSender.sprite = null;
        this.imageReceiver.sprite = null;
        this.labelSenderNickname.text = String.Empty;
        this.labelReceiverNickname.text = String.Empty;
        this.textBoxSenderBalanceAmount.text = String.Empty;
        this.textBoxReceiverBalanceAmount.text = String.Empty;

        this.callback = null;
        this.panel.gameObject.SetActive(false);
    }

    private void OnButtonSendOfferClicked()
    {
        this.PanelDialogResult = PanelSendTradeUI.DialogResult.Offer;
        this.callback?.Invoke();
    }

    private void OnButtonCancelOfferClicked()
    {
        this.PanelDialogResult = PanelSendTradeUI.DialogResult.Cancel;
        this.callback?.Invoke();
        this.Hide();
    }
}
