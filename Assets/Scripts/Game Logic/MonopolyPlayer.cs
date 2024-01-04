using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// TODO: Implement trading
// TODO: Implement upgrading

public sealed class MonopolyPlayer : NetworkBehaviour
{
    #region Setup

    #region Visuals

    [Header("Visuals")]

    [Space]
    [SerializeField] private Image playerImageToken;

    #endregion

    #endregion

    private int balance;

    private bool isInJail;

    private int turnsInJail;

    private bool isSkipTurn;

    public Action OnBalanceUpdated;

    public int Balance 
    {
        get
        {
            return this.balance;
        }
        private set
        {
            this.balance = value;
            this.OnBalanceUpdated?.Invoke();
        }
    }

    public string Nickname { get; private set; }

    public bool IsAbleToBuild { get; private set; }

    public bool HasCompletedTurn { get; private set; }
    
    public bool HasBuilt { get; private set; }

    public MonopolyNode SelectedNode { get; set; }

    public MonopolyNode CurrentNode { get; private set; }

    public Color PlayerColor { get; private set; }

    public List<MonopolyNode> OwnedNodes { get; private set; }

    public ChanceNodeSO CurrentChanceNode { get; private set; }

    public override void OnNetworkSpawn()
    {
        this.OwnedNodes = new List<MonopolyNode>();
        this.Balance = GameManager.Instance.StartingBalance;
        this.CurrentNode = MonopolyBoard.Instance.NodeStart;
        this.transform.position = MonopolyBoard.Instance.NodeStart.transform.position;
        this.PlayerColor = GameManager.Instance.MonopolyPlayersVisuals[GameManager.Instance.CurrentPlayerIndex].ColorPlayerToken;
        this.playerImageToken.sprite = GameManager.Instance.MonopolyPlayersVisuals[GameManager.Instance.CurrentPlayerIndex].SpritePlayerToken;
        this.Nickname = LobbyManager.Instance.LocalLobby.Players[GameManager.Instance.CurrentPlayerIndex].Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;

        GameManager.Instance.UpdatePlayersList(this);

        if (NetworkManager.Singleton.LocalClientId == this.OwnerClientId)
        {
            this.OnBalanceUpdated += this.HandleBalanceUpdated;
            UIManagerMonopolyGame.Instance.ButtonRollDiceClicked += this.HandleButtonRollDiceClicked;
        }
    }

    public override void OnNetworkDespawn()
    {
        this.Surrender();

        if (NetworkManager.Singleton.LocalClientId == this.OwnerClientId)
        {
            this.OnBalanceUpdated -= this.HandleBalanceUpdated;
            UIManagerMonopolyGame.Instance.ButtonRollDiceClicked -= this.HandleButtonRollDiceClicked;
        }
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

    private void Move(int steps)
    {
        this.IsAbleToBuild = false;

        const float POSITION_THRESHOLD = 0.01f;

        Vector3 targetPosition;

        int currentNodeIndex = MonopolyBoard.Instance[this.CurrentNode];

        this.StartCoroutine(MoveCoroutine());

        IEnumerator MoveCoroutine()
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

                if (MonopolyBoard.Instance.NodeStart == MonopolyBoard.Instance[currentNodeIndex])
                {
                    this.Balance += GameManager.Instance.CircleBonus;
                }

                yield return StartCoroutine(MoveStepCoroutine(targetPosition));
            }

            this.CurrentNode = MonopolyBoard.Instance[currentNodeIndex];
            this.HandleLanding();
        }

