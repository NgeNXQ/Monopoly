using System;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Corners;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Mutual
{
    internal sealed class ParkingMutualTileStrategy : ITileStrategy
    {
        public Type TileType { get; } = typeof(ParkingTile);

        public void Execute(PawnController pawn, Tile tile)
        {
            pawn.CompleteTurn();
        }
    }
}
