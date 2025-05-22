using System;
using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Concrete;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Player.Cards
{
    internal sealed class JailCardPlayerStrategy : ICardStrategy
    {
        public Type CardType { get; } = typeof(JailCard);

        public void Execute(PawnController pawn, CardScriptableObject card)
        {
            UIManagerGame.Instance.ShowCardInformationToEveryoneNotMe(card.Description);
            UIManagerGame.Instance.ShowViewCardInformation(card.Description, () => this.OnViewCardInformationShown(pawn, card));
        }

        private void OnViewCardInformationShown(PawnController pawn, CardScriptableObject card)
        {
            if (UIManagerGame.Instance.ViewCardInformation.PanelDialogResult == CardInformationView.DialogResult.Confirmed)
            {
                UIManagerGame.Instance.HideViewCardInformation();
                pawn.MoveToJail();
            }
        }
    }
}
