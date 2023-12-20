using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelMonopolyNodeUI : MonoBehaviour //, IControlUI, IButtonHandlerUI
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
    [SerializeField] private Button buttonUpgrade;

    [Space]
    [SerializeField] private Button buttonDowngrade;

    #endregion

    public static PanelMonopolyNodeUI Instance { get; private set; }

    //public event IButtonHandlerUI.ButtonClickedEventHandler ButtonUpgradeClicked;

    //public event IButtonHandlerUI.ButtonClickedEventHandler ButtonDowngradeClicked;

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
        //this.buttonUpgrade.onClick.AddListener(this.HandleButtonUpgradeClicked);
        //this.buttonDowngrade.onClick.AddListener(this.HandleButtonDowngradeClicked);
    }

    private void OnDisable()
    {
        //this.buttonUpgrade.onClick.RemoveListener(this.HandleButtonUpgradeClicked);
        //this.buttonDowngrade.onClick.RemoveListener(this.HandleButtonDowngradeClicked);
    }

    public void Show() => this.panel.gameObject.SetActive(true);

    public void Hide() => this.panel.gameObject.SetActive(false);

    //private void HandleButtonUpgradeClicked() => this.ButtonUpgradeClicked?.Invoke();

    //private void HandleButtonDowngradeClicked() => this.ButtonDowngradeClicked?.Invoke();
}
