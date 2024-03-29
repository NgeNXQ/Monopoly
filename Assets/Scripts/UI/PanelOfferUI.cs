using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelOfferUI : MonoBehaviour, IActionControlUI
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
    [SerializeField] private Button buttonAccept;

    [Space]
    [SerializeField] private Button buttonDecline;

    #endregion

    #endregion

    public enum DialogResult : byte
    {
        Accepted,
        Declined
    }

    private Action callback;

    public static PanelOfferUI Instance { get; private set; }

    public string PriceText 
    {
        set => this.textPrice.text = value;
    }

    public Sprite PictureSprite 
    { 
        set => this.imagePicture.sprite = value; 
    }

    public Color MonopolyTypeColor 
    { 
        set => this.imageMonopolyType.color = value;
    }

    public DialogResult OfferDialogResult { get; private set; }

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
        this.buttonAccept.onClick.AddListener(this.HandleButtonAcceptClicked);
        this.buttonDecline.onClick.AddListener(this.HandleButtonDeclineClicked);
    }

    private void OnDisable()
    {
        this.buttonAccept.onClick.RemoveListener(this.HandleButtonAcceptClicked);
        this.buttonDecline.onClick.RemoveListener(this.HandleButtonDeclineClicked);
    }

    public void Show(Action actionCallback = default)
    {
        this.callback = actionCallback;

        this.panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.callback = null;

        this.panel.gameObject.SetActive(false);
    }

    private void HandleButtonAcceptClicked()
    {
        this.OfferDialogResult = PanelOfferUI.DialogResult.Accepted;

        this.callback?.Invoke();
    }

    private void HandleButtonDeclineClicked()
    {
        this.OfferDialogResult = PanelOfferUI.DialogResult.Declined;

        this.callback?.Invoke();
    }
}
