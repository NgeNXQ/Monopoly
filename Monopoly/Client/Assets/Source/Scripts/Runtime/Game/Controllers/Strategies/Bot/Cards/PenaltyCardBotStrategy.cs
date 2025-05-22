using System;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Concrete;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Bot.Cards
{
    internal sealed class PenaltyCardBotStrategy : ICardStrategy
    {
        public Type CardType { get; } = typeof(PenaltyCard);

        public async void Execute(PawnController pawn, CardScriptableObject card)
        {
            PenaltyCard penaltyCard = card as PenaltyCard;
            BotPawnController botController = pawn as BotPawnController;

            string description = $"{card.Description} {penaltyCard.Penalty}";
            UIManagerGame.Instance.ShowCardInformationToEveryoneNotMe(description);

            await ((BotPawnController)pawn).DelayActionAsync();

            if (pawn.GetBalance() >= penaltyCard.Penalty)
            {
                pawn.TransactDumpBalance(penaltyCard.Penalty);
                pawn.CompleteTurn();
            }
            else
            {
                if (pawn.NetWorth < penaltyCard.Penalty)
                {
                    pawn.Surrender();
                }
                else
                {
                    await botController.RebalancePropertiesToReachBalance(penaltyCard.Penalty);
                    pawn.TransactDumpBalance(penaltyCard.Penalty);
                    pawn.CompleteTurn();
                }
            }
        }
    }
}
