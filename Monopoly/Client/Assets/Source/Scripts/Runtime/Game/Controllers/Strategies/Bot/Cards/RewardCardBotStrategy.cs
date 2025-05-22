using System;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Concrete;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Bot.Cards
{
    internal sealed class RewardCardBotStrategy : ICardStrategy
    {
        public Type CardType { get; } = typeof(RewardCard);

        public async void Execute(PawnController pawn, CardScriptableObject card)
        {
            RewardCard rewardCard = card as RewardCard;

            string description = $"{card.Description} {rewardCard.Reward}";
            UIManagerGame.Instance.ShowCardInformationToEveryoneNotMe(description);

            await ((BotPawnController)pawn).DelayActionAsync();

            pawn.TransactTakeBalance(rewardCard.Reward);
            pawn.CompleteTurn();
        }
    }
}
