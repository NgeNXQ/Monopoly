using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Concrete
{
    internal sealed class DefaultPropertyMonopolyTile : PropertyMonopolyTile
    {
        [field: SerializeField, Header("Custom"), Space]
        private Image imageMonopolyLevel1;

        [SerializeField]
        private Image imageMonopolyLevel2;

        [SerializeField]
        private Image imageMonopolyLevel3;

        [SerializeField]
        private Image imageMonopolyLevel4;

        [SerializeField]
        private Image imageMonopolyLevel5;

        // [field: SerializeField]
        // internal int[] PriceRentalFees { get; private set; } = new int[5];

        // [field: SerializeField]
        // internal int PriceRentLevel1 { get; private set; }

        // [field: SerializeField]
        // internal int PriceRentLevel2 { get; private set; }

        // [field: SerializeField]
        // internal int PriceRentLevel3 { get; private set; }

        // [field: SerializeField]
        // internal int PriceRentLevel4 { get; private set; }

        // [field: SerializeField]
        // internal int PriceRentLevel5 { get; private set; }

        internal sealed override void HandleLanding(PawnController pawn)
        {
        }
    }
}
