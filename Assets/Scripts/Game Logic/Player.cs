using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// TODO: Implement upgrading
// TODO: Implement Chance node full logic
// TODO: Implement insufficient funds

// TODO: Implement trading
// TODO: Implement lobby
// TODO: Implement surrendering
// TODO: Write all values and messages

public sealed class Player : NetworkBehaviour
{
    #region Visuals

    [Space]
    [Header("Visuals")]
    [Space]

    [SerializeField] private Color playerColor;

    [SerializeField] private string playerNickname;

    [SerializeField] private Image playerImageToken;

    [SerializeField] private Sprite playerSpriteToken;

    #endregion

    #region Values

    private bool isInJail;

    private int turnsInJail;

    private bool isSkipTurn;

    public bool IsAbleToBuild { get; private set; }

    public bool HasCompletedTurn { get; private set; }

    public int Balance { get; set; }

    public bool HasBuilt { get; private set; }

    public MonopolyNode CurrentNode { get; set; }

    public MonopolyNode SelectedNode { get; set; }

    public Color PlayerColor { get => this.playerColor; }

    public List<MonopolyNode> OwnedNodes { get; private set; }

    public ChanceNodeSO CurrentChanceNode { get; private set; }

    #endregion

    #region Setup

    private void Awake()
    {
        this.OwnedNodes = new List<MonopolyNode>();
        this.CurrentNode = MonopolyBoard.Instance.NodeStart;
    }

    private void OnEnable()
    {
        UIManagerMonopoly.Instance.OnButtonRollDiceClicked += this.RollDiceCallback;
        UIManagerMonopoly.Instance.PanelPayment.OnButtonConfirmClicked += this.PayCallback;

        UIManagerMonopoly.Instance.PanelOffer.OnButtonAcceptClicked += this.AcceptPropertyOfferCallback;
        UIManagerMonopoly.Instance.PanelOffer.OnButtonDeclineClicked += this.DeclinePropertyOfferCallback;

        UIManagerMonopoly.Instance.PanelMonopolyNode.OnButtonUpgradeClicked += this.UpgradeNodeCallback;
        UIManagerMonopoly.Instance.PanelMonopolyNode.OnButtonDowngradeClicked += this.DowngradeNodeCallback;

        UIManagerMonopoly.Instance.PanelInfo.OnButtonConfirmClicked += this.ClosePanelInfoCallback;
        UIManagerMonopoly.Instance.PanelMessageBox.OnButtonConfirmClicked += this.ClosePanelMessageBoxCallback;
    }

