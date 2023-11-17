using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Chance card", menuName = "Monopoly/Cards/Chance")]
public sealed class ChanceNodeUISO : ScriptableObject
{
    [System.Serializable]
    public enum ChanceNodeType // Add pay to player
    {
        Reward,
        Penality,
        GoToJail,
        RandomMovement,
    }

    public ChanceNodeType Type;

    public Button ButtonPay;
    public string Description;
    public Image MonopolyNodeImage;

    public int Reward;
    public int Penalty;
}

