using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

    public bool HasBuilt { get; private set; }

    public bool IsAbleToBuild { get; private set; }

    public bool HasCompletedTurn { get; private set; }
    
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

        bool movedOverStart = false;

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
                    movedOverStart = true;
                }

                yield return StartCoroutine(MoveStepCoroutine(targetPosition));
            }

            this.CurrentNode = MonopolyBoard.Instance[currentNodeIndex];

            if (movedOverStart && this.CurrentNode != MonopolyBoard.Instance.NodeStart)
            {
                this.Balance += GameManager.Instance.CircleBonus;
            }

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
            case MonopolyNode.Type.Tax:
                this.HandleChanceLanding();
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
        this.CurrentChanceNode = null;

        this.StartCoroutine(this.PerformTurnCoroutine());

        if (this.isSkipTurn)
        {
            this.isSkipTurn = false;
            this.HasCompletedTurn = true;
            return;
        }

        UIManagerMonopolyGame.Instance.ShowButtonRollDice();
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

    public void HandleChanceLanding()
    {
        this.CurrentChanceNode = MonopolyBoard.Instance.GetChanceNode();

        if (this.CurrentChanceNode.ChanceType != ChanceNodeSO.Type.Penalty)
        {
            UIManagerMonopolyGame.Instance.ShowInfo(this.CurrentChanceNode.Description, this.CallbackChance);
        }
        else
        {
            UIManagerMonopolyGame.Instance.ShowPaymentChance(this.CurrentChanceNode.Description, this.CallbackPayment);
        }

        UIManagerMonopolyGame.Instance.ShowInfoServerRpc(this.CurrentChanceNode.Description, GameManager.Instance.ServerParamsCurrentClient);
    }

    public void HandleSendJailLanding()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageSentJail, PanelMessageBoxUI.Icon.Warning);

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
        if (this.SelectedNode.NodeType == MonopolyNode.Type.Gambling || this.SelectedNode.NodeType == MonopolyNode.Type.Transport)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCannotUpgradeNotProperty, PanelMessageBoxUI.Icon.Warning);
        }
        else if (!this.HasFullMonopoly(this.SelectedNode, out _) && !this.SelectedNode.IsMortgaged)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCompleteMonopolyRequired, PanelMessageBoxUI.Icon.Warning);
        }
        else if (this.HasBuilt)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageAlreadyBuilt, PanelMessageBoxUI.Icon.Warning);
        }
        else if (!this.SelectedNode.IsUpgradable)
        {
            if (this.SelectedNode.Level == MonopolyNode.PROPERTY_MAX_LEVEL)
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
            if (this.SelectedNode.Level == MonopolyNode.PROPERTY_MIN_LEVEL)
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

            this.Balance += this.SelectedNode.PriceDowngrade;

            this.SelectedNode.Downgrade();
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
            UIManagerMonopolyGame.Instance.ShowOffer(this.CurrentNode.NodeSprite, this.CurrentNode.AffiliatedMonopoly.ColorOfSet, this.CurrentNode.PricePurchase, this.CallbackPropertyOffer);
        }
        else if (this.CurrentNode.Owner == this)
        {
            this.HasCompletedTurn = true;
        }
        else
        {
            UIManagerMonopolyGame.Instance.ShowPaymentProperty(this.CurrentNode.NodeSprite, this.CurrentNode.AffiliatedMonopoly.ColorOfSet, this.CurrentNode.PriceRent, this.CallbackPayment);
        }
    }
    
    private void CallbackPropertyOffer()
    {
        if (UIManagerMonopolyGame.Instance.PanelOffer.OfferDialogResult == PanelOfferUI.DialogResult.Accepted)
        {
            if (this.Balance >= this.CurrentNode.PricePurchase)
            {
                UIManagerMonopolyGame.Instance.HideOffer();

                Debug.Log($"I have before {this.Balance}");

                this.OwnedNodes.Add(this.CurrentNode);
                this.CurrentNode.UpdateOwnership();
                this.Balance -= this.CurrentNode.PricePurchase;

                Debug.Log($"I have after {this.Balance}");

                this.HasCompletedTurn = true;
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }
        else
        {
            UIManagerMonopolyGame.Instance.HideOffer();
            this.HasCompletedTurn = true;
        }
    }

    #endregion

    #region GUI Callbacks

    private void CallbackChance()
    {
        if (UIManagerMonopolyGame.Instance.PanelInfo.InfoDialogResult == PanelInfoUI.DialogResult.Confirmed)
        {
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
                    this.GoToJail();
                    break;
                case ChanceNodeSO.Type.MoveForward:
                    {
                        this.CurrentChanceNode = null;
                        GameManager.Instance.RollDice();
                        UIManagerMonopolyGame.Instance.ShowDiceAnimation();
                        this.Move(GameManager.Instance.TotalRollResult);
                    }
                    break;
                case ChanceNodeSO.Type.MoveBackwards:
                    {
                        this.CurrentChanceNode = null;
                        GameManager.Instance.RollDice();
                        UIManagerMonopolyGame.Instance.ShowDiceAnimation();
                        this.Move(-GameManager.Instance.TotalRollResult);
                    }
                    break;
            }
        }
    }

    private void CallbackPayment()
    {
        if (this.CurrentNode.NodeType == MonopolyNode.Type.Chance || this.CurrentNode.NodeType == MonopolyNode.Type.Tax)
        {
            if (this.Balance >= this.CurrentChanceNode.Penalty)
            {
                UIManagerMonopolyGame.Instance.HidePaymentChance();

                this.Balance -= this.CurrentChanceNode.Penalty;

                this.HasCompletedTurn = true;
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }
        else
        {
            if (this.Balance >= this.CurrentNode.PriceRent)
            {
                UIManagerMonopolyGame.Instance.HidePaymentProperty();

                Debug.Log($"I have before {this.Balance}");

                this.Balance -= this.CurrentNode.PriceRent;

                Debug.Log($"I have after {this.Balance}");

                Debug.Log($"I am {NetworkManager.Singleton.LocalClientId} sending balance to {this.CurrentNode.Owner.OwnerClientId}");

                this.SendBalanceServerRpc(this.CurrentNode.PriceRent, this.CurrentNode.Owner.OwnerClientId, GameManager.Instance.ServerParamsCurrentClient);

                this.HasCompletedTurn = true;
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }
    }
    
    private void HandleButtonRollDiceClicked()
    {
        GameManager.Instance.RollDice();
        UIManagerMonopolyGame.Instance.ShowDiceAnimation();

        if (this.isInJail)
        {
            if (GameManager.Instance.HasRolledDouble || ++this.turnsInJail > GameManager.Instance.MaxTurnsInJail)
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

    #endregion

    public void GoToJail()
    {
        this.isInJail = true;
        this.turnsInJail = 0;
        this.Move(MonopolyBoard.Instance.GetDistance(this.CurrentNode, MonopolyBoard.Instance.NodeJail));

        if (this.CurrentChanceNode != null)
        {
            this.CurrentChanceNode = null;
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageSentJail, PanelMessageBoxUI.Icon.Warning);
        }
    }

    public void Surrender()
    {
        foreach (MonopolyNode node in this.OwnedNodes)
        {
            node.ResetOwnership();
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
        Debug.Log(nameof(HandleBalanceUpdated));

        this.UpdateBalanceServerRpc(this.Balance, GameManager.Instance.ServerParamsCurrentClient);
    }
    
    [ClientRpc]
    public void GoToJailClientRpc(ClientRpcParams clientRpcParams)
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageSentJail, PanelMessageBoxUI.Icon.Warning);

        this.GoToJail();
    }

    [ServerRpc]
    private void UpdateBalanceServerRpc(int balance, ServerRpcParams serverRpcParams)
    {
        Debug.Log(nameof(UpdateBalanceServerRpc));

        this.UpdateBalanceClientRpc(balance, GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void UpdateBalanceClientRpc(int balance, ClientRpcParams clientRpcParams)
    {
        Debug.Log(nameof(UpdateBalanceClientRpc));

        this.Balance = balance;

        Debug.Log($"Now I have {this.Balance}");
    }

    [ClientRpc]
    private void ReceiveBalanceClientRpc(int amount, ClientRpcParams clientRpcParams)
    {
        Debug.Log(nameof(ReceiveBalanceClientRpc));

        Debug.Log($"I am {NetworkManager.Singleton.LocalClientId} recieved {amount}");

        Debug.Log($"The balance I has {this.Balance}");

        this.Balance += amount;

        Debug.Log($"The balance I have now {this.Balance}");
    }

    [ServerRpc]
    private void SendBalanceServerRpc(int amount, ulong receiverClientId, ServerRpcParams serverRpcParams)
    {
        Debug.Log(nameof(SendBalanceServerRpc));

        this.ReceiveBalanceClientRpc(amount, GameManager.Instance.GetRedirectionRpc(receiverClientId));
    }

    //private void Update()
    //{
    //    Debug.Log($"{this.Nickname}:{this.Balance}");
    //}
}
