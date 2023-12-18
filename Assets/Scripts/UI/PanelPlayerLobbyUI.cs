using TMPro;
using UnityEngine;

public sealed class PanelPlayerLobbyUI : MonoBehaviour
{
    #region Visuals

    [Space]
    [Header("Visuals")]
    [Space]

    [SerializeField] private TMP_Text textLabelPlayerNickname;

    #endregion

    public string PlayerNickname { set => this.textLabelPlayerNickname.text = value; }
}
