using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

internal sealed class UIManagerConnectionSetup : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]

    #region Messages

    [Header("Messages")]

    [Space]
    [SerializeField] private string messageInitializingGameCoordinator;

    [Space]
    [SerializeField] private string messageInitializationGameCoordinatorFailed;

    #endregion

    #endregion

    public static UIManagerConnectionSetup Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageInitializingGameCoordinator;
        UIManagerGlobal.Instance.PanelMessageBox.Show(null);
    }

    private void OnEnable()
    {
        GameCoordinator.Instance.OnAuthenticationFailed += this.HandleAuthenticationFailed;
    }

    private void OnDisable()
    {
        GameCoordinator.Instance.OnAuthenticationFailed -= this.HandleAuthenticationFailed;
    }

    #region GameCoordinator Callbacks

    private void HandleAuthenticationFailed()
    {
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = this.messageInitializationGameCoordinatorFailed;
        UIManagerGlobal.Instance.PanelMessageBox.Show(this.InvokeAuthenticationFailedCallback);
    }

    private async void InvokeAuthenticationFailedCallback()
    {
        switch (UIManagerGlobal.Instance.PanelMessageBox.MessageBoxDialogResult)
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
