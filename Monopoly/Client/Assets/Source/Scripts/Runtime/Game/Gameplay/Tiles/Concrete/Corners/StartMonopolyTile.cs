using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Corners
{
    internal sealed class StartMonopolyTile : MonopolyTile
    {
        internal sealed override void HandleLanding(PawnController pawn)
        {
            if (pawn == null)
                throw new System.NullReferenceException($"{nameof(pawn)} cannot be null.");

            pawn.UpdateBalance(GameManager.Instance.CircleSuperBonus);
        }
    }
}
