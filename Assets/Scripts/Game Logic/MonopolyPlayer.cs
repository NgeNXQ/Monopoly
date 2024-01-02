using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// TODO: Implement surrendering
// TODO: Implement upgrading
// TODO: Implement trading

public sealed class MonopolyPlayer : NetworkBehaviour
{
    #region Setup

    #region Visuals

    [Header("Visuals")]

    [Space]
    [SerializeField] private Image playerImageToken;

    #endregion

    #endregion

    private bool isInJail;

    private int turnsInJail;

    private bool isSkipTurn;

    public string Nickname { get; private set; }

    public bool IsAbleToBuild { get; private set; }

    public bool HasCompletedTurn { get; private set; }

    public int Balance { get; private set; }

    public bool HasBuilt { get; private set; }

    public MonopolyNode SelectedNode { get; set; }

    public MonopolyNode CurrentNode { get; private set; }

    public Color PlayerColor { get; private set; }

    public List<MonopolyNode> OwnedNodes { get; private set; }

    public ChanceNodeSO CurrentChanceNode { get; private set; }

    private void OnEnable()
    {
        if (this.OwnerClientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        UIManagerMonopolyGame.Instance.ButtonRollDiceClicked += this.HandleButtonRollDiceClicked;
    }

    private void OnDisable()
    {
        if (this.OwnerClientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        UIManagerMonopolyGame.Instance.ButtonRollDiceClicked -= this.HandleButtonRollDiceClicked;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        this.OwnedNodes = new List<MonopolyNode>();

        this.Balance = GameManager.Instance.StartingBalance;
        this.CurrentNode = MonopolyBoard.Instance.NodeStart;
        this.transform.position = MonopolyBoard.Instance.NodeStart.transform.position;
        this.PlayerColor = GameManager.Instance.MonopolyPlayersVisuals[GameManager.Instance.CurrentPlayerIndex].ColorPlayerToken;
        this.playerImageToken.sprite = GameManager.Instance.MonopolyPlayersVisuals[GameManager.Instance.CurrentPlayerIndex].SpritePlayerToken;
        this.Nickname = LobbyManager.Instance.LocalLobby.Players[GameManager.Instance.CurrentPlayerIndex].Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
    }

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

        UIManagerMonopolyGame.Instance.ShowButtonRollDice();

        IEnumerator PerformTurnCoroutine()
        {
            yield return new WaitUntil(() => this.HasCompletedTurn);

            GameManager.Instance.SwitchPlayerServerRpc(GameManager.Instance.ServerParamsCurrentClient);
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
            this.HasCompletedTurn = true;
            //this.HandleLanding();
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

        UIManagerMonopolyGame.Instance.PanelPayment.PictureSprite = this.CurrentNode.NodeSprite;
        UIManagerMonopolyGame.Instance.PanelPayment.DescriptionText = this.CurrentChanceNode.Description;
        UIManagerMonopolyGame.Instance.PanelPayment.Show();
    }

    public void HandleChanceLanding()
    {
        this.CurrentChanceNode = MonopolyBoard.Instance.GetChanceNode();

        if (this.CurrentChanceNode.ChanceType == ChanceNodeSO.Type.Penalty)
        {
            UIManagerMonopolyGame.Instance.PanelPayment.PictureSprite = this.CurrentNode.NodeSprite;
            UIManagerMonopolyGame.Instance.PanelPayment.DescriptionText = this.CurrentChanceNode.Description;
            UIManagerMonopolyGame.Instance.PanelPayment.Show();
        }
        else
        {
            UIManagerMonopolyGame.Instance.PanelInfo.PictureSprite = this.CurrentNode.NodeSprite;
            UIManagerMonopolyGame.Instance.PanelInfo.DescriptionText = this.CurrentChanceNode.Description;
            UIManagerMonopolyGame.Instance.PanelInfo.Show();
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
            //UIManagerMonopolyGame.Instance.PanelOffer.PictureSprite = this.CurrentNode.NodeSprite;
            //UIManagerMonopolyGame.Instance.PanelOffer.MonopolyTypeColor = this.CurrentNode.AffiliatedMonopoly.ColorOfSet;
            //UIManagerMonopolyGame.Instance.PanelOffer.Show();
        }
        else if (this.CurrentNode.Owner == this)
        {
            this.HasCompletedTurn = true;
        }
        else
        {
            //UIManagerMonopolyGame.Instance.PanelPayment.PictureSprite = this.CurrentNode.NodeSprite;
            //UIManagerMonopolyGame.Instance.PanelPayment.DescriptionText = this.CurrentNode.PriceRent.ToString();
            //UIManagerMonopolyGame.Instance.PanelOffer.Show();
        }
    }

    private void AcceptPropertyOfferCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        if (this.Balance >= this.CurrentNode.PriceUpgrade)
        {
            UIManagerMonopolyGame.Instance.PanelOffer.Hide();

            this.CurrentNode.UpdateOwner(this);
            this.OwnedNodes.Add(this.CurrentNode);
            this.Balance -= this.CurrentNode.PriceUpgrade;
            this.HasCompletedTurn = true;
        }
        else
        {
            //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageInsufficientFunds;
            ////UIManagerGlobal.Instance.PanelMessageBox.Show();
        }
    }

    private void DeclinePropertyOfferCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        UIManagerMonopolyGame.Instance.PanelOffer.Hide();
        this.HasCompletedTurn = true;
    }

