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
    [SerializeField] private Image imagePlayerColor;

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
        this.imagePlayerColor.color = GameManager.Instance.CurrentPlayer.PlayerColor;
        this.textPlayerBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {GameManager.Instance.CurrentPlayer.Balance}";

        this.associatedPlayer.OnBalanceUpdated += this.HandleBalanceUpdated;

        this.textPlayerBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {this.associatedPlayer.Balance.Value}";
    }

    public override void OnNetworkDespawn()
    {
        this.associatedPlayer.OnBalanceUpdated -= this.HandleBalanceUpdated;
    }

    #region GUI Callbacks

    private void CallbackSurrender()
    {
        if (UIManagerGlobal.Instance.LastMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
        {
            this.associatedPlayer.Surrender();
            UIManagerMonopolyGame.Instance.HideButtonRollDice();
            UIManagerMonopolyGame.Instance.ShowButtonDisconnect();
        }
    }

    private void HandleBttonInteractClicked()
    {
        if (NetworkManager.Singleton.LocalClientId == this.OwnerClientId)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OKCancel, UIManagerMonopolyGame.Instance.MessageConfirmSurrender, PanelMessageBoxUI.Icon.Question, actionCallback: this.CallbackSurrender);
        }
        else
        {
            if (GameManager.Instance.CurrentPlayer.IsTrading)
            {
                return;
            }

            if (GameManager.Instance.CurrentPlayer.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                UIManagerMonopolyGame.Instance.HideButtonRollDice();

                GameManager.Instance.CurrentPlayer.IsTrading = true;

                GameManager.Instance.CurrentPlayer.PlayerTradingWith = this.associatedPlayer;

                UIManagerMonopolyGame.Instance.ShowTradeOffer(GameManager.Instance.CurrentPlayer.Nickname, this.associatedPlayer.Nickname, GameManager.Instance.CurrentPlayer.CallbackTradeOffer);
            }
        }
    }

    #endregion

    private void HandleBalanceUpdated()
    {
        this.textPlayerBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {this.associatedPlayer.Balance.Value}";
    }
}
