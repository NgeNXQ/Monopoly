using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

internal sealed class GameManager : NetworkBehaviour
{
    [Space]
    [SerializeField]
    [Range(0, 100_000)] private int startingBalance = 15_000;

    [Space]
    [SerializeField]
    [Range(0, 10)]
    private int maxTurnsInJail = 3;

    [Space]
    [SerializeField]
    [Range(0, 10)]
    private int maxDoublesInRow = 2;

    [Space]
    [SerializeField]
    [Range(0, 100_000)]
    private int circleBonus = 2_000;

    [Space]
    [SerializeField]
    [Range(0, 100_000)]
    private int exactCircleBonus = 3_000;

    [Space]
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float pawnMovementSpeed = 35.0f;

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

    private const ulong HOST_ID = 0;

    public static GameManager Instance { get; private set; }

    private int rolledDoublesCount;
    private IList<PawnController> pawns;
    private IList<PanelPawnGameUI> pawnsPanels;

    private ulong[] targetAllClients;
    private ulong[] targetOtherClients;
    private ulong[] targetAllDefaultClients;
    private IList<ulong[]> targetAllClientsExcludingCurrentPlayer;

    public int PawnsCount => this.pawns.Count;
    public int CircleBonus => this.circleBonus;
    public int MaxTurnsInJail => this.maxTurnsInJail;
    public int MaxDoublesInRow => this.maxDoublesInRow;
    public int StartingBalance => this.startingBalance;
    public int ExactCircleBonus => this.exactCircleBonus;
    public float PawnMovementSpeed => this.pawnMovementSpeed;

    public int FirstDieValue { get; private set; }
    public int SecondDieValue { get; private set; }
    public int CurrentPawnIndex { get; private set; }
    public IList<PawnVisuals> PawnsVisuals { get; private set; }
    public int TotalRollResult => this.FirstDieValue + this.SecondDieValue;
    public bool HasRolledDouble => this.FirstDieValue == this.SecondDieValue;

    public PawnController CurrentPawn
    {
        get
        {
            if (this.CurrentPawnIndex >= 0 && this.CurrentPawnIndex < this.pawns.Count)
                return this.pawns[this.CurrentPawnIndex];
            else
                return null;
        }
    }

    public ServerRpcParams SenderLocalClient
    {
        get
        {
            return new ServerRpcParams
            {
                Receive = new ServerRpcReceiveParams { SenderClientId = NetworkManager.Singleton.LocalClientId }
            };
        }
    }

