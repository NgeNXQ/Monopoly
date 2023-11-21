using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIManager : MonoBehaviour
{
    [Space]
    [Header("Button roll dices")]
    [Space]

    [SerializeField] private Button buttonRoll;

    [Space]
    [Header("Panel OK")]
    [Space]

    [SerializeField] private RectTransform panelOk;

    [SerializeField] private Image imagePanelOk;

    [SerializeField] private TMP_Text textPanelOk;

    [SerializeField] private Button buttonOkPanelOk;

    [Space]
    [Header("Panel Fee")]
    [Space]

    [SerializeField] private RectTransform panelFee;

    [SerializeField] private Image imagePanelFee;

    [SerializeField] private TMP_Text textPanelFee;

    [SerializeField] private Button buttonPayPanelFee;

    [Space]
    [Header("Panel Offer")]
    [Space]

    [SerializeField] private RectTransform panelOffer;

    [SerializeField] private Image imagepanelOffer;

    [SerializeField] private TMP_Text textpanelOffer;

    [SerializeField] private Button buttonBuyPanelOffer;

    [SerializeField] private Button buttonCancelPanelOffer;

    //[SerializeField] private Canvas panelTradeOffer;

    //[SerializeField] private Transform panelPlayers;

    //[SerializeField] private Transform panelPlayer;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        //this.buttonRoll.onClick.AddListener(GameManager.Instance.RollDices);
        //this.buttonOkPanelOk.onClick.AddListener(GameManager.Instance.SwitchPlayer);
        //this.buttonPayPanelFee.onClick.AddListener(GameManager.Instance.CollectFee);
    }

    public void ShowPanelOk(Sprite sprite, string description)
    {
        this.panelOk.gameObject.SetActive(true);
        this.textPanelOk.text = description;
        this.imagePanelOk.sprite = sprite;
    }

    public void HidePanelOk() => this.panelOk.gameObject.SetActive(false);

    public void ShowPanelFee(Sprite sprite, string description)
    {
        this.panelFee.gameObject.SetActive(true);
        this.textPanelFee.text = description;
        this.imagePanelFee.sprite = sprite;
    }

    public void HidePanelFee() => this.panelFee.gameObject.SetActive(false);

    public void ShowPanelOffer(Sprite sprite, string description)
    {
        this.panelOffer.gameObject.SetActive(true);
        this.textpanelOffer.text = description;
        this.imagepanelOffer.sprite = sprite;
    }

    public void HidePanelOffer() => this.panelOffer.gameObject.SetActive(false);

    public void ShowButtonRoll() => this.buttonRoll.gameObject.SetActive(true);

    public void HideButtonRolls() => this.buttonRoll.gameObject.SetActive(false);

    private void OnDestroy()
    {
        //this.buttonRoll.onClick.RemoveListener(GameManager.Instance.RollDices);
        //this.buttonOkPanelOk.onClick.RemoveListener(GameManager.Instance.SwitchPlayer);
        //this.buttonPayPanelFee.onClick.RemoveListener(GameManager.Instance.CollectFee);
    }
}
