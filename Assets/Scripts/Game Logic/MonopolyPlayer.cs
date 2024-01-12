using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

    public Action OnBalanceUpdated;
    
    public bool IsTrading { get; set; }

    public bool HasBuilt { get; private set; }

    public bool HasRolled { get; private set; }

    public string Nickname { get; private set; }

    public bool IsAbleToBuild { get; private set; }

    public bool HasCompletedTurn { get; private set; }
    
    public Color PlayerColor { get; private set; }

    public MonopolyNode SelectedNode { get; set; }

    public MonopolyNode CurrentNode { get; private set; }

    public MonopolyPlayer PlayerTradingWith { get; set; }

    public NetworkVariable<int> Balance { get; private set; }

    public List<MonopolyNode> OwnedNodes { get; private set; }

    public ChanceNodeSO CurrentChanceNode { get; private set; }

    private void Awake()
    {
        this.Balance = new NetworkVariable<int>(GameManager.Instance.StartingBalance, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    }

    public override void OnNetworkSpawn()
    {
        this.OwnedNodes = new List<MonopolyNode>();
        this.Balance.Value = GameManager.Instance.StartingBalance;
        this.CurrentNode = MonopolyBoard.Instance.NodeStart;
        this.transform.position = MonopolyBoard.Instance.NodeStart.transform.position;
        this.PlayerColor = GameManager.Instance.MonopolyPlayersVisuals[GameManager.Instance.CurrentPlayerIndex].ColorPlayerToken;
        this.playerImageToken.sprite = GameManager.Instance.MonopolyPlayersVisuals[GameManager.Instance.CurrentPlayerIndex].SpritePlayerToken;
        this.Nickname = LobbyManager.Instance.LocalLobby.Players[GameManager.Instance.CurrentPlayerIndex].Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;

        GameManager.Instance.AddPlayer(this);

        this.Balance.OnValueChanged += this.HandleBalanceChanged;

        if (this.OwnerClientId == NetworkManager.Singleton?.LocalClientId)
        {
            UIManagerMonopolyGame.Instance.ButtonRollDiceClicked += this.HandleButtonRollDiceClicked;
        }
    }

    public override void OnNetworkDespawn()
    {
        this.Balance.OnValueChanged -= this.HandleBalanceChanged;

        if (this.OwnerClientId == NetworkManager.Singleton?.LocalClientId)
        {
            this.Surrender();
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
                this.Balance.Value += GameManager.Instance.CircleBonus;
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

        if (this.isInJail)
        {
            GameManager.Instance.SwitchPlayerForcefullyServerRpc(GameManager.Instance.ServerParamsCurrentClient);
        }
        else
        {
            GameManager.Instance.SwitchPlayerServerRpc(GameManager.Instance.ServerParamsCurrentClient);
        }
    }

    [ClientRpc]
    public void PerformTurnClientRpc(ClientRpcParams clientRpcParams)
    {
        this.HasBuilt = false;
        this.IsTrading = false;
        this.HasRolled = false;
        this.IsAbleToBuild = true;
        this.HasCompletedTurn = false;
        this.CurrentChanceNode = null;
        this.PlayerTradingWith = null;

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
        this.DeclineTradeServerRpc(GameManager.Instance.ServerParamsCurrentClient);

        UIManagerMonopolyGame.Instance.HidePaymentProperty();
        UIManagerMonopolyGame.Instance.HideButtonRollDice();
        UIManagerMonopolyGame.Instance.HidePaymentChance();
        UIManagerMonopolyGame.Instance.HideMonopolyNode();
        UIManagerMonopolyGame.Instance.HideReceiveTrade();
        UIManagerMonopolyGame.Instance.HideTradeOffer();
        UIManagerMonopolyGame.Instance.HideOffer();

        foreach (MonopolyNode node in this.OwnedNodes)
        {
            node.ResetOwnership();
        }

        this.SurrenderServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }
    
    private void ReleaseFromJail()
    {
        this.turnsInJail = 0;
        this.isInJail = false;
    }

    public void HandleJailLanding()
    {
        this.HasCompletedTurn = true;
    }

    public void HandleStartLanding()
    {
        this.Balance.Value += GameManager.Instance.ExactCircleBonus;
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

    [ClientRpc]
    public void GoToJailClientRpc(ClientRpcParams clientRpcParams)
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageSentJail, PanelMessageBoxUI.Icon.Warning);

        this.GoToJail();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SurrenderServerRpc(ServerRpcParams serverRpcParams)
    {
        GameManager.Instance.RemovePlayerServerRpc(serverRpcParams.Receive.SenderClientId, GameManager.Instance.ServerParamsCurrentClient);

        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            NetworkClient client = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId];

            foreach (NetworkObject ownedObject in client.OwnedObjects)
            {
                if (!(bool)ownedObject.IsSceneObject && ownedObject.IsSpawned)
                {
                    ownedObject.Despawn();
                }
            }
        }
    }

    #endregion

    #region Property

    private void UpgradeProperty()
    {
        if (this.SelectedNode.NodeType == MonopolyNode.Type.Transport || this.SelectedNode.NodeType == MonopolyNode.Type.Gambling)
        {
            if (!this.SelectedNode.IsMortgaged)
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCannotUpgradeMaxLevel, PanelMessageBoxUI.Icon.Warning);
            }
            else
            {
                if (this.Balance.Value >= this.SelectedNode.PriceUpgrade)
                {
                    UIManagerMonopolyGame.Instance.HideMonopolyNode();

                    this.Balance.Value -= this.SelectedNode.PriceUpgrade;

                    this.HasBuilt = true;
                    this.SelectedNode.Upgrade();
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
                }
            }
        }
        else if (this.SelectedNode.NodeType == MonopolyNode.Type.Property)
        {
            if (!this.HasFullMonopoly(this.SelectedNode, out _) && !this.SelectedNode.IsMortgaged)
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCompleteMonopolyRequired, PanelMessageBoxUI.Icon.Warning);
            }
            else if (this.HasBuilt)
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageAlreadyBuilt, PanelMessageBoxUI.Icon.Warning);
            }
            else if (!this.SelectedNode.IsUpgradable)
            {
                if (this.SelectedNode.LocalLevel == MonopolyNode.PROPERTY_MAX_LEVEL)
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
                if (this.Balance.Value >= this.SelectedNode.PriceUpgrade)
                {
                    UIManagerMonopolyGame.Instance.HideMonopolyNode();

                    this.Balance.Value -= this.SelectedNode.PriceUpgrade;

                    this.HasBuilt = true;
                    this.SelectedNode.Upgrade();
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
                }
            }
        }
    }

    private void DowngradeProperty()
    {
        if (!this.SelectedNode.IsDowngradable)
        {
            if (this.SelectedNode.LocalLevel == MonopolyNode.PROPERTY_MIN_LEVEL)
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

            this.Balance.Value += this.SelectedNode.PriceDowngrade;

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
        else if (this.CurrentNode.Owner == this || this.CurrentNode.IsMortgaged)
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
            if (this.Balance.Value >= this.CurrentNode.PricePurchase)
            {
                UIManagerMonopolyGame.Instance.HideOffer();

                this.OwnedNodes.Add(this.CurrentNode);

                this.CurrentNode.UpdateOwnership(NetworkManager.Singleton.LocalClientId);
                this.Balance.Value -= this.CurrentNode.PricePurchase;

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

    [ClientRpc]
    private void AddNodeClientRpc(int monopolyNodeIndex, ClientRpcParams clientRpcParams)
    {
        this.OwnedNodes.Add(MonopolyBoard.Instance[monopolyNodeIndex]);
    }

    [ClientRpc]
    private void RemoveNodeClientRpc(int monopolyNodeIndex, ClientRpcParams clientRpcParams)
    {
        this.OwnedNodes.Remove(MonopolyBoard.Instance[monopolyNodeIndex]);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddNodeServerRpc(int monopolyNodeIndex, ulong ownerdId, ServerRpcParams serverRpcParams)
    {
        this.AddNodeClientRpc(monopolyNodeIndex, GameManager.Instance.GetRedirectionRpc(ownerdId));
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveNodeServerRpc(int monopolyNodeIndex, ulong ownerdId, ServerRpcParams serverRpcParams)
    {
        this.RemoveNodeClientRpc(monopolyNodeIndex, GameManager.Instance.GetRedirectionRpc(ownerdId));
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
                        this.Balance.Value += this.CurrentChanceNode.Reward;
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
            if (this.Balance.Value >= this.CurrentChanceNode.Penalty)
            {
                UIManagerMonopolyGame.Instance.HidePaymentChance();

                this.Balance.Value -= this.CurrentChanceNode.Penalty;

                this.HasCompletedTurn = true;
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }
        else
        {
            if (this.Balance.Value >= this.CurrentNode.PriceRent)
            {
                UIManagerMonopolyGame.Instance.HidePaymentProperty();

                this.Balance.Value -= this.CurrentNode.PriceRent;

                this.SendBalanceServerRpc(this.CurrentNode.PriceRent, this.CurrentNode.Owner.OwnerClientId, GameManager.Instance.ServerParamsCurrentClient);

                this.HasCompletedTurn = true;
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }
    }

    public void CallbackTradeOffer()
    {
        if (UIManagerMonopolyGame.Instance.PanelTradeOffer.TradeOfferDialogResult == PanelTradeOfferUI.DialogResult.Offer)
        {
            UIManagerMonopolyGame.Instance.SendTradeOffer();
        }
        else
        {
            this.IsTrading = false;

            if (!this.HasRolled)
            {
                UIManagerMonopolyGame.Instance.ShowButtonRollDice();
            }
        }
    }

    public void CallbackReceiveTrade()
    {
        UIManagerMonopolyGame.Instance.HideReceiveTrade();

        if (UIManagerMonopolyGame.Instance.PanelReceiveTrade.ReceiveTradeDialogResult == PanelReceiveTradeUI.DialogResult.Accept)
        {
            this.AcceptTradeServerRpc(UIManagerMonopolyGame.Instance.PanelReceiveTrade.Credentials, GameManager.Instance.ServerParamsCurrentClient);
        }
        else
        {
            this.DeclineTradeServerRpc(GameManager.Instance.ServerParamsCurrentClient);
        }
    }

    private void HandleButtonRollDiceClicked()
    {
        this.HasRolled = true;

        UIManagerMonopolyGame.Instance.HidePaymentProperty();
        UIManagerMonopolyGame.Instance.HidePaymentChance();
        UIManagerMonopolyGame.Instance.HideMonopolyNode();
        UIManagerMonopolyGame.Instance.HideReceiveTrade();
        UIManagerMonopolyGame.Instance.HideTradeOffer();
        UIManagerMonopolyGame.Instance.HideOffer();

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

    [ServerRpc(RequireOwnership = false)]
    public void DeclineTradeServerRpc(ServerRpcParams serverRpcParams)
    {
        this.CallbackTradeResponseClientRpc(false, GameManager.Instance.ClientParamsCurrentClient);
    }

    [ClientRpc]
    private void CallbackTradeResponseClientRpc(bool result, ClientRpcParams clientRpcParams)
    {
        if (GameManager.Instance.CurrentPlayer == null || !GameManager.Instance.CurrentPlayer.IsTrading)
        {
            return;
        }

        GameManager.Instance.CurrentPlayer.IsTrading = false;

        if (result)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageTradeAccepted, PanelMessageBoxUI.Icon.Warning);
        }
        else
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageTradeDeclined, PanelMessageBoxUI.Icon.Warning);
        }

        if (!GameManager.Instance.CurrentPlayer.HasRolled)
        {
            UIManagerMonopolyGame.Instance.ShowButtonRollDice();
        }
    }

    [ServerRpc]
    public void AcceptTradeServerRpc(TradeCredentials tradeCredentials, ServerRpcParams serverRpcParams)
    {
        UIManagerMonopolyGame.Instance.HideMonopolyNode();

        MonopolyPlayer sender = GameManager.Instance.GetPlayerById(tradeCredentials.SenderId);
        MonopolyPlayer receiver = GameManager.Instance.GetPlayerById(tradeCredentials.ReceiverId);

        if (sender == null || receiver == null)
        {
            return;
        }

        if (tradeCredentials.SenderNodeIndex != -1)
        {
            MonopolyBoard.Instance[tradeCredentials.SenderNodeIndex].UpdateOwnership(tradeCredentials.ReceiverId);
            receiver.AddNodeServerRpc(tradeCredentials.SenderNodeIndex, tradeCredentials.ReceiverId, GameManager.Instance.ServerParamsCurrentClient);
            sender.RemoveNodeServerRpc(tradeCredentials.SenderNodeIndex, tradeCredentials.SenderId, GameManager.Instance.ServerParamsCurrentClient);
        }

        if (tradeCredentials.ReceiverNodeIndex != -1)
        {
            MonopolyBoard.Instance[tradeCredentials.ReceiverNodeIndex].UpdateOwnership(tradeCredentials.SenderId);
            sender.AddNodeServerRpc(tradeCredentials.ReceiverNodeIndex, tradeCredentials.SenderId, GameManager.Instance.ServerParamsCurrentClient);
            receiver.RemoveNodeServerRpc(tradeCredentials.ReceiverNodeIndex, tradeCredentials.ReceiverId, GameManager.Instance.ServerParamsCurrentClient);
        }

        if (tradeCredentials.SenderOffer != 0)
        {
            sender.SendBalanceServerRpc(tradeCredentials.SenderOffer, receiver.OwnerClientId, GameManager.Instance.ServerParamsCurrentClient);
            GameManager.Instance.GetPlayerById(tradeCredentials.SenderId).PayChargeClientRpc(tradeCredentials.SenderOffer, GameManager.Instance.GetRedirectionRpc(tradeCredentials.SenderId));
        }

        if (tradeCredentials.ReceiverOffer != 0)
        {
            sender.SendBalanceServerRpc(tradeCredentials.ReceiverOffer, sender.OwnerClientId, GameManager.Instance.ServerParamsCurrentClient);
            GameManager.Instance.GetPlayerById(tradeCredentials.ReceiverId).PayChargeClientRpc(tradeCredentials.ReceiverOffer, GameManager.Instance.GetRedirectionRpc(tradeCredentials.ReceiverId));
        }

        this.CallbackTradeResponseClientRpc(true, GameManager.Instance.ClientParamsCurrentClient);
    }

    #endregion

    #region Updating Balance

    private void HandleBalanceChanged(int previousValue, int newValue)
    {
        this.OnBalanceUpdated?.Invoke();
    }

    [ClientRpc]
    private void PayChargeClientRpc(int amount, ClientRpcParams clientRpcParams)
    {
        this.Balance.Value -= amount;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendBalanceServerRpc(int amount, ulong receiverClientId, ServerRpcParams serverRpcParams)
    {
        this.ReceiveBalanceClientRpc(amount, receiverClientId, GameManager.Instance.GetRedirectionRpc(receiverClientId));
    }

    [ClientRpc]
    private void ReceiveBalanceClientRpc(int amount, ulong receiverClientId, ClientRpcParams clientRpcParams)
    {
        GameManager.Instance.GetPlayerById(receiverClientId).Balance.Value += amount;
    }

    #endregion
}
