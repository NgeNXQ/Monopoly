using System;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Concrete;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Bot.Cards
{
    internal sealed class TeleportationForwardCardBotStrategy : ICardStrategy
    {
        public Type CardType { get; } = typeof(TeleportationForwardCard);

        public async void Execute(PawnController pawn, CardScriptableObject card)
        {
            UIManagerGame.Instance.ShowCardInformationToEveryoneNotMe(card.Description);

            await ((BotPawnController)pawn).DelayActionAsync();

            GameManager.Instance.RollDice();
            UIManagerGame.Instance.ShowDiceAnimation();
            pawn.MoveToken(GameManager.Instance.TotalRollResult);
        }
    }
}
