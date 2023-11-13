//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public sealed class Player
{
    [SerializeField]
    public string Name;

    private int balance;
    private int turnsInJail;

    [SerializeField]
    public GameObject Token;

    [HideInInspector]
    public MonopolyCell CurrentPosition { get; private set; }

    public bool IsInJail { get; private set; }

    public int[] CountHousesAndHotels()
    {
        int houses = 0;
        int hotels = 0;

        foreach (var node in playerCells) 
        { 
            if (node.NumberOfHouses != 5)
            {
                houses += node.NumberOfHouses;
            }
            else
            {
                hotels += 1;
            }
        }

        int[] allBuildings = new int[] { houses, hotels };
        return allBuildings;
    }

    [SerializeField]
    private List<MonopolyCell> playerCells = new List<MonopolyCell>();

    // PLAYER INFO

    private PlayerInfo playerInfo;



    public Player(MonopolyCell cell, int balance, PlayerInfo playerInfo)
    {
        this.balance = balance;
        this.CurrentPosition = cell;
        this.playerInfo = playerInfo;

        this.playerInfo.SetPlayerName(this.Name);
        this.playerInfo.SetPlayerBalance(this.balance);
    }

    public int ReadMoney()
    {
        return this.balance;
    }

    public void Initialize(MonopolyCell cell, int balance, PlayerInfo playerInfo, GameObject token)
    {
        this.balance = balance;
        this.CurrentPosition = cell;
        this.playerInfo = playerInfo;
        this.Token = token;

        this.playerInfo.SetPlayerName(this.Name);
        this.playerInfo.SetPlayerBalance(this.balance);
    }

    public void SetNewCurrentNode(MonopolyCell newNode)
    {
        CurrentPosition = newNode;
        newNode.PlayerLandedOnNode(this);
    }

    public void CollectMoney(int amount)
    {
        balance += amount;
        playerInfo.SetPlayerBalance(balance);
    }

    internal bool CanAfford(int price)
    {
        return price <= balance;
    }

    public void BuyProperty(MonopolyCell cell)
    {
        this.balance -= cell.BaseRentPrice;
        cell.SetOWner(this);
        playerCells.Add(cell);

        //update ui
        this.playerInfo.SetPlayerBalance(balance);
    }

    public void PayRent(int rentAmount, Player owner)
    {
        if (balance < rentAmount)
        {

        }

        balance -= rentAmount;
        owner.CollectMoney(rentAmount);

        //update ui
    }

    internal void PayTax(int amount)
    {
        if (balance < amount)
        {

        }

        balance -= amount;

        //update ui
    }

    public void GoToJail(int indexOnBoard)
    {
        //const int JAIL_CELL_INDEX = 10;

        IsInJail = true;
        //this.Token.transform.position = MonopolyBoard.instance.route[JAIL_CELL_INDEX].transform.position;
       // CurrentPosition = MonopolyBoard.instance.route[JAIL_CELL_INDEX];

        MonopolyBoard.instance.MovePlayerToken(this, CalculateDistanceFromJail(indexOnBoard));
        GameManager.instance.ResetRolledDouble();
    }

    public void SetOutOfJail()
    {
        IsInJail = false;
        turnsInJail = 0;
    }

    int CalculateDistanceFromJail(int indexOnBoard)
    {
        int result = 0;
        const int JAIL_CELL_INDEX = 10;

        if (indexOnBoard > JAIL_CELL_INDEX)
        {
            result = -(indexOnBoard - JAIL_CELL_INDEX);
        }
        else
        {
            result = JAIL_CELL_INDEX - indexOnBoard;
        }

        return result;
    }

    public int NumberTurnsInJail => turnsInJail;

    public void IncreaseNumberOfTurnsInJail()
    {
        turnsInJail++;
    }

    public void CheckIfPlayerHasASet()
    {
        foreach (var node in playerCells)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyCell> nodeSet = list;

            if (nodeSet != null)
            {
                bool hasMordgadedNode = nodeSet.Any(node => node.IsMortgaged) ? true: false;

                if (!hasMordgadedNode)
                {
                    if (nodeSet[0].Type == MonopolyCell.MonopolyCellType.Property)
                    {

                    }
                }
            }
        }
    }

    public void BuildHouseOrHotelEvenly(List<MonopolyCell> nodesToBuildOn)
    {
        int minHouses = int.MinValue;
        int maxHouses = int.MinValue;

        foreach (var node in nodesToBuildOn)
        {
            int numOfHouses = node.NumberOfHouses;

            if (numOfHouses < minHouses)
            {
                minHouses = numOfHouses;
            }

            if (numOfHouses > maxHouses && numOfHouses < 5)
            {
                maxHouses = numOfHouses;
            }
        }

        foreach (var node in nodesToBuildOn)
        {
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses < 5 && CanAffordAHouse(node.HouseCost))
            {
                node.BuildHouseOrHotel();
                PayTax(node.HouseCost);
            }
        }
    }

    bool CanAffordAHouse(int price)
    {
        return balance >= price;
    }
}
