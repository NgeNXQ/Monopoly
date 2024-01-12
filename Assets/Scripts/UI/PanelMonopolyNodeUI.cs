using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelMonopolyNodeUI : MonoBehaviour, IActionControlUI
{
    #region Setup

    #region Visuals

    [Header("Visuals")]

    [Space]
    [SerializeField] private RectTransform panel;
    
    [Space]
    [SerializeField] private TMP_Text textPrice;

    [Space]
    [SerializeField] private Image imagePicture;

    [Space]
    [SerializeField] private Image imageMonopolyType;

    #endregion

    #region Controls

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField] private Button buttonUpgrade;

    [Space]
    [SerializeField] private Button buttonDowngrade;

    #endregion

    #endregion

    public enum DialogResult : byte
    {
        Upgrade,
        Downgrade
    }

    private Action callback;

    public static PanelMonopolyNodeUI Instance { get; private set; }

    public DialogResult MonopolyNodeDialogResult { get; private set; }

    public string PriceText 
    {
        set => this.textPrice.text = value;
    }

    public Color MonopolyColor 
    {
        set => this.imageMonopolyType.color = value;
    }

    public Sprite PictureSprite 
    {
        set => this.imagePicture.sprite = value;
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");
        }

        Instance = this;
    }

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

    public void Show(Action actionCallback = null)
    {
        this.panel.gameObject.SetActive(true);

        this.callback = actionCallback;
    }

    public void Hide()
    {
        this.panel.gameObject.SetActive(false);
    }

    private void HandleButtonUpgradeClicked()
    {
        this.MonopolyNodeDialogResult = PanelMonopolyNodeUI.DialogResult.Upgrade;

        this.callback?.Invoke();
    }

    private void HandleButtonDowngradeClicked()
    {
        this.MonopolyNodeDialogResult = PanelMonopolyNodeUI.DialogResult.Downgrade;

        this.callback?.Invoke();
    }
}
