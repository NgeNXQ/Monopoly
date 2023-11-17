#define DEBUG

using UnityEngine;
using System.Collections;
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

        foreach (Player player in this.players)
            player.Initialize(START_BALANCE);

#if DEBUG
        //this.players = new List<Player>();
        //this.players.Add(new Player("#TestName", START_BALANCE));
#endif
    }

    private void Start() => UIHandler.Instance.ShowButtonRollDices();

    public void RollDices()
    {
        UIHandler.Instance.DeactivateButtonRollDices();

        const int MIN_CUBE_VALUE = 1;
        const int MAX_CUBE_VALUE = 6;

        this.FirstCubeValue = Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);
        this.SecondCubeValue = Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);

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

#if DEBUG
        Debug.Log($"{this.FirstCubeValue}; {this.SecondCubeValue}");
#endif

        this.MovePlayer(this.CurrentPlayer, this.TotalRollResult);
    }

    public void SwitchPlayer()
    {
        if (this.HasRolledDouble && !this.CurrentPlayer.IsInJail)
        {
            ++this.currentPlayerDoubles;

            if (this.currentPlayerDoubles >= MAX_DOUBLES_IN_ROW + 1)
                this.SendToJail(this.CurrentPlayer);

            UIHandler.Instance.ActivateButtonRollDices();
        }
        else
        {
            ++this.currentPlayerIndex;
            this.currentPlayerIndex %= this.players.Count;

            UIHandler.Instance.HideButtonRollDices();

            if (this.CurrentPlayer.IsInJail)
            {
                UIHandler.Instance.ShowButtonRollDices();

                // Handle jail case
            }
            else
            {
                UIHandler.Instance.ShowButtonRollDices();
            }
        }
    }

    public void SendToJail(Player player)
    {
        player.IsInJail = true;
        this.MovePlayer(player, MonopolyBoard.Instance.GetDistanceBetweenNodes(player.CurrentNode, MonopolyBoard.Instance.NodeJail));
    }

    public void ReleaseFromJail(Player player)
    {
        player.TurnsInJail = 0;
        player.IsInJail = false;
        this.currentPlayerDoubles = 0;
        this.MovePlayer(player, this.TotalRollResult);
    }

    public void SendBalance(Player player, int balanceAmount)
    {
        player.Balance += balanceAmount;

        // Update ui
    }

    // Update player movemet (simplify)
    public void MovePlayer(Player player, int steps)
    {
        StartCoroutine(MovePlayerInSteps(player, steps));

        IEnumerator MovePlayerInSteps(Player player, int steps)
        {
            Vector2 endPosition;
            int indexOnBoard = player.CurrentNodeIndex;

            if (steps > 0)
            {
                while (steps > 0)
                {
                    ++indexOnBoard;

                    if (indexOnBoard > MonopolyBoard.Instance.Nodes.Count - 1)
                    {
                        indexOnBoard = 0;
                        this.SendBalance(player, CIRCLE_BONUS);
                    }

                    endPosition = MonopolyBoard.Instance.Nodes[indexOnBoard].transform.position;

                    while (MoveToNextNode(player.gameObject, endPosition))
                        yield return null;

                    --steps;
                }
            }
            else
            {
                while (steps < 0)
                {
                    --indexOnBoard;

                    if (indexOnBoard < 0)
                    {
                        this.SendBalance(player, CIRCLE_BONUS);
                        indexOnBoard = MonopolyBoard.Instance.Nodes.Count - 1;
                    }

                    endPosition = MonopolyBoard.Instance.Nodes[indexOnBoard].transform.position;

                    while (MoveToNextNode(player.gameObject, endPosition))
                        yield return null;

                    ++steps;
                }
            }

            player.CurrentNode = MonopolyBoard.Instance.Nodes[indexOnBoard];

            this.HandlePlayerLanding(player);
            this.SwitchPlayer();
        }

        bool MoveToNextNode(GameObject tokenToMove, Vector3 endPosition)
            => endPosition != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPosition, PLAYER_MOVEMENT_SPEED * Time.deltaTime));
    }

    public void HandlePlayerLanding(Player player)
    {
        if (player.CurrentNode.Owner != player) 
        {
            switch (player.CurrentNode.Type)
            {
                case MonopolyNode.MonopolyNodeType.Tax:
                    // show ui
                    //player.Pay(player.CurrentNode.TaxAmount);
                    break;
                case MonopolyNode.MonopolyNodeType.Jail:
                    // show ui
                    break;
                case MonopolyNode.MonopolyNodeType.Start:
                    this.SendBalance(player, EXACT_CIRCLE_BONUS);
                    //update ui
                    break;
                case MonopolyNode.MonopolyNodeType.Gamble:
                    {
                        if (player.CurrentNode.Owner == null)
                        {
                            // show ui
                            player.BuyProperty(player.CurrentNode);
                        }
                        else
                        {
                            // show ui
                            player.Pay(player.CurrentNode.CurrentRentingPrice);
                        }
                    }
                    break;
                case MonopolyNode.MonopolyNodeType.Chance:
                    // show ui
                    // handle payment if needed
                    break;
                case MonopolyNode.MonopolyNodeType.SendJail:
                    // show ui
                    this.SendToJail(player);
                    break;
                case MonopolyNode.MonopolyNodeType.Property:
                    {
                        if (player.CurrentNode.Owner == null)
                        {
                            // show ui
                            player.BuyProperty(player.CurrentNode);
                        }
                        else
                        {
                            // show ui
                            player.Pay(player.CurrentNode.CurrentRentingPrice);
                        }
                    }
                    // show ui
                    break;
                case MonopolyNode.MonopolyNodeType.Transport:
                    {
                        if (player.CurrentNode.Owner == null)
                        {
                            // show ui
                            player.BuyProperty(player.CurrentNode);
                        }
                        else
                        {
                            // show ui
                            player.Pay(player.CurrentNode.CurrentRentingPrice);
                        }
                    }
                    // show ui
                    break;
            }
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
