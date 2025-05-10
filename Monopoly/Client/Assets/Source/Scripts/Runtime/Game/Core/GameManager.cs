using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.UI.Managers;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Game;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;
using Monopoly.Client.Scriptable.Objects.Cards.Chance;

namespace Monopoly.Client.Runtime.Game.Core
{
    internal sealed class GameManager : NetworkBehaviour
    {


        // [Space]
        // [SerializeField]
        // [Range(0.0f, 100.0f)]
        // private float pawnMovementSpeed = 35.0f;

        [field: SerializeField, Space]
        internal int MaxJailTurns { get; private set; } = 3;

        [field: SerializeField, Range(0, 10), Space]
        internal int MaxDoublesSequence { get; private set; } = 2;

        [field: SerializeField, Range(0, 100_000), Space]
        internal int InitialBalance { get; private set; } = 15_000;

        [field: SerializeField, Range(0, 100_000), Space]
        internal int CircleSuperBonus { get; private set; } = 3_000;

        [field: SerializeField, Range(0, 100_000), Space]
        internal int CircleDefaultBonus { get; private set; } = 2_000;


        // internal float PawnMovementSpeed => this.pawnMovementSpeed;

        // [Space]
        // [SerializeField]
        // private List<CardChanceScriptableObject> taxNodes = new List<CardChanceScriptableObject>();

        [SerializeField, Space]
        private List<CardChanceScriptableObject> cardsChance = new List<CardChanceScriptableObject>();

        [Space]
        [Header("Visuals")]

        [Space]
        [Header("Pawns")]

        [Space]
        [SerializeField]
        private GameObject bot;

        [Space]
        [SerializeField]
        private GameObject player;

        [Space]
        [SerializeField]
        private GameObject pawnPanel;

        [Space]
        [SerializeField]
        private PawnVisuals[] pawnsVisuals = new PawnVisuals[5];

        private const ulong CLIENT_ID_HOST = 0;

        internal static GameManager Instance { get; private set; }

        private int rolledDoublesCount;
        private IList<PawnController> pawns;
        private IList<PlayerGamePanel> pawnsPanels;

        // private ulong[] targetAllClients;
        // private ulong[] targetOtherClients;
        // private ulong[] targetAllDefaultClients;
        // private IDictionary<int, ulong[]> targetAllClientsExcludingCurrentPlayer;

        private int nextPawnIndex => ++this.CurrentPawnIndex % this.pawns.Count;

        internal int PawnsCount => this.pawns.Count;


        internal int FirstDieValue { get; private set; }
        internal int SecondDieValue { get; private set; }
        internal int CurrentPawnIndex { get; private set; }
        internal IList<PawnVisuals> PawnsVisuals { get; private set; }
        internal int TotalRollResult => this.FirstDieValue + this.SecondDieValue;
        internal bool HasRolledDouble => this.FirstDieValue == this.SecondDieValue;

