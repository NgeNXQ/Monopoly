using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.UI.Managers;
using Monopoly.Client.Runtime.Game.Models;
using Monopoly.Client.Runtime.Game.Managers;
// using Monopoly.Client.Runtime.Game.Gameplay.Boards;
// using Monopoly.Client.Runtime.Game.Gameplay.Groups;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;
using Monopoly.Client.Scriptable.Objects.Cards;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Bot.Factories;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.Game.Gameplay.Boards;

namespace Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete
{
    internal sealed class BotPawnController : PawnController
    {
        private bool isAbleToRollDice;
        private int previousBalanceOffer;

        public override void OnNetworkSpawn()
        {
            base.IsBot = true;
            base.IsPlayer = false;
            base.FactoryTileStrategy = new BotTileStrategyFactory();
            base.FactoryCardStrategy = new BotCardStrategyFactory();
            base.Nickname = $"BOT_{base.PawnId - LobbyManager.Instance.LocalLobby.Players.Count + 1}";
        }

        internal async Task DelayActionAsync()
        {
            await Awaitable.WaitForSecondsAsync(base.turnDelay);
        }

        internal override sealed async void PerformTurn()
        {
            // if (base.IsSkipTurn)
            // {
            //     base.IsSkipTurn = false;
            //     // base.CompleteTurn();
            //     return;
            // }

            await Awaitable.WaitForSecondsAsync(base.turnDelay);

            this.isAbleToRollDice = true;

            foreach (PropertyTile tile in base.OwnedTiles)
            {
                if (!base.HasPartialMonopoly(tile.Monopoly))
                    continue;

                PropertyTile targetTile = tile.Monopoly.Tiles.FirstOrDefault(tile => tile.Owner != null && tile.Owner != this && tile.IsTradable);

                if (targetTile == null)
                    continue;

                TradeCredentialsSerializable credentials = this.GenerateTradeCredentials(targetTile);

                if (credentials != TradeCredentialsSerializable.Blank)
                {
                    await Awaitable.WaitForSecondsAsync(base.turnDelay);

                    base.SendTrade(credentials);
                    this.isAbleToRollDice = false;
                    base.StartCoroutine(this.WaitTradeResponse());
                    break;
                }
            }

            if (this.isAbleToRollDice)
                this.PerformTurnLogic();
        }

