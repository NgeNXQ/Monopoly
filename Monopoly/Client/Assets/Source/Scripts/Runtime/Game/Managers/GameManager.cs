using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Client.Runtime.Game.Models;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;
// using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Scriptable.Objects.Tokens;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Unity.Services.Lobbies.Models.Extensions;

namespace Monopoly.Client.Runtime.Game.Managers
{
    internal sealed class GameManager : NetworkBehaviour
    {
        [SerializeField]
        private GameObject bot;

        [SerializeField]
        private GameObject player;

        [SerializeField]
        private GameObject pawnTable;

        [SerializeField]
        private CardScriptableObject[] cardsTax;

        [SerializeField]
        private CardScriptableObject[] cardsChance;

        [SerializeField, Range(0, LobbyManager.MAX_PLAYERS)]
        private int firstTurnPawnIndex = 0;

        [field: SerializeField, Range(0, Byte.MaxValue)]
        internal int MaxJailTurns { get; private set; } = 3;

        [field: SerializeField, Range(0, Byte.MaxValue)]
        internal int MaxDoublesSequence { get; private set; } = 2;

        [field: SerializeField, Range(0, Int16.MaxValue)]
        internal int InitialBalance { get; private set; } = 15_000;

        [field: SerializeField, Range(0, Int16.MaxValue)]
        internal int CircleSuperBonus { get; private set; } = 3_000;

        [field: SerializeField, Range(0, Int16.MaxValue)]
        internal int CircleDefaultBonus { get; private set; } = 2_000;

        [field: SerializeField]
        internal TokenScriptableObject[] Tokens { get; private set; } = new TokenScriptableObject[5];

        private const ulong CLIENT_ID_HOST = 0;

        internal static GameManager Instance { get; private set; }

        private int currentPawnIndex;
        private int rolledDoublesCount;
        private List<PawnDataHandler> pawns;

        internal int PawnsCount => this.pawns.Count;
        internal int FirstDieValue { get; private set; }
        internal int SecondDieValue { get; private set; }
        private int nextPawnIndex => ++this.currentPawnIndex % this.pawns.Count;
        internal int TotalRollResult => this.FirstDieValue + this.SecondDieValue;
        internal bool HasRolledDouble => this.FirstDieValue == this.SecondDieValue;

        internal PawnController CurrentPawn
        {
            get
            {
                if (this.currentPawnIndex >= 0 && this.currentPawnIndex < this.pawns.Count)
                    return this.pawns[this.currentPawnIndex].Pawn;
                else
                    return null;
            }
        }

        private void Awake()
        {
            if (GameManager.Instance != null)
                throw new InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            GameManager.Instance = this;
        }

        private void OnEnable()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback += this.OnClientDisconnected;
        }

