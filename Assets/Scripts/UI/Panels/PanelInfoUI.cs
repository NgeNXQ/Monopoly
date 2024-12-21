using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelInfoUI : MonoBehaviour, IActionControlUI
{
    [Header("Visuals")]

    [Space]
    [SerializeField]
    private RectTransform panel;

    [Space]
    [SerializeField]
    private TMP_Text textDescription;

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField]
    private Button buttonConfirm;

    internal enum DialogResult : byte
    {
        Confirmed
    }

    private Action callback;

    internal static PanelInfoUI Instance { get; private set; }

    internal string DescriptionText
    {
        set => this.textDescription.text = value;
    }

    internal DialogResult PanelDialogResult { get; private set; }

    private void Awake()
    {
        if (PanelInfoUI.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        PanelInfoUI.Instance = this;
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
        this.PanelDialogResult = PanelInfoUI.DialogResult.Confirmed;
        this.callback?.Invoke();
        this.Hide();
    }
}
