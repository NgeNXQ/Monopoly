using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Monopoly.Client.Runtime.Game.Core;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Controllers.Concrete;
using Monopoly.Client.Runtime.UI.Managers;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Game
{
    internal sealed class PlayerGamePanel : NetworkBehaviour
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
            this.associatedPawn.Balance.OnValueChanged += (int _, int _) => this.textPawnBalance.text = $"{UIManagerGame.Instance.Currency} {this.associatedPawn.Balance.Value}";

            this.imagePawnColor.color = this.associatedPawn.PawnColor;
            this.textPawnNickname.text = this.associatedPawn.Nickname;
            this.textPawnBalance.text = $"{UIManagerGame.Instance.Currency} {this.associatedPawn.Balance.Value}";

            this.transform.SetParent(UIManagerGame.Instance.CanvasPlayersList.transform);
            this.transform.localScale = Vector3.one;

            GameManager.Instance.AddPawnPanel(this);
        }

        private void OnButtonInteractClicked()
        {
            if (PlayerPawnController.LocalInstance == this.associatedPawn)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OKCancel,
                    MessageBoxPanel.Icon.Question,
                    UIManagerGame.Instance.MessageConfirmSurrender,
                    this.OnSurrender
                );
            }
            else
            {
                if (!PlayerPawnController.LocalInstance.IsAbleToTrade || PlayerPawnController.LocalInstance.TradeReceiver != null)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Error,
                        UIManagerGame.Instance.MessageLimitedTradesCount
                    );

                    return;
                }

                if (PlayerPawnController.LocalInstance == GameManager.Instance.CurrentPawn)
                {
                    PlayerPawnController.LocalInstance.TradeReceiver = this.associatedPawn;

                    UIManagerGame.Instance.HideButtonRollDice();
                    UIManagerGame.Instance.ShowPanelTradeSender(PlayerPawnController.LocalInstance, this.associatedPawn, PlayerPawnController.LocalInstance.OnTradeSenderShown);
                }
            }
        }

        private void OnSurrender()
        {
            if (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult == MessageBoxPanel.DialogResult.OK)
                PlayerPawnController.LocalInstance.DeclareBankruptcy();
        }
    }
}
