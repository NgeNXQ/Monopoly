using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

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

    [Header("Visuals")]

    #region Player

    [Header("Player")]

    [Space]
    [SerializeField] private GameObject player;

    #endregion

    #region Players Tokens

    [Header("Players Visuals")]

    [Space]
    [SerializeField] private MonopolyPlayerVisuals[] monopolyPlayersVisuals = new MonopolyPlayerVisuals[5];

    #endregion

    #endregion

    #endregion

    private int rolledDoubles;

    private int currentPlayerIndex;

    private ulong[] targetAllPlayers;

    private ulong[] targetCurrentPlayer;

    private List<ulong[]> targetOtherPlayers;

    private List<MonopolyPlayer> players;

    public static GameManager Instance { get; private set; }

    public MonopolyPlayer CurrentPlayer 
    {
        get => this.players[this.currentPlayerIndex];
    }

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
                Send = new ClientRpcSendParams { TargetClientIds = this.targetOtherPlayers[this.currentPlayerIndex] }
            };
        }
    }

    public ClientRpcParams ClientParamsCurrentClient 
    {
        get
        {
            this.targetCurrentPlayer[0] = NetworkManager.Singleton.ConnectedClientsIds[this.currentPlayerIndex];

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
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private async void Start()
    {
        LobbyManager.Instance.UpdateLocalPlayerData();

        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.None, UIManagerMonopolyGame.Instance.MessageWaitingOtherPlayers, PanelMessageBoxUI.Icon.Loading, null, () => LobbyManager.Instance.HavePlayersLoaded);

        this.players = new List<MonopolyPlayer>();

        this.targetCurrentPlayer = new ulong[1];
        this.targetAllPlayers = new ulong[LobbyManager.Instance.LocalLobby.Players.Count];
        this.targetOtherPlayers = new List<ulong[]>(LobbyManager.Instance.LocalLobby.Players.Count);
        
        if (LobbyManager.Instance.IsHost)
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; ++i)
            {
                this.targetAllPlayers[i] = NetworkManager.Singleton.ConnectedClientsIds[i];
                this.targetOtherPlayers.Add(new ulong[NetworkManager.Singleton.ConnectedClients.Count]);
                this.targetOtherPlayers[i] = NetworkManager.Singleton.ConnectedClientsIds.Where((value, index) => index != i).ToArray();
            }

            LobbyManager.Instance.LocalLobby.Data[LobbyManager.KEY_LOBBY_STATE] = new DataObject(DataObject.VisibilityOptions.Member, LobbyManager.LOBBY_STATE_PENDING);

            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                Data = LobbyManager.Instance.LocalLobby.Data
            };

            await Lobbies.Instance.UpdateLobbyAsync(LobbyManager.Instance.LocalLobby.Id, updateLobbyOptions);

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
            LobbyManager.Instance.OnMonopolyGameFailedToLoad?.Invoke();
            GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.GameLobby);
        }
        else
        {
            Debug.Log("initializing");

            //for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; ++i)
            //{
            //    this.player = GameObject.Instantiate(this.player);
            //    this.player.name = LobbyManager.Instance.LocalLobby.Players[i].Id;
            //    this.player.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.ConnectedClientsIds[i], true);

            //    this.players.Add(this.player.GetComponent<MonopolyPlayer>());

            //    string nickname = LobbyManager.Instance.LocalLobby.Players[i].Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
            //    this.players.Last().InitializePlayer(nickname, this.monopolyPlayersVisuals[i]);
            //}
        }

        //this.StartTurnServerRpc(this.ServerParamsCurrentClient);
    }

    private void HandleClientDisconnectCallback(ulong clientId)
    {
        this.players.Remove(this.players.Where(player => player.OwnerClientId == clientId).FirstOrDefault());

        // clear all nodes and update visuals
    }

    #endregion

    #region Monopoly Turn-based Game Loop

    [ServerRpc]
    private void StartTurnServerRpc(ServerRpcParams serverRpcParams)
    {
        this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentClient);
    }

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
            this.currentPlayerIndex = ++this.currentPlayerIndex % this.players.Count;
        }

        this.SwitchPlayerClientRpc(this.currentPlayerIndex, this.ClientParamsAllClients);

        this.StartTurnServerRpc(this.ServerParamsCurrentClient);
    }

    [ClientRpc]
    private void SwitchPlayerClientRpc(int currentPlayerIndex, ClientRpcParams clientRpcParams)
    {
        this.currentPlayerIndex = currentPlayerIndex;
    }

    #endregion

    #region Rolling Dice & Sync

    public void RollDice()
    {
        const int MIN_DIE_VALUE = 1;
        const int MAX_DIE_VALUE = 6;

        this.FirstDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);
        this.SecondDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);

        this.RollDiceServerRpc(this.FirstDieValue, this.SecondDieValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RollDiceServerRpc(int firstDieValue, int secondDieValue, ServerRpcParams serverRpcParams = default)
    {
        this.FirstDieValue = firstDieValue;
        this.SecondDieValue = secondDieValue;

        this.RollDiceClientRpc(firstDieValue, secondDieValue);
    }

    [ClientRpc]
    private void RollDiceClientRpc(int firstDieValue, int secondDieValue, ClientRpcParams clientRpcParams = default)
    {
        this.FirstDieValue = firstDieValue;
        this.SecondDieValue = secondDieValue;
    }

    #endregion
}
