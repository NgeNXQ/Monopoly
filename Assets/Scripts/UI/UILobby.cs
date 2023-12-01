using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public sealed class UILobby : MonoBehaviour
{
    [SerializeField] private Button buttonHostGame;

    [SerializeField] private Button buttonJoinGame;

    private void Awake()
    {
        buttonHostGame.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            SceneLoader.Load(SceneLoader.Scene.Lobby);
        });

        buttonJoinGame.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); });
    }
}
