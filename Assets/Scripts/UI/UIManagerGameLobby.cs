using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIManagerGameLobby : MonoBehaviour
{
    [SerializeField] private Button buttonHostGame;

    [SerializeField] private Button buttonJoinGame;

    private void Awake()
    {
        buttonHostGame.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            MonopolySceneManager.Load(SceneManager.Scene.Lobby);
        });

        buttonJoinGame.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); });
    }
}
