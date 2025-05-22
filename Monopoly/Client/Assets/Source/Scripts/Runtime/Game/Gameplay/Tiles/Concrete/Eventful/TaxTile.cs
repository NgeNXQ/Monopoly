using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Eventful
{
    internal sealed class TaxTile : Tile
    {
        internal sealed override void HandleLanding(PawnController pawn)
        {
            CardScriptableObject card = GameManager.Instance.GetCardTax();
            pawn.FactoryCardStrategy.Create(card).Execute(pawn, card);
        }
    }
}