    private void OnDisable()
    {
        UIManagerMonopoly.Instance.OnButtonRollDiceClicked -= this.RollDiceCallback;
        UIManagerMonopoly.Instance.PanelPayment.OnButtonConfirmClicked -= this.PayCallback;

        UIManagerMonopoly.Instance.PanelOffer.OnButtonAcceptClicked -= this.AcceptPropertyOfferCallback;
        UIManagerMonopoly.Instance.PanelOffer.OnButtonDeclineClicked -= this.DeclinePropertyOfferCallback;

        UIManagerMonopoly.Instance.PanelMonopolyNode.OnButtonUpgradeClicked -= this.UpgradeNodeCallback;
        UIManagerMonopoly.Instance.PanelMonopolyNode.OnButtonDowngradeClicked -= this.DowngradeNodeCallback;

        UIManagerMonopoly.Instance.PanelInfo.OnButtonConfirmClicked -= this.ClosePanelInfoCallback;
        UIManagerMonopoly.Instance.PanelMessageBox.OnButtonConfirmClicked -= this.ClosePanelMessageBoxCallback;
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

    #endregion

    #region Monopoly

    public bool HasFullMonopoly(MonopolyNode monopolyNode, out MonopolySet monopolySet)
    {
        monopolySet = MonopolyBoard.Instance.GetMonopolySet(monopolyNode);
        return monopolySet?.NodesInSet.Intersect(this.OwnedNodes).Count() == monopolySet.NodesInSet.Count;
    }

    public bool HasPartialMonopoly(MonopolyNode monopolyNode, out MonopolySet monopolySet)
    {
        monopolySet = MonopolyBoard.Instance.GetMonopolySet(monopolyNode);
        return monopolySet?.NodesInSet.Intersect(this.OwnedNodes).Count() > 1;
    }

    #endregion

    #region Movement

    [ClientRpc]
    public void PerformTurnClientRpc(ClientRpcParams clientRpcParams)
    {
        this.HasBuilt = false;
        this.IsAbleToBuild = true;
        this.HasCompletedTurn = false;

        this.StartCoroutine(PerformTurnCoroutine());

        if (this.isSkipTurn)
        {
            this.isSkipTurn = false;
            this.HasCompletedTurn = true;
            return;
        }

        UIManagerMonopoly.Instance.ShowButtonRollDice();

        IEnumerator PerformTurnCoroutine()
        {
            yield return new WaitUntil(() => this.HasCompletedTurn);
            GameManager.Instance.SwitchPlayerServerRpc();
        }
    }

    private void Move(int steps)
    {
        this.IsAbleToBuild = false;

        const float POSITION_THRESHOLD = 0.01f;

        Vector3 targetPosition;
        int currentNodeIndex = MonopolyBoard.Instance[this.CurrentNode];

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

            this.CurrentNode = MonopolyBoard.Instance[currentNodeIndex];
            this.HandleLanding();
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

    private void HandleLanding()
    {
        switch (this.CurrentNode.NodeType)
        {
            case MonopolyNode.Type.Tax:
                this.HandleTaxLanding();
                break;
            case MonopolyNode.Type.Jail:
                this.HandleJailLanding();
                break;
            case MonopolyNode.Type.Start:
                this.HandleStartLanding();
                break;
            case MonopolyNode.Type.Chance:
                this.HandleChanceLanding();
                break;
            case MonopolyNode.Type.SendJail:
                this.HandleSendJailLanding();
                break;
            case MonopolyNode.Type.Property:
                this.HandlePropertyLanding();
                break;
            case MonopolyNode.Type.Gambling:
                this.HandlePropertyLanding();
                break;
            case MonopolyNode.Type.Transport:
                this.HandlePropertyLanding();
                break;
            case MonopolyNode.Type.FreeParking:
                this.HandleFreeParkingLanding();
                break;
        }
    }

    #endregion

    #region Chance

    public void HandleTaxLanding()
    {
        this.CurrentChanceNode = MonopolyBoard.Instance.GetTaxNode();

        UIManagerMonopoly.Instance.PanelPayment.PictureSprite = this.CurrentNode.NodeSprite;
        UIManagerMonopoly.Instance.PanelPayment.DescriptionText = this.CurrentChanceNode.Description;
        UIManagerMonopoly.Instance.PanelPayment.Show();
    }

    public void HandleChanceLanding()
    {
        this.CurrentChanceNode = MonopolyBoard.Instance.GetChanceNode();

        if (this.CurrentChanceNode.ChanceType == ChanceNodeSO.Type.Penalty)
        {
            UIManagerMonopoly.Instance.PanelPayment.PictureSprite = this.CurrentNode.NodeSprite;
            UIManagerMonopoly.Instance.PanelPayment.DescriptionText = this.CurrentChanceNode.Description;
            UIManagerMonopoly.Instance.PanelPayment.Show();
        }
        else
        {
            UIManagerMonopoly.Instance.PanelInfo.PictureSprite = this.CurrentNode.NodeSprite;
            UIManagerMonopoly.Instance.PanelInfo.DescriptionText = this.CurrentChanceNode.Description;
            UIManagerMonopoly.Instance.PanelInfo.Show();
        }
    }

    #endregion

    #region Utility

    public void HandleJailLanding()
    {
        this.HasCompletedTurn = true;
    }

    public void HandleStartLanding()
    {
        this.Balance += GameManager.Instance.ExactCircleBonus;
        this.HasCompletedTurn = true;
    }

    public void HandleSendJailLanding()
    {
        this.isInJail = true;
        this.turnsInJail = 0;
        this.Move(MonopolyBoard.Instance.GetDistance(this.CurrentNode, MonopolyBoard.Instance.NodeJail));
    }

    public void HandleFreeParkingLanding()
    {
        this.HasCompletedTurn = true;
    }

    #endregion

    #region Property

    private void HandlePropertyLanding()
    {
        if (this.CurrentNode.Owner == null)
        {
            UIManagerMonopoly.Instance.PanelOffer.PictureSprite = this.CurrentNode.NodeSprite;
            UIManagerMonopoly.Instance.PanelOffer.MonopolyTypeColor = this.CurrentNode.AffiliatedMonopoly.ColorOfSet;
            UIManagerMonopoly.Instance.PanelOffer.Show();
        }
        else if (this.CurrentNode.Owner == this)
        {
            this.HasCompletedTurn = true;
        }
        else
        {
            UIManagerMonopoly.Instance.PanelPayment.PictureSprite = this.CurrentNode.NodeSprite;
            UIManagerMonopoly.Instance.PanelPayment.DescriptionText = this.CurrentNode.PriceRent.ToString();
            UIManagerMonopoly.Instance.PanelOffer.Show();
        }
    }

    private void AcceptPropertyOfferCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        if (this.Balance >= this.CurrentNode.PriceUpgrade)
        {
            UIManagerMonopoly.Instance.PanelOffer.Hide();

            this.CurrentNode.UpdateOwner(this);
            this.OwnedNodes.Add(this.CurrentNode);
            this.Balance -= this.CurrentNode.PriceUpgrade;
            this.HasCompletedTurn = true;
        }
        else
        {
            UIManagerMonopoly.Instance.PanelMessageBox.MessageText = UIManagerMonopoly.Instance.MessageInsufficientFunds;
            UIManagerMonopoly.Instance.PanelMessageBox.Show();
        }
    }

    private void DeclinePropertyOfferCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        UIManagerMonopoly.Instance.PanelOffer.Hide();
        this.HasCompletedTurn = true;
    }

