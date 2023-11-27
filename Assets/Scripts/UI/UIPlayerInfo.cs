using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;

public sealed class UIPlayerInfo : NetworkBehaviour
{
    [SerializeField] private Image ImageLeftPanel;

    [SerializeField] private Image ImageRightPanel;

    [SerializeField] private TMP_Text textPlayerName;

    [SerializeField] private TMP_Text textPlayerBalance;

    public void SetUpPlayerInfo(FixedString32Bytes nickname, Color color)
    {
        this.textPlayerName.text = nickname.ToString();
        this.ImageLeftPanel.color = this.ImageRightPanel.color = color;
    }

    public void UpdateBalance(Player player) => this.textPlayerBalance.text = $"₴ {player.Balance}";
}
