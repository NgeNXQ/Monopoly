using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Monopoly.Client.Runtime.UI.Managers
{
    internal sealed class GUIManagerBootstrap : MonoBehaviour
    {
        private StringTable localizationTableMessages;

        private string messageDefaultInitializingGameCoordinator;
        private string messageExceptionGameCoordinatorInitializationFailed;

        // string messageDefaultInitializingGameCoordinator;
        // private LocalizedString messageDefaultInitializingGameCoordinator;

        internal static GUIManagerBootstrap Instance { get; private set; }

        private void Awake()
        {
            if (GUIManagerBootstrap.Instance != null)
                throw new TypeInitializationException(nameof(GUIManagerBootstrap), new ApplicationException($"Singleton has already been initialized."));

            GUIManagerBootstrap.Instance = this;
        }

        private async void Start()
        {
            await this.PreloadLocalizationAsync();

            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.None,
                MessageBoxPanel.Icon.Loading,
                this.messageDefaultInitializingGameCoordinator
            );
        }

        private async Task PreloadLocalizationAsync()
        {
            this.localizationTableMessages = await LocalizationSettings.StringDatabase.GetTableAsync("strings_bootstrap_messages").Task;
            this.messageDefaultInitializingGameCoordinator = this.localizationTableMessages.GetEntry("message_default_initializing_game_coordinator").Value;
            this.messageExceptionGameCoordinatorInitializationFailed = this.localizationTableMessages.GetEntry("message_exception_game_coordinator_initialization_failed").Value;
        }

        private void OnEnable()
        {
            // LocalizationSettings.SelectedLocaleChanged += this.OnSelectedLocaleChanged;
            GameCoordinator.Instance.OnAuthenticationFailed += this.HandleAuthenticationFailed;
        }

        private void OnDisable()
        {
            // LocalizationSettings.SelectedLocaleChanged -= this.OnSelectedLocaleChanged;
            GameCoordinator.Instance.OnAuthenticationFailed -= this.HandleAuthenticationFailed;
        }

        // private void OnSelectedLocaleChanged(Locale locale)
        // {
        //     this.messageDefaultInitializingGameCoordinator = this.localizationTable.GetEntry("message_default_initializing_game_coordinator").Value;
        // }

        private void HandleAuthenticationFailed()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.OK, 
                MessageBoxPanel.Icon.Error, 
                this.messageExceptionGameCoordinatorInitializationFailed, 
                this.OnAuthenticationFailed
            );
        }

        private async void OnAuthenticationFailed()
        {
            switch (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult)
            {
                case MessageBoxPanel.DialogResult.OK:
                    {
#if UNITY_EDITOR
                        EditorApplication.ExitPlaymode();
#else
                        Application.Quit();
#endif
                    }
                    break;
                case MessageBoxPanel.DialogResult.Cancel:
                    {
                        await GameCoordinator.Instance.LoadSceneAsync(GameCoordinator.MonopolyScene.MainMenu);
                    }
                    break;
            }
        }
    }
}
