using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Corners
{
    internal sealed class JailTile : Tile
    {
        internal sealed override void HandleLanding(PawnController pawn)
        {
            pawn.FactoryTileStrategy.Create(this).Execute(pawn, this);
        }
    }
}
