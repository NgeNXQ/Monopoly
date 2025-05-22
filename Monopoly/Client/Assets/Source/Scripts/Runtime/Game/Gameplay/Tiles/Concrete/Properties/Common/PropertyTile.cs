using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Gameplay.Boards;
using Monopoly.Client.Runtime.Game.Gameplay.Groups;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common
{
    internal abstract class PropertyTile : Tile
    {
        [SerializeField]
        private protected Image imageOwner;

        [SerializeField]
        private protected Image imageMortgage;

        [SerializeField]
        private protected Image imageMonopoly;

        [SerializeField, Header("Values"), Space]
        private protected int[] pricesRenting;

        [field: SerializeField]
        internal int PricePurchase { get; private set; }

        internal const int LEVEL_MIN = 0;
        internal const int LEVEL_MAX = 6;
        internal const int LEVEL_DEFAULT = 1;
        internal const int LEVEL_MORTGAGE = 0;

        internal int Worth => this.Level * this.PricePurchase;
        internal bool IsMortgaged => this.Level == PropertyTile.LEVEL_MORTGAGE;

        internal int Level { get; private protected set; }
        internal Group Monopoly { get; private protected set; }
        internal PawnController Owner { get; private protected set; }

        internal abstract bool IsTradable { get; }
        internal abstract int PriceRenting { get; }
        internal abstract bool IsUpgradable { get; }
        internal abstract bool IsDowngradable { get; }
        private protected abstract void ResetVisuals();
        private protected abstract void UpdateVisuals();

        private void Start()
        {
            this.Level = PropertyTile.LEVEL_DEFAULT;
            this.Monopoly = Board.Instance.GetMonopolySet(this);
            this.imageMonopoly.color = this.Monopoly.ColorGroup;
        }

        internal void ResetOwnership()
        {
            this.ResetOwnershipRemotelyRpc();
        }

        [Rpc(SendTo.Server)]
        private void ResetOwnershipRemotelyRpc()
        {
            this.ResetOwnershipLocallyRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void ResetOwnershipLocallyRpc()
        {
            this.Level = PropertyTile.LEVEL_DEFAULT;
            this.Owner.OwnedTiles.Remove(this);
            this.Owner = null;

            this.ResetVisuals();
        }

        internal void UpdateOwnership(PawnController newOwner)
        {
            this.UpdateOwnershipRemotelyRpc(newOwner.PawnId);
        }

        [Rpc(SendTo.Server)]
        private void UpdateOwnershipRemotelyRpc(int pawnId)
        {
            this.UpdateOwnershipLocallyRpc(pawnId);
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateOwnershipLocallyRpc(int pawnId)
        {
            this.Owner = GameManager.Instance.GetPawnController(pawnId);
            this.Level = PropertyTile.LEVEL_DEFAULT;
            this.Owner.OwnedTiles.Add(this);

            this.UpdateVisuals();
        }

        internal void Upgrade()
        {
            this.UpgradeRemotelyRpc();
        }

        [Rpc(SendTo.Server)]
        private void UpgradeRemotelyRpc()
        {
            this.UpgradeLocallyRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void UpgradeLocallyRpc()
        {
            this.Level += 1;

            this.UpdateVisuals();
        }

        internal void Downgrade()
        {
            this.DowngradeRemotelyRpc();
        }

        [Rpc(SendTo.Server)]
        private void DowngradeRemotelyRpc()
        {
            this.DowngradeLocallyRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void DowngradeLocallyRpc()
        {
            this.Level -= 1;

            this.UpdateVisuals();
        }
    }
}