    public void UpgradeNodeCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        if (!this.HasFullMonopoly(this.SelectedNode, out _) && !this.CurrentNode.IsMortgaged)
        {
            //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageCompleteMonopolyRequired;
            //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
            ////UIManagerGlobal.Instance.PanelMessageBox.Show();
            return;
        }

        if (this.HasBuilt)
        {
            //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageAlreadyBuilt;
            //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
           // //UIManagerGlobal.Instance.PanelMessageBox.Show();
            return;
        }

        if (!this.SelectedNode.IsUpgradable)
        {
            if (this.SelectedNode.Level == MonopolyNode.PROPERTY_MAX_LEVEL)
            {
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageCannotUpgradeMaxLevel;
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
                ////UIManagerGlobal.Instance.PanelMessageBox.Show();
                return;
            }
            else
            {
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageOnlyEvenBuildingAllowed;
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
                ////UIManagerGlobal.Instance.PanelMessageBox.Show();
                return;
            }
        }

        this.HasBuilt = true;
        this.SelectedNode.Upgrade();
        UIManagerMonopolyGame.Instance.PanelMonopolyNode.Hide();
    }

    public void DowngradeNodeCallback()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            return;

        if (!this.SelectedNode.IsDowngradable)
        {
            if (this.SelectedNode.Level == MonopolyNode.PROPERTY_MIN_LEVEL)
            {
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageCannotDowngradeMinLevel;
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
                ////UIManagerGlobal.Instance.PanelMessageBox.Show();
                return;
            }
            else
            {
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageOnlyEvenBuildingAllowed;
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
                return;
            }
        }

        this.SelectedNode.Downgrade();
        UIManagerMonopolyGame.Instance.PanelMonopolyNode.Hide();
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
                UIManagerMonopolyGame.Instance.PanelPayment.Hide();
                this.HasCompletedTurn = true;
            }
            else
            {
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageInsufficientFunds;
                ////UIManagerGlobal.Instance.PanelMessageBox.Show();
            }
        }
        else
        {
            if (this.Balance >= this.CurrentNode.PriceRent)
            {
                this.Balance -= this.CurrentNode.PriceRent;
                UIManagerMonopolyGame.Instance.PanelPayment.Hide();
                this.HasCompletedTurn = true;
            }
            else
            {
                //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageInsufficientFunds;
                ////UIManagerGlobal.Instance.PanelMessageBox.Show();
            }
        }
    }

    private void HandleButtonRollDiceClicked()
    {
        if (this.OwnerClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
        {
            return;
        }

        GameManager.Instance.RollDice();
        UIManagerMonopolyGame.Instance.ShowDiceAnimation();

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
        UIManagerMonopolyGame.Instance.PanelInfo.Hide();

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
                    UIManagerMonopolyGame.Instance.ShowDiceAnimation();
                    this.Move(GameManager.Instance.TotalRollResult);
                }
                break;
            case ChanceNodeSO.Type.MoveBackwards:
                {
                    GameManager.Instance.RollDice();
                    UIManagerMonopolyGame.Instance.ShowDiceAnimation();
                    this.Move(-GameManager.Instance.TotalRollResult);
                }
                break;
        }
    }

    private void ClosePanelMessageBoxCallback()
    {
        ////UIManagerGlobal.Instance.PanelMessageBox.Hide();
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
