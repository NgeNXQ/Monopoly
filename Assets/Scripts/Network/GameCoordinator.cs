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

    public const string KEY_NICKNAME_PLAYER_PREFS = "Nickname";

    public enum MonopolyScene : byte
    {
        Bootstrap,
        MainMenu,
        GameLobby,
        MonopolyGame
    }

    public static GameCoordinator Instance { get; private set; }

    public event Action OnAuthenticationFailed;

    public event Action<RelayServiceException> OnEstablishingConnectionRelayFailed;

    public event Action<LobbyServiceException> OnEstablishingConnectionLobbyFailed;

    public Player LocalPlayer { get; private set; }

    public MonopolyScene ActiveScene { get; private set; }
    
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

        await this.LoadSceneAsync(GameCoordinator.MonopolyScene.MainMenu);
    }

    #region Updating Player

    public void UpdateLocalPlayer(string newNickname)
    {
        newNickname = newNickname.Trim();

        this.LocalPlayer.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value = newNickname;

        PlayerPrefs.SetString(GameCoordinator.KEY_NICKNAME_PLAYER_PREFS, newNickname);
        PlayerPrefs.Save();
    }

    public void InitializeLocalPlayer(string nickname)
    {
        nickname = nickname.Trim();

        Player player = new Player(AuthenticationService.Instance.PlayerId)
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { LobbyManager.KEY_PLAYER_NICKNAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nickname) },
                { LobbyManager.KEY_PLAYER_SCENE, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GameCoordinator.Instance.ActiveScene.ToString()) }
            }
        };

        PlayerPrefs.SetString(GameCoordinator.KEY_NICKNAME_PLAYER_PREFS, nickname);
        PlayerPrefs.Save();

        this.LocalPlayer = player;
    }

    #endregion

    #region Scenes Management

    public void LoadSceneNetwork(MonopolyScene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }

    public async Task LoadSceneAsync(MonopolyScene scene)
    {
        await SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
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

    public async Task HostLobbyAsync()
    {
        if (this.LocalPlayer == null)
        {
            throw new System.InvalidOperationException($"{nameof(this.LocalPlayer)} is null.");
        }

        try
        {
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(LobbyManager.MAX_PLAYERS);

            RelayServerData relayServerData = new RelayServerData(hostAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            string relayCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            await LobbyManager.Instance?.HostLobbyAsync(relayCode);
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
        {
            throw new System.InvalidOperationException($"{nameof(this.LocalPlayer)} is null.");
        }

        try
        {
            JoinAllocation clientAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(clientAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            await LobbyManager.Instance?.ConnectLobbyAsync(joinCode);
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
