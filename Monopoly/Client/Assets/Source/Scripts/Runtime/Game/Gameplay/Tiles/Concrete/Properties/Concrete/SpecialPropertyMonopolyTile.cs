using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Concrete
{
    internal sealed class SpecialPropertyMonopolyTile : PropertyMonopolyTile
    {
        // [field: SerializeField]
        // internal int[] PriceRents { get; private set; } = new int[3];

        internal sealed override void HandleLanding(PawnController pawn)
        {
        }
    }
}
