using UnityEngine;

namespace Monopoly.Client.Scriptable.Objects.Cards.Common
{
    internal abstract class CardScriptableObject : ScriptableObject
    {
        [field: SerializeField, TextArea(3, 5)]
        internal string Description { get; private set; }
    }
}
