using System;
using UnityEngine;
using Monopoly.Client.Scriptable.Objects.Cards.Common;

namespace Monopoly.Client.Scriptable.Objects.Cards.Concrete
{
    [CreateAssetMenu(fileName = "New Penalty Card", menuName = "Monopoly/Cards/Penalty", order = 3)]
    internal sealed class PenaltyCard : CardScriptableObject
    {
        [field: SerializeField, Range(0, Int16.MaxValue)]
        internal int Penalty { get; private set; }
    }
}
