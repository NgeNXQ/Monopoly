using System;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Concrete;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Bot.Cards
{
    internal sealed class JailCardBotStrategy : ICardStrategy
    {
        public Type CardType { get; } = typeof(JailCard);

        public async void Execute(PawnController pawn, CardScriptableObject card)
        {
            UIManagerGame.Instance.ShowCardInformationToEveryoneNotMe(card.Description);
            await ((BotPawnController)pawn).DelayActionAsync();
            pawn.MoveToJail();
        }
    }
}
