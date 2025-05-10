using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Effects.Common;

namespace Monopoly.Client.Runtime.Game.Cards.Concrete
{
    [CreateAssetMenu(fileName = "New Turn Skip Card Effect", menuName = "Monopoly/Cards/Effects/Turn Skip", order = 4)]
    internal sealed class TurnSkipCardEffect : CardEffectScriptableObject
    {
        internal override sealed void Apply(PawnController pawn)
        {
            
        }
    }
}
