using System;
using UnityEngine;
using Monopoly.Client.Scriptable.Objects.Cards.Common;

namespace Monopoly.Client.Scriptable.Objects.Cards.Concrete
{
    [CreateAssetMenu(fileName = "New Reward Card", menuName = "Monopoly/Cards/Reward", order = 4)]
    internal sealed class RewardCard : CardScriptableObject
    {
        [field: SerializeField, Range(0, Int16.MaxValue)]
        internal int Reward { get; private set; }
    }
}
