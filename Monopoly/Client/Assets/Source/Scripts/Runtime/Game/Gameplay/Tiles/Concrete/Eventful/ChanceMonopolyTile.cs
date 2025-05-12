using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Eventful
{
    internal sealed class ChanceMonopolyTile : MonopolyTile
    {
        internal override sealed void HandleLanding(PawnController pawn)
        {
            if (pawn == null)
                throw new System.NullReferenceException($"{nameof(pawn)} cannot be null.");

            if (pawn.IsPlayer)
            {
                // UIManagerGlobal.Instance.ShowMessageBox(
                //     MessageBoxPanel.Type.OK,
                //     MessageBoxPanel.Icon.Warning,
                //     UIManagerGame.Instance.MessageSentChance
                // );
            }
            else
            {

            }

            // pawn.HandleChanceLanding(GameManager.Instance.GetCardChance());
        }
    }
}
