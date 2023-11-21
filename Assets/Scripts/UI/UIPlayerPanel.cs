using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;

public sealed class UIPlayerPanel : NetworkBehaviour
{
    [SerializeField] private Image ImageLeftPanel;

    [SerializeField] private Image ImageRightPanel;

    [SerializeField] private TMP_Text textPlayerName;

    [SerializeField] private TMP_Text textPlayerBalance;

    public void SetUpPlayerInfo(FixedString32Bytes name, Color color)
    {
        this.ImageLeftPanel.color = color;
        this.ImageRightPanel.color = color;
        this.textPlayerName.text = name.ToString();
    }

    public void UpdateBalance(Player player) => this.textPlayerBalance.text = $"₴ {player.Balance}";
}
