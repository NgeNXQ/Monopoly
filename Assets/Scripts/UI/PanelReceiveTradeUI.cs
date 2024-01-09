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
    [SerializeField] private Image imageThis;

    [Space]
    [SerializeField] private Image imageOther;

    [Space]
    [SerializeField] private TMP_Text textThisOffer;

    [Space]
    [SerializeField] private TMP_Text textOtherOffer;

    [Space]
    [SerializeField] private TMP_Text textThisPlayerNickname;

    [Space]
    [SerializeField] private TMP_Text textOtherPlayerNickname;

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

    private int thisOffer;

    private int otherOffer;

    public static PanelReceiveTradeUI Instance { get; private set; }

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
            return this.thisOffer;
        }
        set
        {
            this.thisOffer = value;
            this.textThisOffer.text = value.ToString();
        }
    }

    public int OtherOffer 
    {
        get
        {
            return this.otherOffer;
        }
        set
        {
            this.otherOffer = value;
            this.textOtherOffer.text = value.ToString();
        }
    }

    public int ThisNodeIndex { get; set; }

    public int OtherNodeIndex { get; set; }

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

        this.thisOffer = 0;
        this.OtherOffer = 0;

        this.ThisNodeIndex = -1;
        this.OtherNodeIndex = -1;

        this.imageThis.sprite = null;
        this.imageOther.sprite = null;

        this.imageThis.gameObject.SetActive(false);
        this.imageOther.gameObject.SetActive(false);

        this.textThisPlayerNickname.text = String.Empty;
        this.textOtherPlayerNickname.text = String.Empty;
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
