using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Concrete
{
    internal sealed class DefaultPropertyTile : PropertyTile
    {
        [SerializeField, Header("Custom"), Space]
        private Image imageMonopolyLevel1;

        [SerializeField]
        private Image imageMonopolyLevel2;

        [SerializeField]
        private Image imageMonopolyLevel3;

        [SerializeField]
        private Image imageMonopolyLevel4;

        [SerializeField]
        private Image imageMonopolyLevel5;

        internal override sealed bool IsTradable
        {
            get
            {
                bool hasTileDefaultLevel = base.Level == PropertyTile.LEVEL_DEFAULT;
                bool hasMonopolyDefaultLevel = base.Monopoly.GetLevel(base.Owner) == PropertyTile.LEVEL_DEFAULT;
                return hasTileDefaultLevel && hasMonopolyDefaultLevel;
            }
        }

        internal override sealed int PriceRenting
        {
            get => base.pricesRenting[base.Level];
        }

        // internal override sealed bool IsUpgradable
        // {
        //     get => base.IsMortgaged || base.Level <= base.Monopoly.GetLevel(base.Owner);
        // }

        // internal override sealed bool IsDowngradable
        // {
        //     get => !base.IsMortgaged || base.Level >= base.Monopoly.GetLevel(base.Owner);
        // }

        internal override sealed bool IsUpgradable
        {
            get
            {
                return base.IsMortgaged ||
                       base.Owner.HasFullMonopoly(base.Monopoly) &&
                       base.Level < DefaultPropertyTile.LEVEL_MAX &&
                       base.Level <= base.Monopoly.GetLevel(base.Owner);
            }
        }

        internal override sealed bool IsDowngradable
        {
            get
            {
                return !base.IsMortgaged &&
                        base.Level > PropertyTile.LEVEL_MORTGAGE &&
                        base.Level >= base.Monopoly.GetLevel(base.Owner);
            }
        }

        private protected override sealed void ResetVisuals()
        {
            base.imageOwner.gameObject.SetActive(false);
            base.imageMortgage.gameObject.SetActive(false);
            this.imageMonopolyLevel1.gameObject.SetActive(false);
            this.imageMonopolyLevel2.gameObject.SetActive(false);
            this.imageMonopolyLevel3.gameObject.SetActive(false);
            this.imageMonopolyLevel4.gameObject.SetActive(false);
            this.imageMonopolyLevel5.gameObject.SetActive(false);
        }

        private protected override sealed void UpdateVisuals()
        {
            base.imageOwner.gameObject.SetActive(true);
            base.imageOwner.color = base.Owner.PawnColor;
            base.imageMortgage.gameObject.SetActive(base.IsMortgaged);
            this.imageMonopolyLevel5.gameObject.SetActive(base.Level == DefaultPropertyTile.LEVEL_MAX);
            this.imageMonopolyLevel1.gameObject.SetActive(base.Level > 1 && base.Level < DefaultPropertyTile.LEVEL_MAX);
            this.imageMonopolyLevel2.gameObject.SetActive(base.Level > 2 && base.Level < DefaultPropertyTile.LEVEL_MAX);
            this.imageMonopolyLevel3.gameObject.SetActive(base.Level > 3 && base.Level < DefaultPropertyTile.LEVEL_MAX);
            this.imageMonopolyLevel4.gameObject.SetActive(base.Level > 4 && base.Level < DefaultPropertyTile.LEVEL_MAX);
        }

        internal override sealed void HandleLanding(PawnController pawn)
        {
            pawn.FactoryTileStrategy.Create(this).Execute(pawn, this);
        }
    }
}
