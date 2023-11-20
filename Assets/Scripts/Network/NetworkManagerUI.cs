using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public sealed class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button buttonHost;
    [SerializeField] private Button buttonClient;

    private void Awake()
    {
        buttonHost.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        buttonClient.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
    }
}
