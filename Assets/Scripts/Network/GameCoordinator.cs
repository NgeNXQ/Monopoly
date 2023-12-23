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

    public const string KEY_NICKNAME_PLAYER_PREFS = "Nickname";

    public enum MonopolyScene : byte
    {
        MainMenu,
        GameLobby,
        MonopolyGame,
        ConnectionSetup
    }

    public static GameCoordinator Instance  { get; private set; }

    public Player LocalPlayer { get; set; }

    public MonopolyScene ActiveScene { get; private set; }

    public event Action OnAuthenticationFailed;

    public event Action<RelayServiceException> OnEstablishingConnectionRelayFailed;

    public event Action<LobbyServiceException> OnEstablishingConnectionLobbyFailed;

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += this.HandleActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= this.HandleActiveSceneChanged;
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

            this.InitializeLocalPlayer(PlayerPrefs.GetString(GameCoordinator.KEY_NICKNAME_PLAYER_PREFS));
        }
        catch
        {
            this.OnAuthenticationFailed?.Invoke();
            return;
        }

        await this.LoadScene(GameCoordinator.MonopolyScene.MainMenu);
    }

    private async void OnApplicationQuit()
    {
        if (LobbyManager.Instance.LocalLobby != null)
        {
            LobbyManager.Instance.StopAllCoroutines();
            await LobbyManager.Instance.DisconnectLobby();
        }
    }

    #region Scenes Management

    public async Task LoadScene(MonopolyScene scene)
    {
        await SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
    }

    public void LoadSceneNetwork(MonopolyScene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }

    private void HandleActiveSceneChanged(Scene previousActiveScene, Scene newActiveScene)
    {
        switch (newActiveScene.name)
        {
            case nameof(GameCoordinator.MonopolyScene.MainMenu):
                    this.ActiveScene = GameCoordinator.MonopolyScene.MainMenu;
                break;
            case nameof(GameCoordinator.MonopolyScene.GameLobby):
                {
                    this.ActiveScene = GameCoordinator.MonopolyScene.GameLobby;
                    LobbyManager.Instance.OnGameLobbyLoaded?.Invoke();
                }
                break;
            case nameof(GameCoordinator.MonopolyScene.MonopolyGame):
                {
                    this.ActiveScene = GameCoordinator.MonopolyScene.MonopolyGame;
                    LobbyManager.Instance.OnMonopolyGameLoaded?.Invoke();
                }
                break;
        }
    }

    #endregion

    #region Establishing Connection

    public void UpdateLocalPlayer(string newNickname)
    {
        this.LocalPlayer.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value = newNickname;

        PlayerPrefs.SetString(GameCoordinator.KEY_NICKNAME_PLAYER_PREFS, newNickname);
        PlayerPrefs.Save();
    }

    public void InitializeLocalPlayer(string nickname)
    {
        Player player = new Player(AuthenticationService.Instance.PlayerId)
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { LobbyManager.KEY_PLAYER_NICKNAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nickname) },
                { LobbyManager.KEY_PLAYER_ACTIVE_SCENE, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GameCoordinator.Instance.ActiveScene.ToString()) }
            }
        };

        PlayerPrefs.SetString(GameCoordinator.KEY_NICKNAME_PLAYER_PREFS, nickname);
        PlayerPrefs.Save();

        this.LocalPlayer = player;
    }

    public async Task HostLobbyAsync()
    {
        if (this.LocalPlayer == null)
            throw new System.ArgumentNullException($"{nameof(this.LocalPlayer)} is null.");

        try
        {
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(GameCoordinator.MAX_PLAYERS);

            RelayServerData relayServerData = new RelayServerData(hostAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            string relayCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            await LobbyManager.Instance?.HostLobby(relayCode);
        }
        catch (RelayServiceException relayServiceException)
        {
            this.OnEstablishingConnectionRelayFailed?.Invoke(relayServiceException);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            this.OnEstablishingConnectionLobbyFailed?.Invoke(lobbyServiceException);
        }
    }

    public async Task ConnectLobbyAsync(string joinCode)
    {
        if (this.LocalPlayer == null)
            throw new System.ArgumentNullException($"{nameof(this.LocalPlayer)} is null.");

        try
        {
            JoinAllocation clientAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(clientAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            await LobbyManager.Instance?.ConnectLobby(joinCode);
        }
        catch (RelayServiceException relayServiceException)
        {
            this.OnEstablishingConnectionRelayFailed?.Invoke(relayServiceException);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            this.OnEstablishingConnectionLobbyFailed?.Invoke(lobbyServiceException);
        }
    }

    #endregion
}
