using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Monopoly.Backend.Gateway.Services;
using Monopoly.Client.Runtime.Game.Core;
using Monopoly.Client.Runtime.UI.Managers;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Lobby;

namespace Monopoly.Client.Runtime.P2P
{
    internal sealed class GameCoordinator : MonoBehaviour
    {
        public enum MonopolyScene : byte
        {
            Bootstrap,
            MainMenu,
            GameLobby,
            MonopolyGame
        }

        private const string CONNECTION_TYPE = "dtls";

        public static GameCoordinator Instance { get; private set; }

        private LinkedList<Type> objectsToLoad;
        private LinkedList<Type> initializedObjects;

        public event Action OnAuthenticationFailed;
        public event Action<RelayServiceException> OnEstablishingConnectionRelayFailed;
        public event Action<LobbyServiceException> OnEstablishingConnectionLobbyFailed;

        public bool IsGameQuiting { get; private set; }
        public Player LocalPlayer { get; private set; }
        public MonopolyScene ActiveScene { get; private set; }

        private void Awake()
        {
            if (GameCoordinator.Instance != null)
                throw new InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            GameCoordinator.Instance = this;
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
            this.objectsToLoad = new LinkedList<Type>();
            this.initializedObjects = new LinkedList<Type>();

            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "mmr", "matchesPlayed" });
                if (playerData.TryGetValue("mmr", out var mmr))
                {
                    Debug.Log($"Player MMR: {mmr.Value.GetAs<int>()}");
                }

                // Debug.Log($"Authenticated player ID: {AuthenticationService.Instance.PlayerId}");
                // RatingServiceBindings module = new RatingServiceBindings(CloudCodeService.Instance);
                // Debug.Log(await module.GetMMR());

                this.InitializeLocalPlayer(PlayerPrefs.GetString(LobbyManager.KEY_PLAYER_NICKNAME));
            }
            catch
            {
                this.OnAuthenticationFailed?.Invoke();
                return;
            }

            await this.LoadSceneAsync(GameCoordinator.MonopolyScene.MainMenu);
        }

        private void OnApplicationQuit()
        {
            this.IsGameQuiting = true;
        }

        public void UpdateLocalPlayer(string newNickname)
        {
            newNickname = newNickname.Trim();

            this.LocalPlayer.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value = newNickname;

            PlayerPrefs.SetString(LobbyManager.KEY_PLAYER_NICKNAME, newNickname);
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

            PlayerPrefs.SetString(LobbyManager.KEY_PLAYER_NICKNAME, nickname);
            PlayerPrefs.Save();

            this.LocalPlayer = player;
        }

        public void LoadSceneNetwork(MonopolyScene scene)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
        }

        public async Task LoadSceneAsync(MonopolyScene scene)
        {
            await SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
        }

        public void UpdateInitializedObjects(Type gameObject)
        {
            if (this.objectsToLoad == null)
            {
                throw new System.InvalidOperationException($"You have to call {nameof(this.SetupInitializedObjects)} at first.");
            }

            if (!this.objectsToLoad.Contains(gameObject))
            {
                throw new System.ArgumentException($"{nameof(gameObject)} is not in {nameof(this.SetupInitializedObjects)}.");
            }

            if (this.initializedObjects.Contains(gameObject))
            {
                throw new System.ArgumentException($"{nameof(gameObject)} has already been initialized.");
            }

            this.initializedObjects.AddLast(gameObject);

            if (this.initializedObjects.Count == this.objectsToLoad.Count)
            {
                LobbyManager.Instance?.UpdateLocalPlayerDataAsync();
            }
        }

        public void SetupInitializedObjects(params Type[] gameObjectsToLoad)
        {
            foreach (Type gameObject in gameObjectsToLoad)
            {
                this.objectsToLoad.AddLast(gameObject);
            }
        }

        private void HandleActiveSceneChanged(Scene previousActiveScene, Scene newActiveScene)
        {
            this.objectsToLoad?.Clear();
            this.initializedObjects?.Clear();

            switch (newActiveScene.name)
            {
                case nameof(GameCoordinator.MonopolyScene.MainMenu):
                    this.ActiveScene = GameCoordinator.MonopolyScene.MainMenu;
                    break;
                case nameof(GameCoordinator.MonopolyScene.GameLobby):
                    {
                        this.ActiveScene = GameCoordinator.MonopolyScene.GameLobby;

                        this.SetupInitializedObjects(typeof(UIManagerUnrankedLobby), typeof(PlayerUnrankedLobbyPanel));

                        LobbyManager.Instance?.OnGameLobbyLoaded?.Invoke();
                    }
                    break;
                case nameof(GameCoordinator.MonopolyScene.MonopolyGame):
                    {
                        this.ActiveScene = GameCoordinator.MonopolyScene.MonopolyGame;

                        this.SetupInitializedObjects(typeof(GameManager), typeof(MonopolyBoard), typeof(UIManagerGame));
                        LobbyManager.Instance?.OnMonopolyGameLoaded?.Invoke();
                    }
                    break;
            }
        }

        public async Task HostLobbyAsync()
        {
            if (this.LocalPlayer == null)
            {
                throw new InvalidOperationException($"{nameof(this.LocalPlayer)} is null.");
            }

            try
            {
                Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(LobbyManager.MAX_PLAYERS);
                RelayServerData relayServerData = AllocationUtils.ToRelayServerData(hostAllocation, GameCoordinator.CONNECTION_TYPE);
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
                RelayServerData relayServerData = AllocationUtils.ToRelayServerData(clientAllocation, GameCoordinator.CONNECTION_TYPE);
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
    }
}
