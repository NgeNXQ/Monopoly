using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

internal sealed class GameManager : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]

    #region Values

    [Space]
    [Header("Values")]
    [Space]

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

    #region Players Tokens

    [Space]
    [Header("Players Tokens")]

    [Space]
    [SerializeField] private List<MonopolyPlayer> monopolyPlayers;

    #endregion

    #endregion

    //private ulong[] targetAllClients;
    //private ulong[] targetOtherClients;
    //private ulong[] targetCurrentClient;
    //private ulong[] targetAllClientsExceptHost;


    //private ulong[] TargetAllClients
    //{
    //    get
    //    {
    //        this.targetAllClients[0] = 0;
    //        return this.targetAllClients;
    //    }
    //}

    //private ulong[] TargetOtherClients
    //{
    //    get
    //    {
    //        this.targetOtherClients[0] = 0;
    //        return this.targetOtherClients;
    //    }
    //}

    //private ulong[] TargetCurrentClient
    //{
    //    get
    //    {
    //        this.targetCurrentClient[0] = 0;
    //        return this.targetCurrentClient;
    //    }
    //}

    //private ulong[] TargetAllClientsExceptHost
    //{
    //    get
    //    {
    //        this.targetAllClientsExceptHost[0] = 0;
    //        return this.targetAllClientsExceptHost;
    //    }
    //}

    //public ClientRpcParams TargetAllClientsParams
    //{
    //    get
    //    {
    //        return new ClientRpcParams
    //        {
    //            Send = new ClientRpcSendParams { TargetClientIds = this.TargetAllClients }
    //        };
    //    }
    //}

    //public ClientRpcParams TargetOtherClientsParams
    //{
    //    get
    //    {
    //        return new ClientRpcParams
    //        {
    //            Send = new ClientRpcSendParams { TargetClientIds = this.TargetOtherClients }
    //        };
    //    }
    //}

    //public ClientRpcParams TargetCurrentClientParams
    //{
    //    get
    //    {
    //        return new ClientRpcParams
    //        {
    //            Send = new ClientRpcSendParams { TargetClientIds = this.TargetCurrentClient }
    //        };
    //    }
    //}

    //public ClientRpcParams TargetAllClientsGameLobby
    //{
    //    get
    //    {
    //        return new ClientRpcParams
    //        {
    //            Send = new ClientRpcSendParams { TargetClientIds = this.targetAllClientsGameLobby.ToArray() }
    //        };
    //    }
    //}




    public static GameManager Instance { get; private set; }

    private int currentMonopolyPlayerIndex; 

    private ulong[] targetOtherPlayers;

    private ulong[] targetCurrentPlayer;

    private int rolledDoubles;

    private int currentPlayerIndex;

    public int FirstDieValue { get; private set; }

    public int SecondDieValue { get; private set; }

    public int CircleBonus { get => this.circleBonus; }

    public int MaxTurnsInJail { get => this.maxTurnsInJail; }

    public int MaxDoublesInRow { get => this.maxDoublesInRow; }

    public int StartingBalance { get => this.startingBalance; }

    public int ExactCircleBonus { get => this.exactCircleBonus; }

    public float PlayerMovementSpeed { get => this.playerMovementSpeed; }

    public int TotalRollResult { get => this.FirstDieValue + this.SecondDieValue; }

    public bool HasRolledDouble { get => this.FirstDieValue == this.SecondDieValue; }

    public MonopolyPlayer CurrentPlayer { get => this.monopolyPlayers[this.currentPlayerIndex]; }

    private ulong[] TargetOtherPlayers
    {
        get
        {
            int index = 0; ;

            foreach (MonopolyPlayer player in this.monopolyPlayers)
            {
                if (player.OwnerClientId != this.CurrentPlayer.OwnerClientId)
                    this.targetOtherPlayers[index++] = player.OwnerClientId;
            }

            return this.targetOtherPlayers;
        }
    }

    private ulong[] TargetCurrentPlayer
    { 
        get
        {
            this.targetCurrentPlayer[0] = this.CurrentPlayer.OwnerClientId;
            return this.targetCurrentPlayer;
        }
    }

    public ClientRpcParams ClientParamsOtherPlayers
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.TargetOtherPlayers }
            };
        }
    }

    public ClientRpcParams ClientParamsCurrentPlayer
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.TargetCurrentPlayer }
            };
        }
    }

    public ServerRpcParams SenderCurrentClient
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

    private void Start()
    {
        this.targetCurrentPlayer = new ulong[1];
        this.targetOtherPlayers = new ulong[LobbyManager.Instance.LocalLobby.Players.Count - 1];

        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
        UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageWaitingOtherPlayers;
        UIManagerGlobal.Instance.PanelMessageBox.Show(null);
    }

    private void OnEnable()
    {
        LobbyManager.Instance.OnMonopolyGameLoaded += this.HandleMonopolyGameLoaded;
    }

    private void OnDisable()
    {
        LobbyManager.Instance.OnMonopolyGameLoaded -= this.HandleMonopolyGameLoaded;
    }

    private void HandleMonopolyGameLoaded()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            this.StartCoroutine(this.WaitOtherPlayersCoroutine());
        }

        

        GameCoordinator.Instance.LocalPlayer.Data[LobbyManager.KEY_PLAYER_ACTIVE_SCENE].Value = GameCoordinator.Instance.ActiveScene.ToString();
    }

    private IEnumerator WaitOtherPlayersCoroutine()
    {
        float elapsedTime = 0f;

        while (LobbyManager.Instance.ArePlayersLoaded && elapsedTime < LobbyManager.PLAYER_LOADING_TIMEOUT)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!LobbyManager.Instance.ArePlayersLoaded)
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessagePlayersFailedToLoad;
            UIManagerGlobal.Instance.PanelMessageBox.Show(null);

            this.StopCoroutine(this.WaitOtherPlayersCoroutine());

            GameCoordinator.Instance.LoadSceneNetwork(GameCoordinator.MonopolyScene.GameLobby);
        }
        else
        {
            this.StartGame();
        }
    }

    private void StartGame()
    {
        //GameObject go = GameObject.Instantiate(this.gameObject);

        //go.GetComponent<NetworkObject>().SpawnWithOwnership();





























        //this.SpawnMonopolyPlayerServerRpc(this.SenderCurrentClient);

        //foreach (Player player in LobbyManager.Instance.LocalLobby.Players)
        //{
        //    GameObject go = Instantiate(myPrefab, Vector3.zero, Quaternion.identity);
        //    go.GetComponent<NetworkObject>().Spawn();
        //}

        //this.players = new List<MonopolyPlayer>();
        //

        //GameObject.Instantiate(this.monopolyPlayersTokens.);
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void SpawnMonopolyPlayerServerRpc(ServerRpcParams serverRpcParams)
    //{
    //    Debug.Log(NetworkManager.Singleton.IsHost);
    //    Debug.Log(NetworkManager.Singleton.IsServer);
    //    Debug.Log(NetworkManager.Singleton.IsClient);

    //    GameObject newMonopolyPlayer = GameObject.Instantiate(this.monopolyPlayers[this.currentMonopolyPlayerIndex++].gameObject);
    //    newMonopolyPlayer.GetComponent<NetworkObject>().Spawn();
    //}

    //[ClientRpc]
    //private void SpawnMonopolyPlayerClientRpc(ClientRpcParams clientRpcParams)
    //{

    //}

    //#region DEBUG

    //[Space]
    //[Header("DEBUG ONLY")]
    //[Space]

    //[SerializeField] private bool RUN;

    //private bool debugBeenInitialized;

    //private void Update()
    //{
    //    if (RUN && !debugBeenInitialized)
    //    {
    //        debugBeenInitialized = true;
    //        this.DebugStartGameClientRpc();
    //        this.StartTurnServerRpc();
    //    }
    //    //else
    //    //{
    //    //    Debug.Log($"Current player: {this.currentPlayerIndex}");
    //    //}
    //}

    //[ClientRpc]
    //private void DebugStartGameClientRpc(ClientRpcParams clientRpcParams = default)
    //{
    //    MonopolyPlayer[] debugPlayers = FindObjectsOfType<MonopolyPlayer>();

    //    foreach (MonopolyPlayer player in debugPlayers)
    //    {
    //        this.players.Add(player);
    //        player.InitializePlayerClientRpc();
    //    }

    //    //this.targetAllPlayers = new ulong[this.players.Count];
    //    this.targetOtherPlayers = new ulong[this.players.Count - 1];
    //}

    //#endregion

    #region Game Loop

    [ServerRpc]
    private void StartTurnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentPlayer);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchPlayerServerRpc(ServerRpcParams serverRpcParams = default)
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
            this.currentPlayerIndex = ++this.currentPlayerIndex % this.monopolyPlayers.Count;
        }

        this.SwitchPlayerClientRpc(this.currentPlayerIndex);

        this.StartTurnServerRpc();
    }

    [ClientRpc]
    private void SwitchPlayerClientRpc(int currentPlayerIndex, ClientRpcParams clientRpcParams = default)
    {
        this.currentPlayerIndex = currentPlayerIndex;
    }

    #endregion

    #region Dice

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
