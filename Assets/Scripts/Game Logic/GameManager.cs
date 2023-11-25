using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public sealed class GameManager : NetworkBehaviour
{
    #region Values

    [Space]
    [Header("Values")]
    [Space]

    [SerializeField] [Range(0, 100_000)] private int startingBalance = 15_000;

    [SerializeField] [Range(0, 100)] private int maxTurnsInJail = 3;

    [SerializeField] [Range(0, 100)] private int maxDoublesInRow = 2;

    [SerializeField][Range(0, 100_000)] private int circleBonus = 2_000;

    [SerializeField][Range(0, 100_000)] private int exactCircleBonus = 3_000;

    [SerializeField] [Range(0.0f, 100.0f)] private float playerMovementSpeed = 25.0f;

    #endregion

    #region Chance Cards

    [Space]
    [Header("Chance cards")]
    [Space]

    [SerializeField] private List<Player> players = new List<Player>();

    [SerializeField] private List<SO_ChanceNode> chanceCards = new List<SO_ChanceNode>();

    #endregion

    public static GameManager Instance { get; private set; }

    //private ulong currentPlayerId;

    private int currentPlayerIndex;

    private int currentPlayerRolledDoubles;

    public int FirstCubeValue { get; private set; }

    public int SecondCubeValue { get; private set; }

    public int CircleBonus { get => this.circleBonus; }

    public int MaxTurnsInJail { get => this.maxTurnsInJail; }

    public int MaxDoublesInRow { get => this.maxDoublesInRow; }

    public int StartingBalance { get => this.startingBalance; }

    public int ExactCircleBonus { get => this.exactCircleBonus; }

    public float PlayerMovementSpeed { get => this.playerMovementSpeed; }

    private int totalRollResult { get => this.FirstCubeValue + this.SecondCubeValue; }

    private bool hasRolledDouble { get => this.FirstCubeValue == this.SecondCubeValue; }

    private Player currentPlayer { get => this.players[this.currentPlayerIndex]; }

    private void Awake()
    {
        Instance = this;

        foreach (Player player in this.players)
            player.OnNetworkSpawn();

        if (this.IsServer)
            Debug.Log("I am host");
    }

    // private void Start() => StartCoroutine(GameLoop());

    private float gameLoopInterval = 5f;

    private IEnumerator GameLoop()
    {
        while (true) 
        {
            yield return new WaitForSeconds(gameLoopInterval);
            SwitchPlayer();
        }
    }

    private void SwitchPlayer()
    {
        this.currentPlayerIndex %= this.players.Count;

        // Show ui to roll dices
        this.RollDices();

        if (this.hasRolledDouble)
        {
            ++this.currentPlayerRolledDoubles;

            if (this.currentPlayerRolledDoubles >= this.MaxDoublesInRow)
            {
                // show ui
                // Send to jail
                return;
            }

            if (this.currentPlayer.IsInJail)
            {
                // show ui
                // Release from jail
            }
        }

        //this.MovePlayerClientRpc(this.totalRollResult);

        //if (this.IsHost)
            //this.MovePlayer(this.currentPlayer, this.totalRollResult);
        //else if (this.IsClient)

        this.MovePlayerClientRpc();
        this.MovePlayerServerRpc();
        this.currentPlayer.gameObject.transform.position = Vector3.zero;

        // handle landing
        // this.HandlePlayerLanding (ClientRpc) ???

        if (!this.hasRolledDouble)
            ++this.currentPlayerIndex;
    }

    private void RollDices()
    {
        const int MIN_CUBE_VALUE = 1;
        const int MAX_CUBE_VALUE = 6;

        this.FirstCubeValue = UnityEngine.Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);
        this.SecondCubeValue = UnityEngine.Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);

        Debug.Log(this.FirstCubeValue + ", " + this.SecondCubeValue);

        // show ui
    }

    [ServerRpc]
    private void MovePlayerServerRpc()
    {
        Debug.Log("Somehow moving on host");
        this.currentPlayer.gameObject.transform.position = Vector3.zero;
    }

    [ClientRpc]
    private void MovePlayerClientRpc()
    {
        Debug.Log("Somehow moving on client");
        this.currentPlayer.gameObject.transform.position = Vector3.zero;
    }

    ///
    public void MovePlayer(int steps, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Moving");
        StartCoroutine(MovePlayerCoroutine(steps));
    }

    private IEnumerator MovePlayerCoroutine(int steps)
    {
        float delayBetweenMoves = 0.1f;
        int currentNodeIndex = this.currentPlayer.CurrentNodeIndex;

        while (steps != 0)
        {
            --steps;

            currentNodeIndex = ++currentNodeIndex % MonopolyBoard.Instance.Nodes.Count;

            Vector3 targetPosition = MonopolyBoard.Instance.Nodes[currentNodeIndex].transform.position;

            Debug.Log(targetPosition);

            this.currentPlayer.transform.Translate(targetPosition - this.currentPlayer.transform.position);

            yield return new WaitForSeconds(delayBetweenMoves);
        }

        this.currentPlayer.CurrentNode = MonopolyBoard.Instance.Nodes[currentNodeIndex];
    }

    //bool hasFinishedCircle = false;
    //int currentNodeIndex = this.currentPlayer.CurrentNodeIndex;

    //while (steps != 0)
    //{
    //    --steps;

    //    currentNodeIndex = ++currentNodeIndex % MonopolyBoard.Instance.Nodes.Count;

    //    hasFinishedCircle = MonopolyBoard.Instance.Nodes[currentNodeIndex] == MonopolyBoard.Instance.NodeStart;

    //    Vector3 direction = MonopolyBoard.Instance.Nodes[currentNodeIndex].transform.position - this.currentPlayer.transform.position;

    //    this.currentPlayer.transform.Translate(direction * Time.deltaTime);
    //}

    //this.currentPlayer.CurrentNode = MonopolyBoard.Instance.Nodes[currentNodeIndex];

    //if (hasFinishedCircle && this.currentPlayer.CurrentNode != MonopolyBoard.Instance.NodeStart)
    //    this.SendBalance(this.currentPlayer, this.circleBonus);

    //IEnumerator MoveNextNode(Vector3 current, Vector3 destination)
    //{
    //    if (current != destination)

    //}
}

    //[ClientRpc]
    //private void SendToJail(Player player)
    //{
    //    player.IsInJail = true;
    //    this.MovePlayer(player, MonopolyBoard.Instance.GetDistanceBetweenNodes(player.CurrentNode, MonopolyBoard.Instance.NodeJail));
    //}

    //public void HandlePlayerLanding(Player player)
    //{
    //    if (player.CurrentNode.Owner != player)
    //    {
    //        switch (player.CurrentNode.Type)
    //        {
    //            case MonopolyNode.MonopolyNodeType.Tax:
    //                UIManager.Instance.ShowPanelFee(this.currentPlayer.CurrentNode.SpriteMonopolyNode, "Test text");
    //                break;
    //            case MonopolyNode.MonopolyNodeType.Jail:
    //                UIManager.Instance.ShowPanelOk(this.currentPlayer.CurrentNode.SpriteMonopolyNode, "Test text");
    //                break;
    //            case MonopolyNode.MonopolyNodeType.Start:
    //                //this.SendBalance(player, EXACT_CIRCLE_BONUS);
    //                break;
    //            case MonopolyNode.MonopolyNodeType.Gamble:
    //                //player.HandlePropertyLanding();
    //                break;
    //            case MonopolyNode.MonopolyNodeType.Chance:
    //                //this.HandleChanceLanding(Random.Range(0, this.chances.Count));
    //                break;
    //            case MonopolyNode.MonopolyNodeType.SendJail:
    //                //UIManager.Instance.ShowPanelOk(this.CurrentPlayer.CurrentNode.SpriteMonopolyNode, "Test text");
    //                //this.SendToJail(player);
    //                break;
    //            case MonopolyNode.MonopolyNodeType.Property:
    //                //player.HandlePropertyLanding();
    //                break;
    //            case MonopolyNode.MonopolyNodeType.Transport:
    //                //player.HandlePropertyLanding();
    //                break;
    //        }
    //    }
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