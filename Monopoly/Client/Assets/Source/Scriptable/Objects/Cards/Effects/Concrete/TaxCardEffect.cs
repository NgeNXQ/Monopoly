using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Effects.Common;

namespace Monopoly.Client.Runtime.Game.Cards.Concrete
{
    [CreateAssetMenu(fileName = "New Tax Card Effect", menuName = "Monopoly/Cards/Effects/Tax", order = 1)]
    internal sealed class TaxCardEffect : CardEffectScriptableObject
    {
        internal override sealed void Apply(PawnController pawn)
        {
            
        }
    }
}
