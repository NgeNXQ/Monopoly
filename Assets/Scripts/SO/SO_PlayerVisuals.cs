using UnityEngine;

[CreateAssetMenu(fileName = "New Player Visuals", menuName = "Monopoly/Create new Player Visuals", order = 1)]
public sealed class SO_PlayerVisuals : ScriptableObject
{
    [Space]
    [Header("Visuals")]
    [Space]

    [SerializeField] public Color PlayerColor;

    [SerializeField] public string PlayerNickname;

    [SerializeField] public GameObject PlayerToken;

    //[SerializeField] public UIPlayerPanel PlayerPanel;
}
