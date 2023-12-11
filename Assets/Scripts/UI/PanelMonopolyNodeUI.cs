using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelMonopolyNodeUI : MonoBehaviour, IControlUI
{
    #region Setup (Visuals)

    [Space]
    [Header("Visuals")]
    [Space]

    [Space]
    [SerializeField] private Image imageLogo;

    [Space]
    [SerializeField] private TMP_Text textPrice;

    [Space]
    [SerializeField] private Button buttonUpgrade;

    [Space]
    [SerializeField] private Button buttonDowngrade;

    #endregion

    public static PanelMonopolyNodeUI Instance { get; private set; }

    public event UIManager.ButtonClickHandler OnButtonUpgradeClicked;

    public event UIManager.ButtonClickHandler OnButtonDowngradeClicked;

    public string PriceText { set => this.textPrice.text = value; }

    public Sprite LogoSprite { set => this.imageLogo.sprite = value; }

    private void Awake() => Instance = this;

    private void Start() => this.Hide();

    private void OnEnable()
    {
        this.buttonUpgrade.onClick.AddListener(this.HandleButtonUpgradeClicked);
        this.buttonDowngrade.onClick.AddListener(this.HandleButtonDowngradeClicked);
    }

    private void OnDisable()
    {
        this.buttonUpgrade.onClick.RemoveListener(this.HandleButtonUpgradeClicked);
        this.buttonDowngrade.onClick.RemoveListener(this.HandleButtonDowngradeClicked);
    }

    public void Show() => this.gameObject.SetActive(true);

    public void Hide() => this.gameObject.SetActive(false);

    private void HandleButtonUpgradeClicked() => this.OnButtonUpgradeClicked?.Invoke();

    private void HandleButtonDowngradeClicked() => this.OnButtonDowngradeClicked?.Invoke();
}
