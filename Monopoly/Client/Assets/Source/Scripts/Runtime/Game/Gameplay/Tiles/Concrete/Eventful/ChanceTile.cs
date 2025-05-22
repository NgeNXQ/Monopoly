using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Eventful
{
    internal sealed class ChanceTile : Tile
    {
        internal override sealed void HandleLanding(PawnController pawn)
        {
            CardScriptableObject card = GameManager.Instance.GetCardChance();
            pawn.FactoryCardStrategy.Create(card).Execute(pawn, card);
        }
    }
}