    public void UpgradeNodeCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        if (!this.HasFullMonopoly(this.SelectedNode, out _) && !this.CurrentNode.IsMortgaged)
        {
            UIManagerMonopoly.Instance.PanelMessageBox.MessageText = UIManagerMonopoly.Instance.MessageCompleteMonopolyRequired;
            UIManagerMonopoly.Instance.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Exclamation;
            UIManagerMonopoly.Instance.PanelMessageBox.Show();
            return;
        }

        if (this.HasBuilt)
        {
            UIManagerMonopoly.Instance.PanelMessageBox.MessageText = UIManagerMonopoly.Instance.MessageAlreadyBuilt;
            UIManagerMonopoly.Instance.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Exclamation;
            UIManagerMonopoly.Instance.PanelMessageBox.Show();
            return;
        }

        if (!this.SelectedNode.IsUpgradable)
        {
            if (this.SelectedNode.Level == MonopolyNode.PROPERTY_MAX_LEVEL)
            {
                UIManagerMonopoly.Instance.PanelMessageBox.MessageText = UIManagerMonopoly.Instance.MessageCannotUpgradeMaxLevel;
                UIManagerMonopoly.Instance.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Exclamation;
                UIManagerMonopoly.Instance.PanelMessageBox.Show();
                return;
            }
            else
            {
                UIManagerMonopoly.Instance.PanelMessageBox.MessageText = UIManagerMonopoly.Instance.MessageOnlyEvenBuildingAllowed;
                UIManagerMonopoly.Instance.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Exclamation;
                UIManagerMonopoly.Instance.PanelMessageBox.Show();
                return;
            }
        }

