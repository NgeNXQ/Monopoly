using System.Threading;
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
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.UI.Managers.Lobby;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Client.Utilities.Scenes;
using Monopoly.Unity.Services.Lobbies.Models.Extensions;

namespace Monopoly.Client.Runtime.Core.P2P
{
    internal sealed class LobbyManager : MonoBehaviour
    {
        private const float CONFIG_UPTIME = 5.0f;

        internal const int MIN_PLAYERS = 1;
        internal const int MAX_PLAYERS = 5;

        internal const float LOBBY_LOADING_TIMEOUT = 15.0f;

        internal const string KEY_LOBBY_DATA_STATE = "state";
        internal const string VALUE_LOBBY_DATA_STATE_IDLE = "state_idle";
        internal const string VALUE_LOBBY_DATA_STATE_GAME = "state_game";
        internal const string VALUE_LOBBY_DATA_STATE_PENDING = "state_pending";
        internal const string VALUE_LOBBY_DATA_STATE_LOADING = "state_loading";
        internal const string VALUE_LOBBY_DATA_STATE_RETURNING = "state_returning";

        internal const string KEY_LOBBY_DATA_RELAY_CODE = "relay_code";

        internal const string KEY_LOBBY_DATA_GAME_MODE = "game_mode";
        internal const string VALUE_LOBBY_DATA_GAME_MODE_RANKED = "game_mode_ranked";
        internal const string VALUE_LOBBY_DATA_GAME_MODE_UNRANKED = "game_mode_unranked";

        // private string lobbyName
        // {
        //     get => $"LOBBY_{this.JoinCode}";
        // }

        private ILobbyEvents localLobbyEvents;
        private CancellationTokenSource heartbeatTokenSource;

        // private QueryLobbiesOptions queryCurrentLobby
        // {
        //     get
        //     {
        //         return new QueryLobbiesOptions()
        //         {
        //             Filters = new List<QueryFilter>()
        //             {
        //                 new QueryFilter(QueryFilter.FieldOptions.Name, this.JoinCode, QueryFilter.OpOptions.CONTAINS)
        //             }
        //         };
        //     }
        // }

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
        internal string LobbyCode { get; private set; }
        internal string RelayCode { get; private set; }
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
                this.StopHeartbeat();

            if (this.LocalLobby != null)
                await this.DisconnectFromLobbyAsync();
        }

        private async void OnApplicationQuit()
        {
            if (this.LocalLobby == null)
                return;

            if (await this.DoesLobbyExistAsync())
                await this.DisconnectFromLobbyAsync();
        }

        private async void OnApplicationPause(bool pause)
        {
            if (this.LocalLobby == null)
                return;

            if (await this.DoesLobbyExistAsync())
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

        internal async Task HostPublicLobbyAsync(string relayCode)
        {
            await this.HostLobby(relayCode, LobbyManager.VALUE_LOBBY_DATA_GAME_MODE_RANKED);
        }

        internal async Task HostPrivateLobbyAsync(string relayCode)
        {
            await this.HostLobby(relayCode, LobbyManager.VALUE_LOBBY_DATA_GAME_MODE_UNRANKED);
        }

        private async Task HostLobby(string relayCode, string gameMode)
        {
            this.IsHost = true;
            this.IsClient = false;
            this.HasHostLeft = false;
            this.HasLocalPlayerLeft = false;

            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions()
            {
                Player = GameCoordinator.Instance.LocalPlayer,
                Data = new Dictionary<string, DataObject>()
                {
                    { LobbyManager.KEY_LOBBY_DATA_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                    { LobbyManager.KEY_LOBBY_DATA_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1) },
                    { LobbyManager.KEY_LOBBY_DATA_STATE, new DataObject(DataObject.VisibilityOptions.Member, LobbyManager.VALUE_LOBBY_DATA_STATE_IDLE) }
                },
            };

            try
            {
                this.LocalLobby = await LobbyService.Instance.CreateLobbyAsync(GameCoordinator.Instance.LocalPlayer.Id, LobbyManager.MAX_PLAYERS, lobbyOptions);
                this.localLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);

                this.RelayCode = relayCode;
                this.LobbyCode = this.LocalLobby.LobbyCode;

                this.StartHeartbeat(this.LocalLobby.Id);
            }
            catch
            {
                throw;
            }
        }

        private void StopHeartbeat()
        {
            this.heartbeatTokenSource?.Cancel();
            this.heartbeatTokenSource?.Dispose();
            this.heartbeatTokenSource = null;
        }

        private void StartHeartbeat(string lobbyId)
        {
            this.StopHeartbeat();

            this.heartbeatTokenSource = new CancellationTokenSource();

            Task.Run(() => this.RunHeartbeatLoopAsync(lobbyId, heartbeatTokenSource.Token));
        }

        private async Task RunHeartbeatLoopAsync(string lobbyId, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (string.IsNullOrEmpty(lobbyId))
                    break;

                await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);

