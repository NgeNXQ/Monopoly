using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Monopoly.Client.Runtime.Core.Utilities
{
    internal static class SceneManagerUtility
    {
        private static AsyncOperationHandle<SceneInstance> cachedSceneHandler;

        internal static Scene CurrentScene { get; private set; }

        internal static void ReleaseCurrentScene()
        {
            Addressables.UnloadSceneAsync(SceneManagerUtility.cachedSceneHandler);
        }

        internal static void LoadScene(AssetReference sceneAsset)
        {
            SceneManager.LoadScene(sceneAsset.Asset.name);
        }

        internal static async Task LoadSceneDefaultAsync(AssetReference sceneAsset, LoadSceneMode loadMode)
        {
            SceneManagerUtility.cachedSceneHandler = Addressables.LoadSceneAsync(sceneAsset, loadMode);
            SceneManagerUtility.CurrentScene = (await SceneManagerUtility.cachedSceneHandler.Task).Scene;
        }

        internal static async Task LoadSceneNetworkAsync(AssetReference sceneAsset, LoadSceneMode loadMode)
        {
            SceneManagerUtility.cachedSceneHandler = Addressables.LoadSceneAsync(sceneAsset, loadMode);
            SceneManagerUtility.CurrentScene = (await SceneManagerUtility.cachedSceneHandler.Task).Scene;
            NetworkManager.Singleton?.SceneManager?.LoadScene(SceneManagerUtility.CurrentScene.ToString(), LoadSceneMode.Single);
        }
    }
}