        private void Start()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxView.Type.None,
                MessageBoxView.Icon.Loading,
                UIManagerGame.Instance.MessageWaitingOtherPlayers,
                () => LobbyManager.Instance.LocalLobby.ArePlayersSynchronized(GameCoordinator.KEY_PLAYER_DATA_SCENE, SceneManager.GetActiveScene().name)
            );

            this.pawns = new List<PawnDataHandler>();

            if (LobbyManager.Instance.IsHost)
                this.StartCoroutine(this.WaitOtherPlayersCoroutine());
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback -= this.OnClientDisconnected;
        }

        private async void OnClientDisconnected(ulong disconnectedClientId)
        {
            if (NetworkManager.Singleton.IsHost)
                this.pawns?.Where(data => data.ClientId == disconnectedClientId).FirstOrDefault()?.Pawn?.Surrender();

            if (disconnectedClientId != GameManager.CLIENT_ID_HOST)
                return;

            if (LobbyManager.Instance != null && await LobbyManager.Instance.DoesLobbyExistAsync())
            {
                await LobbyManager.Instance.DisconnectFromLobbyAsync();

                // UIManagerGlobal.Instance?.ShowMessageBox(
                //     MessageBoxView.Type.OK,
                //     MessageBoxView.Icon.Error,
                //     UIManagerGame.Instance.MessageHostDisconnected
                // );
            }
        }

        private IEnumerator WaitOtherPlayersCoroutine()
        {
            float elapsedTime = 0.0f;

            while (!LobbyManager.Instance.LocalLobby.ArePlayersSynchronized(GameCoordinator.KEY_PLAYER_DATA_SCENE, SceneManager.GetActiveScene().name))
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            this.InitializeGameSession();
        }

        private void InitializeGameSession()
        {
            for (int i = 0; i < NetworkManager.Singleton?.ConnectedClients.Count; ++i)
            {
                ulong clientId = NetworkManager.Singleton.ConnectedClientsIds[i];

                GameObject newPlayer = GameObject.Instantiate(this.player);
                GameObject newTable = GameObject.Instantiate(this.pawnTable);
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
                newTable.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
            }

            string gameMode = LobbyManager.Instance.LocalLobby.GetData(LobbyManager.KEY_LOBBY_DATA_GAME_MODE);

            if (gameMode.Equals(LobbyManager.VALUE_LOBBY_DATA_GAME_MODE_UNRANKED, StringComparison.OrdinalIgnoreCase))
            {
                for (int i = this.pawns.Count; i < LobbyManager.MAX_PLAYERS; ++i)
                {
                    GameObject newBot = GameObject.Instantiate(this.bot);
                    GameObject newTable = GameObject.Instantiate(this.pawnTable);
                    newBot.GetComponent<NetworkObject>().SpawnWithOwnership(GameManager.CLIENT_ID_HOST, true);
                    newTable.GetComponent<NetworkObject>().SpawnWithOwnership(GameManager.CLIENT_ID_HOST, true);
                }
            }

            this.currentPawnIndex = 0;
            this.CurrentPawn.PerformTurn();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void DespawnPawnItemsRemotelyRpc(int pawnId)
        {
            PawnController pawnController = this.pawns.Where(data => data.Pawn.PawnId == pawnId).FirstOrDefault().Pawn;
            NetworkObject pawnControllerNetworkObject = pawnController?.GetComponent<NetworkObject>();

            if (pawnControllerNetworkObject.IsSpawned)
                pawnControllerNetworkObject.Despawn(true);

            PawnTableView pawnTable = this.pawns.Where(data => data.Table.Owner.PawnId == pawnId).FirstOrDefault().Table;
            NetworkObject pawnTableNetworkObject = pawnTable?.GetComponent<NetworkObject>();

            if (pawnTableNetworkObject.IsSpawned)
                pawnTableNetworkObject.Despawn(true);

            this.RemovePawnItemsLocallyRpc(pawnId);
        }

        [Rpc(SendTo.Everyone)]
        private void RemovePawnItemsLocallyRpc(int pawnId)
        {
            if (PlayerPawnController.LocalInstance.PawnId == pawnId)
            {
                UIManagerGame.Instance.HideAllControls();
                UIManagerGame.Instance.ShowButtonDisconnect();
            }

            this.pawns.Remove(this.pawns.Where(data => data.Pawn.PawnId == pawnId).FirstOrDefault());

            if (this.PawnsCount == 1)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Trophy,
                    $"{UIManagerGame.Instance.MessageWon} {this.pawns.First().Pawn.Nickname}"
                );

                if (PlayerPawnController.LocalInstance == this.pawns.First().Pawn)
                {
                    UIManagerGame.Instance.HideAllControls();
                    UIManagerGame.Instance.ShowButtonDisconnect();

                    string gameMode = LobbyManager.Instance.LocalLobby.GetData(LobbyManager.KEY_LOBBY_DATA_GAME_MODE);

                    if (gameMode.Equals(LobbyManager.VALUE_LOBBY_DATA_GAME_MODE_RANKED, StringComparison.OrdinalIgnoreCase))
                    {
                        int trophies = int.Parse(GameCoordinator.Instance.LocalPlayer.GetData(GameCoordinator.KEY_PLAYER_DATA_TROPHIES));
                        GameCoordinator.Instance.ServiceAccount.PutAccountTrophies($"{trophies + new System.Random().Next(5, 15)}");
                    }
                }
            }
        }

        internal PawnController GetPawnController(int pawnId)
        {
            return this.pawns.FirstOrDefault(data => data.Pawn?.PawnId == pawnId)?.Pawn;
        }

        internal PawnController GetPawnController(ulong clientId)
        {
            return this.pawns.FirstOrDefault(data => data.Pawn?.OwnerClientId == clientId)?.Pawn;
        }

        internal void RegisterPawnTable(PawnTableView table)
        {
            // var existing = 
            this.pawns.FirstOrDefault(data => data.Pawn == table.Owner).Table = table;

            // if (existing != null)
            // {
            //     existing.Table = table;
            // }
            // else
            // {
            //     this.pawns.Add(new PawnDataHandler
            //     {
            //         ClientId = table.Owner.OwnerClientId,
            //         Table = table,
            //         Pawn = null
            //     });
            // }
        }

        internal void RegisterPawnController(PawnController pawnController)
        {
            this.pawns.Add(new PawnDataHandler
            {
                ClientId = pawnController.OwnerClientId,
                Pawn = pawnController,
                Table = null,
            });

            // var existing = this.pawns.FirstOrDefault(data => data.Pawn.OwnerClientId == pawnController.OwnerClientId);

            // if (existing != null)
            // {
            //     existing.Pawn = pawnController;
            // }
            // else
            // {
            //     this.pawns.Add(new PawnDataHandler
            //     {
            //         ClientId = pawnController.OwnerClientId,
            //         Pawn = pawnController,
            //         Table = null,
            //     });
            // }
        }

        internal void SwitchPawnSoftly()
        {
            this.SwitchPawnSoftlyRemotelyRpc();
        }

        internal void SwitchPawnForcefully()
        {
            this.SwitchPlayerForcefullyRemotelyRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void SwitchPawnSoftlyRemotelyRpc()
        {
            if (this.HasRolledDouble)
            {
                ++this.rolledDoublesCount;

                if (this.rolledDoublesCount >= this.MaxDoublesSequence)
                {
                    this.rolledDoublesCount = 0;
                    this.SendCurrentPawnToJailLocallyRpc();
                }
                else
                {
                    this.SwitchPawnLocallyRpc(this.currentPawnIndex);
                }
            }
            else
            {
                this.rolledDoublesCount = 0;
                this.SwitchPawnLocallyRpc(this.nextPawnIndex);
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void SwitchPlayerForcefullyRemotelyRpc()
        {
            this.rolledDoublesCount = 0;
            this.SwitchPawnLocallyRpc(this.nextPawnIndex);
        }

        [Rpc(SendTo.Everyone)]
        private void SwitchPawnLocallyRpc(int currentPawnIndex)
        {
            this.currentPawnIndex = currentPawnIndex;

            if (this.CurrentPawn.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                this.CurrentPawn.PerformTurn();
        }

        [Rpc(SendTo.Everyone)]
        private void SendCurrentPawnToJailLocallyRpc()
        {
            if (this.CurrentPawn.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                this.CurrentPawn.MoveToJail();
        }

        internal CardScriptableObject GetCardTax()
        {
            return this.cardsChance[UnityEngine.Random.Range(0, this.cardsChance.Length)];
        }

        internal CardScriptableObject GetCardChance()
        {
            return this.cardsChance[UnityEngine.Random.Range(0, this.cardsChance.Length)];
        }

        internal void RollDice()
        {
            this.RollDiceRemotelyRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void RollDiceRemotelyRpc()
        {
            const int MIN_DIE_VALUE = 1;
            const int MAX_DIE_VALUE = 6;

            this.FirstDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);
            this.SecondDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);

            this.RollDiceLocallyRpc(this.FirstDieValue, this.SecondDieValue);
        }

        [Rpc(SendTo.NotServer)]
        private void RollDiceLocallyRpc(int firstDieValue, int secondDieValue)
        {
            this.FirstDieValue = firstDieValue;
            this.SecondDieValue = secondDieValue;
        }
    }
}
