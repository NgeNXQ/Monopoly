using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

internal sealed class BotPawnController : PawnController
{
    private const float TURN_TIMEOUT_DELAY = 1.0f;

    private bool isAbleToRollDice;

    public override void OnNetworkSpawn()
    {
        this.Nickname = $"BOT_{base.NetworkIndex - LobbyManager.Instance.LocalLobby.Players.Count + 1}";
    }

    internal override sealed async void PerformTurn()
    {
        await Awaitable.WaitForSecondsAsync(PawnController.TURN_DELAY);

        if (base.IsSkipTurn)
        {
            base.IsSkipTurn = false;
            base.CompleteTurn();
            return;
        }

        this.isAbleToRollDice = true;

        foreach (MonopolyNode node in base.OwnedNodes)
        {
            if (!base.HasPartialMonopoly(node.AffiliatedMonopoly))
                continue;

            MonopolyNode targetNode = node.AffiliatedMonopoly.NodesInSet.FirstOrDefault(node => node.Owner != null && node.Owner != this && node.IsTradable);

            if (targetNode == null)
                continue;

            TradeCredentials credentials = this.GenerateTradeCredentials(targetNode);

            if (credentials != TradeCredentials.Blank)
            {
                await Awaitable.WaitForSecondsAsync(BotPawnController.TURN_TIMEOUT_DELAY);

                this.isAbleToRollDice = false;
                base.SendTradeServerRpc(credentials, GameManager.Instance.SenderLocalClient);
                base.StartCoroutine(this.WaitTradeResponse());
                break;
            }
        }

        if (this.isAbleToRollDice)
            this.PerformTurnLogic();
    }

    private TradeCredentials GenerateTradeCredentials(MonopolyNode targetNode)
    {
        const int STRATEGIES_COUNT = 3;
        const int MAX_STRATEGIES_CHANGE_COUNT = 3;

        const int STRATEGY_NODE = 0;
        const int STRATEGY_BALANCE = 1;
        const int STRATEGY_NODE_AND_BALANCE = 2;

        TradeCredentials credentials = new TradeCredentials()
        {
            SenderNetworkIndex = base.NetworkIndex,
            ReceiverNetworkIndex = targetNode.Owner.NetworkIndex,
            ReceiverNodeIndex = MonopolyBoard.Instance.GetIndexOfNode(targetNode)
        };

        int strategiesChangeCount = 0;
        int strategyChoice = Random.Range(0, STRATEGIES_COUNT);

        switch (strategyChoice)
        {
            case STRATEGY_NODE:
                ++strategiesChangeCount;

                if (strategiesChangeCount > MAX_STRATEGIES_CHANGE_COUNT)
                    return TradeCredentials.Blank;

                MonopolyNode strategyNodeSelectedNode = base.OwnedNodes.Where(node => node.AffiliatedMonopoly != targetNode.AffiliatedMonopoly && targetNode.IsTradable
                && targetNode.Owner.OwnedNodes.Any(ownerNode => ownerNode.AffiliatedMonopoly == node.AffiliatedMonopoly)).FirstOrDefault();

                if (strategyNodeSelectedNode == null)
                    goto case STRATEGY_BALANCE;

                credentials.SenderBalanceAmount = 0;
                credentials.ReceiverBalanceAmount = 0;
                credentials.SenderNodeIndex = MonopolyBoard.Instance.GetIndexOfNode(strategyNodeSelectedNode);
                break;
            case STRATEGY_BALANCE:
                ++strategiesChangeCount;

                if (strategiesChangeCount > MAX_STRATEGIES_CHANGE_COUNT)
                    return TradeCredentials.Blank;

                float balanceScaler = Random.Range(1.0f, 2.0f);
                int balanceOffer = (int)(targetNode.PricePurchase * balanceScaler);

                if (base.Balance.Value < balanceOffer)
                    goto case STRATEGY_NODE_AND_BALANCE;

                credentials.ReceiverBalanceAmount = 0;
                credentials.SenderBalanceAmount = balanceOffer;
                credentials.SenderNodeIndex = TradeCredentials.PLACEHOLDER;
                break;
            case STRATEGY_NODE_AND_BALANCE:
                ++strategiesChangeCount;

                if (strategiesChangeCount > MAX_STRATEGIES_CHANGE_COUNT)
                    return TradeCredentials.Blank;

                MonopolyNode strategyNodeAndBalanceNode = base.OwnedNodes.Where(node => node.AffiliatedMonopoly != targetNode.AffiliatedMonopoly && targetNode.IsTradable).FirstOrDefault();

                if (strategyNodeAndBalanceNode == null)
                    goto case STRATEGY_BALANCE;

                float nodeAndBalanceScaler = Random.Range(1.0f, 1.25f);
                int nodeAndBalanceOffer = (int)(targetNode.PricePurchase * nodeAndBalanceScaler);

                if (base.Balance.Value < nodeAndBalanceOffer)
                    goto case STRATEGY_NODE;

                credentials.ReceiverBalanceAmount = 0;
                credentials.SenderBalanceAmount = nodeAndBalanceOffer;
                credentials.SenderNodeIndex = TradeCredentials.PLACEHOLDER;
                break;
        }

        return credentials;
    }

