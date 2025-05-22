using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Concrete
{
    internal sealed class GamblingPropertyTile : PropertyTile
    {
        internal override sealed bool IsTradable
        {
            get => this.Level == PropertyTile.LEVEL_DEFAULT;
        }

        internal override sealed int PriceRenting
        {
            get
            {
                IEnumerable<PropertyTile> tilesOwned = base.Monopoly.GetTilesOwnedBy(base.Owner);
                IEnumerable<PropertyTile> activeTiles = tilesOwned.Where(tile => !tile.IsMortgaged);
                int basePrice = base.pricesRenting[activeTiles.Count()];
                int rollResult = GameManager.Instance.TotalRollResult;

                return basePrice * rollResult;
            }
        }

        internal override sealed bool IsUpgradable
        {
            get => base.IsMortgaged;
        }

        internal override sealed bool IsDowngradable
        {
            get => !base.IsMortgaged;
        }

        private protected override sealed void ResetVisuals()
        {
            base.imageOwner.gameObject.SetActive(false);
            base.imageMortgage.gameObject.SetActive(false);
        }

        private protected override sealed void UpdateVisuals()
        {
            base.imageOwner.gameObject.SetActive(true);
            base.imageOwner.color = base.Owner.PawnColor;
            base.imageMortgage.gameObject.SetActive(base.IsMortgaged);
        }

        internal override sealed void HandleLanding(PawnController pawn)
        {
            pawn.FactoryTileStrategy.Create(this).Execute(pawn, this);
        }
    }
}
