#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public sealed class GameManager : MonoBehaviour
{
    const int CIRCLE_BONUS = 2000;
    const int MAX_TURNS_IN_JAIL = 3;
    const int START_BALANCE = 15000;
    const int MAX_DOUBLES_IN_ROW = 2;
    const float PLAYER_MOVEMENT_SPEED = 10.0f;

    public static GameManager Instance;

    private List<Player> players;

    private int currentPlayerIndex;

    private int currentPlayerDoubles;

    public int FirstCubeValue { get; private set; }

    public int SecondCubeValue { get; private set; }

    public bool HasRolledDouble { get => FirstCubeValue == SecondCubeValue; }

    //public delegate void ShowInputPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    //public static ShowInputPanel OnShowInputPanel;

    private void Awake()
    {
        Instance = this;

#if DEBUG
        this.players = new List<Player>();
        this.players.Add(new Player("#TestName", START_BALANCE));
#endif
    }

    public void RollDices()
    {
        const int MIN_CUBE_VALUE = 1;
        const int MAX_CUBE_VALUE = 6;

        this.FirstCubeValue = Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);
        this.SecondCubeValue = Random.Range(MIN_CUBE_VALUE, MAX_CUBE_VALUE + 1);

        if (this.players[this.currentPlayerIndex].IsInJail)
        {
            if (this.HasRolledDouble)
            {
                this.ReleaseFromJail(this.players[this.currentPlayerIndex]);
            }
            else
            {
                ++this.players[this.currentPlayerIndex].TurnsInJail;

                if (this.players[this.currentPlayerIndex].TurnsInJail >= MAX_TURNS_IN_JAIL)
                {
                    // Ask to pay a fee
                    // Release from jail
                        //ReleaseFromJail(this.players[this.currentPlayerIndex]);
                }
            }  
        }

#if DEBUG
        Debug.Log($"Cube 1: {this.FirstCubeValue}; Cube 2: {this.SecondCubeValue}.");
#endif

        this.MovePlayer(this.players[this.currentPlayerIndex], this.FirstCubeValue + this.SecondCubeValue);
    }

    public void SwitchPlayer()
    {
        if (this.HasRolledDouble)
        {
            ++this.currentPlayerDoubles;

            if (this.currentPlayerDoubles >= MAX_DOUBLES_IN_ROW + 1)
                this.SendToJail(this.players[this.currentPlayerIndex]);
        }
        else
        {
            ++this.currentPlayerIndex;

            if (this.players[this.currentPlayerIndex].IsInJail)
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
        // this.MovePlayer(player, );
        // Update position
    }

    public void ReleaseFromJail(Player player)
    {
        player.TurnsInJail = 0;
        player.IsInJail = false;
    }

    //public void MovePlayerToken(MonopolyCell.MonopolyCellType type, Player player)
    //{
    //    int indexOfNextNodeType = -1;
    //    int indexOnBoard = route.IndexOf(player.CurrentPosition);
    //    int startSearchIndex = (indexOnBoard + 1) % route.Count;
    //    int nodeSearches = 0;

    //    while (indexOfNextNodeType != -1 && nodeSearches <= route.Count)
    //    {
    //        if (route[startSearchIndex].Type == type)
    //        {
    //            indexOfNextNodeType = startSearchIndex;
    //        }

    //        startSearchIndex = (startSearchIndex + 1) % route.Count;
    //        nodeSearches++;
    //    }

    //    if (indexOfNextNodeType != -1)
    //    {
    //        return;
    //    }

    //    StartCoroutine(MovePlayerInSteps(player, nodeSearches));
    //}

    public void MovePlayer(Player player, int steps)
    {
        StartCoroutine(MovePlayerInSteps(player, steps));

        IEnumerator MovePlayerInSteps(Player player, int steps)
        {
            //int stepsLeft = steps;
            //GameObject tokenToMove = player.Token;

            bool movedOverStart = false;
            int currentNodeIndex = MonopolyBoard.Instance.Nodes.IndexOf(player.CurrentPosition);

            //bool isMovingForward = steps > 0;

            if (steps > 0)
            {
                while (steps > 0)
                {
                    ++currentNodeIndex;

                    if (currentNodeIndex > MonopolyBoard.Instance.Nodes.Count - 1)
                    {
                        currentNodeIndex = 0;
                        movedOverStart = true;
                    }

                    Vector2 endPosition = MonopolyBoard.Instance.Nodes[currentNodeIndex].transform.position;

                    while (MoveToNextNode(this.players[this.currentPlayerIndex].Token, endPosition))
                        yield return null;

                    --steps;
                }
            }
            else
            {
                while (steps > 0)
                {
                    --currentNodeIndex;

                    if (currentNodeIndex < 0)
                    {
                        movedOverStart = true;
                        currentNodeIndex = MonopolyBoard.Instance.Nodes.Count - 1;
                    }

                    Vector2 endPosition = MonopolyBoard.Instance.Nodes[currentNodeIndex].transform.position;

                    while (MoveToNextNode(this.players[this.currentPlayerIndex].Token, endPosition))
                        yield return null;

                    ++steps;
                }
            }

            if (movedOverStart)
                this.SendBalance(this.players[this.currentPlayerIndex], CIRCLE_BONUS);

            player.CurrentPosition = MonopolyBoard.Instance.Nodes[currentNodeIndex];

            //GameManager.instance.RollDice();
        }

        bool MoveToNextNode(GameObject tokenToMove, Vector3 endPosition) 
            => endPosition != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPosition, PLAYER_MOVEMENT_SPEED * Time.deltaTime));
    }

    public void SendBalance(Player player, int balanceAmount)
    {
        player.Balance += balanceAmount;

        // Update ui
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
