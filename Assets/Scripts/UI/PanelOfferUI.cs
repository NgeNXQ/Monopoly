using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelOfferUI : MonoBehaviour, IControlUI
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
    [SerializeField] private Button buttonAccept;

    [Space]
    [SerializeField] private Button buttonDecline;

    #endregion

    public static PanelOfferUI Instance { get; private set; }

    public event UIManager.ButtonClickHandler OnButtonAcceptClicked;

    public event UIManager.ButtonClickHandler OnButtonDeclineClicked;

    public string PriceText { set => this.textPrice.text = value; }

    public Sprite LogoSprite { set => this.imageLogo.sprite = value; }

    private void Awake() => Instance = this;

    private void Start() => this.Hide();

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

    public void Show() => this.gameObject.SetActive(true);

    public void Hide() => this.gameObject.SetActive(false);

    private void HandleButtonAcceptClicked() => this.OnButtonAcceptClicked?.Invoke();

    private void HandleButtonDeclineClicked() => this.OnButtonDeclineClicked?.Invoke();
}
