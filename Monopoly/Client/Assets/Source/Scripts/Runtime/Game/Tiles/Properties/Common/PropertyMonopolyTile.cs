using Unity.Netcode;
using Monopoly.Client.Runtime.Game.Board;
using Monopoly.Client.Runtime.Game.Tiles.Common;
using Monopoly.Client.Runtime.Game.Controllers.Common;

namespace Monopoly.Client.Runtime.Game.Tiles.Properties.Common
{
    internal abstract class PropertyMonopolyTile : MonopolyTile
    {
        // [SerializeField]
        // private Image imageLogo;

        // internal abstract void HandleLanding();

        internal int Level { get; private protected set; }
        internal int Worth { get; private protected set; }
        internal bool IsTradable { get; private protected set; }
        internal MonopolySet Monopoly { get; private protected set; }
        internal PawnController Owner { get; private protected set; }

        internal abstract void Upgrade();
        internal abstract void Downgrade();

        internal void ResetOwnership()
        {
            // if (this.Owner.OwnerClientId == PlayerPawnController.LocalInstance.OwnerClientId)
        }

        [Rpc(SendTo.Server)]
        private void ResetOwnershipRpc()
        {

        }

        internal void UpdateOwnership()
        {

        }

        [Rpc(SendTo.Server)]
        private void UpdateOwnershipRpc()
        {

        }
    }
}
