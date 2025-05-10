using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Effects.Common;

namespace Monopoly.Client.Runtime.Game.Cards.Concrete
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Jail Card Effect", menuName = "Monopoly/Cards/Effects/Jail", order = 2)]
    internal sealed class JailCardEffect : CardEffectScriptableObject
    {
        [SerializeField]
        internal int test;

        internal override sealed void Apply(PawnController pawn)
        {
            
        }
    }
}
