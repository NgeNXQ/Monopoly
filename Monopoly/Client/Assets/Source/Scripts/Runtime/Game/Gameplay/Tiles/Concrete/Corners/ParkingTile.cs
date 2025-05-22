using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
// using Monopoly.Client.Runtime.UI.Managers;
// using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Corners
{
    internal sealed class ParkingTile : Tile
    {
        internal sealed override void HandleLanding(PawnController pawn)
        {
            pawn.FactoryTileStrategy.Create(this).Execute(pawn, this);

            // UIManagerGlobal.Instance.ShowMessageBox(
            //     MessageBoxPanel.Type.OK,
            //     MessageBoxPanel.Icon.Success,
            //     "Parking"
            // );
        }
    }
}
