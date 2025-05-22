using System;
using System.Threading.Tasks;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Bot.Tiles
{
    internal sealed class PropertyTileBotStrategy : ITileStrategy
    {
        public Type TileType { get; } = typeof(PropertyTile);

        public async void Execute(PawnController pawn, Tile tile)
        {
            PropertyTile propertyTile = tile as PropertyTile;
            BotPawnController botController = pawn as BotPawnController;

            if (propertyTile.Owner == pawn || propertyTile.IsMortgaged)
            {
                pawn.CompleteTurn();
                return;
            }

            await botController.DelayActionAsync();

            if (propertyTile.Owner == null)
            {
                await HandleUnownedProperty(propertyTile, botController);
            }
            else
            {
                await HandleOwnedProperty(propertyTile, botController);
            }

            pawn.CompleteTurn();
        }

        private async Task HandleUnownedProperty(PropertyTile tile, BotPawnController bot)
        {
            if (bot.GetBalance() >= tile.PricePurchase)
            {
                this.PurchaseProperty(tile, bot);
            }
            else if (bot.HasPartialMonopoly(tile.Monopoly) && bot.NetWorth >= tile.PricePurchase)
            {
                await bot.RebalancePropertiesToReachBalance(tile.PricePurchase);
                this.PurchaseProperty(tile, bot);
            }
        }

        private void PurchaseProperty(PropertyTile tile, BotPawnController bot)
        {
            bot.TransactDumpBalance(tile.PricePurchase);
            tile.UpdateOwnership(bot);
        }

        private async Task HandleOwnedProperty(PropertyTile tile, BotPawnController bot)
        {
            if (bot.GetBalance() >= tile.PriceRenting)
            {
                bot.TransactSendBalance(tile.Owner, tile.PriceRenting);
            }
            else if (bot.NetWorth >= tile.PriceRenting)
            {
                await bot.RebalancePropertiesToReachBalance(tile.PriceRenting);
                bot.TransactSendBalance(tile.Owner, tile.PriceRenting);
            }
            else
            {
                bot.Surrender();
            }
        }
    }
}
