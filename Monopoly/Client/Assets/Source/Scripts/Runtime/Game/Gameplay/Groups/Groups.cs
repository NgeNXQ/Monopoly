using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Groups
{
    [Serializable]
    internal sealed class Group
    {
        [field: SerializeField]
        internal Color ColorGroup { get; private set; }

        [field: SerializeField, Space]
        internal PropertyTile[] Tiles { get; private set; }

        internal int GetLevel(PawnController pawn)
        {
            if (pawn == null)
                throw new ArgumentNullException($"{nameof(pawn)} cannot be null.");

            return this.Tiles.Where(tile => tile.Owner == pawn).Select(tile => tile.Level).Min();
        }

        internal int GetTilesCountOwnedBy(PawnController pawn)
        {
            if (pawn == null)
                throw new ArgumentNullException($"{nameof(pawn)} cannot be null.");

            return this.Tiles.Where(tile => tile.Owner == pawn).Count();
        }

        internal IEnumerable<PropertyTile> GetTilesOwnedBy(PawnController pawn)
        {
            if (pawn == null)
                throw new ArgumentNullException($"{nameof(pawn)} cannot be null.");

            return this.Tiles.Where(tile => tile.Owner == pawn);
        }

        internal bool Contains(PropertyTile tile)
        {
            return this.Tiles.Contains(tile ?? throw new ArgumentNullException($"{nameof(tile)} cannot be null."));
        }
    }
}
