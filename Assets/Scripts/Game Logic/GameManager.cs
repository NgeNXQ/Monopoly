using UnityEngine;
using System.Collections.Generic;

public sealed class GameManager : MonoBehaviour
{
    const int START_BALANCE = 15000;

    const int CIRCLE_BONUS = 2000;
    const int EXACT_CIRCLE_BONUS = 3000;

    const int MAX_TURNS_IN_JAIL = 3;
    const int MAX_DOUBLES_IN_ROW = 2;

    const float PLAYER_MOVEMENT_SPEED = 25.0f;

    [SerializeField] private List<Player> players = new List<Player>();

    [SerializeField] private List<ChanceNodeSO> chances = new List<ChanceNodeSO>();

    public static GameManager Instance { get; private set; }

    private int currentPlayerIndex;

    private int currentPlayerDoubles;

    public int FirstCubeValue { get; private set; }

    public int SecondCubeValue { get; private set; }

    public Player CurrentPlayer { get => this.players[this.currentPlayerIndex]; }

    public int TotalRollResult { get => this.FirstCubeValue + this.SecondCubeValue; }

    public bool HasRolledDouble { get => this.FirstCubeValue == this.SecondCubeValue; }

    private void Awake()
    {
        Instance = this;

        //foreach (Player player in this.players)
        //    player.Initialize(START_BALANCE);

#if Debug
        //this.players = new List<Player>();
        //this.players.Add(new Player("#TestName", START_BALANCE));
#endif
    }

    private void Start() => UIManager.Instance.ShowButtonRoll();

    public void RollDices()
    {
        UIManager.Instance.HideButtonRolls();

        const int MIN_CUBE_VALUE = 1;
        const int MAX_CUBE_VALUE = 6;

        this.FirstCubeValue = Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);
        this.SecondCubeValue = Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);

#if Debug
        this.FirstCubeValue = 2;
        this.SecondCubeValue = 2;
#endif

        if (this.CurrentPlayer.IsInJail)
        {
            if (this.HasRolledDouble)
            {
                this.ReleaseFromJail(this.CurrentPlayer);
            }
            else
            {
                ++this.CurrentPlayer.TurnsInJail;

                if (this.CurrentPlayer.TurnsInJail >= MAX_TURNS_IN_JAIL)
                {
                    // Ask to pay a fee
                    // Release from jail
                        //ReleaseFromJail(this.players[this.currentPlayerIndex]);
                }
            }  
        }

#if Debug
        Debug.Log($"{this.FirstCubeValue}; {this.SecondCubeValue}");
