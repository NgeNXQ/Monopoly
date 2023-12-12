using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelPaymentUI : MonoBehaviour, IControlUI
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
    [SerializeField] private TMP_Text textDescription;

    #endregion

    #region Controls

    [Space]
    [Header("Controls")]
    [Space]

    [Space]
    [SerializeField] private Button buttonConfirm;

    #endregion

    public static PanelPaymentUI Instance { get; private set; }

    public event UIManagerMonopoly.ButtonClickHandler OnButtonConfirmClicked;

    public Sprite PictureSprite { set => this.imagePicture.sprite = value; }

    public string DescriptionText { set => this.textDescription.text = value; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void OnEnable() => this.buttonConfirm.onClick.AddListener(this.HandleButtonConfirmClicked);

    private void OnDisable() => this.buttonConfirm.onClick.RemoveListener(this.HandleButtonConfirmClicked);

    public void Show() => this.panel.gameObject.SetActive(true);

    public void Hide() => this.panel.gameObject.SetActive(false);

    private void HandleButtonConfirmClicked() => this.OnButtonConfirmClicked?.Invoke();
}
