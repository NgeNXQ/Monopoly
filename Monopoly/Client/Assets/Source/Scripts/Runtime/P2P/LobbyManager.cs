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
using Monopoly.Client.Runtime.UI.Managers;
using Monopoly.Client.Runtime.UI.Panels.Concrete;
using Monopoly.Client.Runtime.UI.Utilities.Pools.Concrete;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;
using Monopoly.Client.Runtime.Core.Models;
using Monopoly.Client.Runtime.Core.Utilities;

namespace Monopoly.Client.Runtime.P2P
{
    internal sealed class LobbyManager : MonoBehaviour
    {
        private const float LOBBY_UPTIME = 25.0f;

        internal const int MIN_PLAYERS = 1;
        internal const int MAX_PLAYERS = 5;

        internal const float LOBBY_LOADING_TIMEOUT = 15.0f;

        internal const string KEY_PLAYER_SCENE = "Scene";
        internal const string KEY_PLAYER_NICKNAME = "Nickname";
        internal const string KEY_LOBBY_STATE = "State";
        internal const string LOBBY_STATE_GAME = "Game";
        internal const string LOBBY_STATE_LOBBY = "Lobby";
        internal const string LOBBY_STATE_LOADING = "Loading";
        internal const string LOBBY_STATE_PENDING = "Waiting";
        internal const string LOBBY_STATE_RETURNING = "Returning";

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

        internal Action GameLobbyLoadedEvent;
        internal Action MonopolyGameLoadedEvent;
        internal Action GameLobbyFailedToLoadEvent;
        internal Action MonopolyGameFailedToLoadEvent;

        internal bool HavePlayersLoaded
        {
            get
            {
                return false;
                // return this.LocalLobby != null ? this.LocalLobby.Players.All(player => player.Data[LobbyManager.KEY_PLAYER_SCENE].Value.Equals(GameCoordinator.Instance.ActiveScene.ToString(), StringComparison.Ordinal)) : false;
            }
        }

        internal bool IsHost { get; private set; }
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
            this.LocalLobbyEventCallbacks = new LobbyEventCallbacks();

            this.GameLobbyLoadedEvent += this.OnGameLobbyLoaded;
            this.MonopolyGameLoadedEvent += this.OnMonopolyGameLoaded;
            this.GameLobbyFailedToLoadEvent += this.OnGameLobbyFailedToLoad;
            this.MonopolyGameFailedToLoadEvent += this.OnMonopolyGameFailedToLoad;

            this.LocalLobbyEventCallbacks.PlayerLeft += this.OnPlayerLeft;
            this.LocalLobbyEventCallbacks.DataChanged += this.OnDataChanged;
            this.LocalLobbyEventCallbacks.LobbyDeleted += this.OnLobbyDeleted;
            this.LocalLobbyEventCallbacks.PlayerJoined += this.OnPlayerJoined;
            this.LocalLobbyEventCallbacks.KickedFromLobby += this.OnKickedFromLobby;
            this.LocalLobbyEventCallbacks.PlayerDataChanged += this.OnPlayerDataChanged;

            NetworkManager.Singleton.OnTransportFailure += this.OnTransportFailure;
        }

        private void OnDisable()
        {
            this.LocalLobbyEventCallbacks = new LobbyEventCallbacks();

            this.GameLobbyLoadedEvent -= this.OnGameLobbyLoaded;
            this.MonopolyGameLoadedEvent -= this.OnMonopolyGameLoaded;
            this.GameLobbyFailedToLoadEvent -= this.OnGameLobbyFailedToLoad;
            this.MonopolyGameFailedToLoadEvent -= this.OnMonopolyGameFailedToLoad;

            this.LocalLobbyEventCallbacks.PlayerLeft -= this.OnPlayerLeft;
            this.LocalLobbyEventCallbacks.DataChanged -= this.OnDataChanged;
            this.LocalLobbyEventCallbacks.LobbyDeleted -= this.OnLobbyDeleted;
            this.LocalLobbyEventCallbacks.PlayerJoined -= this.OnPlayerJoined;
            this.LocalLobbyEventCallbacks.KickedFromLobby -= this.OnKickedFromLobby;
            this.LocalLobbyEventCallbacks.PlayerDataChanged -= this.OnPlayerDataChanged;

            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnTransportFailure -= this.OnTransportFailure;
        }

