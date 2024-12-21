using Unity.Netcode;
using Unity.Services.Lobbies;

internal sealed class PlayerPawnController : PawnController
{
    internal static PlayerPawnController LocalInstance { get; private set; }

    private bool isAbleToBuild;
    private ChanceNodeSO currentChanceNode;

    internal MonopolyNode SelectedNode { get; set; }
    internal PawnController TradeReceiver { get; set; }

    public override sealed async void OnNetworkSpawn()
    {
        try
        {
            this.Nickname = LobbyManager.Instance.LocalLobby.Players[GameManager.Instance.PawnsCount - 1].Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
        }
        catch (LobbyServiceException)
        {
            await LobbyManager.Instance.DisconnectFromLobbyAsync();
        }

        if (base.OwnerClientId == NetworkManager.Singleton?.LocalClientId)
        {
            PlayerPawnController.LocalInstance = this;
            UIManagerMonopolyGame.Instance.ButtonRollDiceClicked += this.OnButtonRollDiceClicked;
        }
    }

    internal void DeclareBankruptcy()
    {
        UIManagerMonopolyGame.Instance.HideButtonRollDice();
        UIManagerMonopolyGame.Instance.ShowButtonDisconnect();

        PlayerPawnController.LocalInstance.SurrenderServerRpc(GameManager.Instance.SenderLocalClient);
    }

    private void OnButtonRollDiceClicked()
    {
        this.isAbleToBuild = false;

        UIManagerMonopolyGame.Instance.HidePanelNodeMenu();
        UIManagerMonopolyGame.Instance.HidePanelSendTrade();
        UIManagerMonopolyGame.Instance.HidePanelNodeOffer();
        UIManagerMonopolyGame.Instance.HideButtonRollDice();
        UIManagerMonopolyGame.Instance.HidePanelNodePayment();
        UIManagerMonopolyGame.Instance.HidePanelReceiveTrade();
        UIManagerMonopolyGame.Instance.HidePanelChancePayment();

        GameManager.Instance.RollDice();
        UIManagerMonopolyGame.Instance.ShowDiceAnimation();

        base.PerformDiceRolling();
    }

    internal override sealed void PerformTurn()
    {
        this.SelectedNode = null;
        this.TradeReceiver = null;

        this.isAbleToBuild = true;
        this.currentChanceNode = null;

        if (base.IsSkipTurn)
        {
            base.IsSkipTurn = false;
            base.CompleteTurn();
            return;
        }

        UIManagerMonopolyGame.Instance.ShowButtonRollDice();
    }

    private protected override sealed void HandleJailLanding()
    {
        base.CompleteTurn();
    }

