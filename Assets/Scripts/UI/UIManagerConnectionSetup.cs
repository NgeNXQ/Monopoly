using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

internal sealed class UIManagerConnectionSetup : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]
    [Space]

    #region Messages

    [Space]
    [Header("Messages")]
    [Space]

    [Space]
    [SerializeField] private string messageInitializingGameCoordinator;

    [Space]
    [SerializeField] private string messageInitializationGameCoordinatorFailed;

    #endregion

    #endregion

    public static UIManagerConnectionSetup Instance { get; private set; }

    private PanelMessageBoxUI PanelMessageBox { get => PanelMessageBoxUI.Instance; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        this.PanelMessageBox.Show(PanelMessageBoxUI.Type.None, this.messageInitializingGameCoordinator, PanelMessageBoxUI.Icon.Loading);
    }

    private void OnEnable()
    {
        GameCoordinator.Instance.OnAuthenticationFailed += this.HandleAuthenticationFailed;
    }

    private void OnDisable()
    {
        GameCoordinator.Instance.OnAuthenticationFailed -= this.HandleAuthenticationFailed;
    }

    private void OnDestroy()
    {
        this.PanelMessageBox.Hide();
    }

    #region GameCoordinator Callbacks

    private void HandleAuthenticationFailed()
    {
        this.PanelMessageBox.Hide();

        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
        this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
        this.PanelMessageBox.MessageText = this.messageInitializationGameCoordinatorFailed;
        this.PanelMessageBox.Show(this.InvokeAuthenticationFailedCallback);
    }

    private async void InvokeAuthenticationFailedCallback()
    {
        switch (this.PanelMessageBox.MessageBoxDialogResult)
        {
            case PanelMessageBoxUI.DialogResult.OK:
                {
#if UNITY_EDITOR
                    EditorApplication.ExitPlaymode();
#else
                    Application.Quit();
#endif
                }
                break;
            case PanelMessageBoxUI.DialogResult.Cancel:
                {
                    await GameCoordinator.Instance.LoadScene(GameCoordinator.MonopolyScene.MainMenu);
                }
                break;
        }
    }

    #endregion
}
