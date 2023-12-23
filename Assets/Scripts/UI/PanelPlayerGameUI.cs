using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public sealed class PanelPlayerGameUI : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]

    #region Visuals

    [Header("Visuals")]

    [Space]
    [SerializeField] private Image ImageLeftPanel;

    [Space]
    [SerializeField] private Image ImageRightPanel;

    [Space]
    [SerializeField] private TMP_Text textPlayerName;

    [Space]
    [SerializeField] private TMP_Text textPlayerBalance;

    #endregion

    #region Controls

    [Header("Controls")]

    [Space]
    [SerializeField] private Button buttonInteract;

    #endregion

    #endregion

    private MonopolyPlayer associatedPlayer;

    private void OnEnable()
    {
        this.buttonInteract.onClick.AddListener(this.HandleBttonInteractClicked);
    }

    private void OnDisable()
    {
        this.buttonInteract.onClick.RemoveListener(this.HandleBttonInteractClicked);
    }

    public void InitializePanel(MonopolyPlayer player)
    {
        this.textPlayerName.text = player.Nickname;
        this.ImageLeftPanel.color = this.ImageRightPanel.color = player.PlayerColor;
        this.textPlayerBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {player.Balance}";

        this.associatedPlayer = player;
        this.gameObject.name = player.OwnerClientId.ToString();
    }

    [ClientRpc]
    public void UpdateBalanceClientRpc(ClientRpcParams clientRpcParams)
    {
        this.textPlayerBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {this.associatedPlayer.Balance}";
    }

    private void HandleBttonInteractClicked()
    {
        Debug.Log("Pressed");
    }
}
