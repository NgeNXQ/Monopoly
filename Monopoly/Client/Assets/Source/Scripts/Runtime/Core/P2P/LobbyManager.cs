using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Monopoly.Client.Runtime.App;
using Monopoly.Client.Runtime.UI.Pools;
using Monopoly.Client.Runtime.UI.Managers.Menu;
using Monopoly.Client.Runtime.UI.Managers.Lobby;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Managers.Gameplay;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;
using Monopoly.Client.Utilities.Scenes;
using Monopoly.Unity.Services.Lobbies.Models.Extensions;
using Monopoly.Client.Utilities.Trackers.Loading;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Monopoly.Client.Runtime.Core.P2P
{
    internal sealed class LobbyManager : MonoBehaviour
    {
        private const float CONFIG_UPTIME = 25.0f;

        internal const int MIN_PLAYERS = 1;
        internal const int MAX_PLAYERS = 5;

        internal const float LOBBY_LOADING_TIMEOUT = 15.0f;

        internal const string KEY_LOBBY_DATA_STATE = "lobby_state";
        internal const string VALUE_LOBBY_DATA_STATE_IDLE = "lobby_state_idle";
        internal const string VALUE_LOBBY_DATA_STATE_GAME = "lobby_state_game";
        internal const string VALUE_LOBBY_DATA_STATE_PENDING = "lobby_state_pending";
        internal const string VALUE_LOBBY_DATA_STATE_LOADING = "lobby_state_loading";
        internal const string VALUE_LOBBY_DATA_STATE_RETURNING = "lobby_state_returning";

        private string lobbyName
        {
            get => $"LOBBY_{this.JoinCode}";
        }

        private ILobbyEvents localLobbyEvents;

        private QueryLobbiesOptions queryCurrentLobby
        {
            get
            {
                return new QueryLobbiesOptions()
                {
                    Filters = new List<QueryFilter>()
                    {
                        new QueryFilter(QueryFilter.FieldOptions.Name, this.JoinCode, QueryFilter.OpOptions.CONTAINS)
                    }
                };
            }
        }

        internal static LobbyManager Instance { get; private set; }

        // internal Action GameLobbyLoadedEvent;
        // internal Action MonopolyGameLoadedEvent;
        // internal Action GameLobbyFailedToLoadEvent;
        // internal Action MonopolyGameFailedToLoadEvent;

        // internal bool HavePlayersLoaded
        // {
        //     get
        //     {
        //         // return false;
        //         // return this.LocalLobby != null ? this.LocalLobby.Players.All(player => player.Data[LobbyManager.KEY_PLAYER_DATA_SCENE].Value.Equals(MonopolyApplication.Instance.SceneAssetMonopolyGame.ToString(), StringComparison.Ordinal)) : false;
        //     }
        // }

        internal bool IsHost { get; private set; }
        internal bool IsClient { get; private set; }
        internal string JoinCode { get; private set; }
        internal bool HasHostLeft { get; private set; }
        internal Lobby LocalLobby { get; private set; }
        internal bool HasLocalPlayerLeft { get; private set; }
        internal LobbyEventCallbacks LocalLobbyEventCallbacks { get; private set; }

        private void Awake()
        {
            if (LobbyManager.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            LobbyManager.Instance = this;
            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
        }

        private void OnEnable()
        {
            // this.GameLobbyLoadedEvent += this.OnGameLobbyLoadedAsync;
            // this.MonopolyGameLoadedEvent += this.OnMonopolyGameLoadedAsync;
            // this.GameLobbyFailedToLoadEvent += this.OnGameLobbyFailedToLoadAsync;
            // this.MonopolyGameFailedToLoadEvent += this.OnMonopolyGameFailedToLoadAsync;

            this.LocalLobbyEventCallbacks = new LobbyEventCallbacks();
            this.LocalLobbyEventCallbacks.PlayerLeft += this.OnPlayerLeft;
            this.LocalLobbyEventCallbacks.DataChanged += this.OnDataChanged;
            this.LocalLobbyEventCallbacks.LobbyDeleted += this.OnLobbyDeleted;
            this.LocalLobbyEventCallbacks.PlayerJoined += this.OnPlayerJoined;
            this.LocalLobbyEventCallbacks.KickedFromLobby += this.OnKickedFromLobbyAsync;
            this.LocalLobbyEventCallbacks.PlayerDataChanged += this.OnPlayerDataChanged;

            SceneManager.activeSceneChanged += this.HandleActiveSceneChanged;
            NetworkManager.Singleton.OnTransportFailure += this.OnTransportFailureAsync;
        }

        private void OnDisable()
        {
            // this.GameLobbyLoadedEvent -= this.OnGameLobbyLoadedAsync;
            // this.MonopolyGameLoadedEvent -= this.OnMonopolyGameLoadedAsync;
            // this.GameLobbyFailedToLoadEvent -= this.OnGameLobbyFailedToLoadAsync;
            // this.MonopolyGameFailedToLoadEvent -= this.OnMonopolyGameFailedToLoadAsync;

            this.LocalLobbyEventCallbacks = new LobbyEventCallbacks();
            this.LocalLobbyEventCallbacks.PlayerLeft -= this.OnPlayerLeft;
            this.LocalLobbyEventCallbacks.DataChanged -= this.OnDataChanged;
            this.LocalLobbyEventCallbacks.LobbyDeleted -= this.OnLobbyDeleted;
            this.LocalLobbyEventCallbacks.PlayerJoined -= this.OnPlayerJoined;
            this.LocalLobbyEventCallbacks.KickedFromLobby -= this.OnKickedFromLobbyAsync;
            this.LocalLobbyEventCallbacks.PlayerDataChanged -= this.OnPlayerDataChanged;

            SceneManager.activeSceneChanged -= this.HandleActiveSceneChanged;

            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnTransportFailure -= this.OnTransportFailureAsync;
        }

        private async void OnDestroy()
        {
            if (this.IsHost)
                this.StopCoroutine(this.PingLobbyCoroutine());

            if (this.LocalLobby != null)
                await this.DisconnectFromLobbyAsync();
        }

        private async void HandleActiveSceneChanged(Scene previousScene, Scene currentScene)
        {
            if (this.LocalLobby == null)
                return;

            await GameCoordinator.Instance.LocalPlayer.SetDataRemotely(
                this.LocalLobby.Id,
                GameCoordinator.KEY_PLAYER_DATA_SCENE,
                currentScene.name,
                PlayerDataObject.VisibilityOptions.Member
            );
        }

        internal async Task HostLobbyAsync(string relayCode)
        {
            this.IsHost = true;
            this.IsClient = false;
            this.HasHostLeft = false;
            this.JoinCode = relayCode;
            this.HasLocalPlayerLeft = false;

            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions()
            {
                Player = GameCoordinator.Instance.LocalPlayer,
                Data = new Dictionary<string, DataObject>()
                {
                    { LobbyManager.KEY_LOBBY_DATA_STATE, new DataObject(DataObject.VisibilityOptions.Member, LobbyManager.VALUE_LOBBY_DATA_STATE_IDLE) }
                }
            };

            try
            {
                this.LocalLobby = await LobbyService.Instance.CreateLobbyAsync(this.lobbyName, LobbyManager.MAX_PLAYERS, lobbyOptions);
                this.localLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);

                this.StartCoroutine(this.PingLobbyCoroutine());

                NetworkManager.Singleton?.StartHost();

                await SceneManagerUtility.LoadSceneDefaultAsync(MonopolyApplication.Instance.SceneAssetLobbyUnranked, LoadSceneMode.Single);
            }
            catch
            {
                throw;
            }
        }

        private IEnumerator PingLobbyCoroutine()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(LobbyManager.CONFIG_UPTIME);

            while (this.LocalLobby != null)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(this.LocalLobby?.Id);
                yield return waitForSeconds;
            }
        }

        internal async Task JoinLobbyAsync(string joinCode)
        {
            this.IsHost = false;
            this.IsClient = true;
            this.HasHostLeft = false;
            this.JoinCode = joinCode;
            this.HasLocalPlayerLeft = false;

            JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions()
            {
                Player = GameCoordinator.Instance.LocalPlayer
            };

            try
            {
                QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(this.queryCurrentLobby);
                this.LocalLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results.FirstOrDefault().Id, joinOptions);
                this.localLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);

                NetworkManager.Singleton?.StartClient();
            }
            catch
            {
                throw;
            }
            // catch (NullReferenceException nullReferenceException)
            // {
            //     throw new LobbyServiceException(LobbyExceptionReason.InvalidJoinCode, "Invalid Join Code.", nullReferenceException);
            // }
        }

        // internal async Task UpdateLocalPlayerDataAsync()
        // {
        //     try
        //     {
        //         UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
        //         {
        //             Data = GameCoordinator.Instance.LocalPlayer.Data,
        //         };

        //         // Debug.Log(SceneManager.GetActiveScene().name);
        //         // Debug.Log(GameCoordinator.Instance.LocalPlayer.Data[LobbyManager.KEY_PLAYER_DATA_SCENE].Value);

        //         this.LocalLobby = await LobbyService.Instance.UpdatePlayerAsync(
        //             this.LocalLobby.Id,
        //             GameCoordinator.Instance.LocalPlayer.Id,
        //             updatePlayerOptions
        //         );
        //     }
        //     catch (LobbyServiceException)
        //     {
        //         await this.DisconnectFromLobbyAsync();
        //     }
        // }

        // internal async Task UpdateLocalLobbyDataAsync(bool isPrivate)
        // {
        //     try
        //     {
        //         UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
        //         {
        //             IsPrivate = isPrivate,
        //             Data = this.LocalLobby.Data
        //         };

        //         this.LocalLobby = await LobbyService.Instance.UpdateLobbyAsync(
        //             this.LocalLobby.Id,
        //             updateLobbyOptions
        //         );
        //     }
        //     catch (LobbyServiceException)
        //     {
        //         await this.DisconnectFromLobbyAsync();
        //     }
        // }

        internal async void StartGameAsync()
        {
            // if (!this.HavePlayersLoaded)
            // {
            //     UIManagerGlobal.Instance.ShowMessageBox(
            //         MessageBoxPanel.Type.OK,
            //         MessageBoxPanel.Icon.Warning,
            //         UIManagerUnrankedLobby.Instance.MessageNotAllPlayersLoaded
            //     );
            //     return;
            // }

            if (this.LocalLobby.Players.Count < LobbyManager.MIN_PLAYERS)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Warning,
                    UIManagerUnrankedLobby.Instance.MessageTooFewPlayers
                );
                return;
            }

            await this.LocalLobby.SetData(
                LobbyManager.KEY_LOBBY_DATA_STATE,
                LobbyManager.VALUE_LOBBY_DATA_STATE_LOADING,
                DataObject.VisibilityOptions.Member
            );

            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.None,
                MessageBoxPanel.Icon.Loading,
                UIManagerUnrankedLobby.Instance.MessagePendingGame
            );

            await SceneManagerUtility.LoadSceneNetworkAsync(MonopolyApplication.Instance.SceneAssetMonopolyGame, LoadSceneMode.Single);
        }

        private async Task LeaveLobbyAsync()
        {
            if (UIManagerGlobal.Instance != null)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.None,
                    MessageBoxPanel.Icon.Loading,
                    UIManagerUnrankedLobby.Instance?.MessageDisconnecting ?? UIManagerMainMenu.Instance?.MessageDisconnecting
                );
            }

            NetworkManager.Singleton?.Shutdown();

            if (this != null)
                await this.localLobbyEvents?.UnsubscribeAsync();

            if (!this.IsHost && MessageBoxPanelsPool.Instance != null)
            {
                if (this.HasHostLeft)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Error,
                        UIManagerUnrankedLobby.Instance?.MessageHostDisconnected ?? UIManagerMainMenu.Instance?.MessageHostDisconnected
                    );
                }
                else if (!this.HasLocalPlayerLeft)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Error,
                        UIManagerUnrankedLobby.Instance?.MessageKicked ?? UIManagerMainMenu.Instance?.MessageKicked
                    );
                }
            }

            this.IsHost = false;
            this.LocalLobby = null;
            this.HasHostLeft = false;
            this.HasLocalPlayerLeft = false;
        }

        internal async Task<bool> DoesLobbyExistAsync()
        {
            try
            {
                QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(this.queryCurrentLobby);
            }
            catch
            {
                return false;
            }

            return true;
        }

        internal async Task DisconnectFromLobbyAsync()
        {
            this.HasLocalPlayerLeft = true;

            if (this.LocalLobby == null)
                return;

            if (this.IsHost)
            {
                if (this != null)
                    this.StopAllCoroutines();

                if (await this.DoesLobbyExistAsync())
                    await LobbyService.Instance?.DeleteLobbyAsync(this.LocalLobby?.Id);
                else
                    await this.LeaveLobbyAsync();
            }
            else
            {
                if (await this.DoesLobbyExistAsync())
                    await LobbyService.Instance?.RemovePlayerAsync(this.LocalLobby.Id, GameCoordinator.Instance.LocalPlayer.Id);
                else
                    await this.LeaveLobbyAsync();
            }
        }

        internal async Task KickFromLobbyAsync(string playerId)
        {
            await LobbyService.Instance.RemovePlayerAsync(this.LocalLobby.Id, playerId);
        }

        private void OnLobbyDeleted()
        {
            this.HasHostLeft = true;
        }

        private async void OnKickedFromLobbyAsync()
        {
            await this.LeaveLobbyAsync();
        }

        private async void OnTransportFailureAsync()
        {
            await this.DisconnectFromLobbyAsync();
        }

        private void OnPlayerLeft(List<int> leftPlayers)
        {
            foreach (int playerIndex in leftPlayers)
            {
                this.LocalLobby.Players.RemoveAt(playerIndex);
            }
        }

        private void OnPlayerJoined(List<LobbyPlayerJoined> joinedPlayers)
        {
            foreach (LobbyPlayerJoined newPlayer in joinedPlayers)
            {
                this.LocalLobby.Players.Add(newPlayer.Player);
            }
        }

        private void OnDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> changedLobbyData)
        {
            foreach (string key in changedLobbyData.Keys)
            {
                this.LocalLobby.Data[key] = changedLobbyData[key].Value;
            }

            switch (this.LocalLobby.Data[LobbyManager.KEY_LOBBY_DATA_STATE].Value)
            {
                case LobbyManager.VALUE_LOBBY_DATA_STATE_LOADING:
                case LobbyManager.VALUE_LOBBY_DATA_STATE_PENDING:
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.None,
                        MessageBoxPanel.Icon.Loading,
                        UIManagerUnrankedLobby.Instance?.MessagePendingGame ?? UIManagerGame.Instance?.MessageWaitingOtherPlayers,
                        stateHandler: () => this.LocalLobby.ArePlayersSynchronized(GameCoordinator.KEY_PLAYER_DATA_SCENE, SceneManager.GetActiveScene().name)
                    );
                    break;
                case LobbyManager.VALUE_LOBBY_DATA_STATE_RETURNING:
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.None,
                        MessageBoxPanel.Icon.Loading,
                        UIManagerUnrankedLobby.Instance?.MessageFailedToConnect ?? UIManagerGame.Instance?.MessagePlayersFailedToLoad,
                        stateHandler: () => this.LocalLobby.ArePlayersSynchronized(GameCoordinator.KEY_PLAYER_DATA_SCENE, SceneManager.GetActiveScene().name));
                    break;
            }
        }

        private void OnPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> changedPlayerData)
        {
            foreach (int playerIndex in changedPlayerData.Keys)
            {
                foreach (string key in changedPlayerData[playerIndex].Keys)
                {
                    this.LocalLobby.Players[playerIndex].Data[key] = changedPlayerData[playerIndex][key].Value;
                }
            }
        }

        // private async void OnGameLobbyLoadedAsync()
        // {
        //     if (this.IsHost)
        //     {
        //         await this.UpdateLocalLobbyDataAsync(LobbyManager.VALUE_LOBBY_DATA_STATE_IDLE, false);
        //     }
        // }

        // private async void OnMonopolyGameLoadedAsync()
        // {
        //     if (this.IsHost)
        //     {
        //         await this.UpdateLocalLobbyDataAsync(LobbyManager.VALUE_LOBBY_DATA_STATE_GAME, true);
        //     }
        // }

        // private async void OnMonopolyGameFailedToLoadAsync()
        // {
        //     if (this.IsHost)
        //     {
        //         await this.UpdateLocalLobbyDataAsync(LobbyManager.VALUE_LOBBY_DATA_STATE_RETURNING, true);
        //         await SceneManagerUtility.LoadSceneNetworkAsync(MonopolyApplication.Instance.SceneAssetLobbyUnranked, LoadSceneMode.Single);
        //     }
        // }

        // private async void OnGameLobbyFailedToLoadAsync()
        // {
        //     await this.DisconnectFromLobbyAsync();
        // }
    }
}
