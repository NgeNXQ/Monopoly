using TMPro;
using UnityEngine;

public sealed class PanelPlayerLobbyUI : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]
    [Space]

    #region Visuals

    [Space]
    [Header("Visuals")]
    [Space]

    [SerializeField] private TMP_Text textLabelPlayerNickname;

    #endregion

    #endregion

    public int PlayerNumber { get; set; }

    public string PlayerNickname 
    {
        set => this.textLabelPlayerNickname.text = value; 
    }
}
