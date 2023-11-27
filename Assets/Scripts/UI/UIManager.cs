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
    [Header("Panel OK")]
    [Space]

    [SerializeField] private RectTransform panelInformation;

    [SerializeField] private Image imagePanelInformation;

    [SerializeField] private TMP_Text textPanelInformation;

    [SerializeField] private Button buttonPanelInformation;

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

    public static UIManager Instance { get; private set; }

    public enum UIControl : byte
    {
        ButtonRollDices,
        PanelInformation,
    }

    private void Awake()
    {
        Instance = this;

        this.buttonRoll.onClick.AddListener(this.HandleButtonRollDicesClicked);
    }

    public void ShowControl(UIControl control)
    {
        switch (control)
        {
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

    private void HandleButtonRollDicesClicked() => this.OnButtonRollDicesClicked?.Invoke();

    //public void ButtonRollDicesPressed() => OnButtonRollDicesPressed?.Invoke();

    //public void ShowPanelOk(Sprite sprite, string description)
    //{
    //    this.panelOk.gameObject.SetActive(true);
    //    this.textPanelOk.text = description;
    //    this.imagePanelOk.sprite = sprite;
    //}

    //public void HidePanelOk() => this.panelOk.gameObject.SetActive(false);

    //public void ShowPanelFee(Sprite sprite, string description)
    //{
    //    this.panelFee.gameObject.SetActive(true);
    //    this.textPanelFee.text = description;
    //    this.imagePanelFee.sprite = sprite;
    //}

    //public void HidePanelFee() => this.panelFee.gameObject.SetActive(false);

    //public void ShowPanelOffer(Sprite sprite, string description)
    //{
    //    this.panelOffer.gameObject.SetActive(true);
    //    this.textpanelOffer.text = description;
    //    this.imagepanelOffer.sprite = sprite;
    //}

    //public void HidePanelOffer() => this.panelOffer.gameObject.SetActive(false);

    //public void ShowButtonRoll() => this.buttonRoll.gameObject.SetActive(true);

    //public void HideButtonRolls() => this.buttonRoll.gameObject.SetActive(false);

    private void OnDestroy()
    {
        this.buttonRoll.onClick.RemoveListener(this.HandleButtonRollDicesClicked);
    }
}
