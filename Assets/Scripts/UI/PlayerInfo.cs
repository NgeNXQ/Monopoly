using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerInfo : MonoBehaviour
{ 
    //[SerializeField] private Color colorPlayer;

    [SerializeField] private TMP_Text textPlayerName;

    [SerializeField] private TMP_Text textPlayerBalance;

    //private void Awake() => this.GetComponent<Image>().color = colorPlayer;

    public void SetPlayerName(string name) => this.textPlayerName.text = name;

    public void UpdateBalance(Player player) => this.textPlayerBalance.text = $"₴ {player.Balance}";
}