                await Task.Delay((int)(CONFIG_UPTIME * 1000), cancellationToken);
            }
        }

        internal async Task JoinLobbyAsync(string joinCode)
        {
            this.IsHost = false;
            this.IsClient = true;
            this.HasHostLeft = false;
            this.HasLocalPlayerLeft = false;

            JoinLobbyByCodeOptions lobbyOptions = new JoinLobbyByCodeOptions()
            {
                Player = GameCoordinator.Instance.LocalPlayer
            };

            try
            {
                this.LocalLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, lobbyOptions);
                this.localLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);

                this.LobbyCode = this.LocalLobby.LobbyCode;
                this.RelayCode = this.LocalLobby.GetData(LobbyManager.KEY_LOBBY_DATA_RELAY_CODE);
            }
            catch
            {
                throw;
            }
        }

        internal async Task FindLobbyAsync()
        {
            this.IsHost = false;
            this.IsClient = true;
            this.HasHostLeft = false;
            this.HasLocalPlayerLeft = false;

            QuickJoinLobbyOptions queryOptions = new QuickJoinLobbyOptions
            {
                Player = GameCoordinator.Instance.LocalPlayer,
                Filter = new List<QueryFilter>
                {
                    new QueryFilter(
                        QueryFilter.FieldOptions.S1,
                        LobbyManager.VALUE_LOBBY_DATA_GAME_MODE_RANKED,
                        QueryFilter.OpOptions.EQ
                    )
                },
            };

            try
            {
                this.LocalLobby = await LobbyService.Instance.QuickJoinLobbyAsync(queryOptions);
                this.localLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);

                this.LobbyCode = this.LocalLobby.LobbyCode;
                this.RelayCode = this.LocalLobby.GetData(LobbyManager.KEY_LOBBY_DATA_RELAY_CODE);
            }
            catch
            {
                throw;
            }
        }

        internal async void StartGameAsync()
        {
            // if (!this.HavePlayersLoaded)
            // {
            //     UIManagerGlobal.Instance.ShowMessageBox(
            //         MessageBoxView.Type.OK,
            //         MessageBoxView.Icon.Warning,
            //         UIManagerUnrankedLobby.Instance.MessageNotAllPlayersLoaded
            //     );
            //     return;
            // }

            if (this.LocalLobby.Players.Count < LobbyManager.MIN_PLAYERS)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Warning,
                    UIManagerPrivateLobby.Instance.MessageTooFewPlayers
                );
                return;
            }

            await this.LocalLobby.SetData(
                LobbyManager.KEY_LOBBY_DATA_STATE,
                LobbyManager.VALUE_LOBBY_DATA_STATE_LOADING,
                DataObject.VisibilityOptions.Member
            );

            if (this.LocalLobby.GetData(LobbyManager.KEY_LOBBY_DATA_GAME_MODE).Equals(LobbyManager.VALUE_LOBBY_DATA_GAME_MODE_UNRANKED, System.StringComparison.OrdinalIgnoreCase))
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.None,
                    MessageBoxView.Icon.Loading,
                    UIManagerPrivateLobby.Instance.MessagePendingGame
                );
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.None,
                    MessageBoxView.Icon.Loading,
                    UIManagerPublicLobby.Instance.MessagePendingGame
                );
            }

            await SceneManagerUtility.LoadSceneNetworkAsync(MonopolyApplication.Instance.SceneAssetMonopolyGame, LoadSceneMode.Single);
        }

        private async Task LeaveLobbyAsync()
        {
            if (UIManagerGlobal.Instance != null)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.None,
                    MessageBoxView.Icon.Loading,
                    UIManagerPrivateLobby.Instance?.MessageDisconnecting ?? UIManagerMainMenu.Instance?.MessageDisconnecting
                );
            }

            NetworkManager.Singleton?.Shutdown();

            if (this != null)
                await this.localLobbyEvents?.UnsubscribeAsync();

            if (!this.IsHost && MessageBoxViewsPool.Instance != null)
            {
                if (this.HasHostLeft)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Error,
                        UIManagerPrivateLobby.Instance?.MessageHostDisconnected ?? UIManagerMainMenu.Instance?.MessageHostDisconnected
                    );
                }
                else if (!this.HasLocalPlayerLeft)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Error,
                        UIManagerPrivateLobby.Instance?.MessageKicked ?? UIManagerMainMenu.Instance?.MessageKicked
                    );
                }
            }

            this.IsHost = false;
            this.LocalLobby = null;
            this.HasHostLeft = false;
            this.HasLocalPlayerLeft = false;

            await SceneManagerUtility.LoadSceneDefaultAsync(MonopolyApplication.Instance.SceneAssetMainMenu, LoadSceneMode.Single);
        }

        internal async Task<bool> DoesLobbyExistAsync()
        {
            try
            {
                QueryLobbiesOptions queryOptions = new QueryLobbiesOptions()
                {
                    Filters = new List<QueryFilter>()
                    {
                        new QueryFilter(QueryFilter.FieldOptions.S1, this.RelayCode, QueryFilter.OpOptions.EQ)
                    }
                };

                QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
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
                this.LocalLobby.Players.RemoveAt(playerIndex);
        }

        private void OnPlayerJoined(List<LobbyPlayerJoined> joinedPlayers)
        {
            foreach (LobbyPlayerJoined newPlayer in joinedPlayers)
                this.LocalLobby.Players.Add(newPlayer.Player);
        }

        private void OnDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> changedLobbyData)
        {
            foreach (string key in changedLobbyData.Keys)
                this.LocalLobby.Data[key] = changedLobbyData[key].Value;

            switch (this.LocalLobby.Data[LobbyManager.KEY_LOBBY_DATA_STATE].Value)
            {
                case LobbyManager.VALUE_LOBBY_DATA_STATE_LOADING:
                case LobbyManager.VALUE_LOBBY_DATA_STATE_PENDING:
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.None,
                        MessageBoxView.Icon.Loading,
                        UIManagerPrivateLobby.Instance?.MessagePendingGame ?? UIManagerGame.Instance?.MessageWaitingOtherPlayers,
                        stateHandler: () => this.LocalLobby.ArePlayersSynchronized(GameCoordinator.KEY_PLAYER_DATA_SCENE, SceneManager.GetActiveScene().name)
                    );
                    break;
                case LobbyManager.VALUE_LOBBY_DATA_STATE_RETURNING:
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.None,
                        MessageBoxView.Icon.Loading,
                        UIManagerPrivateLobby.Instance?.MessageFailedToConnect ?? UIManagerGame.Instance?.MessagePlayersFailedToLoad,
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
