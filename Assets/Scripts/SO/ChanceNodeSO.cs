using UnityEngine;

[CreateAssetMenu(fileName = "New Chance card", menuName = "Monopoly/Cards/Chance", order = 1)]
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