        IEnumerator MoveStepCoroutine(Vector3 targetPosition)
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
            //case MonopolyNode.Type.Tax:
            //    this.HandleTaxLanding();
            //    break;
            case MonopolyNode.Type.Jail:
                this.HandleJailLanding();
                break;
            case MonopolyNode.Type.Start:
                this.HandleStartLanding();
                break;
            //case MonopolyNode.Type.Chance:
            //    this.HandleChanceLanding();
            //    break;
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
            default:
                this.HasCompletedTurn = true;
                break;
        }
    }

    private IEnumerator PerformTurnCoroutine()
    {
        yield return new WaitUntil(() => this.HasCompletedTurn);

        GameManager.Instance.SwitchPlayerServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    [ClientRpc]
    public void PerformTurnClientRpc(ClientRpcParams clientRpcParams)
    {
        this.HasBuilt = false;
        this.IsAbleToBuild = true;
        this.HasCompletedTurn = false;

        if (this.isSkipTurn)
        {
            this.isSkipTurn = false;
            this.HasCompletedTurn = true;
            return;
        }

        this.StartCoroutine(this.PerformTurnCoroutine());

        UIManagerMonopolyGame.Instance.ShowButtonRollDice();
    }

    #endregion

    #region Chance

    public void HandleTaxLanding()
    {
        this.CurrentChanceNode = MonopolyBoard.Instance.GetTaxNode();

        //UIManagerMonopolyGame.Instance.PanelPayment.PictureSprite = this.CurrentNode.NodeSprite;
        //UIManagerMonopolyGame.Instance.PanelPayment.DescriptionText = this.CurrentChanceNode.Description;
        //UIManagerMonopolyGame.Instance.PanelPayment.Show();
    }

    public void HandleChanceLanding()
    {
        this.CurrentChanceNode = MonopolyBoard.Instance.GetChanceNode();

        if (this.CurrentChanceNode.ChanceType == ChanceNodeSO.Type.Penalty)
        {
            //UIManagerMonopolyGame.Instance.PanelPayment.PictureSprite = this.CurrentNode.NodeSprite;
            //UIManagerMonopolyGame.Instance.PanelPayment.DescriptionText = this.CurrentChanceNode.Description;
            //UIManagerMonopolyGame.Instance.PanelPayment.Show();
        }
        else
        {
            //UIManagerMonopolyGame.Instance.PanelInfo.PictureSprite = this.CurrentNode.NodeSprite;
            //UIManagerMonopolyGame.Instance.PanelInfo.DescriptionText = this.CurrentChanceNode.Description;
            //UIManagerMonopolyGame.Instance.PanelInfo.Show();
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
        this.GoToJail();
    }

    public void HandleFreeParkingLanding()
    {
        this.HasCompletedTurn = true;
    }

    #endregion

    #region Property

    private void UpgradeProperty()
    {
        if (!this.HasFullMonopoly(this.SelectedNode, out _) && !this.CurrentNode.IsMortgaged)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCompleteMonopolyRequired, PanelMessageBoxUI.Icon.Warning);
        }
        else if (this.HasBuilt)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageAlreadyBuilt, PanelMessageBoxUI.Icon.Warning);
        }
        else if (!this.SelectedNode.IsUpgradable)
        {
            if (this.CurrentNode.Level == MonopolyNode.PROPERTY_MAX_LEVEL)
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCannotUpgradeMaxLevel, PanelMessageBoxUI.Icon.Warning);
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageOnlyEvenBuildingAllowed, PanelMessageBoxUI.Icon.Warning);
            }
        }
        else
        {
            if (this.Balance >= this.SelectedNode.PriceUpgrade)
            {
                UIManagerMonopolyGame.Instance.HideMonopolyNode();

                this.Balance -= this.SelectedNode.PriceUpgrade;

                this.HasBuilt = true;
                this.SelectedNode.Upgrade();
                this.SelectedNode.UpgradeServerRpc(GameManager.Instance.ServerParamsCurrentClient);
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }
    }

    private void DowngradeProperty()
    {
        if (!this.SelectedNode.IsDowngradable)
        {
            if (this.CurrentNode.Level == MonopolyNode.PROPERTY_MIN_LEVEL)
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCannotDowngradeMinLevel, PanelMessageBoxUI.Icon.Warning);
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageOnlyEvenBuildingAllowed, PanelMessageBoxUI.Icon.Warning);
            }
        }
        else
        {
            UIManagerMonopolyGame.Instance.HideMonopolyNode();

            this.Balance += this.SelectedNode.PriceRent;

            this.SelectedNode.Downgrade();
            this.SelectedNode.DowngradeServerRpc(GameManager.Instance.ServerParamsCurrentClient);
        }
    }

    public void CallbackMonopolyNode()
    {
        if (UIManagerMonopolyGame.Instance.PanelMonopolyNode.MonopolyNodeDialogResult == PanelMonopolyNodeUI.DialogResult.Upgrade)
        {
            this.UpgradeProperty();
        }
        else
        {
            this.DowngradeProperty();
        }
    }

    private void HandlePropertyLanding()
    {
        if (this.CurrentNode.Owner == null)
        {
            UIManagerMonopolyGame.Instance.ShowOffer(this.CurrentNode.NodeSprite, this.CurrentNode.AffiliatedMonopoly.ColorOfSet, this.CallbackPropertyLanding);
        }
        else if (this.CurrentNode.Owner == this)
        {
            this.HasCompletedTurn = true;
        }
        else
        {
            UIManagerMonopolyGame.Instance.ShowPayment(this.CurrentNode.NodeSprite, this.CurrentNode.PriceRent.ToString(), this.CallbackPayment);
        }
    }

    private void CallbackPropertyLanding()
    {
        if (UIManagerMonopolyGame.Instance.PanelOffer.OfferDialogResult == PanelOfferUI.DialogResult.Accepted)
        {
            if (this.Balance >= this.CurrentNode.PriceUpgrade)
            {
                this.CurrentNode.UpdateOwner();
                this.OwnedNodes.Add(this.CurrentNode);
                this.Balance -= this.CurrentNode.PriceUpgrade;
                this.CurrentNode.UpdateOwnerServerRpc(GameManager.Instance.ServerParamsCurrentClient);

                this.HasCompletedTurn = true;
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }
    }

    #endregion

    #region UI Callbacks

    private void CallbackPayment()
    {
        if (UIManagerMonopolyGame.Instance.PanelPayment.PaymentDialogResult == PanelPaymentUI.DialogResult.Confirmed)
        {
            if (this.Balance >= this.CurrentNode.PriceRent)
            {
                this.Balance -= this.CurrentNode.PriceRent;
                this.HasCompletedTurn = true;
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }

        //if (this.CurrentNode.NodeType == MonopolyNode.Type.Chance || this.CurrentNode.NodeType == MonopolyNode.Type.Tax)
        //{
        //    if (this.Balance >= this.CurrentChanceNode.Penalty)
        //    {
        //        this.Balance -= this.CurrentChanceNode.Penalty;
        //        UIManagerMonopolyGame.Instance.PanelPayment.Hide();
        //        this.HasCompletedTurn = true;
        //    }
        //    else
        //    {
        //        //UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerMonopolyGame.Instance.MessageInsufficientFunds;
        //        ////UIManagerGlobal.Instance.PanelMessageBox.Show();
        //    }
        //}
        //else
        //{

        //}
    }

    private void HandleButtonRollDiceClicked()
    {
        GameManager.Instance.RollDice();
        UIManagerMonopolyGame.Instance.ShowDiceAnimation();

        if (this.isInJail)
        {
            if (GameManager.Instance.HasRolledDouble || ++this.turnsInJail >= GameManager.Instance.MaxTurnsInJail)
            {
                this.ReleaseFromJail();
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

    //private void ClosePanelInfoCallback()
    //{
    //    UIManagerMonopolyGame.Instance.PanelInfo.Hide();

    //    switch (this.CurrentChanceNode.ChanceType)
    //    {
    //        case ChanceNodeSO.Type.Reward:
    //            {
    //                this.Balance += this.CurrentChanceNode.Reward;
    //                this.HasCompletedTurn = true;
    //            }
    //            break;
    //        case ChanceNodeSO.Type.SkipTurn:
    //            {
    //                this.isSkipTurn = true;
    //                this.HasCompletedTurn = true;
    //            }
    //            break;
    //        case ChanceNodeSO.Type.SendJail:
    //            this.HandleSendJailLanding();
    //            break;
    //        case ChanceNodeSO.Type.MoveForward:
    //            {
    //                GameManager.Instance.RollDice();
    //                UIManagerMonopolyGame.Instance.ShowDiceAnimation();
    //                this.Move(GameManager.Instance.TotalRollResult);
    //            }
    //            break;
    //        case ChanceNodeSO.Type.MoveBackwards:
    //            {
    //                GameManager.Instance.RollDice();
    //                UIManagerMonopolyGame.Instance.ShowDiceAnimation();
    //                this.Move(-GameManager.Instance.TotalRollResult);
    //            }
    //            break;
    //    }
    //}

    #endregion

    public void GoToJail()
    {
        this.isInJail = true;
        this.turnsInJail = 0;
        this.Move(MonopolyBoard.Instance.GetDistance(this.CurrentNode, MonopolyBoard.Instance.NodeJail));

        this.HasCompletedTurn = true;

        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageSentJail);
    }

    public void Surrender()
    {
        foreach (MonopolyNode node in this.OwnedNodes)
        {
            node.ResetOwnership();
            node.ResetOwnershipServerRpc(GameManager.Instance.ServerParamsCurrentClient);
        }

        GameManager.Instance.SwitchPlayerServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    private void ReleaseFromJail()
    {
        this.turnsInJail = 0;
        this.isInJail = false;
    }

    private void HandleBalanceUpdated()
    {
        this.UpdateBalanceServerRpc(this.Balance, GameManager.Instance.ServerParamsCurrentClient);
    }

    [ClientRpc]
    public void GoToJailClientRpc(ClientRpcParams clientRpcParams)
    {
        this.GoToJail();
    }

    [ServerRpc]
    private void UpdateBalanceServerRpc(int balance, ServerRpcParams serverRpcParams)
    {
        this.UpdateBalanceClientRpc(balance, GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void UpdateBalanceClientRpc(int balance, ClientRpcParams clientRpcParams)
    {
        this.Balance = balance;
    }
}
