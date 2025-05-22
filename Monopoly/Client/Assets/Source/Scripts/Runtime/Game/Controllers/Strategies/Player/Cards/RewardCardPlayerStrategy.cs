using System;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Concrete;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Player.Cards
{
    internal sealed class RewardCardPlayerStrategy : ICardStrategy
    {
        public Type CardType { get; } = typeof(RewardCard);

        public void Execute(PawnController pawn, CardScriptableObject card)
        {
            string description = $"{card.Description} {((RewardCard)card).Reward}";

            UIManagerGame.Instance.ShowCardInformationToEveryoneNotMe(description);
            UIManagerGame.Instance.ShowViewCardInformation(description, () => this.OnViewCardInformationShown(pawn, card));
        }

        private void OnViewCardInformationShown(PawnController pawn, CardScriptableObject card)
        {
            if (UIManagerGame.Instance.ViewCardInformation.PanelDialogResult == CardInformationView.DialogResult.Confirmed)
                this.AcceptCardReward(pawn, card);
        }

        private void AcceptCardReward(PawnController pawn, CardScriptableObject card)
        {
            UIManagerGame.Instance.HideViewCardInformation();

            RewardCard rewardCard = card as RewardCard;
            pawn.TransactTakeBalance(rewardCard.Reward);
            pawn.CompleteTurn();
        }
    }
}
