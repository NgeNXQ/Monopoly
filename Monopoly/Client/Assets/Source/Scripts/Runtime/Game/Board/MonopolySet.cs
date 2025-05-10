using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.Game.Board
{
    [System.Serializable]
    internal sealed class MonopolySet
    {
        [field: SerializeField]
        internal Color ColorOfSet { get; private set; }

        [field: SerializeField]
        internal ICollection<PropertyMonopolyTile> Tiles { get; private set; } = new List<PropertyMonopolyTile>();

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
