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
using UnityEngine.AddressableAssets;
using Monopoly.Backend.Gateway.Services;
using Monopoly.Client.Runtime.Game.Core;
using Monopoly.Client.Runtime.Game.Board;
// using UnityEditor.SearchService;
using Monopoly.Client.Runtime.Core.Models;
using Monopoly.Client.Runtime.Core.Utilities;
// using Monopoly.Client.Runtime.Core

namespace Monopoly.Client.Runtime.P2P
{
    internal sealed class GameCoordinator : MonoBehaviour
    {
        private const string CONNECTION_TYPE = "dtls";

        internal static GameCoordinator Instance { get; private set; }

        // private LinkedList<Type> objectsToLoad;
        // private LinkedList<Type> initializedObjects;

        internal event Action AuthenticationFailedEvent;
        internal event Action<RelayServiceException> RelayConnectionFailedEvent;
        internal event Action<LobbyServiceException> LobbyConnectionFailedEvent;

        internal Player LocalPlayer { get; private set; }

        private void Awake()
        {
            if (GameCoordinator.Instance != null)
                throw new TypeInitializationException(nameof(GameCoordinator), new ApplicationException($"Singleton has already been initialized."));

            GameCoordinator.Instance = this;
            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
        }

        private void OnEnable()
        {
            // SceneManager.activeSceneChanged += this.HandleActiveSceneChanged;
        }

        private void OnDisable()
        {
            // SceneManager.activeSceneChanged -= this.HandleActiveSceneChanged;
        }

        private async void Start()
        {
            // this.objectsToLoad = new LinkedList<Type>();
            // this.initializedObjects = new LinkedList<Type>();

            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await SceneManagerUtility.LoadSceneDefaultAsync(MonopolyApplication.Instance.SceneAssetLobbyUnranked, LoadSceneMode.Single);

                RankedServiceBindings rankedService = new RankedServiceBindings(CloudCodeService.Instance);
                Debug.Log(await rankedService.GetELO());


                // var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "elo" });

                // if (playerData.TryGetValue("mmr", out var mmr))
                // {
                //     Debug.Log($"Player MMR: {mmr.Value.GetAs<int>()}");
                // }

                // Debug.Log($"Authenticated player ID: {AuthenticationService.Instance.PlayerId}");
                // RatingServiceBindings module = new RatingServiceBindings(CloudCodeService.Instance);
                // Debug.Log(await module.GetMMR());

                this.InitializeLocalPlayer(PlayerPrefs.GetString(LobbyManager.KEY_PLAYER_NICKNAME));
            }
            catch
            {
                this.AuthenticationFailedEvent?.Invoke();
                return;
            }

            // await this.LoadSceneAsync(GameCoordinator.MonopolyScene.MainMenu);
        }

        internal void UpdateLocalPlayer(string newNickname)
        {
            newNickname = newNickname.Trim();

            this.LocalPlayer.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value = newNickname;

            PlayerPrefs.SetString(LobbyManager.KEY_PLAYER_NICKNAME, newNickname);
            PlayerPrefs.Save();
        }

        internal void InitializeLocalPlayer(string nickname)
        {
            // nickname = nickname.Trim();
            nickname = "Player";

            PlayerDataObject nicknamePlayerData = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nickname);
            PlayerDataObject scenePlayerData = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, SceneManagerUtility.CurrentScene.name);

            this.LocalPlayer = new Player(AuthenticationService.Instance.PlayerId)
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { LobbyManager.KEY_PLAYER_SCENE, scenePlayerData },
                    { LobbyManager.KEY_PLAYER_NICKNAME, nicknamePlayerData },
                }
            };

            PlayerPrefs.SetString(LobbyManager.KEY_PLAYER_NICKNAME, nickname);
            PlayerPrefs.Save();
        }

        // internal void UpdateInitializedObjects(Type gameObject)
        // {
        //     if (this.objectsToLoad == null)
        //     {
        //         throw new System.InvalidOperationException($"You have to call {nameof(this.SetupInitializedObjects)} at first.");
        //     }

        //     if (!this.objectsToLoad.Contains(gameObject))
        //     {
        //         throw new System.ArgumentException($"{nameof(gameObject)} is not in {nameof(this.SetupInitializedObjects)}.");
        //     }

        //     if (this.initializedObjects.Contains(gameObject))
        //     {
        //         throw new System.ArgumentException($"{nameof(gameObject)} has already been initialized.");
        //     }

        //     this.initializedObjects.AddLast(gameObject);

        //     if (this.initializedObjects.Count == this.objectsToLoad.Count)
        //     {
        //         LobbyManager.Instance?.UpdateLocalPlayerDataAsync();
        //     }
        // }

        // internal void SetupInitializedObjects(params Type[] gameObjectsToLoad)
        // {
        //     foreach (Type gameObject in gameObjectsToLoad)
        //     {
        //         this.objectsToLoad.AddLast(gameObject);
        //     }
        // }

        // private void HandleActiveSceneChanged(Scene previousActiveScene, Scene newActiveScene)
        // {
        //     this.objectsToLoad?.Clear();
        //     this.initializedObjects?.Clear();

        //     switch (newActiveScene.name)
        //     {
        //         case nameof(GameCoordinator.MonopolyScene.MainMenu):
        //             this.ActiveScene = GameCoordinator.MonopolyScene.MainMenu;
        //             break;
        //         case nameof(GameCoordinator.MonopolyScene.GameLobby):
        //             {
        //                 this.ActiveScene = GameCoordinator.MonopolyScene.GameLobby;

        //                 this.SetupInitializedObjects(typeof(UIManagerUnrankedLobby), typeof(PlayerUnrankedLobbyPanel));

        //                 LobbyManager.Instance?.OnGameLobbyLoaded?.Invoke();
        //             }
        //             break;
        //         case nameof(GameCoordinator.MonopolyScene.MonopolyGame):
        //             {
        //                 this.ActiveScene = GameCoordinator.MonopolyScene.MonopolyGame;

        //                 this.SetupInitializedObjects(typeof(GameManager), typeof(MonopolyBoard), typeof(UIManagerGame));
        //                 LobbyManager.Instance?.OnMonopolyGameLoaded?.Invoke();
        //             }
        //             break;
        //     }
        // }

        internal async Task HostLobbyAsync()
        {
            if (this.LocalPlayer == null)
                throw new InvalidOperationException($"{nameof(this.LocalPlayer)} is null.");

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
                this.RelayConnectionFailedEvent?.Invoke(relayServiceException);
            }
            catch (LobbyServiceException lobbyServiceException)
            {
                this.LobbyConnectionFailedEvent?.Invoke(lobbyServiceException);
            }
        }

        internal async Task ConnectLobbyAsync(string joinCode)
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
                this.RelayConnectionFailedEvent?.Invoke(relayServiceException);
            }
            catch (LobbyServiceException lobbyServiceException)
            {
                this.LobbyConnectionFailedEvent?.Invoke(lobbyServiceException);
            }
        }
    }
}