        internal PawnController CurrentPawn
        {
            get
            {
                if (this.CurrentPawnIndex >= 0 && this.CurrentPawnIndex < this.pawns.Count)
                    return this.pawns[this.CurrentPawnIndex];
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
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.None,
                MessageBoxPanel.Icon.Loading,
                UIManagerGame.Instance.MessageWaitingOtherPlayers,
                () => LobbyManager.Instance.HavePlayersLoaded
            );

            this.pawns = new List<PawnController>();
            this.pawnsPanels = new List<PlayerGamePanel>();
            this.PawnsVisuals = new List<PawnVisuals>(this.pawnsVisuals);

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

        private async void OnApplicationQuit()
        {
            if (await LobbyManager.Instance?.DoesLobbyExistAsync())
                await LobbyManager.Instance.DisconnectFromLobbyAsync();
        }

        private async void OnApplicationPause(bool pause)
        {
            if (await LobbyManager.Instance?.DoesLobbyExistAsync())
                await LobbyManager.Instance.DisconnectFromLobbyAsync();
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
            this.pawnsPanels.Remove(this.pawnsPanels.Where(pawnPanel => pawnPanel.NetworkIndex == networkIndex).First());

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
            float elapsedTime = 0f;

            while (!LobbyManager.Instance.HavePlayersLoaded && elapsedTime < LobbyManager.LOBBY_LOADING_TIMEOUT)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (!LobbyManager.Instance.HavePlayersLoaded)
                LobbyManager.Instance?.MonopolyGameFailedToLoadEvent?.Invoke();
            else
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

            // for (int i = 0; i < NetworkManager.Singleton?.ConnectedClients.Count; ++i)
            // {
            //     this.targetAllClients[i] = NetworkManager.Singleton.ConnectedClientsIds[i];
            //     this.targetAllClientsExcludingCurrentPlayer.Add(i, NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != NetworkManager.Singleton.ConnectedClientsIds[i]).ToArray());

            //     GameObject newPlayer = GameObject.Instantiate(this.player);
            //     GameObject newPlayerPanel = GameObject.Instantiate(this.pawnPanel);
            //     newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
            //     newPlayerPanel.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[i], true);
            // }

            // for (int i = this.pawns.Count; i < LobbyManager.MAX_PLAYERS; ++i)
            // {
            //     this.targetAllClientsExcludingCurrentPlayer.Add(i, NetworkManager.Singleton.ConnectedClientsIds.ToArray());

            //     GameObject newBot = GameObject.Instantiate(this.bot);
            //     GameObject newBotPanel = GameObject.Instantiate(this.pawnPanel);
            //     newBot.GetComponent<NetworkObject>().SpawnWithOwnership(GameManager.CLIENT_ID_HOST, true);
            //     newBotPanel.GetComponent<NetworkObject>().SpawnWithOwnership(GameManager.CLIENT_ID_HOST, true);
            // }

            // this.CurrentPawnIndex = 0;
            // this.SwitchPawnClientRpc(this.CurrentPawnIndex, this.TargetAllClients);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SwitchPawnServerRpc(ServerRpcParams serverRpcParams)
        {
            // if (this.HasRolledDouble)
            // {
            //     ++this.rolledDoublesCount;

            //     if (this.rolledDoublesCount >= this.MaxDoublesInRow)
            //     {
            //         this.rolledDoublesCount = 0;
            //         this.SendCurrentPawnToJailClientRpc(this.TargetAllClients);
            //     }
            //     else
            //     {
            //         this.SwitchPawnClientRpc(this.CurrentPawnIndex, this.TargetAllClients);
            //     }
            // }
            // else
            // {
            //     this.rolledDoublesCount = 0;
            //     this.SwitchPawnClientRpc(this.nextPawnIndex, this.TargetAllClients);
            // }
        }

        [ClientRpc]
        private void SendCurrentPawnToJailClientRpc(ClientRpcParams clientRpcParams)
        {
            if (this.CurrentPawn.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                this.CurrentPawn.GoToJail();
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SwitchPlayerForcefullyServerRpc(ServerRpcParams serverRpcParams)
        {
            // this.rolledDoublesCount = 0;
            // this.SwitchPawnClientRpc(this.nextPawnIndex, this.TargetAllClients);
        }

        [ClientRpc]
        private void SwitchPawnClientRpc(int currentPawnIndex, ClientRpcParams clientRpcParams)
        {
            this.CurrentPawnIndex = currentPawnIndex;

            if (this.CurrentPawn.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                this.CurrentPawn.PerformTurn();
        }

        internal void AddPawnController(PawnController pawn)
        {
            this.pawns.Add(pawn);
        }

        internal void AddPawnPanel(PlayerGamePanel pawnPanel)
        {
            this.pawnsPanels.Add(pawnPanel);
        }

        internal PlayerGamePanel GetPawnPanel(int networkIndex)
        {
            return this.pawnsPanels.Where(pawnPanel => pawnPanel.NetworkIndex == networkIndex).FirstOrDefault();
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

        internal CardChanceScriptableObject GetCardChance()
        {
            return this.cardsChance[UnityEngine.Random.Range(0, this.cardsChance.Count)];
        }
    }
}
