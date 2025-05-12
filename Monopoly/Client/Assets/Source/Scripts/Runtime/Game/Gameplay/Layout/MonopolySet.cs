using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Layout
{
    [System.Serializable]
    internal sealed class MonopolySet
    {
        [field: SerializeField]
        internal Color ColorSet { get; private set; }

        [field: SerializeField, Space]
        internal PropertyMonopolyTile[] Tiles { get; private set; }

        internal int GetLevel(PawnController pawn)
        {
            if (pawn == null)
                throw new System.ArgumentNullException($"{nameof(pawn)} cannot be null.");

            return this.Tiles.Where(tile => tile.Owner == pawn).Select(tile => tile.Level).Max();
        }

        internal IEnumerable<PropertyMonopolyTile> GetTilesOwnedBy(PawnController pawn)
        {
            if (pawn == null)
                throw new System.ArgumentNullException($"{nameof(pawn)} cannot be null.");

            return this.Tiles.Where(tile => tile.Owner == pawn);
        }

        internal bool Contains(PropertyMonopolyTile tile)
        {
            return this.Tiles.Contains(tile ?? throw new System.ArgumentNullException($"{nameof(tile)} cannot be null."));
        }
    }
}
