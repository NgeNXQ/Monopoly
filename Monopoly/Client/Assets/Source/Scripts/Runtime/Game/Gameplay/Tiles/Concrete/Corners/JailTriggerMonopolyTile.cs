using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Managers.Gameplay;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Corners
{
    internal sealed class JailTriggerMonopolyTile : MonopolyTile
    {
        internal sealed override void HandleLanding(PawnController pawn)
        {
            if (pawn == null)
                throw new System.NullReferenceException($"{nameof(pawn)} cannot be null.");

            if (pawn.IsPlayer)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Warning,
                    UIManagerGame.Instance.MessageSentJail
                );
            }

            pawn.GoToJail();
        }
    }
}
