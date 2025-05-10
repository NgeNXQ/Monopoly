using Unity.Netcode;
using Monopoly.Client.Runtime.Game.Controllers.Common;

namespace Monopoly.Client.Runtime.Game.Tiles.Common
{
    internal abstract class MonopolyTile : NetworkBehaviour
    {
        // [SerializeField]
        // private Image imageLogo;

        internal abstract void HandleLanding(PawnController pawn);
    }
}
