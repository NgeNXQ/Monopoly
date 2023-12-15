using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelOfferUI : MonoBehaviour, IControlUI, IButtonHandlerUI
{
    #region Visuals

    [Space]
    [Header("Visuals")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panel;

    [Space]
    [SerializeField] private Image imagePicture;

    [Space]
    [SerializeField] private Image imageMonopolyType;

    #endregion

    #region Controls

    [Space]
    [Header("Controls")]
    [Space]

    [Space]
    [SerializeField] private Button buttonAccept;

    [Space]
    [SerializeField] private Button buttonDecline;

    #endregion

    public static PanelOfferUI Instance { get; private set; }

    public event IButtonHandlerUI.ButtonClickedEventHandler ButtonAcceptClicked;

    public event IButtonHandlerUI.ButtonClickedEventHandler ButtonDeclineClicked;

    public Sprite PictureSprite { set => this.imagePicture.sprite = value; }

    public Color MonopolyTypeColor { set => this.imageMonopolyType.color = value; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

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

    public void Show() => this.panel.gameObject.SetActive(true);

    public void Hide() => this.panel.gameObject.SetActive(false);

    private void HandleButtonAcceptClicked() => this.ButtonAcceptClicked?.Invoke();

    private void HandleButtonDeclineClicked() => this.ButtonDeclineClicked?.Invoke();
}