#endif

        this.MovePlayer(this.CurrentPlayer, this.TotalRollResult);
    }

    public void SwitchPlayer()
    {
        UIManager.Instance.ShowButtonRoll();

        //if (this.HasRolledDouble && !this.CurrentPlayer.IsInJail)
        //{
        //    ++this.currentPlayerDoubles;

        //    if (this.currentPlayerDoubles >= MAX_DOUBLES_IN_ROW + 1)
        //        this.SendToJail(this.CurrentPlayer);

        //    UIHandler.Instance.ActivateButtonRollDices();
        //}
        //else
        //{
        //    ++this.currentPlayerIndex;
        //    this.currentPlayerIndex %= this.players.Count;

        //    UIHandler.Instance.HideButtonRollDices();

        //    if (this.CurrentPlayer.IsInJail)
        //    {
        //        UIHandler.Instance.ShowButtonRollDices();

        //        // Handle jail case
        //    }
        //    else
        //    {
        //        UIHandler.Instance.ShowButtonRollDices();
        //    }
        //}
    }

    public void MovePlayer(Player player, int steps)
    {
        bool hasFinishedCircle = false;
        int currentNodeIndex = player.CurrentNodeIndex;

        while (steps != 0)
        {
            --steps;

            currentNodeIndex = ++currentNodeIndex % MonopolyBoard.Instance.Nodes.Count;

            hasFinishedCircle = MonopolyBoard.Instance.Nodes[currentNodeIndex] == MonopolyBoard.Instance.NodeStart;

            Vector3 direction = MonopolyBoard.Instance.Nodes[currentNodeIndex].transform.position - player.transform.position;

            player.transform.Translate(direction);
        }

        player.CurrentNode = MonopolyBoard.Instance.Nodes[currentNodeIndex];

        if (hasFinishedCircle && player.CurrentNode != MonopolyBoard.Instance.NodeStart)
            this.SendBalance(player, CIRCLE_BONUS);

        this.HandlePlayerLanding(player);
        this.SwitchPlayer();
    }

    public void HandlePlayerLanding(Player player)
    {
        if (player.CurrentNode.Owner != player)
        {
            switch (player.CurrentNode.Type)
            {
                case MonopolyNode.MonopolyNodeType.Tax:
                    UIManager.Instance.ShowPanelFee(this.CurrentPlayer.CurrentNode.SpriteMonopolyNode, "Test text");
                    break;
                case MonopolyNode.MonopolyNodeType.Jail:
                    UIManager.Instance.ShowPanelOk(this.CurrentPlayer.CurrentNode.SpriteMonopolyNode, "Test text");
                    break;
                case MonopolyNode.MonopolyNodeType.Start:
                    this.SendBalance(player, EXACT_CIRCLE_BONUS);
                    break;
                case MonopolyNode.MonopolyNodeType.Gamble:
                    player.HandlePropertyLanding();
                    break;
                case MonopolyNode.MonopolyNodeType.Chance:
                    this.HandleChanceLanding(Random.Range(0, this.chances.Count));
                    break;
                case MonopolyNode.MonopolyNodeType.SendJail:
                    UIManager.Instance.ShowPanelOk(this.CurrentPlayer.CurrentNode.SpriteMonopolyNode, "Test text");
                    this.SendToJail(player);
                    break;
                case MonopolyNode.MonopolyNodeType.Property:
                    player.HandlePropertyLanding();
                    break;
                case MonopolyNode.MonopolyNodeType.Transport:
                    player.HandlePropertyLanding();
                    break;
            }
        }
    }

    private void SendToJail(Player player)
    {
        player.IsInJail = true;
        this.MovePlayer(player, MonopolyBoard.Instance.GetDistanceBetweenNodes(player.CurrentNode, MonopolyBoard.Instance.NodeJail));
    }

    private void ReleaseFromJail(Player player)
    {
        player.TurnsInJail = 0;
        player.IsInJail = false;
        this.currentPlayerDoubles = 0;
        this.MovePlayer(player, this.TotalRollResult);
    }

    private void SendBalance(Player player, int amount)
    {
        player.Balance += amount;

        // Update ui
    }

    public void CollectFee()
    {
        if (this.CurrentPlayer.CurrentNode.TaxAmount <= this.CurrentPlayer.Balance)
            this.CurrentPlayer.Balance -= this.CurrentPlayer.CurrentNode.TaxAmount;
        else
        {
            // handle insufficient funds
        }

        UIManager.Instance.HidePanelFee();
    }

    public void BuyProperty()
    {
        UIManager.Instance.HidePanelFee();
    }

    private void HandleChanceLanding(int index)
    {
        switch (this.chances[index].Type) 
        {
            case ChanceNodeSO.ChanceNodeType.Reward:
                UIManager.Instance.ShowPanelOk(null, null);
                break;
            case ChanceNodeSO.ChanceNodeType.Penalty:
                UIManager.Instance.ShowPanelFee(null, null);
                break;
            case ChanceNodeSO.ChanceNodeType.SkipTurn:
                UIManager.Instance.ShowPanelOk(null, null);
                //this.CurrentPlayer.SkipTurn = true;
                //this.SwitchPlayer();
                break;
            case ChanceNodeSO.ChanceNodeType.SendJail:
                UIManager.Instance.ShowPanelOk(null, null);
                this.SendToJail(this.CurrentPlayer);
                break;
            case ChanceNodeSO.ChanceNodeType.RandomMovement:
                UIManager.Instance.ShowPanelOk(null, null);
                //this.MovePlayer(this.CurrentPlayer, Random.Range())
                break;
        }
    }


























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
}
