using UnityEngine;

[CreateAssetMenu(fileName = "New Chance card", menuName = "Monopoly/Cards/Chance", order = 1)]
public sealed class ChanceNodeSO : ScriptableObject
{
    public enum Type
    {
        Reward,
        Penalty,
        SkipTurn,
        SendJail,
        MoveForward,
        MoveBackwards
    }

    [SerializeField] private Type type;

    [SerializeField] private string description;

    [SerializeField] private int penalty;

    [SerializeField] private int reward;

    public Type ChanceType { get => this.type; }

    public string Description { get => this.description; }

    public int Penalty { get => this.penalty; }

    public int Reward { get => this.reward; }
}
