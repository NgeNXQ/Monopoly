using System;
using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Player.Tiles
{
    internal sealed class PropertyTilePlayerStrategy : ITileStrategy
    {
        public Type TileType { get; } = typeof(PropertyTile);

        public void Execute(PawnController pawn, Tile tile)
        {
            PropertyTile currentTile = tile as PropertyTile;

            if (currentTile.Owner == null)
            {
                UIManagerGame.Instance.ShowViewTileProposal(currentTile, () => this.OnTileProposalViewShown(pawn, currentTile));
                return;
            }
            else if (currentTile.Owner == pawn || currentTile.IsMortgaged)
            {
                pawn.CompleteTurn();
                return;
            }

            if (pawn.NetWorth < currentTile.PriceRenting)
            {
                pawn.TransactSendBalance(currentTile.Owner, currentTile.PriceRenting);

                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Error,
                    UIManagerGame.Instance.MessageBankrupt,
                    pawn.Surrender
                );
            }
            else
            {
                UIManagerGame.Instance.ShowViewTilePayment(currentTile, () => this.OnTilePaymentViewShown(pawn, currentTile));
            }
        }

        private void OnTileProposalViewShown(PawnController pawn, PropertyTile tile)
        {
            if (UIManagerGame.Instance.ViewTileProposal.DialogResult == TileProposalView.UserDialogResult.Declined)
            {
                UIManagerGame.Instance.HideViewTileProposal();
                pawn.CompleteTurn();
                return;
            }

            if (pawn.GetBalance() >= tile.PricePurchase)
            {
                UIManagerGame.Instance.HideViewTileProposal();
                pawn.TransactDumpBalance(tile.PricePurchase);
                tile.UpdateOwnership(pawn);
                pawn.CompleteTurn();
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Warning,
                    UIManagerGame.Instance.MessageInsufficientFunds
                );
            }
        }

        private void OnTilePaymentViewShown(PawnController pawn, PropertyTile tile)
        {
            if (pawn.GetBalance() >= tile.PriceRenting)
            {
                UIManagerGame.Instance.HideViewTilePayment();
                pawn.TransactSendBalance(tile.Owner, tile.PriceRenting);
                pawn.CompleteTurn();
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Warning,
                    UIManagerGame.Instance.MessageInsufficientFunds
                );
            }
        }
    }
}
