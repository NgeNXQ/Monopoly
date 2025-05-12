using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Monopoly.Client.Utilities.Scenes
{
    internal static class SceneManagerUtility
    {
        // internal static Scene CurrentScene { get; private set; }
        // internal static AssetReference CurrentSceneName { get; private set; }
        internal static AssetReference CurrentSceneAsset { get; private set; }

        internal static async Task LoadSceneDefaultAsync(AssetReference sceneAsset, LoadSceneMode loadMode)
        {
            if (sceneAsset == null)
                throw new ArgumentNullException($"{nameof(sceneAsset)} cannot be null.");

            AsyncOperationHandle<IList<IResourceLocation>> handler = Addressables.LoadResourceLocationsAsync(sceneAsset);
            IList<IResourceLocation> locations = await handler.Task;
            Addressables.Release(handler);

            string sceneName = Path.GetFileNameWithoutExtension(locations[0].PrimaryKey);
            // SceneManagerUtility.CurrentScene = SceneManager.GetSceneByName(sceneName);
            await SceneManager.LoadSceneAsync(sceneName, loadMode);
            SceneManagerUtility.CurrentSceneAsset = sceneAsset;
        }

        internal static async Task LoadSceneNetworkAsync(AssetReference sceneAsset, LoadSceneMode loadMode)
        {
            if (sceneAsset == null)
                throw new ArgumentNullException($"{nameof(sceneAsset)} cannot be null.");

            AsyncOperationHandle<IList<IResourceLocation>> handler = Addressables.LoadResourceLocationsAsync(sceneAsset);
            IList<IResourceLocation> locations = await handler.Task;
            Addressables.Release(handler);

            string sceneName = Path.GetFileNameWithoutExtension(locations[0].PrimaryKey);
            // SceneManagerUtility.CurrentScene = SceneManager.GetSceneByName(sceneName);
            NetworkManager.Singleton?.SceneManager?.LoadScene(sceneName, loadMode);
            SceneManagerUtility.CurrentSceneAsset = sceneAsset;
        }
    }
}