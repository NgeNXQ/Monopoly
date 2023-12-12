using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class MonopolySceneManager
{
    public enum Scene
    {
        MainMenuScene,
        GameScene,
        LoadingScene,
        LobbyScene,
        CharacterSelectScene,
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        MonopolySceneManager.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
