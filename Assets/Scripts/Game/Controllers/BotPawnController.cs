using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

internal sealed class BotPawnController : PawnController
{
    private const float TURN_TIMEOUT_DELAY = 1.0f;

    public override void OnNetworkSpawn()
    {
        this.Nickname = $"BOT_{base.NetworkIndex}";
    }

    internal override sealed async void PerformTurn()
    {
        if (base.IsSkipTurn)
        {
            base.IsSkipTurn = false;
            base.CompleteTurn();
            return;
        }

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
                Debug.Log("upgraded");
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
                await this.ManageProperties(chanceNode.Penalty);

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
                    Debug.Log("partial");

                    if (base.NetWorth >= base.CurrentNode.PricePurchase)
                        await this.ManageProperties(base.CurrentNode.PricePurchase);

                    Debug.Log("managed");

                    base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentNode.PricePurchase, GameManager.Instance.SenderLocalClient);
                    base.CurrentNode.UpdateOwnershipServerRpc(base.NetworkIndex, GameManager.Instance.SenderLocalClient);
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
                    Debug.Log("net worth");

                    base.SurrenderServerRpc(GameManager.Instance.SenderLocalClient);
                }
                else
                {
                    Debug.Log("management");

                    await this.ManageProperties(base.CurrentNode.PriceRent);
                    base.UpdateBalanceServerRpc(base.CurrentNode.Owner.NetworkIndex, base.CurrentNode.Owner.Balance.Value + base.CurrentNode.PriceRent, GameManager.Instance.SenderLocalClient);
                    base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentNode.PriceRent, GameManager.Instance.SenderLocalClient);
                    base.CompleteTurn();
                }
            }
        }
    }

    private async Task ManageProperties(int targetBalance)
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

    private protected override sealed void RespondToTrade(TradeCredentials tradeCredentials)
    {

    }

    private protected override sealed void HandleTradeResponse(TradeCredentials tradeCredentials)
    {

    }
}
