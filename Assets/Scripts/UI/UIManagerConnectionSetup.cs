using UnityEngine;

internal sealed class UIManagerConnectionSetup : MonoBehaviour
{
    #region Messages

    [Space]
    [Header("Messages")]
    [Space]

    [Space]
    [SerializeField] private string messageInitializingGameCoordinator;

    [Space]
    [SerializeField] private string messageInitializationGameCoordinatorFailed;

    #endregion

    public static UIManagerConnectionSetup Instance { get; private set; }

    public PanelMessageBoxUI PanelMessageBox { get => PanelMessageBoxUI.Instance; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
        this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
        this.PanelMessageBox.MessageText = this.messageInitializingGameCoordinator;
        this.PanelMessageBox.Show();
    }

    private void OnEnable()
    {
        GameCoordinator.Instance.AuthenticationFailed += this.HandleAuthenticationFailed;
        this.PanelMessageBox.ButtonConfirmPanelOKClicked += this.HandleClosePanelMessageBoxClicked;
    }

    private void OnDisable()
    {
        GameCoordinator.Instance.AuthenticationFailed -= this.HandleAuthenticationFailed;
        this.PanelMessageBox.ButtonConfirmPanelOKClicked -= this.HandleClosePanelMessageBoxClicked;
    }

    private void OnDestroy()
    {
        this.PanelMessageBox.Hide();
    }

    private void HandleAuthenticationFailed()
    {
        this.PanelMessageBox.Hide();

        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
        this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
        this.PanelMessageBox.MessageText = this.messageInitializationGameCoordinatorFailed;
        this.PanelMessageBox.Show();
    }

    private void HandleClosePanelMessageBoxClicked()
    {
        this.PanelMessageBox.Hide();

        GameCoordinator.Instance.LoadScene(GameCoordinator.MonopolyScene.MainMenu);
    }
}
