using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [SerializeField]
    private MonopolyBoard monopolyBoard;

    [SerializeField]
    public List<Player> players = new List<Player>();

    [SerializeField]
    private List<GameObject> tokens = new List<GameObject>();

    [SerializeField]
    private int currentPlayer;

    [SerializeField]
    private int maxTurnsInJail = 3;

    [SerializeField]
    private int startBalance = 15000;

    [SerializeField]
    public int circleBonus = 2000;

    [SerializeField]
    private GameObject playerInfoPrefab;

    internal bool RolledADounle { get; private set; }

    [SerializeField]
    private Transform playersPanel;

    public int taxPool = 0;

    [SerializeField]
    public float SecondsBetweenTurns = 2.0f;

    public static GameManager instance;

    int[] rolledDice;
    bool isDoubleRolled;
    int doubleRollCount;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Add players to the game

        Initialize();
        RollDice();
    }

    //private void Update()
    //{
    //    Invoke("RollDice", 1f);
    //    SwitchPlayer();
    //}

    public void ResetRolledDouble()
    {
        RolledADounle = false;
    }

    private void Initialize()
    {
        for (int i = 0; i < players.Count; ++i)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playersPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

            int randomIndex = Random.Range(0, tokens.Count);

            GameObject playerToken = Instantiate(tokens[randomIndex], monopolyBoard.route[0].transform.position, Quaternion.identity);

            players[i].Initialize(monopolyBoard.route[0], this.startBalance, info, playerToken);
        }
    }

    public void RollDice()
    {
        bool allowedToMove = true;

        rolledDice = new int[2];

        rolledDice[0] = Random.Range(1, 7);
        rolledDice[1] = Random.Range(1, 7);

        isDoubleRolled = rolledDice[0] == rolledDice[1];

        if (players[currentPlayer].IsInJail)
        {
            players[currentPlayer].IncreaseNumberOfTurnsInJail();

            if (isDoubleRolled)
            {
                players[currentPlayer].SetOutOfJail();
                doubleRollCount++;
            }
            else if (players[currentPlayer].NumberTurnsInJail >= maxTurnsInJail)
            {
                players[currentPlayer].SetOutOfJail();
            }
            else
            {
                allowedToMove = false;
            }
        }
        else
        {
            if (!isDoubleRolled)
            {
                doubleRollCount = 0;
            }
            else
            {
                doubleRollCount++;

                if (doubleRollCount >= 3)
                {
                    int indexOnBoard = MonopolyBoard.instance.route.IndexOf(players[currentPlayer].CurrentPosition);
                    players[currentPlayer].GoToJail(indexOnBoard);
                    RolledADounle = false;
                    return;
                }
            }
        }

        if (allowedToMove)
        {
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            StartCoroutine(DelayBetweenSwitchPlayer());
        }
    }

    IEnumerator DelayBeforeMove(int rolledDice)
    {
        yield return new WaitForSeconds(SecondsBetweenTurns);
        monopolyBoard.MovePlayerToken(players[currentPlayer], rolledDice);
    }

    IEnumerator DelayBetweenSwitchPlayer()
    {
        yield return new WaitForSeconds(SecondsBetweenTurns);
        SwitchPlayer();
    }

    public void SwitchPlayer()
    {
        currentPlayer++;

        doubleRollCount = 0;

        if (currentPlayer >= players.Count)
        {
            currentPlayer = 0;
        }
    }

    public int[] LastRolledDice()
    {
        return rolledDice;
    }

    public void AddTaxToPool(int amount)
    {
        taxPool += amount;
    }

    public int GetTaxPool()
    {
        int currentTaxCollected = taxPool;
        taxPool = 0;
        return currentTaxCollected;
    }
}
