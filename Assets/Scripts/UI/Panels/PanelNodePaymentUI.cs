using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelNodePaymentUI : MonoBehaviour, IActionControlUI
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
    private Image imageMonopoly;

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField] private Button buttonConfirm;

    internal enum DialogResult : byte
    {
        Confirmed
    }

    private Action callback;

    internal static PanelNodePaymentUI Instance { get; private set; }

    internal string PriceText
    {
        set => this.textPrice.text = value;
    }

    internal Color MonopolyColor
    {
        set => this.imageMonopoly.color = value;
    }

    internal Sprite PictureSprite
    {
        set => this.imagePicture.sprite = value;
    }

    internal DialogResult PanelDialogResult { get; private set; }

    private void Awake()
    {
        if (PanelNodePaymentUI.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        PanelNodePaymentUI.Instance = this;
    }

    private void OnEnable()
    {
        this.buttonConfirm.onClick.AddListener(this.OnButtonConfirmClicked);
    }

    private void OnDisable()
    {
        this.buttonConfirm.onClick.RemoveListener(this.OnButtonConfirmClicked);
    }

    public void Show(Action actionCallback = null)
    {
        this.callback = actionCallback;
        this.panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.callback = null;
        this.panel.gameObject.SetActive(false);
    }

    private void OnButtonConfirmClicked()
    {
        this.PanelDialogResult = PanelNodePaymentUI.DialogResult.Confirmed;
        this.callback?.Invoke();
    }
}