    private IEnumerator WaitTradeResponse()
    {
        yield return new WaitUntil(() => this.isAbleToRollDice);
        this.PerformTurnLogic();
    }

    private async void PerformTurnLogic()
    {
        base.OwnedNodes.Sort((x, y) => y.PriceUpgrade.CompareTo(x.PriceUpgrade));

        foreach (MonopolyNode monopolyNode in base.OwnedNodes)
        {
            if (!base.HasFullMonopoly(monopolyNode.AffiliatedMonopoly))
                continue;

            if (!monopolyNode.IsUpgradable)
                continue;

            if (base.Balance.Value >= monopolyNode.PriceUpgrade)
            {
                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - monopolyNode.PriceUpgrade, GameManager.Instance.SenderLocalClient);
                monopolyNode.Upgrade();
                break;
            }
        }

        await Awaitable.WaitForSecondsAsync(BotPawnController.TURN_TIMEOUT_DELAY);

        GameManager.Instance.RollDice();
        UIManagerMonopolyGame.Instance.ShowDiceAnimation();

        base.PerformDiceRolling();
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

    private protected override sealed void HandleSendJailLanding()
    {
        base.GoToJail();
    }

    private protected override sealed void HandleFreeParkingLanding()
    {
        base.CompleteTurn();
    }

