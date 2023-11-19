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

    [SerializeField] private UIMonopolyNodeCard panelOk;

    [SerializeField] private Button buttonOkPanelOk;

    [Space]
    [Header("Panel Fee")]
    [Space]

    [SerializeField] private UIMonopolyNodeCard panelFee;

    [SerializeField] private Button buttonPayPanelFee;

    [Space]
    [Header("Panel Offer")]
    [Space]

    [SerializeField] private UIMonopolyNodeCard panelOffer;

    [SerializeField] private Button buttonBuyPanelOffer;

    [SerializeField] private Button buttonCancelPanelOffer;

    //[SerializeField] private Canvas panelTradeOffer;

    //[SerializeField] private Transform panelPlayers;

    //[SerializeField] private Transform panelPlayer;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        this.buttonRoll.onClick.AddListener(GameManager.Instance.RollDices);
        this.buttonOkPanelOk.onClick.AddListener(GameManager.Instance.SwitchPlayer);
        this.buttonPayPanelFee.onClick.AddListener(GameManager.Instance.CollectFee);
    }

    public void ShowPanelOk(Sprite sprite, string description)
    {
        this.panelOk.gameObject.SetActive(true);
        this.panelOk.UpdateUI(sprite, description);
    }

    public void HidePanelOk() => this.panelOk.gameObject.SetActive(false);

    public void ShowPanelFee(Sprite sprite, string description)
    {
        this.panelFee.gameObject.SetActive(true);
        this.panelOk.UpdateUI(sprite, description);
    }

    public void HidePanelFee() => this.panelFee.gameObject.SetActive(false);

    public void ShowButtonRoll() => this.buttonRoll.gameObject.SetActive(true);

    public void HideButtonRolls() => this.buttonRoll.gameObject.SetActive(false);

    private void OnDestroy()
    {
        this.buttonRoll.onClick.RemoveListener(GameManager.Instance.RollDices);
        this.buttonOkPanelOk.onClick.RemoveListener(GameManager.Instance.SwitchPlayer);
        this.buttonPayPanelFee.onClick.RemoveListener(GameManager.Instance.CollectFee);
    }
}
