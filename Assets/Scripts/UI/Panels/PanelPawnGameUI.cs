using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

internal sealed class PanelPawnGameUI : NetworkBehaviour
{
    [Header("Visuals")]

    [Space]
    [SerializeField]
    private Image imagePawnColor;

    [Space]
    [SerializeField]
    private TMP_Text textPawnNickname;

    [Space]
    [SerializeField]
    private TMP_Text textPawnBalance;

    [Space]
    [Header("Controls")]

    [Space]
    [SerializeField]
    private Button buttonInteract;

    private PawnController associatedPawn;

    internal int NetworkIndex => this.associatedPawn.NetworkIndex;

    private void OnEnable()
    {
        this.buttonInteract.onClick.AddListener(this.OnButtonInteractClicked);
    }

    private void OnDisable()
    {
        this.buttonInteract.onClick.RemoveListener(this.OnButtonInteractClicked);
    }

    public override void OnNetworkSpawn()
    {
        this.associatedPawn = GameManager.Instance.GetPawnController(GameManager.Instance.PawnsCount - 1);
        this.associatedPawn.Balance.OnValueChanged += (int _, int _) => this.textPawnBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {this.associatedPawn.Balance.Value}";

        this.imagePawnColor.color = this.associatedPawn.PawnColor;
        this.textPawnNickname.text = this.associatedPawn.Nickname;
        this.textPawnBalance.text = $"{UIManagerMonopolyGame.Instance.Currency} {this.associatedPawn.Balance.Value}";

        this.transform.SetParent(UIManagerMonopolyGame.Instance.CanvasPlayersList.transform);
        this.transform.localScale = Vector3.one;

        GameManager.Instance.AddPawnPanel(this);
    }

    private void OnButtonInteractClicked()
    {
        if (PlayerPawnController.LocalInstance == this.associatedPawn)
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OKCancel, UIManagerMonopolyGame.Instance.MessageConfirmSurrender, PanelMessageBoxUI.Icon.Question, this.OnSurrender);
        }
        else
        {
            if (!PlayerPawnController.LocalInstance.IsAbleToTrade || PlayerPawnController.LocalInstance.TradeReceiver != null)
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerMonopolyGame.Instance.MessageLimitedTradesCount, PanelMessageBoxUI.Icon.Error);
                return;
            }

            if (PlayerPawnController.LocalInstance == GameManager.Instance.CurrentPawn)
            {
                PlayerPawnController.LocalInstance.TradeReceiver = this.associatedPawn;

                UIManagerMonopolyGame.Instance.HideButtonRollDice();
                UIManagerMonopolyGame.Instance.ShowPanelSendTrade(PlayerPawnController.LocalInstance, this.associatedPawn, PlayerPawnController.LocalInstance.OnSendTradeShown);
            }
        }
    }

    private void OnSurrender()
    {
        if (UIManagerGlobal.Instance.LastMessageBox.PanelDialogResult == PanelMessageBoxUI.DialogResult.OK)
            PlayerPawnController.LocalInstance.DeclareBankruptcy();
    }
}
