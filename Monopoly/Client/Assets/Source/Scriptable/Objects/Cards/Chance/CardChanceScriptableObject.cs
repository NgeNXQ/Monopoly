using UnityEngine;
using Monopoly.Client.Scriptable.Objects.Cards.Effects.Common;

namespace Monopoly.Client.Scriptable.Objects.Cards.Chance
{
    [CreateAssetMenu(fileName = "New Card Chance", menuName = "Monopoly/Cards/Chance", order = 1)]
    internal sealed class CardChanceScriptableObject : ScriptableObject
    {
        [field: SerializeField]
        internal CardEffectScriptableObject Effect { get; private set; }

        [field: SerializeField, TextArea(3, 5)]
        internal string Description { get; private set; }
    }
}