        this.HasBuilt = true;
        this.SelectedNode.Upgrade();
        UIManagerMonopoly.Instance.PanelMonopolyNode.Hide();
    }

    public void DowngradeNodeCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        if (!this.SelectedNode.IsDowngradable)
        {
            if (this.SelectedNode.Level == MonopolyNode.PROPERTY_MIN_LEVEL)
            {
                UIManagerMonopoly.Instance.PanelMessageBox.MessageText = UIManagerMonopoly.Instance.MessageCannotDowngradeMinLevel;
                UIManagerMonopoly.Instance.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Exclamation;
                UIManagerMonopoly.Instance.PanelMessageBox.Show();
                return;
            }
            else
            {
                UIManagerMonopoly.Instance.PanelMessageBox.MessageText = UIManagerMonopoly.Instance.MessageOnlyEvenBuildingAllowed;
                UIManagerMonopoly.Instance.PanelMessageBox.MessageType = PanelMessageBoxUI.Type.Exclamation;
                UIManagerMonopoly.Instance.PanelMessageBox.Show();
                return;
            }
        }

        this.SelectedNode.Downgrade();
        UIManagerMonopoly.Instance.PanelMonopolyNode.Hide();
    }

    #endregion

    #region UI Callbacks

    private void PayCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        if (this.CurrentNode.NodeType == MonopolyNode.Type.Chance || this.CurrentNode.NodeType == MonopolyNode.Type.Tax)
        {
            if (this.Balance >= this.CurrentChanceNode.Penalty)
            {
                this.Balance -= this.CurrentChanceNode.Penalty;
                UIManagerMonopoly.Instance.PanelPayment.Hide();
                this.HasCompletedTurn = true;
            }
            else
            {
                UIManagerMonopoly.Instance.PanelMessageBox.MessageText = UIManagerMonopoly.Instance.MessageInsufficientFunds;
                UIManagerMonopoly.Instance.PanelMessageBox.Show();
            }
        }
        else
        {
            if (this.Balance >= this.CurrentNode.PriceRent)
            {
                this.Balance -= this.CurrentNode.PriceRent;
                UIManagerMonopoly.Instance.PanelPayment.Hide();
                this.HasCompletedTurn = true;
            }
            else
            {
                UIManagerMonopoly.Instance.PanelMessageBox.MessageText = UIManagerMonopoly.Instance.MessageInsufficientFunds;
                UIManagerMonopoly.Instance.PanelMessageBox.Show();
            }
        }
    }

    private void RollDiceCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        UIManagerMonopoly.Instance.HideButtonRollDice();
        GameManager.Instance.RollDice();
        UIManagerMonopoly.Instance.ShowDiceAnimation();

        if (this.isInJail)
        {
            if (GameManager.Instance.HasRolledDouble || ++this.turnsInJail >= GameManager.Instance.MaxTurnsInJail)
            {
                this.turnsInJail = 0;
                this.isInJail = false;
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

    private void ClosePanelInfoCallback()
    {
        UIManagerMonopoly.Instance.PanelInfo.Hide();

        switch (this.CurrentChanceNode.ChanceType)
        {
            case ChanceNodeSO.Type.Reward:
                {
                    this.Balance += this.CurrentChanceNode.Reward;
                    this.HasCompletedTurn = true;
                }
                break;
            case ChanceNodeSO.Type.SkipTurn:
                {
                    this.isSkipTurn = true;
                    this.HasCompletedTurn = true;
                }
                break;
            case ChanceNodeSO.Type.SendJail:
                this.HandleSendJailLanding();
                break;
            case ChanceNodeSO.Type.MoveForward:
                {
                    GameManager.Instance.RollDice();
                    UIManagerMonopoly.Instance.ShowDiceAnimation();
                    this.Move(GameManager.Instance.TotalRollResult);
                }
                break;
            case ChanceNodeSO.Type.MoveBackwards:
                {
                    GameManager.Instance.RollDice();
                    UIManagerMonopoly.Instance.ShowDiceAnimation();
                    this.Move(-GameManager.Instance.TotalRollResult);
                }
                break;
        }
    }

    private void ClosePanelMessageBoxCallback()
    {
        UIManagerMonopoly.Instance.PanelMessageBox.Hide();
    }

    #endregion

    //private void GoToJail()
    //{

    //}

    //private void HandlePayment()
    //{

    //}

    //private void HandleInsuffcientFunds()
    //{

    //}
}
