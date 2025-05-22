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
using UnityEngine;
using UnityEngine.SceneManagement;
using Monopoly.Domain.DTOs.Api;
using Monopoly.Client.Runtime.App;
using Monopoly.Client.Utilities.Scenes;
using Monopoly.Backend.Gateway.Services.Account;
using Monopoly.Unity.Services.Lobbies.Models.Extensions;

namespace Monopoly.Client.Runtime.Core.P2P
{
    internal sealed class GameCoordinator : MonoBehaviour
    {
        private const string CONNECTION_TYPE = "dtls";

        internal const string KEY_PLAYER_DATA_SCENE = "player_data_scene";
        internal const string KEY_PLAYER_DATA_NICKNAME = "player_data_nickname";
        internal const string KEY_PLAYER_DATA_TROPHIES = "player_data_trophies";

        internal static GameCoordinator Instance { get; private set; }

        internal event Action AuthenticationFailedEvent;
        internal event Action<RelayServiceException> RelayConnectionFailedEvent;
        internal event Action<LobbyServiceException> LobbyConnectionFailedEvent;

        internal Player LocalPlayer { get; private set; }
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
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                this.ServiceAccount = new AccountServiceBinding(CloudCodeService.Instance);

                await this.InitializeLocalPlayer();
                // await this.ServiceAccount.PutAccountTrophies("1000");

                await SceneManagerUtility.LoadSceneDefaultAsync(MonopolyApplication.Instance.SceneAssetMainMenu, LoadSceneMode.Single);
            }
            catch
            {
                this.AuthenticationFailedEvent?.Invoke();
            }
        }

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

        internal async Task HostPublicLobbyAsync()
        {
            await HostLobbyAsync(true);
        }

        internal async Task HostPrivateLobbyAsync()
        {
            await HostLobbyAsync(false);
        }

        private async Task HostLobbyAsync(bool isPublic)
        {
            try
            {
                Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(LobbyManager.MAX_PLAYERS);
                string relayCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

                if (isPublic)
                    await LobbyManager.Instance?.HostPublicLobbyAsync(relayCode);
                else
                    await LobbyManager.Instance?.HostPrivateLobbyAsync(relayCode);

                RelayServerData relayServerData = AllocationUtils.ToRelayServerData(hostAllocation, GameCoordinator.CONNECTION_TYPE);

                NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton?.StartHost();

                if (isPublic)
                    await SceneManagerUtility.LoadSceneDefaultAsync(MonopolyApplication.Instance.SceneAssetLobbyPublic, LoadSceneMode.Single);
                else
                    await SceneManagerUtility.LoadSceneDefaultAsync(MonopolyApplication.Instance.SceneAssetLobbyPrivate, LoadSceneMode.Single);
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
                await LobbyManager.Instance?.JoinLobbyAsync(joinCode);

                JoinAllocation clientAllocation = await RelayService.Instance.JoinAllocationAsync(LobbyManager.Instance.RelayCode);
                RelayServerData relayServerData = AllocationUtils.ToRelayServerData(clientAllocation, GameCoordinator.CONNECTION_TYPE);

                NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton?.StartClient();
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

        internal async Task FindLobbyAsync()
        {
            try
            {
                await LobbyManager.Instance?.FindLobbyAsync();

                JoinAllocation clientAllocation = await RelayService.Instance.JoinAllocationAsync(LobbyManager.Instance.RelayCode);
                RelayServerData relayServerData = AllocationUtils.ToRelayServerData(clientAllocation, GameCoordinator.CONNECTION_TYPE);

                NetworkManager.Singleton?.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton?.StartClient();
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
