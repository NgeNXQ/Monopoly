using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
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
// using Unity.Services.CloudSave;
// using Unity.Services.CloudSave.Models;
// using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityEngine.AddressableAssets;
using Monopoly.Domain.DTOs.Api;
using Monopoly.Client.Runtime.App;
using Monopoly.Client.Utilities.Scenes;
using Monopoly.Backend.Gateway.Services.Account;
using Monopoly.Unity.Services.Lobbies.Models.Extensions;
// using Monopoly.Backend.Gateway.Services;
// using Monopoly.Client.Runtime.Game.Core;
// using Monopoly.Client.Runtime.Game.Board;
// using UnityEditor.SearchService;
// using Monopoly.Client.Runtime.Core.Models;

namespace Monopoly.Client.Runtime.Core.P2P
{
    internal sealed class GameCoordinator : MonoBehaviour
    {
        private const string CONNECTION_TYPE = "dtls";

        internal const string KEY_PLAYER_DATA_SCENE = "player_data_scene";
        internal const string KEY_PLAYER_DATA_NICKNAME = "player_data_nickname";
        internal const string KEY_PLAYER_DATA_TROPHIES = "player_data_trophies";

        internal static GameCoordinator Instance { get; private set; }

        // private LinkedList<Type> objectsToLoad;
        // private LinkedList<Type> initializedObjects;

        internal event Action AuthenticationFailedEvent;
        internal event Action<RelayServiceException> RelayConnectionFailedEvent;
        internal event Action<LobbyServiceException> LobbyConnectionFailedEvent;

        internal Player LocalPlayer { get; private set; }
        // internal MonopolyPlayer Player { get; private set; }

        internal AccountServiceBinding ServiceAccount { get; private set; }

        private void Awake()
        {
            if (GameCoordinator.Instance != null)
                throw new TypeInitializationException(nameof(GameCoordinator), new ApplicationException($"Singleton has already been initialized."));

            GameCoordinator.Instance = this;
            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
        }

        private async void Start()
        {
            // this.objectsToLoad = new LinkedList<Type>();
            // this.initializedObjects = new LinkedList<Type>();

            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                this.ServiceAccount = new AccountServiceBinding(CloudCodeService.Instance);

                await this.InitializeLocalPlayer();

                await SceneManagerUtility.LoadSceneDefaultAsync(MonopolyApplication.Instance.SceneAssetMainMenu, LoadSceneMode.Single);
            }
            catch
            {
                this.AuthenticationFailedEvent?.Invoke();
            }
            // finally
            // {
            //     SceneManager.activeSceneChanged += this.HandleActiveSceneChanged;
            // }
        }

        // private void OnDestroy()
        // {
        //     SceneManager.activeSceneChanged -= this.HandleActiveSceneChanged;
        // }

        internal async Task InitializeLocalPlayer()
        {
            string accountGetResponse = await this.ServiceAccount.GetAccount();
            ApiResponse<AccountPayload> parsedAccountResponse = JsonConvert.DeserializeObject<ApiResponse<AccountPayload>>(accountGetResponse);

            if (parsedAccountResponse.Code == HttpStatusCode.NotFound)
            {
                accountGetResponse = await this.ServiceAccount.PostAccount("<blank>", "0");
                parsedAccountResponse = JsonConvert.DeserializeObject<ApiResponse<AccountPayload>>(accountGetResponse);
            }

            string playerTrophies = parsedAccountResponse.Payload.Trophies;
            string playerNickname = parsedAccountResponse.Payload.Nickname;

            PlayerDataObject nicknamePlayerData = new PlayerDataObject(
                PlayerDataObject.VisibilityOptions.Member,
                playerNickname
            );

            PlayerDataObject trophiesPlayerData = new PlayerDataObject(
                PlayerDataObject.VisibilityOptions.Member,
                playerTrophies
            );

            PlayerDataObject scenePlayerData = new PlayerDataObject(
                PlayerDataObject.VisibilityOptions.Member,
                SceneManager.GetActiveScene().name
            );

            this.LocalPlayer = new Player(AuthenticationService.Instance.PlayerId)
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { GameCoordinator.KEY_PLAYER_DATA_SCENE, scenePlayerData },
                    { GameCoordinator.KEY_PLAYER_DATA_NICKNAME, nicknamePlayerData },
                    { GameCoordinator.KEY_PLAYER_DATA_TROPHIES, trophiesPlayerData },
                }
            };
        }

        internal async Task UpdateLocalPlayerNickname(string newNickname)
        {
            this.LocalPlayer.SetDataLocally(
                GameCoordinator.KEY_PLAYER_DATA_NICKNAME,
                newNickname,
                PlayerDataObject.VisibilityOptions.Member
            );

            await GameCoordinator.Instance.ServiceAccount.PutAccountNickname(newNickname);
        }

        // private void HandleActiveSceneChanged(Scene previousScene, Scene currentScene)
        // {
        //     this.LocalPlayer.SetDataLocally(
        //         GameCoordinator.KEY_PLAYER_DATA_SCENE,
        //         currentScene.name,
        //         PlayerDataObject.VisibilityOptions.Member
        //     );
        // }

        // internal async Task UpdateLocalPlayerNicknameAsync(string newNickname)
        // {
        //     this.LocalPlayer.SetNickname(newNickname);
        //     await this.ServiceAccount.PutAccountNickname(newNickname);
        // }

        // internal async Task UpdateLocalPlayerTrophies(int newTrophies)
        // {
        //     await this.ServiceAccount.PutAccountTrophies(newTrophies.ToString());
        //     this.LocalPlayer.Data[LobbyManager.KEY_PLAYER_DATA_TROPHIES].Value = newTrophies.ToString();
        // }

        // internal void UpdateLocalPlayerScene()
        // {
        //     this.LocalPlayer.Data[LobbyManager.KEY_PLAYER_DATA_SCENE].Value = SceneManagerUtility.CurrentScene.name;
        //     // this.LocalPlayer.Data[LobbyManager.KEY_PLAYER_DATA_SCENE] = new PlayerDataObject(
        //     //     PlayerDataObject.VisibilityOptions.Member,
        //     //     SceneManagerUtility.CurrentScene.name
        //     // );

        //     // await this.ServiceAccount.PutAccountTrophies(newTrophies.ToString());
        //     // this.LocalPlayer.Data[LobbyManager.KEY_PLAYER_DATA_TROPHIES].Value = newTrophies.ToString();
        // }



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

        internal async Task JoinLobbyAsync(string joinCode)
        {
            try
            {
                JoinAllocation clientAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                RelayServerData relayServerData = AllocationUtils.ToRelayServerData(clientAllocation, GameCoordinator.CONNECTION_TYPE);
                NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                await LobbyManager.Instance?.JoinLobbyAsync(joinCode);
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
