using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine.SceneManagement;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Networking.Transport.Relay;

internal sealed class GameCoordinator : MonoBehaviour
{
    public const int MAX_PLAYERS = 5;

    private const string CONNECTION_TYPE = "dtls";

    public enum Scene : byte
    {
        MainMenu,
        GameLobby,
        MonopolyGame
    }

    public static GameCoordinator LocalInstance  { get; private set; }

    public event Action<RelayServiceException> EstablishingConnectionFailed;

    private void Awake()
    {
        if (LocalInstance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        LocalInstance = this;
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        GameLobbyManager.LocalInstance.HostDisconnected += this.HandleHostDisconnected;
    }

    private void OnDisable()
    {
        GameLobbyManager.LocalInstance.HostDisconnected -= this.HandleHostDisconnected;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    #region Event Callbacks

    private void HandleHostDisconnected()
    {
        this.LoadScene(GameCoordinator.Scene.MainMenu);
    }

    #endregion

    #region Scenes Management

    public void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }

    public void LoadSceneNetwork(Scene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }

    #endregion

    #region Establishing Connection

    public async void HostLobbyAsync()
    {
        try
        {
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(GameCoordinator.MAX_PLAYERS);

            RelayServerData relayServerData = new RelayServerData(hostAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            GameLobbyManager.LocalInstance.InitializeGameLobby(joinCode);
        }
        catch (RelayServiceException relayServiceException)
        {
            this.EstablishingConnectionFailed?.Invoke(relayServiceException);
        }
    }

    public async void ConnectLobbyAsync(string joinCode)
    {
        try
        {
            JoinAllocation clientAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(clientAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException relayServiceException)
        {
            this.EstablishingConnectionFailed?.Invoke(relayServiceException);
        }
    }

    #endregion
}
