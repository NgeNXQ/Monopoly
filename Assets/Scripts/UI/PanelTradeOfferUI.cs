using TMPro;
using System;
using UnityEngine;
using Unity.Netcode;
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
    [SerializeField] private Image imageSender;

    [Space]
    [SerializeField] private Image imageReceiver;

    [Space]
    [SerializeField] private TMP_Text textSenderNickname;

    [Space]
    [SerializeField] private TMP_Text textReceiverNickname;

    #endregion

    #region Controls

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField] private Button buttonSendOffer;

    [Space]
    [SerializeField] private Button buttonCancelOffer;

    [Space]
    [SerializeField] private TMP_InputField textBoxSenderOffer;

    [Space]
    [SerializeField] private TMP_InputField textBoxReceiverOffer;

    #endregion

    #endregion

    public enum DialogResult : byte
    {
        Offer,
        Cancel
    }

    private Action callback;

    private int senderOffer;

    private int receiverOffer;

    private ulong senderId 
    {
        get => NetworkManager.Singleton.LocalClientId;
    }

    private ulong receiverId 
    {
        get => GameManager.Instance.GetPlayerById(NetworkManager.Singleton.LocalClientId).PlayerTradingWith.OwnerClientId;
    }

    public static PanelTradeOfferUI Instance { get; private set; }

    public int SenderOffer 
    {
        get
        {
            this.senderOffer = this.textBoxSenderOffer.text.Length == 0 ? 0 : Int32.Parse(this.textBoxSenderOffer.text);
            return this.senderOffer;
        }
        set
        {
            this.senderOffer = value;
        }
    }

    public int ReceiverOffer 
    {
        get
        {
            this.receiverOffer = this.textBoxReceiverOffer.text.Length == 0 ? 0 : Int32.Parse(this.textBoxReceiverOffer.text);
            return this.receiverOffer;
        }
        set
        {
            this.receiverOffer = value;
        }
    }

    public Sprite SenderSprite 
    {
        set
        {
            this.imageSender.sprite = value;
            this.imageSender.gameObject.SetActive(true);
        }
    }

    public Sprite ReceiverSprite 
    {
        set
        {
            this.imageReceiver.sprite = value;
            this.imageReceiver.gameObject.SetActive(true);
        }
    }

    public string SenderNicknameText 
    {
        set => this.textSenderNickname.text = value;
    }

    public string ReceiverNicknameText 
    {
        set => this.textReceiverNickname.text = value;
    }

    public int SenderNodeIndex { get; set; }

    public int ReceiverNodeIndex { get; set; }

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
        this.buttonSendOffer.onClick.AddListener(this.HandleButtonSendOfferClicked);
        this.buttonCancelOffer.onClick.AddListener(this.HandleButtonCancelOfferClicked);
    }

    private void OnDisable()
    {
        this.buttonSendOffer.onClick.RemoveListener(this.HandleButtonSendOfferClicked);
        this.buttonCancelOffer.onClick.RemoveListener(this.HandleButtonCancelOfferClicked);
    }

    public void Show(Action actionCallback)
    {
        this.SenderNodeIndex = -1;
        this.ReceiverNodeIndex = -1;

        this.imageSender.sprite = null;
        this.imageReceiver.sprite = null;

        this.textBoxSenderOffer.text = String.Empty;
        this.textBoxReceiverOffer.text = String.Empty;

        this.imageSender.gameObject.SetActive(false);
        this.imageReceiver.gameObject.SetActive(false);

        this.callback = actionCallback;

        this.panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.callback = null;

        this.textSenderNickname.text = String.Empty;
        this.textReceiverNickname.text = String.Empty;

        this.panel.gameObject.SetActive(false);
    }

    public TradeCredentials GetTradeCredentials()
    {
        if (this.SenderOffer > GameManager.Instance.GetPlayerById(this.senderId).Balance.Value)
        {
            this.senderOffer = GameManager.Instance.GetPlayerById(this.senderId).Balance.Value;
        }
        else if (this.SenderOffer < 0)
        {
            this.senderOffer = 0;
        }

        if (this.ReceiverOffer > GameManager.Instance.GetPlayerById(this.receiverId).Balance.Value)
        {
            this.receiverOffer = GameManager.Instance.GetPlayerById(this.receiverId).Balance.Value;
        }
        else if (this.ReceiverOffer < 0)
        {
            this.receiverOffer = 0;
        }

        TradeCredentials tradeCredentials = new TradeCredentials()
        {
            SenderNodeIndex = this.SenderNodeIndex,
            ReceiverNodeIndex = this.ReceiverNodeIndex,

            SenderOffer = this.senderOffer,
            ReceiverOffer = this.receiverOffer,

            SenderId = this.senderId,
            ReceiverId = GameManager.Instance.GetPlayerById(this.receiverId).OwnerClientId
        };

        return tradeCredentials;
    }

    private void HandleButtonSendOfferClicked()
    {
        this.TradeOfferDialogResult = PanelTradeOfferUI.DialogResult.Offer;

        this.callback?.Invoke();
    }

    private void HandleButtonCancelOfferClicked()
    {
        this.TradeOfferDialogResult = PanelTradeOfferUI.DialogResult.Cancel;

        this.callback?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == this.imageSender.gameObject)
        {
            this.SenderNodeIndex = -1;
            this.imageSender.gameObject.SetActive(false);
        }
        else if (eventData.pointerCurrentRaycast.gameObject == this.imageReceiver.gameObject)
        {
            this.ReceiverNodeIndex = -1;
            this.imageReceiver.gameObject.SetActive(false);
        }
    }
}
