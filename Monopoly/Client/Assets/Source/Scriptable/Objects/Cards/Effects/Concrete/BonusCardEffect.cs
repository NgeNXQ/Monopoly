using System;
using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Effects.Common;

namespace Monopoly.Client.Scriptable.Objects.Cards.Chance.Effects.Concrete
{
    [CreateAssetMenu(fileName = "New Bonus Card Effect", menuName = "Monopoly/Cards/Effects/Bonus", order = 3)]
    internal sealed class BonusCardEffect : CardEffectScriptableObject
    {
        [SerializeField]
        private int reward;

        internal override sealed void Apply(PawnController pawn)
        {
            (pawn ?? throw new NullReferenceException($"{nameof(pawn)} cannot be null.")).Balance.Value += this.reward;
        }
    }
}
