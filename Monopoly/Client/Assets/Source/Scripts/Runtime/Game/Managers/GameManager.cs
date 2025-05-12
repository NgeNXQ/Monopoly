using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Managers.Gameplay;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Gameplay;
using Monopoly.Client.Scriptable.Objects.Tokens;
using Monopoly.Client.Scriptable.Objects.Cards.Chance;
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
        private GameObject pawnPanel;

        [SerializeField]
        private CardChanceScriptableObject[] cardsTax;

        [SerializeField]
        private CardChanceScriptableObject[] cardsChance;

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
        private IList<PawnController> pawns;
        private IList<PawnPanel> panelsPawn;

        private int nextPawnIndex => ++this.currentPawnIndex % this.pawns.Count;

        // private ulong[] targetAllClients;
        // private ulong[] targetOtherClients;
        // private ulong[] targetAllDefaultClients;
        // private IDictionary<int, ulong[]> targetAllClientsExcludingCurrentPlayer;

        internal int PawnsCount => this.pawns.Count;
        internal int FirstDieValue { get; private set; }
        internal int SecondDieValue { get; private set; }
        // internal int CurrentPawnIndex { get; private set; }
        // internal IList<TokenScriptableObject> PawnsVisuals { get; private set; }
        internal int TotalRollResult => this.FirstDieValue + this.SecondDieValue;
        internal bool HasRolledDouble => this.FirstDieValue == this.SecondDieValue;

        // private NetworkVariable<int> CurrentPawnIndex;

        internal PawnController CurrentPawn
        {
            get
            {
                if (this.currentPawnIndex >= 0 && this.currentPawnIndex < this.pawns.Count)
                    return this.pawns[this.currentPawnIndex];
                else
                    return null;
            }
        }

        // internal ServerRpcParams SenderLocalClient
        // {
        //     get
        //     {
        //         return new ServerRpcParams
        //         {
        //             Receive = new ServerRpcReceiveParams { SenderClientId = NetworkManager.Singleton.LocalClientId }
        //         };
        //     }
        // }

        // internal ClientRpcParams TargetAllClients
        // {
        //     get
        //     {
        //         return new ClientRpcParams
        //         {
        //             Send = new ClientRpcSendParams { TargetClientIds = this.targetAllClients }
        //         };
        //     }
        // }

        // internal ClientRpcParams TargetOtherClients
        // {
        //     get
        //     {
        //         return new ClientRpcParams
        //         {
        //             Send = new ClientRpcSendParams { TargetClientIds = this.targetOtherClients }
        //         };
        //     }
        // }

        // internal ClientRpcParams TargetAllDefaultClients
        // {
        //     get
        //     {
        //         return new ClientRpcParams
        //         {
        //             Send = new ClientRpcSendParams { TargetClientIds = this.targetAllDefaultClients }
        //         };
        //     }
        // }

        // internal ClientRpcParams TargetAllClientsExcludingCurrentPlayer
        // {
        //     get
        //     {
        //         return new ClientRpcParams
        //         {
        //             Send = new ClientRpcSendParams { TargetClientIds = this.targetAllClientsExcludingCurrentPlayer[this.CurrentPawnIndex] }
        //         };
        //     }
        // }

        private void Awake()
        {
            if (GameManager.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            GameManager.Instance = this;
        }

        private void Start()
        {
            // GameCoordinator.Instance.LocalPlayer.SetDataCurrentSceneName(SceneManager.GetActiveScene().name);

            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.None,
                MessageBoxPanel.Icon.Loading,
                UIManagerGame.Instance.MessageWaitingOtherPlayers,
                () => LobbyManager.Instance.LocalLobby.ArePlayersSynchronized(GameCoordinator.KEY_PLAYER_DATA_SCENE, SceneManager.GetActiveScene().name)
            );

            this.pawns = new List<PawnController>();
            this.panelsPawn = new List<PawnPanel>();
            // this.PawnsVisuals = new List<TokenScriptableObject>(this.tokens);

            // foreach (var player in LobbyManager.Instance.LocalLobby.Players)
            // {
            //     UnityEngine.Debug.Log(player.Data[LobbyManager.KEY_PLAYER_DATA_SCENE].Value);
            // }

            // Debug.Log(GameCoordinator.Instance.LocalPlayer.Data[LobbyManager.KEY_PLAYER_DATA_SCENE].Value);

            if (LobbyManager.Instance.IsHost)
                this.StartCoroutine(this.WaitOtherPlayersCoroutine());

            // GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
        }

        private void OnEnable()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback += this.OnClientDisconnected;
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback -= this.OnClientDisconnected;
        }

        private async void OnClientDisconnected(ulong disconnectedClientId)
        {
            // if (NetworkManager.Singleton.IsHost)
            // {
            //     this.targetAllClients = this.targetAllClients.Where(clientId => clientId != disconnectedClientId).ToArray();
            //     this.targetOtherClients = this.targetOtherClients.Where(clientId => clientId != disconnectedClientId).ToArray();
            //     this.targetAllDefaultClients = this.targetAllDefaultClients.Where(clientId => clientId != disconnectedClientId).ToArray();

            //     this.RemoveSurrenderedPawn(this.pawns.Where(pawn => pawn.OwnerClientId == disconnectedClientId).First().NetworkIndex);
            // }
            // else
            // {
            //     if (disconnectedClientId != GameManager.CLIENT_ID_HOST)
            //         return;

            //     if (LobbyManager.Instance != null && await LobbyManager.Instance.DoesLobbyExistAsync())
            //     {
            //         await LobbyManager.Instance.DisconnectFromLobbyAsync();

            //         UIManagerGlobal.Instance?.ShowMessageBox(
            //             MessageBoxPanel.Type.OK,
            //             MessageBoxPanel.Icon.Error,
            //             UIManagerGame.Instance.MessageHostDisconnected
            //         );
            //     }
            // }
        }

        internal void RemoveSurrenderedPawn(int networkIndex)
        {
            // if (this.CurrentPawn.NetworkIndex == networkIndex)
            //     this.SwitchPlayerForcefullyServerRpc(this.SenderLocalClient);

            // this.targetAllClientsExcludingCurrentPlayer.Remove(networkIndex);
            // this.RemoveSurrenderedPawnClientRpc(networkIndex, this.TargetAllClients);
        }

        // [Rpc(SendTo.Everyone)]
        [ClientRpc]
        private void RemoveSurrenderedPawnClientRpc(int networkIndex, ClientRpcParams clientRpcParams)
        {
            this.pawns.Remove(this.pawns.Where(pawn => pawn.NetworkIndex == networkIndex).First());
            this.panelsPawn.Remove(this.panelsPawn.Where(pawnPanel => pawnPanel.NetworkIndex == networkIndex).First());

            if (this.pawns.Count == 1)
            {
                UIManagerGame.Instance.ShowButtonDisconnect();

                UIManagerGame.Instance.HidePanelTileManagement();
                UIManagerGame.Instance.HidePanelTradeSender();
                UIManagerGame.Instance.HidePanelNodeOffer();
                UIManagerGame.Instance.HideButtonRollDice();
                UIManagerGame.Instance.HidePanelTilePayment();
                UIManagerGame.Instance.HidePanelTradeReceiver();
                UIManagerGame.Instance.HidePanelChancePayment();

                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Trophy,
                    $"{UIManagerGame.Instance.MessageWon} {this.pawns.First().Nickname}"
                );
            }
        }

        private IEnumerator WaitOtherPlayersCoroutine()
        {
            float elapsedTime = 0.0f;

            while (!LobbyManager.Instance.LocalLobby.ArePlayersSynchronized(GameCoordinator.KEY_PLAYER_DATA_SCENE, SceneManager.GetActiveScene().name))
            {
                // Debug.Log(SceneManager.GetActiveScene().name);

                elapsedTime += Time.deltaTime;

                // if (elapsedTime > LobbyManager.LOBBY_LOADING_TIMEOUT)
                //     LobbyManager.Instance?.MonopolyGameFailedToLoadEvent?.Invoke();

                yield return null;
            }

            // if (!LobbyManager.Instance.LocalLobby.HaveAllPlayersLoaded(SceneManager.GetActiveScene().name))
            // else
            this.InitializeGameSession();
        }

        private void InitializeGameSession()
        {
            // this.targetAllClientsExcludingCurrentPlayer = new Dictionary<int, ulong[]>();
            // this.targetAllClients = new ulong[NetworkManager.Singleton.ConnectedClients.Count];
            // this.targetOtherClients = new ulong[NetworkManager.Singleton.ConnectedClients.Count - 1];
            // this.targetAllDefaultClients = new ulong[NetworkManager.Singleton.ConnectedClients.Count - 1];

            // int defaultClientsCount = NetworkManager.Singleton.ConnectedClients.Count - 1;

            // for (int i = 0; i < defaultClientsCount; ++i)
            //     this.targetAllDefaultClients[i] = NetworkManager.Singleton.ConnectedClientsIds[i + 1];

            for (int i = 0; i < NetworkManager.Singleton?.ConnectedClients.Count; ++i)
            {
                // this.targetAllClients[i] = NetworkManager.Singleton.ConnectedClientsIds[i];
                // this.targetAllClientsExcludingCurrentPlayer.Add(i, NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != NetworkManager.Singleton.ConnectedClientsIds[i]).ToArray());

                GameObject newPlayer = GameObject.Instantiate(this.player);
                GameObject newPlayerPanel = GameObject.Instantiate(this.pawnPanel);
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
                newPlayerPanel.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[i], true);
            }

            // for (int i = this.pawns.Count; i < LobbyManager.MAX_PLAYERS; ++i)
            // {
            //     this.targetAllClientsExcludingCurrentPlayer.Add(i, NetworkManager.Singleton.ConnectedClientsIds.ToArray());

            //     GameObject newBot = GameObject.Instantiate(this.bot);
            //     GameObject newBotPanel = GameObject.Instantiate(this.pawnPanel);
            //     newBot.GetComponent<NetworkObject>().SpawnWithOwnership(GameManager.CLIENT_ID_HOST, true);
            //     newBotPanel.GetComponent<NetworkObject>().SpawnWithOwnership(GameManager.CLIENT_ID_HOST, true);
            // }

            this.currentPawnIndex = 0;
            this.CurrentPawn.PerformTurn();
        }

        internal void SwitchPawn()
        {
            this.SwitchPawnRemotelyRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void SwitchPawnRemotelyRpc()
        {
            if (this.HasRolledDouble)
            {
                ++this.rolledDoublesCount;

                if (this.rolledDoublesCount >= this.MaxDoublesSequence)
                {
                    this.rolledDoublesCount = 0;
                    // this.SendCurrentPawnToJailClientRpc(this.TargetAllClients);
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

        internal void SwitchPawnForcefully()
        {
            this.SwitchPlayerForcefullyRemotelyRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void SwitchPlayerForcefullyRemotelyRpc()
        {
            this.rolledDoublesCount = 0;
            this.SwitchPawnLocallyRpc(this.nextPawnIndex);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        private void SwitchPawnLocallyRpc(int currentPawnIndex)
        {
            this.currentPawnIndex = currentPawnIndex;

            if (this.CurrentPawn.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                this.CurrentPawn.PerformTurn();
        }

        internal CardChanceScriptableObject GetCardChance()
        {
            return this.cardsChance[UnityEngine.Random.Range(0, this.cardsChance.Length)];
        }







        [ClientRpc]
        private void SendCurrentPawnToJailClientRpc(ClientRpcParams clientRpcParams)
        {
            if (this.CurrentPawn.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                this.CurrentPawn.GoToJail();
        }

        internal void AddPawnController(PawnController pawn)
        {
            this.pawns.Add(pawn);
        }

        internal void AddPawnPanel(PawnPanel pawnPanel)
        {
            this.panelsPawn.Add(pawnPanel);
        }

        internal PawnPanel GetPawnPanel(int networkIndex)
        {
            return this.panelsPawn.Where(pawnPanel => pawnPanel.NetworkIndex == networkIndex).FirstOrDefault();
        }

        internal PawnController GetPawnController(int networkIndex)
        {
            return this.pawns.Where(pawn => pawn.NetworkIndex == networkIndex).FirstOrDefault();
        }

        internal void RollDice()
        {
            const int MIN_DIE_VALUE = 1;
            const int MAX_DIE_VALUE = 6;

            this.FirstDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);
            this.SecondDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);

            // this.RollDiceServerRpc(this.FirstDieValue, this.SecondDieValue, this.SenderLocalClient);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RollDiceServerRpc(int firstDieValue, int secondDieValue, ServerRpcParams serverRpcParams)
        {
            this.FirstDieValue = firstDieValue;
            this.SecondDieValue = secondDieValue;

            // this.RollDiceClientRpc(firstDieValue, secondDieValue, this.TargetAllDefaultClients);
        }

        [ClientRpc]
        private void RollDiceClientRpc(int firstDieValue, int secondDieValue, ClientRpcParams clientRpcParams)
        {
            this.FirstDieValue = firstDieValue;
            this.SecondDieValue = secondDieValue;
        }
    }
}
