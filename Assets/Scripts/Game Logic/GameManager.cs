using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

using System.Diagnostics;
using System.Reflection;
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

    private ulong[] targetCurrentPlayer;

    private List<ulong[]> targetOtherPlayers;

    private List<MonopolyPlayer> players;

    public ReadOnlyCollection<MonopolyPlayerVisuals> MonopolyPlayersVisuals;

    public static GameManager Instance { get; private set; }

    public MonopolyPlayer CurrentPlayer 
    {
        get => this.players[this.CurrentPlayerIndex];
    }

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
                Send = new ClientRpcSendParams { TargetClientIds = this.targetOtherPlayers[this.CurrentPlayerIndex] }
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
        
        this.players = new List<MonopolyPlayer>();

        this.MonopolyPlayersVisuals = new ReadOnlyCollection<MonopolyPlayerVisuals>(this.monopolyPlayersVisuals);

        this.targetCurrentPlayer = new ulong[1];
        this.targetAllPlayers = new ulong[LobbyManager.Instance.LocalLobby.Players.Count];
        this.targetOtherPlayers = new List<ulong[]>(LobbyManager.Instance.LocalLobby.Players.Count);

        LobbyManager.Instance.UpdateLocalPlayerData();

        if (LobbyManager.Instance.IsHost)
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; ++i)
            {
                this.targetAllPlayers[i] = NetworkManager.Singleton.ConnectedClientsIds[i];
                this.targetOtherPlayers.Add(new ulong[NetworkManager.Singleton.ConnectedClients.Count]);
                this.targetOtherPlayers[i] = NetworkManager.Singleton.ConnectedClientsIds.Where((value, index) => index != i).ToArray();
            }

            LobbyManager.Instance?.UpdateLocalLobbyData(LobbyManager.LOBBY_STATE_PENDING, true);

            this.StartCoroutine(this.WaitOtherPlayersCoroutine());
        }
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

    #region Loading & Disconnect Callbacks

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
            for (int i = 0; i < NetworkManager.Singleton?.ConnectedClientsIds.Count; ++i)
            {
                this.CurrentPlayerIndex = i;

                this.player = GameObject.Instantiate(this.player);
                //this.playerPanel = GameObject.Instantiate(this.playerPanel, UIManagerMonopolyGame.Instance.CanvasPlayersList.transform);

                this.players.Add(this.player.GetComponent<MonopolyPlayer>());

                this.player.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
                //this.playerPanel.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[i], true);
            }
        }
        
        //this.CurrentPlayer.PerformTurnClientRpc(ClientParamsCurrentClient);
    }

    private void HandleClientDisconnectCallback(ulong clientId)
    {
        this.players.Remove(this.players.Where(player => player.OwnerClientId == clientId).FirstOrDefault());
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
                this.CurrentPlayer.HandleSendJailLanding();
            }
        }
        else
        {
            this.rolledDoubles = 0;
            this.CurrentPlayerIndex = ++this.CurrentPlayerIndex % this.players.Count;
        }

        this.SwitchPlayerClientRpc(this.CurrentPlayerIndex, this.ClientParamsOtherClients);

        this.CurrentPlayer.PerformTurnClientRpc(ClientParamsCurrentClient);
    }

    [ClientRpc]
    private void SwitchPlayerClientRpc(int CurrentPlayerIndex, ClientRpcParams clientRpcParams)
    {
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
    
    private void FixedUpdate()
    {
        StackTrace stackTrace = new StackTrace();

        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            StackFrame frame = stackTrace.GetFrame(i);
            MethodBase method = frame.GetMethod();

            UnityEngine.Debug.Log($"Method: {method.DeclaringType}.{method.Name}, Line: {frame.GetFileLineNumber()}");
        }
    }
}
