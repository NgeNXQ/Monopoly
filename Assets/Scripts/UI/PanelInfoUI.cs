using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelInfoUI : MonoBehaviour, IControlUI
{
    #region Setup (Visuals)

    [Space]
    [Header("Visuals")]
    [Space]

    [Space]
    [SerializeField] private Image imageLogo;

    [Space]
    [SerializeField] private TMP_Text textDescription;

    [Space]
    [SerializeField] private Button buttonConfirm;

    #endregion

    public static PanelInfoUI Instance { get; private set; }

    public event UIManager.ButtonClickHandler OnButtonConfirmClicked;

    public Sprite LogoSprite { set => this.imageLogo.sprite = value; }

    public string DescriptionText { set => this.textDescription.text = value; }

    private void Awake() => Instance = this;

    private void Start() => this.Hide();

    private void OnEnable() => this.buttonConfirm.onClick.AddListener(this.HandleButtonConfirmClicked);

    private void OnDisable() => this.buttonConfirm.onClick.RemoveListener(this.HandleButtonConfirmClicked);

    public void Show() => this.gameObject.SetActive(true);

    public void Hide() => this.gameObject.SetActive(false);

    private void HandleButtonConfirmClicked() => this.OnButtonConfirmClicked?.Invoke();
}