        private TradeCredentialsSerializable GenerateTradeCredentials(PropertyTile targetTile)
        {
            const int STRATEGIES_COUNT = 3;
            const int MAX_STRATEGIES_CHANGE_COUNT = 3;

            const int STRATEGY_TILE = 0;
            const int STRATEGY_BALANCE = 1;
            const int STRATEGY_TILE_AND_BALANCE = 2;

            TradeCredentialsSerializable credentials = new TradeCredentialsSerializable()
            {
                SenderPawnId = base.PawnId,
                ReceiverPawnId = targetTile.Owner.PawnId,
                ReceiverTileIndex = Board.Instance.GetIndexOfTile(targetTile)
            };

            int strategyChangesCount = 0;
            int strategyChoice = Random.Range(0, STRATEGIES_COUNT);

            switch (strategyChoice)
            {
                case STRATEGY_TILE:
                    ++strategyChangesCount;

                    if (strategyChangesCount > MAX_STRATEGIES_CHANGE_COUNT)
                        return TradeCredentialsSerializable.Blank;

                    PropertyTile strategyTileSelectedTile = base.OwnedTiles
                        .Where(tile => tile.Monopoly != targetTile.Monopoly
                            && targetTile.IsTradable
                            && targetTile.Owner.OwnedTiles.Any(ownerNode => ownerNode.Monopoly == tile.Monopoly))
                        .OrderBy(tile => UnityEngine.Random.value)
                        .FirstOrDefault();

                    if (strategyTileSelectedTile == null)
                        goto case STRATEGY_BALANCE;

                    credentials.SenderBalanceAmount = 0;
                    credentials.ReceiverBalanceAmount = 0;
                    credentials.SenderTileIndex = Board.Instance.GetIndexOfTile(strategyTileSelectedTile);
                    break;
                case STRATEGY_BALANCE:
                    ++strategyChangesCount;

                    if (strategyChangesCount > MAX_STRATEGIES_CHANGE_COUNT)
                        return TradeCredentialsSerializable.Blank;

                    float balanceScaler = Random.Range(1.0f, 2.0f);
                    int balanceOffer = (int)(targetTile.PricePurchase * balanceScaler);

                    if (base.GetBalance() < balanceOffer || this.previousBalanceOffer > balanceOffer)
                        goto case STRATEGY_TILE_AND_BALANCE;

                    this.previousBalanceOffer = balanceOffer;
                    credentials.ReceiverBalanceAmount = 0;
                    credentials.SenderBalanceAmount = balanceOffer;
                    credentials.SenderTileIndex = TradeCredentialsSerializable.PLACEHOLDER;
                    break;
                case STRATEGY_TILE_AND_BALANCE:
                    ++strategyChangesCount;

                    if (strategyChangesCount > MAX_STRATEGIES_CHANGE_COUNT)
                        return TradeCredentialsSerializable.Blank;

                    PropertyTile strategyNodeAndBalanceNode = base.OwnedTiles
                        .Where(tile => tile.Monopoly != targetTile.Monopoly && targetTile.IsTradable)
                        .OrderBy(tile => UnityEngine.Random.value)
                        .FirstOrDefault();

                    if (strategyNodeAndBalanceNode == null)
                        goto case STRATEGY_BALANCE;

                    float tileAndBalanceScaler = Random.Range(1.0f, 1.25f);
                    int tileAndBalanceOffer = (int)(targetTile.PricePurchase * tileAndBalanceScaler);

                    if (base.Balance.Value < tileAndBalanceOffer || this.previousBalanceOffer > tileAndBalanceOffer)
                        goto case STRATEGY_TILE;

                    this.previousBalanceOffer = tileAndBalanceOffer;
                    credentials.ReceiverBalanceAmount = 0;
                    credentials.SenderBalanceAmount = tileAndBalanceOffer;
                    credentials.SenderTileIndex = Board.Instance.GetIndexOfTile(strategyNodeAndBalanceNode);
                    break;
            }

            return credentials;
        }

        internal override sealed async void RespondToTrade(TradeCredentialsSerializable credentials)
        {
            await Awaitable.WaitForSecondsAsync(base.turnDelay);

            const float WORTH_SENT_FULL_MONOPOLY_RATIO_THRESHOLD = 2.25f;
            const float WORTH_RECEIVED_FULL_MONOPOLY_RATIO_THRESHOLD = 1.5f;

            const float WORTH_SENT_PARTIAL_MONOPOLY_RATIO_THRESHOLD = 1.7f;
            const float WORTH_RECEIVED_PARTIAL_MONOPOLY_RATIO_THRESHOLD = 1.5f;

            PropertyTile tileToSend = null;
            PropertyTile tileToReceive = null;
            bool willSenderHaveFullMonopoly = false;
            bool willReceiverHaveFullMonopoly = false;
            bool willSenderHavePartialMonopoly = false;
            bool willReceiverHavePartialMonopoly = false;

            if (credentials.ReceiverTileIndex != TradeCredentialsSerializable.PLACEHOLDER)
            {
                tileToSend = Board.Instance.GetTileByIndex(credentials.ReceiverTileIndex) as PropertyTile;
                PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderPawnId);
                willSenderHavePartialMonopoly = sender.OwnedTiles.Count(tile => tile.Monopoly == tileToSend.Monopoly) + 1 > 1;
                willSenderHaveFullMonopoly = sender.OwnedTiles.Count(tile => tile.Monopoly == tileToSend.Monopoly) + 1 == tileToSend.Monopoly.Tiles.Length;
            }

            if (credentials.SenderTileIndex != TradeCredentialsSerializable.PLACEHOLDER)
            {
                tileToReceive = Board.Instance.GetTileByIndex(credentials.SenderTileIndex) as PropertyTile;
                willReceiverHavePartialMonopoly = this.OwnedTiles.Count(tile => tile.Monopoly == tileToReceive.Monopoly) + 1 > 1;
                willReceiverHaveFullMonopoly = this.OwnedTiles.Count(tile => tile.Monopoly == tileToReceive.Monopoly) + 1 == tileToReceive.Monopoly.Tiles.Length;
            }

