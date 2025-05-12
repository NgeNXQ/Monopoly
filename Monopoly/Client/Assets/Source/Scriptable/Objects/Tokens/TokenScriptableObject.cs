using UnityEngine;

namespace Monopoly.Client.Scriptable.Objects.Tokens
{
    [CreateAssetMenu(fileName = "New Token", menuName = "Monopoly/Tokens", order = 1)]
    internal sealed class TokenScriptableObject : ScriptableObject
    {
        [field: SerializeField]
        internal Color ColorToken { get; private set; }

        [field: SerializeField]
        internal Sprite SpriteToken { get; private set; }
    }
}
