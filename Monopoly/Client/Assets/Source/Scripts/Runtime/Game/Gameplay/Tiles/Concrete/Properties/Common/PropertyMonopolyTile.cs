using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Monopoly.Client.Runtime.Game.Gameplay.Layout;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Controllers.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common
{
    internal abstract class PropertyMonopolyTile : MonopolyTile
    {
        [SerializeField]
        private protected Image imageOwner;

        [SerializeField]
        private protected Image imageMortgage;

        [SerializeField]
        private protected Image imageMonopoly;

        [field: SerializeField, Header("Values"), Space]
        internal int PricePurchase { get; private set; }

        [field: SerializeField]
        internal int[] PricesRenting { get; private set; }

        internal int Level { get; private protected set; }
        internal int Worth { get; private protected set; }
        internal bool IsTradable { get; private protected set; }
        internal int PriceRenting { get; private protected set; }
        internal MonopolySet Monopoly { get; private protected set; }
        internal PawnController Owner { get; private protected set; }

        private void Start()
        {
            this.imageMonopoly.color = MonopolyBoard.Instance.GetMonopolySet(this).ColorSet;
        }

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
