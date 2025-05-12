using System;
using UnityEngine;
// using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Monopoly.Client.Runtime.Core.P2P;
// using Monopoly.Client.Runtime.Core.Models;
// using Monopoly.Client.Utilities.Scenes;
// using Monopoly.Client.Utilities.Trackers.Loading;

namespace Monopoly.Client.Runtime.App
{
    internal sealed class MonopolyApplication : MonoBehaviour
    {
        [field: SerializeField]
        internal AssetReference SceneAssetMainMenu { get; private set; }

        [field: SerializeField]
        internal AssetReference SceneAssetMonopolyGame { get; private set; }

        [field: SerializeField]
        internal AssetReference SceneAssetLobbyUnranked { get; private set; }

        internal static MonopolyApplication Instance { get; private set; }

        // internal LoadingGroup CurrentLoadingGroup { get; private set; }

        private void Awake()
        {
            if (MonopolyApplication.Instance != null)
                throw new TypeInitializationException(nameof(MonopolyApplication), new ApplicationException($"Singleton has already been initialized."));

            GameObject.DontDestroyOnLoad(this);
            MonopolyApplication.Instance = this;
        }

        // private void OnEnable()
        // {
        //     SceneManager.activeSceneChanged += this.OnActiveSceneChangedAsync;
        // }

        // private void OnDisable()
        // {
        //     SceneManager.activeSceneChanged -= this.OnActiveSceneChangedAsync;
        // }

        private async void OnApplicationQuit()
        {
            if (LobbyManager.Instance?.LocalLobby == null)
                return;

            if (await LobbyManager.Instance?.DoesLobbyExistAsync())
                await LobbyManager.Instance?.DisconnectFromLobbyAsync();
        }

        private async void OnApplicationPause(bool pause)
        {
            if (LobbyManager.Instance?.LocalLobby == null)
                return;

            if (await LobbyManager.Instance?.DoesLobbyExistAsync())
                await LobbyManager.Instance?.DisconnectFromLobbyAsync();
        }

        // private async void OnActiveSceneChangedAsync(Scene previousScene, Scene currentScene)
        // {
        //     // if (SceneManagerUtility.CurrentSceneAsset == this.SceneAssetLobbyUnranked)
        //     // {
        //     //     // this.CurrentLoadingGroup = new LobbyLoadingGroup();
        //     //     await LobbyManager.Instance.UpdateLocalPlayerDataAsync();
        //     //     // this.CurrentLoadingGroup.ObjectsLoadedEvent
        //     // }
        // }
    }
}
