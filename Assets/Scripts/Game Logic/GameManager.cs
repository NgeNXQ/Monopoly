using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

internal sealed class GameManager : NetworkBehaviour
{
    #region Setup

    #region Values

    [Header("Values")]

    [Space]
    [SerializeField] [Range(0, 100_000)] private int startingBalance = 15_000;

    [Space]
    [SerializeField] [Range(0, 10)] private int maxTurnsInJail = 3;

    [Space]
    [SerializeField] [Range(0, 10)] private int maxDoublesInRow = 2;

    [Space]
    [SerializeField] [Range(0, 100_000)] private int circleBonus = 2_000;

    [Space]
    [SerializeField] [Range(0, 100_000)] private int exactCircleBonus = 3_000;

    [Space]
    [SerializeField] [Range(0.0f, 100.0f)] private float playerMovementSpeed = 35.0f;

    #endregion

    #region Visuals

    [Space]
    [Header("Visuals")]

    #region Player

    [Space]
    [Header("Player")]

    [Space]
    [SerializeField] private GameObject player;

    [Space]
    [SerializeField] private GameObject playerPanel;

    #endregion

    #region Players Tokens

    [Space]
    [Header("Players Visuals")]

    [Space]
    [SerializeField] private MonopolyPlayerVisuals[] monopolyPlayersVisuals = new MonopolyPlayerVisuals[5];

    #endregion

    #endregion

    #endregion

    private const int HOST_ID = 0;

    private int rolledDoubles;

    private List<MonopolyPlayer> players;

    private ulong[] targetCurrentClient;

    private ulong[] targetClientOtherClients;

    private List<ulong[]> targetHostOtherClients;

    public static GameManager Instance { get; private set; }

    public int CircleBonus 
    {
        get => this.circleBonus;
    }

    public int MaxTurnsInJail 
    {
        get => this.maxTurnsInJail;
    }

    public int TotalRollResult 
    {
        get => this.FirstDieValue + this.SecondDieValue;
    }

    public int MaxDoublesInRow 
    {
        get => this.maxDoublesInRow;
    }

    public int StartingBalance 
    {
        get => this.startingBalance;
    }

    public bool HasRolledDouble 
    {
        get => this.FirstDieValue == this.SecondDieValue;
    }

    public int ExactCircleBonus 
    {
        get => this.exactCircleBonus;
    }

    public float PlayerMovementSpeed 
    {
        get => this.playerMovementSpeed;
    }

    public MonopolyPlayer CurrentPlayer 
    {
        get
        {
            if (this.CurrentPlayerIndex >= 0 && this.CurrentPlayerIndex < this.players.Count)
            {
                return this.players[this.CurrentPlayerIndex];
            }
            else
            {
                return null;
            }
        }
    }

    public int FirstDieValue { get; private set; }

    public int SecondDieValue { get; private set; }

    public int CurrentPlayerIndex { get; private set; }

    public ServerRpcParams ServerParamsCurrentClient 
    {
        get
        {
            return new ServerRpcParams
            {
                Receive = new ServerRpcReceiveParams { SenderClientId = NetworkManager.Singleton.LocalClientId }
            };
        }
    }

