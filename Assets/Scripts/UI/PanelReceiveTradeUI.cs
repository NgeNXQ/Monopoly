using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class PanelReceiveTradeUI : MonoBehaviour, IActionControlUI
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
    [SerializeField] private TMP_Text textSenderOffer;

    [Space]
    [SerializeField] private TMP_Text textReceiverOffer;

    [Space]
    [SerializeField] private TMP_Text textSenderNickname;

    [Space]
    [SerializeField] private TMP_Text textReceiverNickname;

    #endregion

    #region Controls

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField] private Button buttonAccept;

    [Space]
    [SerializeField] private Button buttonDecline;

    #endregion

    #endregion

    public enum DialogResult : byte
    {
        Accept,
        Decline
    }

    private Action callback;

    private TradeCredentials credentials;

    public static PanelReceiveTradeUI Instance { get; private set; }

    public TradeCredentials Credentials 
    { 
        get
        {
            return this.credentials;
        }
        set
        {
            if (GameManager.Instance.GetPlayerById(value.SenderId) == null)
            {
                return;
            }
            else
            {
                this.textSenderNickname.text = GameManager.Instance.GetPlayerById(value.SenderId).Nickname;
                this.textReceiverNickname.text = GameManager.Instance.GetPlayerById(value.ReceiverId).Nickname;
            }

            if (value.SenderNodeIndex != -1)
            {
                this.imageSender.gameObject.SetActive(true);
                this.imageSender.sprite = MonopolyBoard.Instance[value.SenderNodeIndex].NodeSprite;
            }

            if (value.ReceiverNodeIndex != -1) 
            {
                this.imageReceiver.gameObject.SetActive(true);
                this.imageReceiver.sprite = MonopolyBoard.Instance[value.ReceiverNodeIndex].NodeSprite; ;
            }

            this.textSenderOffer.text = value.SenderOffer.ToString();
            this.textReceiverOffer.text = value.ReceiverOffer.ToString();
        }
    }

    public DialogResult ReceiveTradeDialogResult { get; private set; }

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
        this.buttonAccept.onClick.AddListener(this.HandleButtonAcceptClicked);
        this.buttonDecline.onClick.AddListener(this.HandleButtonDeclineClicked);
    }

    private void OnDisable()
    {
        this.buttonAccept.onClick.RemoveListener(this.HandleButtonAcceptClicked);
        this.buttonDecline.onClick.RemoveListener(this.HandleButtonDeclineClicked);
    }

    public void Show(Action actionCallback)
    {
        this.callback = actionCallback;

        this.panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.callback = null;

        this.panel.gameObject.SetActive(false);
    }

    private void HandleButtonAcceptClicked()
    {
        this.ReceiveTradeDialogResult = PanelReceiveTradeUI.DialogResult.Accept;

        this.callback?.Invoke();
    }

    private void HandleButtonDeclineClicked()
    {
        this.ReceiveTradeDialogResult = PanelReceiveTradeUI.DialogResult.Decline;

        this.callback?.Invoke();
    }
}
