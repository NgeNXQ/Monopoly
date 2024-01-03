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
    [SerializeField] [Range(0.0f, 10.0f)] private float delayBetweenTurns = 0.5f;

    [Space]
    [SerializeField] [Range(0.0f, 10.0f)] private float delayBetweenNodes = 1.0f;

    [Space]
    [SerializeField] [Range(0.0f, 100.0f)] private float playerMovementSpeed = 25.0f;

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

    private int rolledDoubles;

    private ulong[] targetAllPlayers;

    private ulong[] targetOtherPlayers;

    private ulong[] targetCurrentPlayer;
    
    public static GameManager Instance { get; private set; }

    public MonopolyPlayer CurrentPlayer 
    {
        get => this.Players[this.CurrentPlayerIndex];
    }

    public List<MonopolyPlayer> Players { get; private set; }

    public ReadOnlyCollection<MonopolyPlayerVisuals> MonopolyPlayersVisuals;

    public int CurrentPlayerIndex { get; private set; }
    
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

    public int FirstDieValue { get; private set; }

    public int SecondDieValue { get; private set; }

    public ClientRpcParams ClientParamsAllClients 
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetAllPlayers }
            };
        }
    }

    public ClientRpcParams ClientParamsOtherClients 
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetOtherPlayers }
            };
        }
    }

    public ClientRpcParams ClientParamsCurrentClient 
    {
        get
        {
            this.targetCurrentPlayer[0] = NetworkManager.Singleton.ConnectedClientsIds[this.CurrentPlayerIndex];

            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.targetCurrentPlayer }
            };
        }
    }

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
        
        this.Players = new List<MonopolyPlayer>();

        this.MonopolyPlayersVisuals = new ReadOnlyCollection<MonopolyPlayerVisuals>(this.monopolyPlayersVisuals);

        this.targetCurrentPlayer = new ulong[1];
        this.targetAllPlayers = new ulong[LobbyManager.Instance.LocalLobby.Players.Count];
        this.targetOtherPlayers = new ulong[LobbyManager.Instance.LocalLobby.Players.Count - 1];

        if (LobbyManager.Instance.IsHost)
        {
            LobbyManager.Instance?.UpdateLocalLobbyData(LobbyManager.LOBBY_STATE_PENDING, true);

            this.StartCoroutine(this.WaitOtherPlayersCoroutine());
        }

        GameCoordinator.Instance?.UpdateInitializedObjects(this.gameObject);
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

    #region Init & Callbacks

    private async void CallbackWonTheGameAsync()
    {
        await LobbyManager.Instance.DisconnectFromLobbyAsync();
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
        {
            LobbyManager.Instance?.OnMonopolyGameFailedToLoad?.Invoke();
        }
        else
        {
            this.InitializeGameServerRpc(this.ServerParamsCurrentClient);
        }
    }

    private void HandleClientDisconnectCallback(ulong clientId)
    {
        this.targetAllPlayers = this.targetAllPlayers.Where(val => val != clientId).ToArray();
        this.targetOtherPlayers = this.targetOtherPlayers.Where(val => val != clientId).ToArray();

        this.Players.Remove(this.Players.Where(player => player.OwnerClientId == clientId).FirstOrDefault());

        if (this.Players.Count == 1)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageWon, PanelMessageBoxUI.Icon.Trophy, actionCallback: this.CallbackWonTheGameAsync);
        }
    }

    [ServerRpc]
    private void InitializeGameServerRpc(ServerRpcParams serverRpcParams)
    {
        this.targetAllPlayers = NetworkManager.Singleton.ConnectedClientsIds.ToArray();
        this.targetOtherPlayers = NetworkManager.Singleton.ConnectedClientsIds.Where((value) => value != NetworkManager.Singleton.LocalClientId).ToArray();

        for (int i = 0; i < NetworkManager.Singleton?.ConnectedClientsIds.Count; ++i)
        {
            this.CurrentPlayerIndex = i;

            this.SwitchPlayerClientRpc(this.CurrentPlayerIndex, this.ClientParamsOtherClients);
            this.InitializeNetworkClientRpc(this.targetAllPlayers, NetworkManager.Singleton.ConnectedClientsIds.Where((value) => value != NetworkManager.Singleton.ConnectedClientsIds[i]).ToArray(), this.ClientParamsCurrentClient);

            this.player = GameObject.Instantiate(this.player);
            this.playerPanel = GameObject.Instantiate(this.playerPanel);

            this.player.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
            this.playerPanel.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[i], true);
        }

        this.CurrentPlayerIndex = 0;

        this.SwitchPlayerClientRpc(this.CurrentPlayerIndex, this.ClientParamsOtherClients);

        this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentClient);
    }

    [ClientRpc]
    private void InitializeNetworkClientRpc(ulong[] targetAllPlayers, ulong[] targetOtherPlayers,  ClientRpcParams clientRpcParams)
    {
        this.targetAllPlayers = targetAllPlayers;
        this.targetOtherPlayers = targetOtherPlayers;
    }

    #endregion

    #region Turn-based Game Loop

    [ServerRpc(RequireOwnership = false)]
    public void SwitchPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("SwitchPlayerServerRpc");

        //if (this.HasRolledDouble)
        //{
        //    ++this.rolledDoubles;

        //    if (this.rolledDoubles >= this.MaxDoublesInRow)
        //    {
        //        this.rolledDoubles = 0;
        //        this.CurrentPlayer.HandleSendJailLanding();
        //    }
        //}
        //else
        //{
        //    this.rolledDoubles = 0;
        //    this.CurrentPlayerIndex = ++this.CurrentPlayerIndex % this.Players.Count;
        //}

        this.CurrentPlayerIndex = ++this.CurrentPlayerIndex % this.Players.Count;

        Debug.Log(GameManager.Instance.ClientParamsOtherClients.Send.TargetClientIds.FirstOrDefault());

        this.SwitchPlayerClientRpc(this.CurrentPlayerIndex, this.ClientParamsOtherClients);

        this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentClient);
    }

    [ClientRpc]
    private void SwitchPlayerClientRpc(int CurrentPlayerIndex, ClientRpcParams clientRpcParams)
    {
        Debug.Log("SwitchPlayerClientRpc");

        this.CurrentPlayerIndex = CurrentPlayerIndex;
    }

    #endregion

    #region Rolling Dice & Syncing

    public void RollDice()
    {
        const int MIN_DIE_VALUE = 1;
        const int MAX_DIE_VALUE = 6;

        this.FirstDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);
        this.SecondDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);
        
        this.RollDiceClientRpc(this.FirstDieValue, this.SecondDieValue, this.ClientParamsOtherClients);
    }

    [ClientRpc]
    private void RollDiceClientRpc(int firstDieValue, int secondDieValue, ClientRpcParams clientRpcParams)
    {
        this.FirstDieValue = firstDieValue;
        this.SecondDieValue = secondDieValue;
    }

    #endregion
}