    public ClientRpcParams ClientParamsCurrentClient 
    {
        get
        {
            this.targetCurrentClient[0] = this.players[this.CurrentPlayerIndex].OwnerClientId;

            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetCurrentClient }
            };
        }
    }

    public ClientRpcParams ClientParamsHostOtherClients 
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetHostOtherClients[this.CurrentPlayerIndex] }
            };
        }
    }

    public ClientRpcParams ClientParamsClientOtherClients 
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetClientOtherClients }
            };
        }
    }

    public ReadOnlyCollection<MonopolyPlayerVisuals> MonopolyPlayersVisuals { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");
        }

        Instance = this;
    }

    private void Start()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerMonopolyGame.Instance.MessageWaitingOtherPlayers, PanelMessageBoxUI.Icon.Loading, stateCallback: () => LobbyManager.Instance.HavePlayersLoaded);

        this.players = new List<MonopolyPlayer>();

        this.MonopolyPlayersVisuals = new ReadOnlyCollection<MonopolyPlayerVisuals>(this.monopolyPlayersVisuals);

        if (LobbyManager.Instance.IsHost)
        {
            this.StartCoroutine(this.WaitOtherPlayersCoroutine());
        }

        GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += this.HandleClientDisconnectCallback;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= this.HandleClientDisconnectCallback;
        }
    }

    private async void OnApplicationQuit()
    {
        if (LobbyManager.Instance != null && await LobbyManager.Instance.PingLobbyExists())
        {
            await LobbyManager.Instance.DisconnectFromLobbyAsync();
        }
    }

    private async void OnApplicationPause(bool pause)
    {
        if (LobbyManager.Instance != null && await LobbyManager.Instance.PingLobbyExists())
        {
            await LobbyManager.Instance.DisconnectFromLobbyAsync();
        }
    }

    #region Callbacks

    public MonopolyPlayer GetPlayerById(ulong clientId)
    {
        return this.players.Where(player => player.OwnerClientId == clientId).FirstOrDefault();
    }

    public ClientRpcParams GetRedirectionRpc(ulong clientId)
    {
        this.targetCurrentClient[0] = clientId;

        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = this.targetCurrentClient }
        };
    }

    private async void HandleClientDisconnectCallback(ulong surrenderedClientId)
    {
        if (surrenderedClientId == GameManager.HOST_ID)
        {
            if (await LobbyManager.Instance.PingLobbyExists())
            {
                if (ObjectPoolMessageBoxes.Instance != null)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageHostDisconnected, PanelMessageBoxUI.Icon.Error);
                }
            }

            await LobbyManager.Instance.DisconnectFromLobbyAsync();
        }
        else
        {
            if (this.players.Any(player => player.OwnerClientId == surrenderedClientId))
            {
                this.targetClientOtherClients = this.targetClientOtherClients?.Where(clientId => clientId != surrenderedClientId).ToArray();
                this.targetHostOtherClients = this.targetHostOtherClients.Select(array => array.Where(id => id != surrenderedClientId).ToArray()).ToList();

                MonopolyPlayer player = this.players[this.players.IndexOf(this.players.Where(player => player.OwnerClientId == surrenderedClientId).First())];

                if (player.NetworkObject.IsSpawned)
                {
                    player.DeclineTradeServerRpc(this.ServerParamsCurrentClient);
                }

                for (int i = 0; i < MonopolyBoard.Instance.NumberOfNodes; ++i)
                {
                    if (MonopolyBoard.Instance[i].Owner != null && MonopolyBoard.Instance[i].Owner.OwnerClientId == surrenderedClientId)
                    {
                        MonopolyBoard.Instance[i].ResetOwnership();
                    }
                }

                this.RemovePlayerServerRpc(surrenderedClientId, this.ServerParamsCurrentClient);
            }
        }
    }

    [ServerRpc]
    public void RemovePlayerServerRpc(ulong surrenderedClientId, ServerRpcParams serverRpcParams)
    {
        if (this.players.Any(player => player.OwnerClientId == surrenderedClientId))
        {
            this.targetHostOtherClients.RemoveAt(this.players.IndexOf(this.players.Where(player => player.OwnerClientId == surrenderedClientId).First()));

            MonopolyPlayer player = this.players[this.players.IndexOf(this.players.Where(player => player.OwnerClientId == surrenderedClientId).First())];

            bool isCurrent = player == this.CurrentPlayer;

            this.players.Remove(player);
            this.RemovePlayerClientRpc(surrenderedClientId, this.ClientParamsClientOtherClients);

            if (isCurrent && this.players.Count != 0)
            {
                this.CurrentPlayerIndex %= this.players.Count;
                this.SwitchPlayerClientRpc(this.CurrentPlayerIndex, this.ClientParamsClientOtherClients);
                this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentClient);
            }

            if (this.players.Count == 1 && this.players.First().OwnerClientId == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsConnectedClient && NetworkManager.Singleton.IsListening)
            {
                UIManagerMonopolyGame.Instance.HidePaymentProperty();
                UIManagerMonopolyGame.Instance.HideButtonRollDice();
                UIManagerMonopolyGame.Instance.HidePaymentChance();
                UIManagerMonopolyGame.Instance.HideMonopolyNode();
                UIManagerMonopolyGame.Instance.HideReceiveTrade();
                UIManagerMonopolyGame.Instance.HideTradeOffer();
                UIManagerMonopolyGame.Instance.HideOffer();

                UIManagerMonopolyGame.Instance.ShowButtonDisconnect();
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageWon, PanelMessageBoxUI.Icon.Trophy);
            }
        }
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(ulong surrenderedClientId, ClientRpcParams clientRpcParams)
    {
        this.players.Remove(this.players.Where(player => player.OwnerClientId == surrenderedClientId).First());
        this.targetClientOtherClients = this.targetClientOtherClients?.Where(clientId => clientId != surrenderedClientId).ToArray();

        if (this.players.Count == 1 && this.players.First().OwnerClientId == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsConnectedClient)
        {
            UIManagerMonopolyGame.Instance.HidePaymentProperty();
            UIManagerMonopolyGame.Instance.HideButtonRollDice();
            UIManagerMonopolyGame.Instance.HidePaymentChance();
            UIManagerMonopolyGame.Instance.HideMonopolyNode();
            UIManagerMonopolyGame.Instance.HideReceiveTrade();
            UIManagerMonopolyGame.Instance.HideTradeOffer();
            UIManagerMonopolyGame.Instance.HideOffer();

            UIManagerMonopolyGame.Instance.ShowButtonDisconnect();
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageWon, PanelMessageBoxUI.Icon.Trophy);
        }
    }