    private protected override sealed void HandleStartLanding()
    {
        base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value + GameManager.Instance.ExactCircleBonus, GameManager.Instance.SenderLocalClient);
        base.CompleteTurn();
    }

    private protected override sealed void HandleChanceLanding()
    {
        this.currentChanceNode = MonopolyBoard.Instance.GetChanceNode();

        if (this.currentChanceNode.ChanceType != ChanceNodeSO.Type.Penalty)
            UIManagerMonopolyGame.Instance.ShowPanelInfo(this.currentChanceNode.Description, this.OnChanceActionAccepted);
        else
            UIManagerMonopolyGame.Instance.ShowPanelChancePayment(this.currentChanceNode.Description, this.OnChancePaymentAccepted);

        UIManagerMonopolyGame.Instance.ShowPanelInfoServerRpc(this.currentChanceNode.Description, GameManager.Instance.SenderLocalClient);
    }

    private void OnChanceActionAccepted()
    {
        if (UIManagerMonopolyGame.Instance.PanelInfo.PanelDialogResult == PanelInfoUI.DialogResult.Confirmed)
            base.PerformChanceAction(this.currentChanceNode);
    }

    private void OnChancePaymentAccepted()
    {
        if (base.NetWorth < this.currentChanceNode.Penalty)
        {
            base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.NetWorth, GameManager.Instance.SenderLocalClient);
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageBankrupt, PanelMessageBoxUI.Icon.Error, this.DeclareBankruptcy);
        }
        else
        {
            if (base.Balance.Value >= this.currentChanceNode.Penalty)
            {
                UIManagerMonopolyGame.Instance.HidePanelChancePayment();

                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - this.currentChanceNode.Penalty, GameManager.Instance.SenderLocalClient);
                base.CompleteTurn();
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }
    }

    private protected override sealed void HandleSendJailLanding()
    {
        UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageSentJail, PanelMessageBoxUI.Icon.Warning);
        base.GoToJail();
    }

    private protected override sealed void HandlePropertyLanding()
    {
        if (base.CurrentNode.Owner == null)
        {
            UIManagerMonopolyGame.Instance.ShowPanelNodeOffer(base.CurrentNode, this.OnNodeOfferShown);
            return;
        }

        if (base.CurrentNode.Owner == this || base.CurrentNode.IsMortgaged)
        {
            base.CompleteTurn();
            return;
        }

        if (base.NetWorth < base.CurrentNode.PriceRent)
        {
            base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.NetWorth, GameManager.Instance.SenderLocalClient);
            base.UpdateBalanceServerRpc(base.CurrentNode.Owner.NetworkIndex, base.CurrentNode.Owner.Balance.Value + base.NetWorth, GameManager.Instance.SenderLocalClient);

            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageBankrupt, PanelMessageBoxUI.Icon.Error, this.DeclareBankruptcy);
        }
        else
        {
            UIManagerMonopolyGame.Instance.ShowPanelPropertyPayment(base.CurrentNode, this.OnNodePaymentShown);
        }
    }

    private void OnNodeOfferShown()
    {
        if (UIManagerMonopolyGame.Instance.PanelNodeOffer.PanelDialogResult == PanelNodeOfferUI.DialogResult.Accepted)
        {
            if (base.Balance.Value >= base.CurrentNode.PricePurchase)
            {
                UIManagerMonopolyGame.Instance.HidePanelNodeOffer();

                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentNode.PricePurchase, GameManager.Instance.SenderLocalClient);
                base.CurrentNode.UpdateOwnershipServerRpc(base.NetworkIndex, GameManager.Instance.SenderLocalClient);
                base.CompleteTurn();
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            }
        }
        else
        {
            UIManagerMonopolyGame.Instance.HidePanelNodeOffer();
            base.CompleteTurn();
        }
    }

    private void OnNodePaymentShown()
    {
        if (base.Balance.Value >= base.CurrentNode.PriceRent)
        {
            UIManagerMonopolyGame.Instance.HidePanelNodePayment();

            base.UpdateBalanceServerRpc(base.CurrentNode.Owner.NetworkIndex, base.CurrentNode.Owner.Balance.Value + base.CurrentNode.PriceRent, GameManager.Instance.SenderLocalClient);
            base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentNode.PriceRent, GameManager.Instance.SenderLocalClient);
            base.CompleteTurn();
        }
        else
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
        }
    }

    private protected override sealed void HandleFreeParkingLanding()
    {
        base.CompleteTurn();
    }

    internal void OnNodeMenuShown()
    {
        if (UIManagerMonopolyGame.Instance.PanelNodeMenu.PanelDialogResult == PanelNodeMenuUI.DialogResult.Upgrade)
            this.UpgradeProperty();
        else
            this.DowngradeProperty();
    }

    private void UpgradeProperty()
    {
        if (!this.isAbleToBuild)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageAlreadyBuilt, PanelMessageBoxUI.Icon.Warning);
            return;
        }

        if (base.Balance.Value < this.SelectedNode.PriceUpgrade)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageInsufficientFunds, PanelMessageBoxUI.Icon.Warning);
            return;
        }

        if (!this.SelectedNode.IsUpgradable)
        {
            if (this.SelectedNode.NodeType == MonopolyNode.Type.Property)
            {
                if (this.SelectedNode.Level == MonopolyNode.PROPERTY_MAX_LEVEL)
                    UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCannotUpgradeMaxLevel, PanelMessageBoxUI.Icon.Warning);
                else if (!base.HasFullMonopoly(this.SelectedNode.AffiliatedMonopoly))
                    UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCompleteMonopolyRequired, PanelMessageBoxUI.Icon.Warning);
                else
                    UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageOnlyEvenBuildingAllowed, PanelMessageBoxUI.Icon.Warning);
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCannotUpgradeMaxLevel, PanelMessageBoxUI.Icon.Warning);
            }
        }
        else
        {
            this.isAbleToBuild = false;
            UIManagerMonopolyGame.Instance.HidePanelNodeMenu();

            this.SelectedNode.Upgrade();
            base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - this.SelectedNode.PriceUpgrade, GameManager.Instance.SenderLocalClient);
        }
    }

    private void DowngradeProperty()
    {
        if (!this.SelectedNode.IsDowngradable)
        {
            if (this.SelectedNode.Level == MonopolyNode.PROPERTY_MIN_LEVEL)
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageCannotDowngradeMinLevel, PanelMessageBoxUI.Icon.Warning);
            else
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageOnlyEvenBuildingAllowed, PanelMessageBoxUI.Icon.Warning);
        }
        else
        {
            UIManagerMonopolyGame.Instance.HidePanelNodeMenu();

            base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value + this.SelectedNode.PriceDowngrade, GameManager.Instance.SenderLocalClient);
            this.SelectedNode.Downgrade();
        }
    }

    internal void OnSendTradeShown()
    {
        if (PanelSendTradeUI.Instance.PanelDialogResult == PanelSendTradeUI.DialogResult.Offer)
        {
            base.SendTradeServerRpc(PanelSendTradeUI.Instance.Credentials, GameManager.Instance.SenderLocalClient);
            UIManagerMonopolyGame.Instance.HideButtonRollDice();
        }
        else
        {
            this.TradeReceiver = null;
            UIManagerMonopolyGame.Instance.ShowButtonRollDice();
        }

        UIManagerMonopolyGame.Instance.HidePanelSendTrade();
    }

    private protected override sealed void RespondToTrade(TradeCredentials tradeCredentials)
    {
        UIManagerMonopolyGame.Instance.ShowPanelReceiveTrade(tradeCredentials, () => this.OnTradeReceived(tradeCredentials));
    }

    private void OnTradeReceived(TradeCredentials tradeCredentials)
    {
        if (UIManagerMonopolyGame.Instance.PanelReceiveTrade.PanelDialogResult == PanelReceiveTradeUI.DialogResult.Accept)
            base.AcceptTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
        else
            base.DeclineTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
    }

    private protected override sealed void HandleTradeResponse(TradeCredentials tradeCredentials)
    {
        this.TradeReceiver = null;
        UIManagerMonopolyGame.Instance.ShowButtonRollDice();
    }
}