            int worthToSend = credentials.ReceiverBalanceAmount + (tileToSend?.PricePurchase ?? 0);
            int worthToReceive = credentials.SenderBalanceAmount + (tileToReceive?.PricePurchase ?? 0);

            if (worthToSend == 0)
            {
                base.AcceptTrade(credentials);
                return;
            }

            if (worthToReceive == 0)
            {
                base.DeclineTrade(credentials);
                return;
            }

            if (tileToSend != null)
            {
                if (base.OwnedTiles.Where(tile => tile.Monopoly == tileToSend.Monopoly).Count() == tileToSend.Monopoly.Tiles.Length)
                {
                    base.DeclineTrade(credentials);
                    return;
                }
            }

            if (willReceiverHaveFullMonopoly)
            {
                if (((float)worthToSend / worthToReceive) <= WORTH_SENT_FULL_MONOPOLY_RATIO_THRESHOLD)
                    base.AcceptTrade(credentials);
                else
                    base.DeclineTrade(credentials);
                return;
            }

            if (willSenderHaveFullMonopoly)
            {
                if (((float)worthToReceive / worthToSend) >= WORTH_RECEIVED_FULL_MONOPOLY_RATIO_THRESHOLD)
                    base.AcceptTrade(credentials);
                else
                    base.DeclineTrade(credentials);
                return;
            }

            if (willReceiverHavePartialMonopoly)
            {
                if (((float)worthToSend / worthToReceive) <= WORTH_SENT_PARTIAL_MONOPOLY_RATIO_THRESHOLD)
                    base.AcceptTrade(credentials);
                else
                    base.DeclineTrade(credentials);
                return;
            }

            if (willSenderHavePartialMonopoly)
            {
                if (((float)worthToReceive / worthToSend) >= WORTH_RECEIVED_PARTIAL_MONOPOLY_RATIO_THRESHOLD)
                    base.AcceptTrade(credentials);
                else
                    base.DeclineTrade(credentials);
                return;
            }

            if (worthToReceive > worthToSend)
                base.AcceptTrade(credentials);
            else
                base.DeclineTrade(credentials);
        }

        internal override sealed void HandleTradeResponse(TradeCredentialsSerializable credentials)
        {
            if (credentials.Result == TradeResult.Accepted)
                this.previousBalanceOffer = 0;

            this.isAbleToRollDice = true;
        }

        private IEnumerator WaitTradeResponse()
        {
            yield return new WaitUntil(() => this.isAbleToRollDice);
            this.PerformTurnLogic();
        }

        private async void PerformTurnLogic()
        {
            base.OwnedTiles.Sort((tile1, tile2) => tile2.PricePurchase.CompareTo(tile1.PricePurchase));

            foreach (PropertyTile tile in base.OwnedTiles)
            {
                if (!base.HasFullMonopoly(tile.Monopoly))
                    continue;

                if (!tile.IsUpgradable)
                    continue;

                if (base.GetBalance() >= tile.PricePurchase)
                {
                    base.TransactDumpBalance(tile.PricePurchase);
                    tile.Upgrade();
                    break;
                }
            }

            await Awaitable.WaitForSecondsAsync(base.turnDelay);

            base.RollDice();
        }

        internal async Task RebalancePropertiesToReachBalance(int targetBalance)
        {
            while (base.GetBalance() < targetBalance)
            {
                await Awaitable.WaitForSecondsAsync(base.turnDelay);

                base.OwnedTiles.Sort((tile1, tile2) => tile1.PricePurchase.CompareTo(tile2.PricePurchase));

                PropertyTile selectedTile = base.OwnedTiles.Where(tile => tile.IsDowngradable).First();

                base.TransactTakeBalance(selectedTile.PricePurchase);
                selectedTile.Downgrade();
            }
        }
    }
}
