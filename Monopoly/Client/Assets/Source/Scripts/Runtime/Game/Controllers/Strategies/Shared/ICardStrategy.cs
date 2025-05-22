using System;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Common;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared
{
    internal interface ICardStrategy
    {
        Type CardType { get; }

        void Execute(PawnController pawn, CardScriptableObject card);
    }
}
