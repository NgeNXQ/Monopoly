using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public sealed class PanelPlayerGameUI : NetworkBehaviour
{
    #region Setup

    #region Visuals

    [Header("Visuals")]

    [Space]
    [SerializeField] private Image ImagePlayerColor;

    [Space]
    [SerializeField] private TMP_Text textPlayerName;

    [Space]
    [SerializeField] private TMP_Text textPlayerBalance;

    #endregion

    #region Controls

    [Space]
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        this.transform.SetParent(UIManagerMonopolyGame.Instance.CanvasPlayersList.transform);
        this.transform.localScale = Vector3.one;

        this.associatedPlayer = GameManager.Instance.CurrentPlayer;
        this.textPlayerName.text = GameManager.Instance.CurrentPlayer.Nickname;
        this.ImagePlayerColor.color = GameManager.Instance.CurrentPlayer.PlayerColor;
        this.textPlayerBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {GameManager.Instance.CurrentPlayer.Balance}";
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
