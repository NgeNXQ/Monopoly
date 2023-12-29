using System;
using UnityEngine;
using Unity.Netcode;
using System.Threading;
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

    public static GameCoordinator Instance  { get; private set; }

    public Player LocalPlayer { get; set; }

    public MonopolyScene ActiveScene { get; private set; }

    public event Action OnAuthenticationFailed;

    public event Action<OperationCanceledException> OnOperationCanceledException;

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

        await this.LoadSceneAsync(GameCoordinator.MonopolyScene.MainMenu);
    }

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
            throw new System.ArgumentNullException($"{nameof(this.LocalPlayer)} is null.");

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(LobbyManager.LOBBY_LOADING_TIMEOUT));

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3600));

            Allocation hostAllocation = await Task.Run(() => RelayService.Instance.CreateAllocationAsync(LobbyManager.MAX_PLAYERS), cancellationTokenSource.Token);

            RelayServerData relayServerData = new RelayServerData(hostAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            string relayCode = await Task.Run(() => RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId), cancellationTokenSource.Token);

            await LobbyManager.Instance?.HostLobbyAsync(relayCode, cancellationTokenSource.Token);
        }
        catch (RelayServiceException relayServiceException)
        {
            this.OnEstablishingConnectionRelayFailed?.Invoke(relayServiceException);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            this.OnEstablishingConnectionLobbyFailed?.Invoke(lobbyServiceException);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            this.OnOperationCanceledException?.Invoke(operationCanceledException);
        }
        finally
        { 
            cancellationTokenSource.Dispose(); 
        }
    }

    public async Task ConnectLobbyAsync(string joinCode)
    {
        if (this.LocalPlayer == null)
            throw new System.ArgumentNullException($"{nameof(this.LocalPlayer)} is null.");

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(LobbyManager.LOBBY_LOADING_TIMEOUT));

        try
        {
            JoinAllocation clientAllocation = await Task.Run(() => RelayService.Instance.JoinAllocationAsync(joinCode), cancellationTokenSource.Token);

            RelayServerData relayServerData = new RelayServerData(clientAllocation, GameCoordinator.CONNECTION_TYPE);

            NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            await LobbyManager.Instance?.ConnectLobbyAsync(joinCode, cancellationTokenSource.Token);
        }
        catch (RelayServiceException relayServiceException)
        {
            this.OnEstablishingConnectionRelayFailed?.Invoke(relayServiceException);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            this.OnEstablishingConnectionLobbyFailed?.Invoke(lobbyServiceException);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            this.OnOperationCanceledException?.Invoke(operationCanceledException);
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }
    }

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
                { LobbyManager.KEY_PLAYER_SCENE, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GameCoordinator.Instance.ActiveScene.ToString()) }
            }
        };

        PlayerPrefs.SetString(GameCoordinator.KEY_NICKNAME_PLAYER_PREFS, nickname);
        PlayerPrefs.Save();

        this.LocalPlayer = player;
    }

    #endregion
}
