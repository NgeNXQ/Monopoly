using TMPro;
using UnityEngine;

public sealed class PlayerInfo : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerNameText;

    [SerializeField]
    private TMP_Text playerBalanceText;

    internal void SetPlayerName(string name) => this.playerNameText.text = name;

    internal void SetPlayerBalance(int balance) => this.playerBalanceText.text = $"₴ {balance}";
}