#endregion

    #region Initialization

    private IEnumerator WaitOtherPlayersCoroutine()
    {
        float elapsedTime = 0f;

        while (!LobbyManager.Instance.HavePlayersLoaded && elapsedTime < LobbyManager.LOBBY_LOADING_TIMEOUT)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!LobbyManager.Instance.HavePlayersLoaded)
        {
            LobbyManager.Instance?.OnMonopolyGameFailedToLoad?.Invoke();
        }
        else
        {
            this.InitializeGameServerRpc(this.ServerParamsCurrentClient);
        }
    }

    public void AddPlayer(MonopolyPlayer monopolyPlayer)
    {
        this.players.Add(monopolyPlayer);
    }

    [ServerRpc]
    private void InitializeGameServerRpc(ServerRpcParams serverRpcParams)
    {
        this.targetCurrentClient = new ulong[1];
        this.targetHostOtherClients = new List<ulong[]>();
        this.targetClientOtherClients = NetworkManager.Singleton.ConnectedClientsIds.Where((value) => value != NetworkManager.Singleton.ConnectedClientsIds[0]).ToArray();

        for (int i = 0; i < NetworkManager.Singleton?.ConnectedClientsIds.Count; ++i)
        {
            this.CurrentPlayerIndex = i;

            this.SwitchPlayerClientRpc(this.CurrentPlayerIndex, this.ClientParamsClientOtherClients);

            this.targetHostOtherClients.Add(NetworkManager.Singleton.ConnectedClientsIds.Where((value) => value != NetworkManager.Singleton.ConnectedClientsIds[i]).ToArray());

            this.player = GameObject.Instantiate(this.player);
            this.playerPanel = GameObject.Instantiate(this.playerPanel);

            this.player.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
            this.playerPanel.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[i], true);
        }

        this.CurrentPlayerIndex = 0;

        this.SwitchPlayerClientRpc(this.CurrentPlayerIndex, this.ClientParamsClientOtherClients);

        this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentClient);
    }

    #endregion

    #region Turn-based Game Loop

    [ServerRpc(RequireOwnership = false)]
    public void SwitchPlayerServerRpc(ServerRpcParams serverRpcParams)
    {
        if (this.HasRolledDouble)
        {
            ++this.rolledDoubles;

            if (this.rolledDoubles >= this.MaxDoublesInRow)
            {
                this.rolledDoubles = 0;
                this.CurrentPlayer.GoToJailClientRpc(this.ClientParamsCurrentClient);
            }
        }
        else
        {
            this.rolledDoubles = 0;
            this.CurrentPlayerIndex = ++this.CurrentPlayerIndex % this.players.Count;
        }

        this.SwitchPlayerClientRpc(this.CurrentPlayerIndex, this.ClientParamsClientOtherClients);

        UIManagerMonopolyGame.Instance.HideButtonRollDice();

        UIManagerMonopolyGame.Instance.HideButtonRollDiceClientRpc(this.ClientParamsClientOtherClients);

        this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentClient);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchPlayerForcefullyServerRpc(ServerRpcParams serverRpcParams)
    {
        if (this.players.Count > 1)
        {
            this.rolledDoubles = 0;

            this.CurrentPlayerIndex = ++this.CurrentPlayerIndex % this.players.Count;

            this.SwitchPlayerClientRpc(this.CurrentPlayerIndex, this.ClientParamsClientOtherClients);

            UIManagerMonopolyGame.Instance.HideButtonRollDice();

            UIManagerMonopolyGame.Instance.HideButtonRollDiceClientRpc(this.ClientParamsClientOtherClients);

            this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentClient);
        }
    }

    [ClientRpc]
    private void SwitchPlayerClientRpc(int currentPlayerIndex, ClientRpcParams clientRpcParams)
    {
        this.CurrentPlayerIndex = currentPlayerIndex;
    }

    #endregion

    #region Rolling Dice & Syncing

    public void RollDice()
    {
        const int MIN_DIE_VALUE = 1;
        const int MAX_DIE_VALUE = 6;
        
        this.FirstDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);
        this.SecondDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);

        this.RollDiceServerRpc(this.FirstDieValue, this.SecondDieValue, this.ServerParamsCurrentClient);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RollDiceServerRpc(int firstDieValue, int secondDieValue, ServerRpcParams serverRpcParams)
    {
        this.FirstDieValue = firstDieValue;
        this.SecondDieValue = secondDieValue;

        this.RollDiceClientRpc(firstDieValue, secondDieValue, this.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void RollDiceClientRpc(int firstDieValue, int secondDieValue, ClientRpcParams clientRpcParams)
    {
        this.FirstDieValue = firstDieValue;
        this.SecondDieValue = secondDieValue;
    }

    #endregion
}
