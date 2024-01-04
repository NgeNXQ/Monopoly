using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelPaymentUI : MonoBehaviour, IActionControlUI
{
    #region Setup

    #region Visuals

    [Header("Visuals")]

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
    [SerializeField] private Button buttonConfirm;

    #endregion

    #endregion

    public enum DialogResult : byte
    {
        Confirmed
    }

    private Action actionCallback;

    public static PanelPaymentUI Instance { get; private set; }
    
    public Sprite PictureSprite 
    { 
        set => this.imagePicture.sprite = value; 
    }

    public string DescriptionText 
    { 
        set => this.textDescription.text = value; 
    }

    public DialogResult PaymentDialogResult { get; private set; }

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
        this.buttonConfirm.onClick.AddListener(this.HandleButtonConfirmClicked);
    }

    private void OnDisable()
    {
        this.buttonConfirm.onClick.RemoveListener(this.HandleButtonConfirmClicked);
    }
    
    public void Show(Action actionCallback = null)
    {
        this.actionCallback = actionCallback;

        this.panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.panel.gameObject.SetActive(false);
    }

    private void HandleButtonConfirmClicked()
    {
        this.Hide();

        this.PaymentDialogResult = PanelPaymentUI.DialogResult.Confirmed;

        this.actionCallback?.Invoke();
    }
}
