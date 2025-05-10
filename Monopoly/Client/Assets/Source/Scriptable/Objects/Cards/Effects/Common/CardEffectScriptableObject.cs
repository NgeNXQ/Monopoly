using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Common;

namespace Monopoly.Client.Scriptable.Objects.Cards.Effects.Common
{
    internal abstract class CardEffectScriptableObject : ScriptableObject
    {
        internal abstract void Apply(PawnController pawn);
    }
}
