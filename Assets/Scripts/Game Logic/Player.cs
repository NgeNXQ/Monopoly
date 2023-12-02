using UnityEngine;
using System.Linq;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public sealed class Player : NetworkBehaviour
{
    #region In-editor Setup (Visuals)

    [Space]
    [Header("Visuals")]
    [Space]

    [SerializeField] private Color playerColor;

    [SerializeField] private string playerNickname;

    [SerializeField] private Image playerImageToken;

    [SerializeField] private Sprite playerSpriteToken;

    #endregion

    private bool hasBuilt;

    public int Balance { get; set; }

    public bool IsInJail { get; set; }

    public int TurnsInJail { get; set; }

    public bool HasBuilt { get => this.hasBuilt; }

    public bool HasCompletedTurn { get; private set; }

    public MonopolyNode CurrentNode { get; set; }

    public Color PlayerColor { get => this.playerColor; }

    public List<MonopolyNode> OwnedNodes { get; private set; }

    public int CurrentNodeIndex { get => MonopolyBoard.Instance[this.CurrentNode]; }

    private void Awake()
    {
        this.OwnedNodes = new List<MonopolyNode>();
        this.CurrentNode = MonopolyBoard.Instance.NodeStart;
    }

    private void OnEnable()
    {
        UIManager.Instance.OnButtonRollDiceClicked += this.RollDice;
        UIManager.Instance.OnButtonPayPanelPaymentClicked += this.Pay;
        UIManager.Instance.OnButtonConfirmPanelInfoClicked += this.ClosePanelInfo;
        UIManager.Instance.OnButtonAcceptPanelOfferClicked += this.AcceptPropertyOffer;
        UIManager.Instance.OnButtonDeclinePanelOfferClicked += this.DeclinePropertyOffer;
        UIManager.Instance.OnButtonConfirmPanelMessageClicked += this.ClosePanelMessage;
        UIManager.Instance.OnButtonUpgradePanelMonopolyNodeClicked += this.UpgradeNode;
        UIManager.Instance.OnButtonDowngradePanelMonopolyNodeClicked += this.DowngradeNode;
    }

    private void OnDisable()
    {
        UIManager.Instance.OnButtonRollDiceClicked -= this.RollDice;
        UIManager.Instance.OnButtonPayPanelPaymentClicked -= this.Pay;
        UIManager.Instance.OnButtonConfirmPanelInfoClicked -= this.ClosePanelInfo;
        UIManager.Instance.OnButtonAcceptPanelOfferClicked -= this.AcceptPropertyOffer;
        UIManager.Instance.OnButtonDeclinePanelOfferClicked -= this.DeclinePropertyOffer;
        UIManager.Instance.OnButtonConfirmPanelMessageClicked -= this.ClosePanelMessage;
        UIManager.Instance.OnButtonUpgradePanelMonopolyNodeClicked -= this.UpgradeNode;
        UIManager.Instance.OnButtonDowngradePanelMonopolyNodeClicked -= this.DowngradeNode;
    }

    public override void OnNetworkSpawn()
    {
        if (!this.IsOwner)
            return;

        base.OnNetworkSpawn();
        this.InitializePlayer();
        this.InitializePlayerClientRpc();
    }

    [ClientRpc]
    public void InitializePlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!this.IsOwner)
            this.InitializePlayer();
    }

    private void InitializePlayer()
    {
        this.OwnedNodes = new List<MonopolyNode>();
        this.CurrentNode = MonopolyBoard.Instance.NodeStart;

        this.playerImageToken.sprite = playerSpriteToken;
        this.Balance = GameManager.Instance.StartingBalance;
        this.transform.position = MonopolyBoard.Instance.NodeStart.transform.position;

        //UIManager.Instance.AddPlayer(this.playerNickname, this.playerColor);
    }

    #region Movement

    [ClientRpc]
    public void PerformTurnClientRpc(ClientRpcParams clientRpcParams)
    {
        this.HasCompletedTurn = false;
        this.StartCoroutine(PerformTurnCoroutine());

        IEnumerator PerformTurnCoroutine()
        {
            UIManager.Instance.SetControlState(UIManager.UIControl.ButtonRollDice, true);
            yield return new WaitUntil(() => this.HasCompletedTurn);
            GameManager.Instance.SyncSwitchPlayerServerRpc();
        }
    }

    private void RollDice()
    {
        if (this.OwnerClientId == GameManager.Instance.CurrentPlayer.OwnerClientId)
        {
            UIManager.Instance.SetControlState(UIManager.UIControl.ButtonRollDice, false);
            
            GameManager.Instance.RollDice();

            UIManager.Instance.ShowDice();
            UIManager.Instance.SyncShowDiceServerRpc();

            if (this.IsInJail)
            {
                if (GameManager.Instance.HasRolledDouble || ++this.TurnsInJail >= GameManager.Instance.MaxTurnsInJail)
                {
                    this.TurnsInJail = 0;
                    this.IsInJail = false;
                    this.Move(GameManager.Instance.TotalRollResult);
                }
                else
                {
                    this.HasCompletedTurn = true;
                }
            }
            else
            {
                this.Move(GameManager.Instance.TotalRollResult);
            }
        }
    }

    private void Move(int steps)
    {
        const float POSITION_THRESHOLD = 0.01f;

        Vector3 targetPosition;
        int currentNodeIndex = this.CurrentNodeIndex;

        this.StartCoroutine(MovePlayerSequence());

        IEnumerator MovePlayerSequence()
        {
            while (steps != 0)
            {
                if (steps < 0)
                {
                    ++steps;
                    currentNodeIndex = Mathf.Abs(--currentNodeIndex + MonopolyBoard.Instance.NumberOfNodes);
                    currentNodeIndex = currentNodeIndex % MonopolyBoard.Instance.NumberOfNodes;
                }
                else
                {
                    --steps;
                    currentNodeIndex = ++currentNodeIndex % MonopolyBoard.Instance.NumberOfNodes;
                }

                targetPosition = MonopolyBoard.Instance[currentNodeIndex].transform.position;

                yield return StartCoroutine(MovePlayerCoroutine(targetPosition));
            }

            this.HasCompletedTurn = true;
            this.CurrentNode = MonopolyBoard.Instance[currentNodeIndex];
            //GameManager.Instance.HandlePlayerLanding(this);
        }

        IEnumerator MovePlayerCoroutine(Vector3 targetPosition)
        {
            while (Vector3.Distance(this.transform.position, targetPosition) > POSITION_THRESHOLD)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, GameManager.Instance.PlayerMovementSpeed * Time.deltaTime);
                yield return null;
            }

            this.transform.position = targetPosition;
        }
    }

    #endregion

    #region Property

    public void HandlePropertyLanding()
    {
        if (this.CurrentNode.Owner == this)
        {
            this.HasCompletedTurn = true;
            return;
        }

        UIManager.Instance.SetControlState(UIManager.UIControl.PanelOffer, true, this.CurrentNode);
    }

    private void AcceptPropertyOffer()
    {
        if (this.Balance >= this.CurrentNode.Price)
        {
            this.CurrentNode.Owner = this;
            this.OwnedNodes.Add(this.CurrentNode);
            this.Balance -= this.CurrentNode.Price;

            this.HasCompletedTurn = true;
            UIManager.Instance.SetControlState(UIManager.UIControl.PanelOffer, false);
        }
        else
        {
            this.HandleInsufficientFunds();
        }
    }

    private void DeclinePropertyOffer()
    {
        this.HasCompletedTurn = true;
        UIManager.Instance.SetControlState(UIManager.UIControl.PanelOffer, false);
    }

    #endregion

    #region Utility

    public void HandleTaxLanding()
    {
        UIManager.Instance.SetControlState(UIManager.UIControl.PanelPayment, true, this.CurrentNode);
    }

    public void HandleFreeParkingLanding()
    {
        UIManager.Instance.SetControlState(UIManager.UIControl.PanelInfo, true, this.CurrentNode);
    }

    public void HandleJailLanding()
    {
        if (!this.IsInJail)
        {
            UIManager.Instance.SetControlState(UIManager.UIControl.PanelInfo, true, this.CurrentNode);
        }
        else
        {
            this.HasCompletedTurn = true;
        }
    }

    public void HandleStartLanding()
    {
        this.Balance += GameManager.Instance.ExactCircleBonus;
        UIManager.Instance.SetControlState(UIManager.UIControl.PanelInfo, true);
    }

    public void HandleSendJailLanding()
    {
        UIManager.Instance.SetControlState(UIManager.UIControl.PanelInfo, true);

        this.IsInJail = true;
        this.TurnsInJail = 0;
        this.Move(MonopolyBoard.Instance.GetDistance(this.CurrentNode, MonopolyBoard.Instance.NodeJail));
    }

    public void HandleChanceLanding()
    {
        bool hasInteracted = false;
        SO_ChanceNode chance = GameManager.Instance.GetChance();

        this.HasCompletedTurn = true;

        //this.UpdateHasCompletedTurn();
        //UIManager.Instance.SetControlVisibility(UIManager.UIControl.PanelInformation, this.CurrentNode);
        //UIManager.Instance.SetControlVisibility(UIManager.UIControl.PanelInformation, true);
        //UIManager.Instance.WaitPlayerInput(hasInteracted);

        switch (chance.Type)
        {
            case SO_ChanceNode.ChanceNodeType.Reward:
                {

                }
                break;
            case SO_ChanceNode.ChanceNodeType.Penalty:
                {

                }
                break;
            case SO_ChanceNode.ChanceNodeType.SkipTurn:
                {

                }
                break;
            case SO_ChanceNode.ChanceNodeType.SendJail:
                {

                }
                break;
            case SO_ChanceNode.ChanceNodeType.MoveBackwards:
                {

                }
                break;
            case SO_ChanceNode.ChanceNodeType.RandomMovement:
                {

                }
                break;

        }
    }

    private void Pay()
    {
        Debug.Log("Paying tax");

        this.HasCompletedTurn = true;

        UIManager.Instance.SetControlState(UIManager.UIControl.PanelPayment, false);

        //GameManager.Instance.SyncSwitchPlayerServerRpc();

        //this.UpdateHasCompletedTurn();

        //switch (this.CurrentNode.Type)
        //{
        //    case MonopolyNode.MonopolyNodeType.Tax:
        //        {

        //        }
        //        break;
        //    case MonopolyNode.MonopolyNodeType.Chance:
        //        {

        //        }
        //        break;
        //    case MonopolyNode.MonopolyNodeType.Property:
        //    case MonopolyNode.MonopolyNodeType.Gambling:
        //    case MonopolyNode.MonopolyNodeType.Transport:
        //        {

        //        }
        //        break;
        //}

        //if (this.Balance >= this.CurrentNode.TaxAmount)
        //{
        //    UIManager.Instance.SetControlVisibility(UIManager.UIControl.PanelPayment, false);
        //    this.hasActioned = true;

        //    this.Balance -= this.CurrentNode.TaxAmount;
        //}
        //else
        //{
        //    this.HandleInsufficientFunds();
        //    UIManager.Instance.WaitPlayerInput(this.hasHandledInsufficientFunds);
        //}


        //Debug.Log("Paying rent");

        //if (this.Balance >= this.CurrentNode.Price)
        //{
        //    UIManager.Instance.SetControlVisibility(UIManager.UIControl.PanelPayment, false);
        //    this.hasActioned = true;

        //    this.Balance -= this.CurrentNode.Price;
        //}
        //else
        //{
        //    this.HandleInsufficientFunds();
        //    UIManager.Instance.WaitPlayerInput(this.hasHandledInsufficientFunds);
        //}
    }

    public void UpgradeNode()
    {
        if (this.hasBuilt) 
        {
            //UIManager.Instance.SetControlVisibility(UIManager.UIControl.PanelMessage, true);
            //UIManager.Instance.WaitPlayerInput(this.hasRead);
        }

        this.hasBuilt = true;
        this.CurrentNode.Upgrade();
        //UIManager.Instance.SetControlVisibility(UIManager.UIControl.PanelMonopolyNode, false);
    }

    public void DowngradeNode() => this.CurrentNode.Downgrade();

    #endregion

    private void ClosePanelInfo()
    {
        this.HasCompletedTurn = true;
        //UIManager.Instance.SetControlVisibility(UIManager.UIControl.PanelInfo, false);
    }

    private void ClosePanelMessage()
    {
        this.HasCompletedTurn = true;
        //UIManager.Instance.SetControlVisibility(UIManager.UIControl.PanelMessage, false);
    }

    private void HandleInsufficientFunds()
    {
        this.HasCompletedTurn = true;
        //UIManager.Instance.WaitPlayerInput(this.hasHandledInsufficientFunds);
    }

    public bool HasFullMonopoly(MonopolyNode monopolyNode)
    {
        MonopolySet monopolySet = MonopolyBoard.Instance.GetMonopolySetOfNode(monopolyNode);
        return monopolySet?.NodesInSet.Intersect(this.OwnedNodes).Count() == monopolySet.NodesInSet.Count;
    }


























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
