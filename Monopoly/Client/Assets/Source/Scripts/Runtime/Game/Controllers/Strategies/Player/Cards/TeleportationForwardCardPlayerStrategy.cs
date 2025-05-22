using System;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Concrete;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Player.Cards
{
    internal sealed class TeleportationForwardCardPlayerStrategy : ICardStrategy
    {
        public Type CardType { get; } = typeof(TeleportationForwardCard);

        public void Execute(PawnController pawn, CardScriptableObject card)
        {
            UIManagerGame.Instance.ShowCardInformationToEveryoneNotMe(card.Description);
            UIManagerGame.Instance.ShowViewCardInformation(card.Description, () => this.OnViewCardInformationShown(pawn, card));
        }

        private void OnViewCardInformationShown(PawnController pawn, CardScriptableObject card)
        {
            if (UIManagerGame.Instance.ViewCardInformation.PanelDialogResult == CardInformationView.DialogResult.Confirmed)
                this.AcceptTeleportationForward(pawn, card);
        }

        private void AcceptTeleportationForward(PawnController pawn, CardScriptableObject card)
        {
            UIManagerGame.Instance.HideViewCardInformation();

            GameManager.Instance.RollDice();
            UIManagerGame.Instance.ShowDiceAnimation();
            pawn.MoveToken(GameManager.Instance.TotalRollResult);
        }
    }
}
