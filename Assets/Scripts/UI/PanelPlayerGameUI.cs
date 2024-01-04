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
        this.transform.SetParent(UIManagerMonopolyGame.Instance.CanvasPlayersList.transform);
        this.transform.localScale = Vector3.one;

        this.associatedPlayer = GameManager.Instance.CurrentPlayer;
        this.textPlayerName.text = GameManager.Instance.CurrentPlayer.Nickname;
        this.ImagePlayerColor.color = GameManager.Instance.CurrentPlayer.PlayerColor;
        this.textPlayerBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {GameManager.Instance.CurrentPlayer.Balance}";

        if (NetworkManager.Singleton.LocalClientId == this.OwnerClientId)
        {
            this.associatedPlayer.OnBalanceUpdated += this.HandleBalanceUpdated;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton.LocalClientId == this.OwnerClientId)
        {
            this.associatedPlayer.OnBalanceUpdated -= this.HandleBalanceUpdated;
        }
    }

    #region GUI Callbacks

    private void HandleBttonInteractClicked()
    {
        if (NetworkManager.Singleton.LocalClientId == this.OwnerClientId)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OKCancel, UIManagerMonopolyGame.Instance.MessageConfirmSurrender, PanelMessageBoxUI.Icon.Question, actionCallback: this.CallbackSurrender);
        }
        else
        {
            Debug.Log("Trading placeholder");
        }
    }

    private void CallbackSurrender()
    {
        if (UIManagerGlobal.Instance.LastMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
        {
            this.associatedPlayer.Surrender();
        }
    }

    #endregion

    #region Updating Balance

    private void UpdateBalance()
    {
        this.textPlayerBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {this.associatedPlayer.Balance}";
    }

    private void HandleBalanceUpdated()
    {
        this.UpdateBalance();
        this.UpdateBalanceServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    [ServerRpc]
    public void UpdateBalanceServerRpc(ServerRpcParams serverRpcParams)
    {
        this.UpdateBalanceClientRpc(GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    public void UpdateBalanceClientRpc(ClientRpcParams clientRpcParams)
    {
        this.UpdateBalance();
    }

    #endregion
}
