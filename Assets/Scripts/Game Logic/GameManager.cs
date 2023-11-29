using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public sealed class GameManager : NetworkBehaviour
{
    #region Values

    [Space]
    [Header("Values")]
    [Space]

    [SerializeField] [Range(0, 100_000)] private int startingBalance = 15_000;

    [SerializeField] [Range(0, 10)] private int maxTurnsInJail = 3;

    [SerializeField] [Range(0, 10)] private int maxDoublesInRow = 2;

    [SerializeField] [Range(0, 100_000)] private int circleBonus = 2_000;

    [SerializeField] [Range(0, 100_000)] private int exactCircleBonus = 3_000;

    [SerializeField] [Range(0.0f, 10.0f)] private float delayBetweenTurns = 0.5f;

    [SerializeField] [Range(0.0f, 10.0f)] private float delayBetweenNodes = 1.0f;

    [SerializeField] [Range(0.0f, 100.0f)] private float playerMovementSpeed = 25.0f;

    #endregion

    #region Chance Cards

    [Space]
    [Header("Chance cards")]
    [Space]

    [SerializeField] private List<SO_ChanceNode> chanceCards = new List<SO_ChanceNode>();

    #endregion

    public static GameManager Instance { get; private set; }

    private ulong[] targetTurnId;

    private int doublesInRow;

    private List<Player> players;

    private int currentPlayerIndex;

    public int FirstCubeValue { get; private set; }

    public int SecondCubeValue { get; private set; }

    public int CircleBonus { get => this.circleBonus; }

    public int MaxTurnsInJail { get => this.maxTurnsInJail; }

    public int MaxDoublesInRow { get => this.maxDoublesInRow; }

    public int StartingBalance { get => this.startingBalance; }

    public int ExactCircleBonus { get => this.exactCircleBonus; }

    public float PlayerMovementSpeed { get => this.playerMovementSpeed; }

    private Player currentPlayer { get => this.players[this.currentPlayerIndex]; }

    public int TotalRollResult { get => this.FirstCubeValue + this.SecondCubeValue; }

    public bool HasRolledDouble { get => this.FirstCubeValue == this.SecondCubeValue; }

    private void Awake() => Instance = this;

    private void Start()
    {
        this.targetTurnId = new ulong[1];
        this.players = new List<Player>();
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

        this.StartCoroutine(this.GameLoop());
    }

    #endregion

    private IEnumerator GameLoop()
    {
        while (true)
        {
            yield return StartCoroutine(this.StartPlayerTurn());
        }
    }

    private IEnumerator StartPlayerTurn()
    {
        this.targetTurnId[0] = this.currentPlayer.OwnerClientId;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = this.targetTurnId }
        };

        //this.players[this.currentPlayerIndex].PerformTurnClientRpc(clientRpcParams);
        this.players[this.currentPlayerIndex].PerformTurn();

        yield return new WaitUntil(() => this.currentPlayer.HasCompletedTurn);

        //this.SyncSwitchPlayerClientRpc(this.currentPlayerIndex);

        yield return new WaitForSeconds(this.delayBetweenTurns);
    }

    //[ClientRpc]
    //private void SyncSwitchPlayerClientRpc(int indexOfPlayer, ClientRpcParams clientRpcParams = default)
    //{
    //    this.players[indexOfPlayer].HasCompletedTurn = true;

    //    if (!this.HasRolledDouble)
    //    {
    //        this.doublesInRow = 0;
    //        this.currentPlayerIndex = ++this.currentPlayerIndex % players.Count;
    //    }
    //    else
    //    {
    //        if (++this.doublesInRow >= this.MaxDoublesInRow)
    //            this.currentPlayer.GoToJail();
    //    }
    //}

    public void RollDices()
    {
        const int MIN_CUBE_VALUE = 1;
        const int MAX_CUBE_VALUE = 6;

        int firstCubeValue = UnityEngine.Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);
        int secondCubeValue = UnityEngine.Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);

        //this.SyncRollDicesClientRpc(firstCubeValue, secondCubeValue);

        this.FirstCubeValue = firstCubeValue;
        this.SecondCubeValue = secondCubeValue;
    }

    public SO_ChanceNode GetChance() => this.chanceCards[UnityEngine.Random.Range(0, this.chanceCards.Count)];

    //[ClientRpc]
    //private void SyncRollDicesClientRpc(int firstCubeValue, int secondCubeValue, ClientRpcParams clientRpcParams = default)
    //{
    //    this.FirstCubeValue = firstCubeValue;
    //    this.SecondCubeValue = secondCubeValue;

    //    //this.FirstCubeValue = -1;
    //    //this.SecondCubeValue = -1;
    //}

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
//        this.currentPlayerDoubles = 0;
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

//    if (players[currentPlayer].IsInJail)
//    {
//        players[currentPlayer].IncreaseNumberOfTurnsInJail();

//        if (isDoubleRolled)
//        {
//            players[currentPlayer].SetOutOfJail();
//            doubleRollCount++;
//        }
//        else if (players[currentPlayer].NumberTurnsInJail >= maxTurnsInJail)
//        {
//            players[currentPlayer].SetOutOfJail();
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
//                int indexOnBoard = MonopolyBoard.instance.route.IndexOf(players[currentPlayer].CurrentPosition);
//                players[currentPlayer].GoToJail(indexOnBoard);
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
//    monopolyBoard.MovePlayerToken(players[currentPlayer], rolledDice);
//}

//IEnumerator DelayBetweenSwitchPlayer()
//{
//    yield return new WaitForSeconds(SecondsBetweenTurns);
//    SwitchPlayer();
//}

//public void SwitchPlayer()
//{
//    currentPlayer++;

//    doubleRollCount = 0;

//    if (currentPlayer >= players.Count)
//    {
//        currentPlayer = 0;
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