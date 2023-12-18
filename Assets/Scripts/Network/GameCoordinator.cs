using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Networking.Transport.Relay;

#if UNITY_EDITOR
using ParrelSync;
#endif

internal sealed class GameCoordinator : MonoBehaviour
{
    private const string CONNECTION_TYPE = "dtls";

    public const int MAX_PLAYERS = 5;

    public enum MonopolyScene : byte
    {
        MainMenu,
        GameLobby,
        MonopolyGame,
        ConnectionSetup
    }

    public MonopolyScene ActiveScene { get; private set; }

    public static GameCoordinator Instance  { get; private set; }

    public event Action AuthenticationFailed;

    public event Action<RelayServiceException> EstablishingConnectionRelayFailed;

    public event Action<LobbyServiceException> EstablishingConnectionLobbyFailed;

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += this.HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= this.HandleSceneLoaded;
    }

    private async void Start()
    {
        try
        {
#if UNITY_EDITOR
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
            await UnityServices.InitializeAsync(options);
#else
            await UnityServices.InitializeAsync();
#endif
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch
        {
            this.AuthenticationFailed?.Invoke();
            return;
        }

        this.LoadScene(GameCoordinator.MonopolyScene.MainMenu);
    }

    #region Scenes Management

    public void LoadScene(MonopolyScene scene)
    {
        SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }

    public void LoadSceneNetwork(MonopolyScene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case nameof(GameCoordinator.MonopolyScene.MainMenu):
                this.ActiveScene = GameCoordinator.MonopolyScene.MainMenu;
                break;
            case nameof(GameCoordinator.MonopolyScene.GameLobby):
                {
                    this.ActiveScene = GameCoordinator.MonopolyScene.GameLobby;
                    LobbyManager.LocalInstance.GameLobbyLoaded?.Invoke();
                }
                break;
            case nameof(GameCoordinator.MonopolyScene.MonopolyGame):
                this.ActiveScene = GameCoordinator.MonopolyScene.MonopolyGame;
                break;
        }
    }

    #endregion

    #region Establishing Connection

    public Player InitializePlayer(string nickname)
    {
        Player player = new Player(AuthenticationService.Instance.PlayerId)
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { LobbyManager.KEY_PLAYER_NICKNAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nickname) }
            }
        };

        return player;
    }

    public async Task HostLobbyAsync(Player player)
    {
        if (player == null)
            throw new System.ArgumentNullException($"{nameof(player)} is null.");

        try
        {
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(GameCoordinator.MAX_PLAYERS);

            RelayServerData relayServerData = new RelayServerData(hostAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            NetworkManager.Singleton.StartHost();

            await LobbyManager.LocalInstance.HostLobby(player, joinCode);
        }
        catch (RelayServiceException relayServiceException)
        {
            this.EstablishingConnectionRelayFailed?.Invoke(relayServiceException);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            this.EstablishingConnectionLobbyFailed?.Invoke(lobbyServiceException);
        }
    }

    public async Task ConnectLobbyAsync(Player player, string joinCode)
    {
        if (player == null)
            throw new System.ArgumentNullException($"{nameof(player)} is null.");

        try
        {
            JoinAllocation clientAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(clientAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            await LobbyManager.LocalInstance.ConnectLobby(player, joinCode);
        }
        catch (RelayServiceException relayServiceException)
        {
            this.EstablishingConnectionRelayFailed?.Invoke(relayServiceException);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            this.EstablishingConnectionLobbyFailed?.Invoke(lobbyServiceException);
        }
    }

    #endregion
}
