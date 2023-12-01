using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public sealed class GameManager : NetworkBehaviour
{
    #region In-editor Setup (Logic)

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

    [Space]
    [Header("Chance nodes")]
    [Space]

    [Space]
    [SerializeField] private List<SO_ChanceNode> chanceCards = new List<SO_ChanceNode>();

    #endregion

    public static GameManager Instance { get; private set; }

    private ulong[] targetAllPlayers;

    private ulong[] targetTradePlayers;

    private ulong[] targetCurrentPlayer;

    private int rolledDoubles;

    private List<Player> players;

    private int currentPlayerIndex;

    public int FirstDieValue { get; private set; }

    public int SecondDieValue { get; private set; }

    public int CircleBonus { get => this.circleBonus; }

    public int MaxTurnsInJail { get => this.maxTurnsInJail; }

    public int MaxDoublesInRow { get => this.maxDoublesInRow; }

    public int StartingBalance { get => this.startingBalance; }

    public int ExactCircleBonus { get => this.exactCircleBonus; }

    public float PlayerMovementSpeed { get => this.playerMovementSpeed; }

    public Player CurrentPlayer { get => this.players[this.currentPlayerIndex]; }

    public int TotalRollResult { get => this.FirstDieValue + this.SecondDieValue; }

    public bool HasRolledDouble { get => this.FirstDieValue == this.SecondDieValue; }

    public ulong[] TargetAllPlayers
    {
        get
        {
            for (int i = 0; i < this.players.Count; ++i)
                this.targetAllPlayers[i] = this.players[i].OwnerClientId;

            return this.targetAllPlayers;
        }
    }

    public ulong[] TargetOtherPlayers
    {
        get
        {
            for (int i = 0; i < this.players.Count; ++i)
            {
                if (this.CurrentPlayer.OwnerClientId != this.players[i].OwnerClientId)
                    this.targetAllPlayers[i] = this.players[i].OwnerClientId;
            }

            return this.targetAllPlayers;
        }
    }

    public ulong[] TargetCurrentPlayer
    { 
        get
        {
            this.targetCurrentPlayer[0] = this.CurrentPlayer.OwnerClientId;
            return this.targetCurrentPlayer;
        }
    }

    public ClientRpcParams ClientParamsAllPlayers
    {
        get
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = this.TargetAllPlayers }
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

    private void Awake() => Instance = this;

    private void Start()
    {
        this.players = new List<Player>();
        this.targetCurrentPlayer = new ulong[1];
    }

    #region DEBUG

    [Space]
    [Header("DEBUG ONLY")]
    [Space]

    [SerializeField] private bool RUN;

    private bool debugBeenInitialized;

    private void Update()
    {
        if (RUN && !debugBeenInitialized)
        {
            debugBeenInitialized = true;
            this.DebugStartGameClientRpc();
        }
        //else
        //{
        //    Debug.Log($"Current player: {this.currentPlayerIndex}");
        //}
    }

    [ClientRpc]
    private void DebugStartGameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Player[] debugPlayers = FindObjectsOfType<Player>();

        foreach (Player player in debugPlayers)
        {
            this.players.Add(player);
            player.InitializePlayerClientRpc();
        }

        this.targetAllPlayers = new ulong[this.players.Count];

        this.StartCoroutine(this.StartPlayerTurn());
    }

    #endregion

    #region Turn-based Game Logic

    //private void GameLoop()
    //{
    //    while (true)
    //    {
    //        this.StartCoroutine(Loop());
    //    }

    //    IEnumerator Loop()
    //    {
    //        yield return this.StartCoroutine(this.StartPlayerTurn());
    //    }
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void StartPlayerTurnServerRpc(ServerRpcParams serverRpcParams = default) => this.StartPlayerTurnClientRpc(this.ClientParamsCurrentPlayer);

    //[ClientRpc]
    //private void StartPlayerTurnClientRpc(ClientRpcParams clientRpcParams) => this.StartCoroutine(this.StartPlayerTurn());

    private IEnumerator StartPlayerTurn()
    {
        Debug.Log("In StartPlayerTurn");

        this.CurrentPlayer.HasCompletedTurn = false;
        this.CurrentPlayer.PerformTurnClientRpc(this.ClientParamsCurrentPlayer);
        yield return new WaitUntil(() => this.CurrentPlayer.HasCompletedTurn);
        
        this.SyncSwitchPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncSwitchPlayerServerRpc()
    {
        Debug.Log("In SyncSwitchPlayerServerRpc");

        //if (!this.HasRolledDouble)
        //{
        //    this.rolledDoubles = 0;
        //    this.currentPlayerIndex = ++this.currentPlayerIndex % this.players.Count;
        //}
        //else
        //{
        //    ++this.rolledDoubles;

        //    if (this.rolledDoubles >= this.MaxDoublesInRow)
        //    {
        //        this.rolledDoubles = 0;
        //        this.CurrentPlayer.HandleSendJailLanding();
        //    }
        //}

        this.currentPlayerIndex = ++this.currentPlayerIndex % this.players.Count;

        this.SyncSwitchPlayerClientRpc(this.currentPlayerIndex, this.ClientParamsAllPlayers);

        this.StartCoroutine(this.StartPlayerTurn());
    }

    [ClientRpc]
    private void SyncSwitchPlayerClientRpc(int currentPlayerIndex, ClientRpcParams clientRpcParams)
    {
        Debug.Log("In SyncSwitchPlayerClientRpc");
        this.currentPlayerIndex = currentPlayerIndex;
    }

    #endregion

    #region Dice (Logic & Sync)

    public void RollDice()
    {
        const int MIN_DIE_VALUE = 1;
        const int MAX_DIE_VALUE = 6;

        this.FirstDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);
        this.SecondDieValue = UnityEngine.Random.Range(MIN_DIE_VALUE, MAX_DIE_VALUE + 1);

        this.SyncRollDiceServerRpc(this.FirstDieValue, this.SecondDieValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncRollDiceServerRpc(int firstDieValue, int secondDieValue)
    {
        this.FirstDieValue = firstDieValue;
        this.SecondDieValue = secondDieValue;

        this.SyncRollDiceClientRpc(firstDieValue, secondDieValue, this.ClientParamsAllPlayers);
    }

    [ClientRpc]
    private void SyncRollDiceClientRpc(int firstDieValue, int secondDieValue, ClientRpcParams clientRpcParams)
    {
        this.FirstDieValue = firstDieValue;
        this.SecondDieValue = secondDieValue;
    }

    #endregion

    public void HandlePlayerLanding(Player player)
    {
        switch (player.CurrentNode.Type)
        {
            case MonopolyNode.MonopolyNodeType.Tax:
                player.HandleTaxLanding();
                break;
            case MonopolyNode.MonopolyNodeType.Jail:
                player.HandleJailLanding();
                break;
            case MonopolyNode.MonopolyNodeType.Start:
                player.HandleStartLanding();
                break;
            case MonopolyNode.MonopolyNodeType.Chance:
                player.HandleChanceLanding();
                break;
            case MonopolyNode.MonopolyNodeType.SendJail:
                player.HandleSendJailLanding();
                break;
            case MonopolyNode.MonopolyNodeType.Property:
                player.HandlePropertyLanding();
                break;
            case MonopolyNode.MonopolyNodeType.Gambling:
                player.HandlePropertyLanding();
                break;
            case MonopolyNode.MonopolyNodeType.Transport:
                player.HandlePropertyLanding();
                break;
            case MonopolyNode.MonopolyNodeType.FreeParking:
                player.HandleFreeParkingLanding();
                break;
        }
    }

    public SO_ChanceNode GetChance() => this.chanceCards[UnityEngine.Random.Range(0, this.chanceCards.Count)];

    private void SendFunds(Player player, int amount)
    {
        player.Balance += amount;

        //Update ui
    }

    private void CollectFunds(Player player, int amount)
    {
        if (player.Balance < amount)
        {
            throw new NotImplementedException();
        }
        else
        {
            player.Balance -= amount;
        }

        //Update ui
    }
}









































//[ClientRpc]
//private void SendToJail(Player player)
//{
//    player.IsInJail = true;
//    this.MovePlayer(player, MonopolyBoard.Instance.GetDistanceBetweenNodes(player.CurrentNode, MonopolyBoard.Instance.NodeJail));
//}



//    private void ReleaseFromJail(Player player)
//    {
//        player.TurnsInJail = 0;
//        player.IsInJail = false;
//        this.CurrentPlayerDoubles = 0;
//        this.MovePlayer(player, this.TotalRollResult);
//    }

//    private void SendBalance(Player player, int amount)
//    {
//        player.Balance += amount;

//        // Update ui
//    }

//    public void CollectFee()
//    {
//        if (this.CurrentPlayer.CurrentNode.TaxAmount <= this.CurrentPlayer.Balance)
//            this.CurrentPlayer.Balance -= this.CurrentPlayer.CurrentNode.TaxAmount;
//        else
//        {
//            // handle insufficient funds
//        }

//        UIManager.Instance.HidePanelFee();
//    }

//    public void BuyProperty()
//    {
//        UIManager.Instance.HidePanelFee();
//    }

//    private void HandleChanceLanding(int index)
//    {
//        switch (this.chances[index].Type) 
//        {
//            case ChanceNodeSO.ChanceNodeType.Reward:
//                UIManager.Instance.ShowPanelOk(null, null);
//                break;
//            case ChanceNodeSO.ChanceNodeType.Penalty:
//                UIManager.Instance.ShowPanelFee(null, null);
//                break;
//            case ChanceNodeSO.ChanceNodeType.SkipTurn:
//                UIManager.Instance.ShowPanelOk(null, null);
//                //this.CurrentPlayer.SkipTurn = true;
//                //this.SwitchPlayer();
//                break;
//            case ChanceNodeSO.ChanceNodeType.SendJail:
//                UIManager.Instance.ShowPanelOk(null, null);
//                this.SendToJail(this.CurrentPlayer);
//                break;
//            case ChanceNodeSO.ChanceNodeType.RandomMovement:
//                UIManager.Instance.ShowPanelOk(null, null);
//                //this.MovePlayer(this.CurrentPlayer, Random.Range())
//                break;
//        }
//    }























































//[SerializeField]
//private MonopolyBoard monopolyBoard;

//[SerializeField]
//public List<Player> players = new List<Player>();

//[SerializeField]
//private List<GameObject> tokens = new List<GameObject>();

//[SerializeField]
//private GameObject playerInfoPrefab;

//[SerializeField]
//private Transform playersPanel;

//public int taxPool = 0;

//[SerializeField]
//public float SecondsBetweenTurns = 2.0f;

//public static GameManager instance;

//int[] rolledDice;
//bool isDoubleRolled;
//int doubleRollCount;


//private void Start()
//{
//    // Add players to the game

//    Initialize();
//    RollDice();
//}

////private void Update()
////{
////    Invoke("RollDice", 1f);
////    SwitchPlayer();
////}

//public void ResetRolledDouble()
//{
//    RolledADounle = false;
//}

//private void Initialize()
//{
//    for (int i = 0; i < players.Count; ++i)
//    {
//        GameObject infoObject = Instantiate(playerInfoPrefab, playersPanel, false);
//        PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

//        int randomIndex = Random.Range(0, tokens.Count);

//        GameObject playerToken = Instantiate(tokens[randomIndex], monopolyBoard.route[0].transform.position, Quaternion.identity);

//        players[i].Initialize(monopolyBoard.route[0], this.startBalance, info, playerToken);
//    }

//    OnShowInputPanel.Invoke(true, true, true);
//}

//public void RollDice()
//{
//    bool allowedToMove = true;

//    rolledDice = new int[2];

//    rolledDice[0] = Random.Range(1, 7);
//    rolledDice[1] = Random.Range(1, 7);

//    isDoubleRolled = rolledDice[0] == rolledDice[1];

//    if (players[CurrentPlayer].IsInJail)
//    {
//        players[CurrentPlayer].IncreaseNumberOfTurnsInJail();

//        if (isDoubleRolled)
//        {
//            players[CurrentPlayer].SetOutOfJail();
//            doubleRollCount++;
//        }
//        else if (players[CurrentPlayer].NumberTurnsInJail >= maxTurnsInJail)
//        {
//            players[CurrentPlayer].SetOutOfJail();
//        }
//        else
//        {
//            allowedToMove = false;
//        }
//    }
//    else
//    {
//        if (!isDoubleRolled)
//        {
//            doubleRollCount = 0;
//        }
//        else
//        {
//            doubleRollCount++;

//            if (doubleRollCount >= 3)
//            {
//                int indexOnBoard = MonopolyBoard.instance.route.IndexOf(players[CurrentPlayer].CurrentPosition);
//                players[CurrentPlayer].GoToJail(indexOnBoard);
//                RolledADounle = false;
//                return;
//            }
//        }
//    }

//    if (allowedToMove)
//    {
//        StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
//    }
//    else
//    {
//        StartCoroutine(DelayBetweenSwitchPlayer());
//    }

//    OnShowInputPanel.Invoke(true, false, false);
//}

//IEnumerator DelayBeforeMove(int rolledDice)
//{
//    yield return new WaitForSeconds(SecondsBetweenTurns);
//    monopolyBoard.MovePlayerToken(players[CurrentPlayer], rolledDice);
//}

//IEnumerator DelayBetweenSwitchPlayer()
//{
//    yield return new WaitForSeconds(SecondsBetweenTurns);
//    SwitchPlayer();
//}

//public void SwitchPlayer()
//{
//    CurrentPlayer++;

//    doubleRollCount = 0;

//    if (CurrentPlayer >= players.Count)
//    {
//        CurrentPlayer = 0;
//    }

//    OnShowInputPanel.Invoke(true, true, true);
//}

//public int[] LastRolledDice()
//{
//    return rolledDice;
//}

//public void AddTaxToPool(int amount)
//{
//    taxPool += amount;
//}

//public int GetTaxPool()
//{
//    int currentTaxCollected = taxPool;
//    taxPool = 0;
//    return currentTaxCollected;
//}

//public void RemovePlayer(Player player)
//{
//    players.Remove(player);
//    CheckForGameOver();
//}

//void CheckForGameOver()
//{
//    if (players.Count == 1)
//    {

//    }
//}