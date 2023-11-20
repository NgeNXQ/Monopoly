using UnityEngine;

public sealed class ChanceNodeSO : MonoBehaviour
{
    [System.Serializable]
    public enum ChanceNodeType
    {
        Reward,
        Penalty,
        SkipTurn,
        SendJail,
        RandomMovement
    }

    [SerializeField] public ChanceNodeType Type;

    [SerializeField] public string Description;

    [SerializeField] public int Penalty;

    [SerializeField] public int Reward;
}
