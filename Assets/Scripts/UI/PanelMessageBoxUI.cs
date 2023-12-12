using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelMessageBoxUI : MonoBehaviour, IControlUI
{
    #region Visuals

    [Space]
    [Header("Visuals")]
    [Space]

    [Space]
    [SerializeField] private RectTransform panel;

    [Space]
    [SerializeField] private Image imageIcon;

    [Space]
    [SerializeField] private Sprite spriteTrophy;

    [Space]
    [SerializeField] private Sprite spriteWarning;

    [Space]
    [SerializeField] private Sprite spriteExclamation;

    [Space]
    [SerializeField] private TMP_Text textMessage;

    #endregion

    #region Controls

    [Space]
    [Header("Controls")]
    [Space]

    [Space]
    [SerializeField] private Button buttonConfirm;

    #endregion

    public enum Type : byte
    {
        Trophy,
        Warning,
        Exclamation,
    }

    public static PanelMessageBoxUI Instance { get; private set; }

    public event UIManagerMonopoly.ButtonClickHandler OnButtonConfirmClicked;

    public Type MessageType { get; set; }

    public string MessageText { set => this.textMessage.text = value; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void OnEnable() => this.buttonConfirm.onClick.AddListener(this.HandleButtonConfirmClicked);

    private void OnDisable() => this.buttonConfirm.onClick.RemoveListener(this.HandleButtonConfirmClicked);

    public void Show()
    {
        switch (this.MessageType)
        {
            case Type.Trophy:
                this.imageIcon.sprite = this.spriteTrophy;
                break;
            case Type.Warning:
                this.imageIcon.sprite = this.spriteWarning;
                break;
            case Type.Exclamation:
                this.imageIcon.sprite = this.spriteExclamation;
                break;
        }

        this.panel.gameObject.SetActive(true);
    }

    public void Hide() => this.panel.gameObject.SetActive(false);

    private void HandleButtonConfirmClicked() => this.OnButtonConfirmClicked?.Invoke();
}
