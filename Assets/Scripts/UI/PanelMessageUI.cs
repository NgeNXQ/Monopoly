using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PanelMessageUI : MonoBehaviour, IControlUI
{
    #region Setup (Visuals)

    [Space]
    [Header("Visuals")]
    [Space]

    [Space]
    [SerializeField] private TMP_Text textMessage;

    [Space]
    [SerializeField] private Button buttonConfirm;

    #endregion

    public static PanelMessageUI Instance { get; private set; }

    public event UIManager.ButtonClickHandler OnButtonConfirmClicked;

    public string MessageText { set => this.textMessage.text = value; }

    private void Awake() => Instance = this;

    private void Start() => this.Hide();

    private void OnEnable() => this.buttonConfirm.onClick.AddListener(this.HandleButtonConfirmClicked);

    private void OnDisable() => this.buttonConfirm.onClick.RemoveListener(this.HandleButtonConfirmClicked);

    public void Show() => this.gameObject.SetActive(true);

    public void Hide() => this.gameObject.SetActive(false);

    private void HandleButtonConfirmClicked() => this.OnButtonConfirmClicked?.Invoke();
}
