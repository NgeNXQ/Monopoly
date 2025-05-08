using UnityEngine;
using Monopoly.Client.Runtime.UI.Managers;

namespace Monopoly.Client.Scriptable.Objects.Cards
{
    [CreateAssetMenu(fileName = "New Chance card", menuName = "Monopoly/Cards/Chance", order = 1)]
    public sealed class ChanceCardScriptableObject : ScriptableObject
    {
        [SerializeField]
        private Type type;

        [SerializeField]
        private int penalty;

        [SerializeField]
        private int reward;

        [TextArea]
        private string description;

        public enum Type : byte
        {
            Reward,
            Penalty,
            SkipTurn,
            SendJail,
            MoveForward,
            MoveBackwards
        }

        public Type ChanceType => this.type;
        internal int Reward => this.reward;
        internal int Penalty => this.penalty;

        internal string Description
        {
            get
            {
                switch (this.type)
                {
                    case ChanceCardScriptableObject.Type.Reward:
                        return $"{this.description} {UIManagerGame.Instance.Currency}{this.Reward}";
                    case ChanceCardScriptableObject.Type.Penalty:
                        return $"{this.description} {UIManagerGame.Instance.Currency}{this.Penalty}";
                    default:
                        return this.description;
                }
            }
        }
    }
}

