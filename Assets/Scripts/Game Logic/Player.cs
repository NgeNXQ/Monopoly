using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

public sealed class Player : NetworkBehaviour
{
    [SerializeField] private GameObject panelPlayersInfo;

    [SerializeField] private SO_PlayerVisuals playerVisuals;

    private List<MonopolyNode> nodes;

    public int Balance { get; set; }

    public bool IsInJail { get; set; }

    public bool IsSkipTurn { get; set; }

    public int TurnsInJail { get; set; }

    public MonopolyNode CurrentNode { get; set; }

    public int CurrentNodeIndex { get => MonopolyBoard.Instance.Nodes.IndexOf(this.CurrentNode); }

    public Player()
    {
        this.nodes = new List<MonopolyNode>();
        this.CurrentNode = MonopolyBoard.Instance.NodeStart;

        //NetworkObjectReference
    }

    // Fix double spawn bug
    public override void OnNetworkSpawn()
    {
        if (!this.IsOwner)
            GameObject.Destroy(this);

        this.InitializePlayer();
        this.InitializePlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitializePlayerServerRpc()
    {
        if (!this.IsOwner)
            this.InitializePlayer();
    }

    private void InitializePlayer()
    {
        this.Balance = GameManager.Instance.StartingBalance;
        this.playerVisuals.PlayerPanel.UpdateBalance(this);

        this.nodes = new List<MonopolyNode>();
        this.CurrentNode = MonopolyBoard.Instance.NodeStart;

        GameObject.Instantiate(this.playerVisuals.PlayerToken, this.transform);
        this.transform.position = MonopolyBoard.Instance.NodeStart.transform.position;

        // Add panel
        // GameObject.Instantiate(this.playerVisuals.PlayerPanel.gameObject, this.panelPlayersInfo.transform);

        this.playerVisuals.PlayerPanel.SetUpPlayerInfo(new FixedString32Bytes(this.playerVisuals.PlayerNickname), this.playerVisuals.PlayerColor);
    }


    //public void Pay(int amount)
    //{
    //    if (this.Balance >= amount)
    //    {
    //        this.Balance -= amount;
    //    }
    //    else
    //    {
    //        // Handle insufficient balance
    //    }
    //}

    //public void BuyProperty(MonopolyNode node)
    //{
    //    if (this.Balance >= node.priceInitial)
    //    {
    //        this.nodes.Add(node);
    //        node.Owner = this;

    //        //Update ui
    //    }
    //    else
    //    {
    //        // handle this case
    //    }
    //}

    //public void PledgeProperty(MonopolyNode node)
    //{

    //}

    //public void UpgradeProperty(MonopolyNode node)
    //{

    //}

    //public void HandlePropertyLanding()
    //{
    //    switch (this.CurrentNode.Owner)
    //    {
    //        case null:
    //            UIManager.Instance.ShowPanelOffer(null, "");
    //            break;
    //        case Player nodeOwner when nodeOwner != this:
    //            UIManager.Instance.ShowPanelFee(null, "");
    //            break;
    //    }
    //}






































    //public delegate void ShowInputPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    //public static ShowInputPanel OnShowInputPanel;

    //public int[] CountHousesAndHotels()
    //{
    //    int houses = 0;
    //    int hotels = 0;

    //    foreach (var node in playerCells)
    //    {
    //        if (node.NumberOfHouses != 5)
    //        {
    //            houses += node.NumberOfHouses;
    //        }
    //        else
    //        {
    //            hotels += 1;
    //        }
    //    }

    //    int[] allBuildings = new int[] { houses, hotels };
    //    return allBuildings;
    //}

    //[SerializeField]
    //private List<MonopolyCell> playerCells = new List<MonopolyCell>();

    //PLAYER INFO

    //private PlayerInfo playerInfo;



    //public Player(MonopolyCell cell, int balance, PlayerInfo playerInfo)
    //{
    //    this.balance = balance;
    //    this.CurrentPosition = cell;
    //    this.playerInfo = playerInfo;

    //    this.playerInfo.SetPlayerName(this.Name);
    //    this.playerInfo.SetPlayerBalance(this.balance);
    //}

    //public int ReadMoney()
    //{
    //    return this.balance;
    //}

    //public void Initialize(MonopolyCell cell, int balance, PlayerInfo playerInfo, GameObject token)
    //{
    //    this.balance = balance;
    //    this.CurrentPosition = cell;
    //    this.playerInfo = playerInfo;
    //    this.Token = token;

    //    this.playerInfo.SetPlayerName(this.Name);
    //    this.playerInfo.SetPlayerBalance(this.balance);
    //}

    //public void SetNewCurrentNode(MonopolyCell newNode)
    //{
    //    CurrentPosition = newNode;
    //    newNode.PlayerLandedOnNode(this);

    //}

    //public void CollectMoney(int amount)
    //{
    //    balance += amount;
    //    playerInfo.SetPlayerBalance(balance);
    //}

    //internal bool CanAfford(int price)
    //{
    //    return price <= balance;
    //}

    //public void BuyProperty(MonopolyCell cell)
    //{
    //    this.balance -= cell.BaseRentPrice;
    //    cell.SetOWner(this);
    //    playerCells.Add(cell);

    //    update ui
    //    this.playerInfo.SetPlayerBalance(balance);


    //}

    //public void PayRent(int rentAmount, Player owner)
    //{
    //    if (balance < rentAmount)
    //    {
    //        if (balance < rentAmount)
    //        {
    //            HandleInsufficientFunds(rentAmount);
    //        }

    //        OnShowInputPanel.Invoke(true, false, false);
    //    }

    //    balance -= rentAmount;
    //    owner.CollectMoney(rentAmount);

    //    update ui
    //}

    //internal void PayTax(int amount)
    //{
    //    if (balance < amount)
    //    {
    //        HandleInsufficientFunds(amount);
    //    }

    //    OnShowInputPanel.Invoke(true, false, false);

    //    balance -= amount;

    //    update ui
    //}

    //public void GoToJail(int indexOnBoard)
    //{
    //    const int JAIL_CELL_INDEX = 10;

    //    IsInJail = true;
    //    this.Token.transform.position = MonopolyBoard.instance.route[JAIL_CELL_INDEX].transform.position;
    //    CurrentPosition = MonopolyBoard.instance.route[JAIL_CELL_INDEX];

    //    MonopolyBoard.instance.MovePlayerToken(this, CalculateDistanceFromJail(indexOnBoard));
    //    GameManager.instance.ResetRolledDouble();
    //}

    //public void SetOutOfJail()
    //{
    //    IsInJail = false;
    //    turnsInJail = 0;
    //}

    //int CalculateDistanceFromJail(int indexOnBoard)
    //{
    //    int result = 0;
    //    const int JAIL_CELL_INDEX = 10;

    //    if (indexOnBoard > JAIL_CELL_INDEX)
    //    {
    //        result = -(indexOnBoard - JAIL_CELL_INDEX);
    //    }
    //    else
    //    {
    //        result = JAIL_CELL_INDEX - indexOnBoard;
    //    }

    //    return result;
    //}

    //public int NumberTurnsInJail => turnsInJail;

    //public void IncreaseNumberOfTurnsInJail()
    //{
    //    turnsInJail++;
    //}

    //public void CheckIfPlayerHasASet()
    //{
    //    List<MonopolyCell> processedSet = null;

    //    foreach (var node in playerCells)
    //    {
    //        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);

    //        if (!allSame)
    //        {
    //            continue;
    //        }

    //        List<MonopolyCell> nodeSet = list;

    //        if (nodeSet != null && nodeSet != processedSet)
    //        {
    //            bool hasMordgadedNode = nodeSet.Any(node => node.IsMortgaged) ? true : false;

    //            if (!hasMordgadedNode)
    //            {
    //                if (nodeSet[0].Type == MonopolyCell.MonopolyCellType.Property)
    //                {
    //                    BuildHouseOrHotelEvenly(nodeSet);
    //                    processedSet = nodeSet;
    //                }
    //            }
    //        }
    //    }
    //}

    //public void BuildHouseOrHotelEvenly(List<MonopolyCell> nodesToBuildOn)
    //{
    //    int minHouses = int.MinValue;
    //    int maxHouses = int.MinValue;

    //    foreach (var node in nodesToBuildOn)
    //    {
    //        int numOfHouses = node.NumberOfHouses;

    //        if (numOfHouses < minHouses)
    //        {
    //            minHouses = numOfHouses;
    //        }

    //        if (numOfHouses > maxHouses && numOfHouses < 5)
    //        {
    //            maxHouses = numOfHouses;
    //        }
    //    }

    //    foreach (var node in nodesToBuildOn)
    //    {
    //        if (node.NumberOfHouses == minHouses && node.NumberOfHouses < 5 && CanAffordAHouse(node.HouseCost))
    //        {
    //            node.BuildHouseOrHotel();
    //            PayTax(node.HouseCost);
    //            break;
    //        }
    //    }
    //}

    //bool CanAffordAHouse(int price)
    //{
    //    return balance >= price;
    //}

    //void HandleInsufficientFunds(int amounToPay)
    //{
    //    int housesToSell = 0;
    //    int allHouses = 0;
    //    int propertiesToMortgage = 0;
    //    int allPropertiesToMortgage = 0;

    //    foreach (var node in playerCells)
    //    {
    //        allHouses += node.NumberOfHouses;
    //    }

    //    while (balance < amounToPay && allHouses > 0)
    //    {
    //        foreach (var node in playerCells)
    //        {
    //            housesToSell = node.NumberOfHouses;

    //            if (housesToSell > 0)
    //            {
    //                CollectMoney(node.SellHouseOrHotel());
    //                allHouses--;

    //                if (balance >= amounToPay)
    //                {
    //                    return;
    //                }
    //            }
    //        }
    //    }

    //    foreach (var node in playerCells)
    //    {
    //        allPropertiesToMortgage += (node.IsMortgaged) ? 0 : 1;
    //    }

    //    while (balance < amounToPay && allPropertiesToMortgage > 0)
    //    {
    //        foreach (var node in playerCells)
    //        {
    //            propertiesToMortgage = (node.IsMortgaged) ? 0 : 1;

    //            if (propertiesToMortgage > 0)
    //            {
    //                this.CollectMoney(node.MortgageCell());
    //                allPropertiesToMortgage--;

    //                if (balance >= amounToPay)
    //                {
    //                    return;
    //                }
    //            }
    //        }
    //    }

    //    Bankrupt();
    //}

    //public void Bankrupt()
    //{
    //    update visual

    //    for (int i = playerCells.Count; i >= 0; i--)
    //    {
    //        playerCells[i].ResetNode();
    //    }

    //    GameManager.instance.RemovePlayer(this);
    //}

    //public void RemoveProperty(MonopolyCell node)
    //{
    //    playerCells.Remove(node);
    //}
}
