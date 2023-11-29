using UnityEngine;

[CreateAssetMenu(fileName = "New Chance card", menuName = "Monopoly/Cards/Chance", order = 1)]
public sealed class SO_ChanceNode : ScriptableObject
{
    public enum ChanceNodeType
    {
        Reward,
        Penalty,
        SkipTurn,
        SendJail,
        MoveBackwards,
        RandomMovement
    }

    [SerializeField] private ChanceNodeType type;

    [SerializeField] private string description;

    [SerializeField] private int penalty;

    [SerializeField] private int reward;

    public ChanceNodeType Type { get => this.type; }

    public string Description { get => this.description; }

    public int Penalty { get => this.penalty; }

    public int Reward { get => this.reward; }
}