    public ClientRpcParams TargetAllClients
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetAllClients }
            };
        }
    }

    public ClientRpcParams TargetOtherClients
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetOtherClients }
            };
        }
    }

    public ClientRpcParams TargetAllDefaultClients
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetAllDefaultClients }
            };
        }
    }

    public ClientRpcParams TargetAllClientsExcludingCurrentPlayer
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetAllClientsExcludingCurrentPlayer[this.CurrentPawnIndex] }
            };
        }
    }

    private void Awake()
    {
        if (GameManager.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        GameManager.Instance = this;
    }

    private void Start()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerMonopolyGame.Instance.MessageWaitingOtherPlayers, PanelMessageBoxUI.Icon.Loading, () => LobbyManager.Instance.HavePlayersLoaded);

        this.pawns = new List<PawnController>();
        this.pawnsPanels = new List<PanelPawnGameUI>();
        this.PawnsVisuals = new List<PawnVisuals>(this.pawnsVisuals);

        if (LobbyManager.Instance.IsHost)
            this.StartCoroutine(this.WaitOtherPlayersCoroutine());

        GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
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
        if (LobbyManager.Instance != null && await LobbyManager.Instance.PingLobbyExists())
            await LobbyManager.Instance.DisconnectFromLobbyAsync();
    }

    private async void OnApplicationPause(bool pause)
    {
        if (LobbyManager.Instance != null && await LobbyManager.Instance.PingLobbyExists())
            await LobbyManager.Instance.DisconnectFromLobbyAsync();
    }

    private async void OnClientDisconnected(ulong surrenderedClientId)
    {
        // private ulong[] targetAllClients;
        // private ulong[] targetOtherClients;
        // private ulong[] targetAllDefaultClients;
        // private IList<ulong[]> targetAllClientsExcludingCurrentPlayer;


        // if (surrenderedClientId == GameManager.HOST_ID)
        // {
        //     if (await LobbyManager.Instance.PingLobbyExists())
        //     {
        //         if (ObjectPoolMessageBoxes.Instance != null)
        //             UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageHostDisconnected, PanelMessageBoxUI.Icon.Error);
        //     }

        //     await LobbyManager.Instance.DisconnectFromLobbyAsync();
        // }
        // else
        // {
        //     if (this.pawns.Any(pawn => pawn.OwnerClientId == surrenderedClientId))
        //     {
        //         this.targetClientOtherClients = this.targetClientOtherClients?.Where(clientId => clientId != surrenderedClientId).ToArray();
        //         this.targetAllDefaultClients = this.targetAllDefaultClients.Select(array => array.Where(id => id != surrenderedClientId).ToArray()).ToList();

        //         PawnController pawn = this.pawns[this.pawns.IndexOf(this.pawns.Where(pawn => pawn.OwnerClientId == surrenderedClientId).First())];

        //         if (pawn.NetworkObject.IsSpawned)
        //             pawn.DeclineTradeServerRpc(this.ServerParamsCurrentClient);

        //         for (int i = 0; i < MonopolyBoard.Instance.NumberOfNodes; ++i)
        //         {
        //             if (MonopolyBoard.Instance[i].Owner != null && MonopolyBoard.Instance[i].Owner.OwnerClientId == surrenderedClientId)
        //                 MonopolyBoard.Instance[i].ResetOwnership();
        //         }

        //         this.RemovePlayerServerRpc(surrenderedClientId, this.ServerParamsCurrentClient);
        //     }
        // }
    }

    public void RemoveSurrenderedPawn(int networkIndex)
    {
        if (this.CurrentPawn.NetworkIndex == networkIndex)
            this.SwitchPlayerForcefullyServerRpc(this.SenderLocalClient);

        this.targetAllClientsExcludingCurrentPlayer.RemoveAt(networkIndex);

        this.RemoveSurrenderedPawnClientRpc(networkIndex, this.TargetAllClients);



        // if (this.pawns.Any(pawn => pawn.OwnerClientId == surrenderedClientId))
        // {
        //     this.targetAllDefaultClients.RemoveAt(this.pawns.IndexOf(this.pawns.Where(pawn => pawn.OwnerClientId == surrenderedClientId).First()));

        //     MonopolyPlayer pawn = this.pawns[this.pawns.IndexOf(this.pawns.Where(pawn => pawn.OwnerClientId == surrenderedClientId).First())];

        //     bool isCurrent = pawn == this.CurrentPlayer;

        //     this.pawns.Remove(pawn);
        //     this.RemovePlayerClientRpc(surrenderedClientId, this.ClientParamsClientOtherClients);

        //     if (isCurrent && this.pawns.Count != 0)
        //     {
        //         this.CurrentPawnIndex %= this.pawns.Count;
        //         this.SwitchPlayerClientRpc(this.CurrentPawnIndex, this.ClientParamsClientOtherClients);
        //         this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentClient);
        //     }

        //     if (this.pawns.Count == 1 && this.pawns.First().OwnerClientId == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsConnectedClient && NetworkManager.Singleton.IsListening)
        //     {
        //         UIManagerMonopolyGame.Instance.HidePaymentProperty();
        //         UIManagerMonopolyGame.Instance.HideButtonRollDice();
        //         UIManagerMonopolyGame.Instance.HidePaymentChance();
        //         UIManagerMonopolyGame.Instance.HideMonopolyNode();
        //         UIManagerMonopolyGame.Instance.HideReceiveTrade();
        //         UIManagerMonopolyGame.Instance.HideTradeOffer();
        //         UIManagerMonopolyGame.Instance.HideOffer();

        //         UIManagerMonopolyGame.Instance.ShowButtonDisconnect();
        //         UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageWon, PanelMessageBoxUI.Icon.Trophy);
        //     }
        // }
    }

    [ClientRpc]
    private void RemoveSurrenderedPawnClientRpc(int networkIndex, ClientRpcParams clientRpcParams)
    {
        this.pawns.Remove(this.pawns.Where(pawn => pawn.NetworkIndex == networkIndex).First());
        this.pawnsPanels.Remove(this.pawnsPanels.Where(pawnPanel => pawnPanel.NetworkIndex == networkIndex).First());

        if (this.pawns.Count > 1)
            return;

        if (PlayerPawnController.LocalInstance == null)
            return;

        if (PlayerPawnController.LocalInstance.NetworkIndex == this.pawns.First().NetworkIndex)
        {
            UIManagerMonopolyGame.Instance.HidePanelNodeMenu();
            UIManagerMonopolyGame.Instance.HidePanelSendTrade();
            UIManagerMonopolyGame.Instance.HidePanelNodeOffer();
            UIManagerMonopolyGame.Instance.HideButtonRollDice();
            UIManagerMonopolyGame.Instance.HidePanelNodePayment();
            UIManagerMonopolyGame.Instance.HidePanelReceiveTrade();
            UIManagerMonopolyGame.Instance.HidePanelChancePayment();

            UIManagerMonopolyGame.Instance.ShowButtonDisconnect();
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageWon, PanelMessageBoxUI.Icon.Trophy);
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
            LobbyManager.Instance?.OnMonopolyGameFailedToLoad?.Invoke();
        else
            this.InitializeGameSession();
    }

    private void InitializeGameSession()
    {
        const int FIRST_CLIENT_INDEX = 1;

        this.targetAllClientsExcludingCurrentPlayer = new List<ulong[]>();
        this.targetAllClients = new ulong[NetworkManager.Singleton.ConnectedClientsIds.Count];
        this.targetOtherClients = new ulong[NetworkManager.Singleton.ConnectedClientsIds.Count - 1];
        this.targetAllDefaultClients = new ulong[NetworkManager.Singleton.ConnectedClientsIds.Count - 1];

        int clientsCount = NetworkManager.Singleton.ConnectedClientsIds.Count - 1;

        for (int i = FIRST_CLIENT_INDEX; i < clientsCount; ++i)
            this.targetAllDefaultClients[i] = NetworkManager.Singleton.ConnectedClientsIds[i];

        for (int i = 0; i < NetworkManager.Singleton?.ConnectedClientsIds.Count; ++i)
        {
            this.targetAllClients[i] = NetworkManager.Singleton.ConnectedClientsIds[i];
            this.targetAllClientsExcludingCurrentPlayer.Add(NetworkManager.Singleton.ConnectedClientsIds.Where((value) => value != NetworkManager.Singleton.ConnectedClientsIds[i]).ToArray());

            GameObject newPlayer = GameObject.Instantiate(this.player);
            GameObject newPlayerPanel = GameObject.Instantiate(this.pawnPanel);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
            newPlayerPanel.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[i], true);
        }

        // for (int i = this.pawns.Count; i < LobbyManager.MAX_PLAYERS; ++i)
        for (int i = this.pawns.Count; i < 2; ++i)
        {
            this.targetAllClientsExcludingCurrentPlayer.Add(NetworkManager.Singleton.ConnectedClientsIds.ToArray());

            GameObject newBot = GameObject.Instantiate(this.bot);
            GameObject newBotPanel = GameObject.Instantiate(this.pawnPanel);
            newBot.GetComponent<NetworkObject>().SpawnWithOwnership(GameManager.HOST_ID, true);
            newBotPanel.GetComponent<NetworkObject>().SpawnWithOwnership(GameManager.HOST_ID, true);
        }

        this.CurrentPawnIndex = 0;
        this.SwitchPawnClientRpc(this.CurrentPawnIndex, this.TargetAllClients);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchPawnServerRpc(ServerRpcParams serverRpcParams)
    {
        if (this.HasRolledDouble)
        {
            ++this.rolledDoublesCount;

            if (this.rolledDoublesCount >= this.MaxDoublesInRow)
            {
                this.rolledDoublesCount = 0;
                this.SendCurrentPawnToJailClientRpc(this.TargetAllClients);
            }
        }
        else
        {
            this.rolledDoublesCount = 0;
            this.CurrentPawnIndex = ++this.CurrentPawnIndex % this.pawns.Count;
        }

        this.SwitchPawnClientRpc(this.CurrentPawnIndex, this.TargetAllClients);
    }

    [ClientRpc]
    private void SendCurrentPawnToJailClientRpc(ClientRpcParams clientRpcParams)
    {
        if (this.CurrentPawn.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            if (NetworkManager.Singleton.IsHost)
                this.CurrentPawn.GoToJail();
            else
                PlayerPawnController.LocalInstance.GoToJail();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchPlayerForcefullyServerRpc(ServerRpcParams serverRpcParams)
    {
        this.rolledDoublesCount = 0;
        this.CurrentPawnIndex = ++this.CurrentPawnIndex % this.pawns.Count;
        this.SwitchPawnClientRpc(this.CurrentPawnIndex, this.TargetAllClients);
    }

    [ClientRpc]
    private void SwitchPawnClientRpc(int currentPawnIndex, ClientRpcParams clientRpcParams)
    {
        this.CurrentPawnIndex = currentPawnIndex;

        if (this.CurrentPawn.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            if (NetworkManager.Singleton.IsHost)
                this.CurrentPawn.PerformTurn();
            else
                PlayerPawnController.LocalInstance.PerformTurn();
        }
    }

    public void AddPawnController(PawnController pawn)
    {
        this.pawns.Add(pawn);
    }

    public void AddPawnPanel(PanelPawnGameUI pawnPanel)
    {
        this.pawnsPanels.Add(pawnPanel);
    }

    public PanelPawnGameUI GetPawnPanel(int networkIndex)
    {
        return this.pawnsPanels.Where(pawnPanel => pawnPanel.NetworkIndex == networkIndex).FirstOrDefault();
    }

    public PawnController GetPawnController(int networkIndex)
    {
        return this.pawns.Where(pawn => pawn.NetworkIndex == networkIndex).FirstOrDefault();
    }

    public void RollDice()
    {
        const int MIN_DIE_VALUE = 1;
        const int MAX_DIE_VALUE = 6;

        this.FirstDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);
        this.SecondDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);

        this.RollDiceServerRpc(this.FirstDieValue, this.SecondDieValue, this.SenderLocalClient);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RollDiceServerRpc(int firstDieValue, int secondDieValue, ServerRpcParams serverRpcParams)
    {
        this.FirstDieValue = firstDieValue;
        this.SecondDieValue = secondDieValue;

        this.RollDiceClientRpc(firstDieValue, secondDieValue, this.TargetAllDefaultClients);
    }

    [ClientRpc]
    private void RollDiceClientRpc(int firstDieValue, int secondDieValue, ClientRpcParams clientRpcParams)
    {
        this.FirstDieValue = firstDieValue;
        this.SecondDieValue = secondDieValue;
    }
}