    private protected override sealed async void HandleChanceLanding()
    {
        ChanceNodeSO chanceNode = MonopolyBoard.Instance.GetChanceNode();
        UIManagerMonopolyGame.Instance.ShowPanelInfoServerRpc(chanceNode.Description, GameManager.Instance.SenderLocalClient);

        await Awaitable.WaitForSecondsAsync(BotPawnController.TURN_TIMEOUT_DELAY);

        if (chanceNode.ChanceType != ChanceNodeSO.Type.Penalty)
        {
            base.PerformChanceAction(chanceNode);
            return;
        }

        if (base.Balance.Value >= chanceNode.Penalty)
        {
            base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - chanceNode.Penalty, GameManager.Instance.SenderLocalClient);
            base.CompleteTurn();
        }
        else
        {
            if (base.NetWorth < chanceNode.Penalty)
            {
                base.SurrenderServerRpc(GameManager.Instance.SenderLocalClient);
            }
            else
            {
                await this.ManagePropertiesToAvoidBankruptcy(chanceNode.Penalty);

                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - chanceNode.Penalty, GameManager.Instance.SenderLocalClient);
                base.CompleteTurn();
            }
        }
    }

    private protected override sealed async void HandlePropertyLanding()
    {
        await Awaitable.WaitForSecondsAsync(BotPawnController.TURN_TIMEOUT_DELAY);

        if (base.CurrentNode.Owner == null)
        {
            if (base.Balance.Value >= base.CurrentNode.PricePurchase)
            {
                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentNode.PricePurchase, GameManager.Instance.SenderLocalClient);
                base.CurrentNode.UpdateOwnershipServerRpc(base.NetworkIndex, GameManager.Instance.SenderLocalClient);
                base.CompleteTurn();
            }
            else
            {
                if (base.HasPartialMonopoly(base.CurrentNode.AffiliatedMonopoly))
                {
                    if (base.NetWorth >= base.CurrentNode.PricePurchase)
                    {
                        await this.ManagePropertiesToAvoidBankruptcy(base.CurrentNode.PricePurchase);

                        base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentNode.PricePurchase, GameManager.Instance.SenderLocalClient);
                        base.CurrentNode.UpdateOwnershipServerRpc(base.NetworkIndex, GameManager.Instance.SenderLocalClient);
                    }
                }

                base.CompleteTurn();
            }
        }
        else
        {
            if (base.CurrentNode.Owner == this || base.CurrentNode.IsMortgaged)
            {
                base.CompleteTurn();
                return;
            }

            if (base.Balance.Value >= base.CurrentNode.PriceRent)
            {
                base.UpdateBalanceServerRpc(base.CurrentNode.Owner.NetworkIndex, base.CurrentNode.Owner.Balance.Value + base.CurrentNode.PriceRent, GameManager.Instance.SenderLocalClient);
                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentNode.PriceRent, GameManager.Instance.SenderLocalClient);
                base.CompleteTurn();
            }
            else
            {
                if (base.NetWorth < base.CurrentNode.PriceRent)
                {
                    base.SurrenderServerRpc(GameManager.Instance.SenderLocalClient);
                }
                else
                {
                    await this.ManagePropertiesToAvoidBankruptcy(base.CurrentNode.PriceRent);

                    base.UpdateBalanceServerRpc(base.CurrentNode.Owner.NetworkIndex, base.CurrentNode.Owner.Balance.Value + base.CurrentNode.PriceRent, GameManager.Instance.SenderLocalClient);
                    base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentNode.PriceRent, GameManager.Instance.SenderLocalClient);
                    base.CompleteTurn();
                }
            }
        }
    }

    private async Task ManagePropertiesToAvoidBankruptcy(int targetBalance)
    {
        while (base.Balance.Value < targetBalance)
        {
            await Awaitable.WaitForSecondsAsync(BotPawnController.TURN_TIMEOUT_DELAY);

            base.OwnedNodes.Sort((x, y) => x.PriceRent.CompareTo(y.PriceRent));

            MonopolyNode selectedNode = base.OwnedNodes.Where(node => node.IsDowngradable).First();

            base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value + selectedNode.PriceDowngrade, GameManager.Instance.SenderLocalClient);
            selectedNode.Downgrade();
        }
    }

    private protected override sealed async void RespondToTrade(TradeCredentials tradeCredentials)
    {
        await Awaitable.WaitForSecondsAsync(BotPawnController.TURN_TIMEOUT_DELAY);

        const float WORTH_SENT_FULL_MONOPOLY_RATIO_THRESHOLD = 2.5f;
        const float WORTH_RECEIVED_FULL_MONOPOLY_RATIO_THRESHOLD = 1.5f;

        const float WORTH_SENT_PARTIAL_MONOPOLY_RATIO_THRESHOLD = 1.7f;
        const float WORTH_RECEIVED_PARTIAL_MONOPOLY_RATIO_THRESHOLD = 1.5f;

        MonopolyNode nodeToSend = null;
        MonopolyNode nodeToReceive = null;
        bool willSenderHaveFullMonopoly = false;
        bool willReceiverHaveFullMonopoly = false;
        bool willSenderHavePartialMonopoly = false;
        bool willReceiverHavePartialMonopoly = false;

        if (tradeCredentials.ReceiverNodeIndex != TradeCredentials.PLACEHOLDER)
        {
            nodeToSend = MonopolyBoard.Instance.GetNodeByIndex(tradeCredentials.ReceiverNodeIndex);
            PawnController sender = GameManager.Instance.GetPawnController(tradeCredentials.SenderNetworkIndex);
            willSenderHavePartialMonopoly = sender.OwnedNodes.Count(node => node.AffiliatedMonopoly == nodeToSend.AffiliatedMonopoly) + 1 > 1;
            willSenderHaveFullMonopoly = sender.OwnedNodes.Count(node => node.AffiliatedMonopoly == nodeToSend.AffiliatedMonopoly) + 1 == nodeToSend.AffiliatedMonopoly.NodesCount;
        }

        if (tradeCredentials.SenderNodeIndex != TradeCredentials.PLACEHOLDER)
        {
            nodeToReceive = MonopolyBoard.Instance.GetNodeByIndex(tradeCredentials.SenderNodeIndex);
            willSenderHavePartialMonopoly = this.OwnedNodes.Count(node => node.AffiliatedMonopoly == nodeToReceive.AffiliatedMonopoly) + 1 > 1;
            willReceiverHaveFullMonopoly = this.OwnedNodes.Count(node => node.AffiliatedMonopoly == nodeToReceive.AffiliatedMonopoly) + 1 == nodeToReceive.AffiliatedMonopoly.NodesCount;
        }

        int worthToSend = tradeCredentials.ReceiverBalanceAmount + (nodeToSend?.PricePurchase ?? 0);
        int worthToReceive = tradeCredentials.SenderBalanceAmount + (nodeToReceive?.PricePurchase ?? 0);

        if (worthToSend == 0)
        {
            base.AcceptTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            return;
        }

        if (worthToReceive == 0)
        {
            base.DeclineTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            return;
        }

        if (willReceiverHaveFullMonopoly)
        {
            if (((float)worthToSend / worthToReceive) <= WORTH_SENT_FULL_MONOPOLY_RATIO_THRESHOLD)
                base.AcceptTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            else
                base.DeclineTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            return;
        }

        if (willSenderHaveFullMonopoly)
        {
            if (((float)worthToReceive / worthToSend) >= WORTH_RECEIVED_FULL_MONOPOLY_RATIO_THRESHOLD)
                base.AcceptTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            else
                base.DeclineTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            return;
        }

        if (willReceiverHavePartialMonopoly)
        {
            if (((float)worthToSend / worthToReceive) <= WORTH_SENT_PARTIAL_MONOPOLY_RATIO_THRESHOLD)
                base.AcceptTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            else
                base.DeclineTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            return;
        }

        if (willSenderHavePartialMonopoly)
        {
            if (((float)worthToReceive / worthToSend) >= WORTH_RECEIVED_PARTIAL_MONOPOLY_RATIO_THRESHOLD)
                base.AcceptTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            else
                base.DeclineTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
            return;
        }

        if (worthToReceive > worthToSend)
            base.AcceptTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
        else
            base.DeclineTradeServerRpc(tradeCredentials, GameManager.Instance.SenderLocalClient);
    }

    private protected override sealed void HandleTradeResponse(TradeCredentials tradeCredentials)
    {
        this.isAbleToRollDice = true;
    }
}