        private async void OnDestroy()
        {
            if (this.IsHost)
            {
                this.StopCoroutine(this.PingLobbyCoroutine());
            }

            if (this.LocalLobby != null)
            {
                await this.DisconnectFromLobbyAsync();
            }
        }

        internal bool HasPlayerWithId(string playerId)
        {
            if (this.LocalLobby == null)
                return false;

            return this.LocalLobby.Players.Any(player => player.Id.Equals(playerId, StringComparison.Ordinal));
        }

        internal void StartGame()
        {
            if (!this.HavePlayersLoaded)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Warning,
                    UIManagerUnrankedLobby.Instance.MessageNotAllPlayersLoaded
                );
                return;
            }

            if (this.LocalLobby.Players.Count < LobbyManager.MIN_PLAYERS)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Warning,
                    UIManagerUnrankedLobby.Instance.MessageTooFewPlayers
                );
                return;
            }

            Task.Run(async () => await this.UpdateLocalLobbyDataAsync(LobbyManager.LOBBY_STATE_LOADING, true));

            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.None,
                MessageBoxPanel.Icon.Loading,
                UIManagerUnrankedLobby.Instance.MessagePendingGame
            );

            // GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.MonopolyGame);
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
            catch (LobbyServiceException)
            {
                return false;
            }
            catch (NullReferenceException)
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
                {
                    this.StopAllCoroutines();
                }

                if (await this.DoesLobbyExistAsync())
                {
                    await LobbyService.Instance.DeleteLobbyAsync(this.LocalLobby.Id);
                }
                else
                {
                    await this.LeaveLobbyAsync();
                }
            }
            else
            {
                if (await this.DoesLobbyExistAsync())
                {
                    await LobbyService.Instance.RemovePlayerAsync(this.LocalLobby.Id, GameCoordinator.Instance.LocalPlayer.Id);
                }
                else
                {
                    await this.LeaveLobbyAsync();
                }
            }
        }

        internal async Task HostLobbyAsync(string relayCode)
        {
            this.IsHost = true;
            this.HasHostLeft = false;
            this.JoinCode = relayCode;
            this.HasLocalPlayerLeft = false;

            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions()
            {
                Player = GameCoordinator.Instance.LocalPlayer,
                Data = new Dictionary<string, DataObject>()
                {
                    { LobbyManager.KEY_LOBBY_STATE, new DataObject(DataObject.VisibilityOptions.Member, LobbyManager.LOBBY_STATE_LOBBY) }
                }
            };

            try
            {
                this.LocalLobby = await LobbyService.Instance.CreateLobbyAsync(this.lobbyName, LobbyManager.MAX_PLAYERS, lobbyOptions);
                this.localLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(this.LocalLobby.Id, this.LocalLobbyEventCallbacks);

                NetworkManager.Singleton?.StartHost();
            }
            catch
            {
                throw;
            }

            if (this != null)
            {
                this.StartCoroutine(this.PingLobbyCoroutine());
                await SceneManagerUtility.LoadSceneDefaultAsync(MonopolyApplication.Instance.SceneAssetLobbyUnranked, LoadSceneMode.Single);
            }
        }

        internal async Task ConnectLobbyAsync(string joinCode)
        {
            this.IsHost = false;
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
            catch (LobbyServiceException lobbyServiceException)
            {
                throw lobbyServiceException;
            }
            catch (NullReferenceException nullReferenceException)
            {
                throw new LobbyServiceException(LobbyExceptionReason.InvalidJoinCode, "Invalid Join Code.", nullReferenceException);
            }
        }

        internal async Task KickFromLobbyAsync(string playerId)
        {
            await LobbyService.Instance.RemovePlayerAsync(this.LocalLobby.Id, playerId);
        }

        private IEnumerator PingLobbyCoroutine()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(LobbyManager.LOBBY_UPTIME);

            while (this.LocalLobby != null)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(this.LocalLobby?.Id);
                yield return waitForSeconds;
            }
        }

        internal async Task UpdateLocalPlayerDataAsync()
        {
            try
            {
                // GameCoordinator.Instance.LocalPlayer.Data[LobbyManager.KEY_PLAYER_SCENE] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GameCoordinator.Instance.ActiveScene.ToString());

                UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
                {
                    Data = GameCoordinator.Instance.LocalPlayer.Data
                };

                this.LocalLobby = await LobbyService.Instance.UpdatePlayerAsync(this.LocalLobby.Id, GameCoordinator.Instance.LocalPlayer.Id, updatePlayerOptions);
            }
            catch (LobbyServiceException)
            {
                await this.DisconnectFromLobbyAsync();
            }
        }

        internal async Task UpdateLocalLobbyDataAsync(string lobbyState, bool isPrivate = true)
        {
            try
            {
                this.LocalLobby.Data[LobbyManager.KEY_LOBBY_STATE] = new DataObject(DataObject.VisibilityOptions.Member, lobbyState);

                UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
                {
                    IsPrivate = isPrivate,
                    Data = this.LocalLobby.Data
                };

                await LobbyService.Instance.UpdateLobbyAsync(this.LocalLobby.Id, updateLobbyOptions);
            }
            catch (LobbyServiceException)
            {
                await this.DisconnectFromLobbyAsync();
            }
        }

        private void OnLobbyDeleted()
        {
            this.HasHostLeft = true;
        }

        private void OnKickedFromLobby()
        {
            Task.Run(async () => await this.LeaveLobbyAsync());
        }

        private void OnTransportFailure()
        {
            Task.Run(async () => await this.DisconnectFromLobbyAsync());
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

            switch (this.LocalLobby.Data[LobbyManager.KEY_LOBBY_STATE].Value)
            {
                case LobbyManager.LOBBY_STATE_LOADING:
                case LobbyManager.LOBBY_STATE_PENDING:
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.None,
                        MessageBoxPanel.Icon.Loading,
                        UIManagerUnrankedLobby.Instance?.MessagePendingGame ?? UIManagerGame.Instance?.MessageWaitingOtherPlayers,
                        stateHandler: () => this.HavePlayersLoaded);
                    break;
                case LobbyManager.LOBBY_STATE_RETURNING:
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.None,
                        MessageBoxPanel.Icon.Loading,
                        UIManagerUnrankedLobby.Instance?.MessageFailedToConnect ?? UIManagerGame.Instance?.MessagePlayersFailedToLoad,
                        stateHandler: () => this.HavePlayersLoaded);
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

        private void OnGameLobbyLoaded()
        {
            if (this.IsHost)
            {
                Task.Run(async () => await this.UpdateLocalLobbyDataAsync(LobbyManager.LOBBY_STATE_LOBBY, false));
            }
        }

        private void OnMonopolyGameLoaded()
        {
            if (this.IsHost)
            {
                Task.Run(async () => await this.UpdateLocalLobbyDataAsync(LobbyManager.LOBBY_STATE_LOBBY, true));
            }
        }

        private void OnMonopolyGameFailedToLoad()
        {
            if (this.IsHost)
            {
                Task.Run(async () => await this.UpdateLocalLobbyDataAsync(LobbyManager.LOBBY_STATE_RETURNING, true));
                // GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.GameLobby);
            }
        }

        private void OnGameLobbyFailedToLoad()
        {
            Task.Run(async () => await this.DisconnectFromLobbyAsync());
        }
    }
}
