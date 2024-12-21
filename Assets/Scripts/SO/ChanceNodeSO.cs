using UnityEngine;

[CreateAssetMenu(fileName = "New Chance card", menuName = "Monopoly/Cards/Chance", order = 1)]
public sealed class ChanceNodeSO : ScriptableObject
{
    [SerializeField]
    private Type type;

    [SerializeField]
    private int penalty;

    [SerializeField]
    private int reward;

    [SerializeField]
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
                case ChanceNodeSO.Type.Reward:
                    return $"{this.description} {UIManagerMonopolyGame.Instance.Currency}{this.Reward}";
                case ChanceNodeSO.Type.Penalty:
                    return $"{this.description} {UIManagerMonopolyGame.Instance.Currency}{this.Penalty}";
                default:
                    return this.description;
            }
        }
    }
}
