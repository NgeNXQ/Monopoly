using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelNodeMenuUI : MonoBehaviour, IActionControlUI
{
    [Header("Visuals")]

    [Space]
    [SerializeField]
    private RectTransform panel;

    [Space]
    [SerializeField]
    private TMP_Text textPrice;

    [Space]
    [SerializeField]
    private Image imagePicture;

    [Space]
    [SerializeField]
    private Image imageMonopolyType;

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField]
    private Button buttonUpgrade;

    [Space]
    [SerializeField]
    private Button buttonDowngrade;

    internal enum DialogResult : byte
    {
        Upgrade,
        Downgrade
    }

    internal static PanelNodeMenuUI Instance { get; private set; }

    private Action callback;

    internal DialogResult PanelDialogResult { get; private set; }

    internal string PriceText
    {
        set => this.textPrice.text = value;
    }

    internal Color MonopolyColor
    {
        set => this.imageMonopolyType.color = value;
    }

    internal Sprite PictureSprite
    {
        set => this.imagePicture.sprite = value;
    }

    private void Awake()
    {
        if (PanelNodeMenuUI.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        PanelNodeMenuUI.Instance = this;
    }

    private void OnEnable()
    {
        this.buttonUpgrade.onClick.AddListener(this.OnButtonUpgradeClicked);
        this.buttonDowngrade.onClick.AddListener(this.OnButtonDowngradeClicked);
    }

    private void OnDisable()
    {
        this.buttonUpgrade.onClick.RemoveListener(this.OnButtonUpgradeClicked);
        this.buttonDowngrade.onClick.RemoveListener(this.OnButtonDowngradeClicked);
    }

    public void Show(Action actionCallback = null)
    {
        this.panel.gameObject.SetActive(true);
        this.callback = actionCallback;
    }

    public void Hide()
    {
        this.panel.gameObject.SetActive(false);
    }

    private void OnButtonUpgradeClicked()
    {
        this.PanelDialogResult = PanelNodeMenuUI.DialogResult.Upgrade;
        this.callback?.Invoke();
    }

    private void OnButtonDowngradeClicked()
    {
        this.PanelDialogResult = PanelNodeMenuUI.DialogResult.Downgrade;
        this.callback?.Invoke();
    }
}
