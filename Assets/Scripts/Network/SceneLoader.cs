using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public enum Scene : byte
    {
        Game,
        Lobby,
        Loading,
        MainMenu,
    }

    private static Scene targerScene;

    public static void Load(Scene targerScene) => NetworkManager.Singleton.SceneManager.LoadScene(targerScene.ToString(), LoadSceneMode.Single);

    public static void LoaderCallback() => SceneManager.LoadScene(targerScene.ToString());
}
