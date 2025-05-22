using System;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared
{
    internal interface ITileStrategy
    {
        Type TileType { get; }

        void Execute(PawnController pawn, Tile tile);
    }
}
