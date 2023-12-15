using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

internal sealed class UIManagerGameLobby : NetworkBehaviour
{
    #region Controls

    [Space]
    [Header("Controls")]
    [Space]

    [Space]
    [SerializeField] private Button buttonCancel;

    [Space]
    [SerializeField] private TMP_Text labelJoinCode;

    #endregion

    public static UIManagerGameLobby LocalInstance { get; private set; }

    public PanelMessageBoxUI PanelMessageBox { get => PanelMessageBoxUI.Instance; }

    public string LabelJoinCode 
    { 
        get => this.labelJoinCode.text; 
        set => this.labelJoinCode.text = value; 
    }

    private void Awake()
    {
        if (LocalInstance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        LocalInstance = this;
    }

    private void OnEnable()
    {
        this.buttonCancel.onClick.AddListener(this.HandleButtonCancelClicked);
    }

    private void OnDisable()
    {
        this.buttonCancel.onClick.RemoveListener(this.HandleButtonCancelClicked);
    }

    private void HandleButtonCancelClicked()
    {
        GameCoordinator.LocalInstance.LoadScene(GameCoordinator.Scene.MainMenu);
    }
}
