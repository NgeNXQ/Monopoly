using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Corners
{
    internal sealed class StartTile : Tile
    {
        internal sealed override void HandleLanding(PawnController pawn)
        {
            pawn.FactoryTileStrategy.Create(this).Execute(pawn, this);

            // if (pawn == null)
            //     throw new System.NullReferenceException($"{nameof(pawn)} cannot be null.");

            // pawn.StrategyFactory.Create(this).Execute(pawn, this);
        }
    }
}
