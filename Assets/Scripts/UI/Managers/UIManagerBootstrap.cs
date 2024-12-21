using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

internal sealed class UIManagerBootstrap : MonoBehaviour
{
    #region Setup

    #region Messages

    [Header("Messages")]

    [Space]
    [SerializeField] private string messageInitializingGameCoordinator;

    [Space]
    [SerializeField] private string messageInitializationGameCoordinatorFailed;

    #endregion

    #endregion

    public static UIManagerBootstrap Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, this.messageInitializingGameCoordinator, PanelMessageBoxUI.Icon.Loading);
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
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, this.messageInitializationGameCoordinatorFailed, PanelMessageBoxUI.Icon.Error, this.CallbackAuthenticationFailed);
    }

    private async void CallbackAuthenticationFailed()
    {
        switch (UIManagerGlobal.Instance.LastMessageBox.PanelDialogResult)
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
                    await GameCoordinator.Instance.LoadSceneAsync(GameCoordinator.MonopolyScene.MainMenu);
                }
                break;
        }
    }

    #endregion
}
