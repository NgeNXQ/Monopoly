using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Controllers.Concrete;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Managers.Gameplay;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Gameplay
{
    internal sealed class PawnPanel : NetworkBehaviour
    {
        [SerializeField]
        private Image imagePawnColor;

        [SerializeField]
        private Button buttonInteract;

        [SerializeField]
        private TMP_Text textPawnBalance;

        [SerializeField]
        private TMP_Text textPawnNickname;

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

            this.imagePawnColor.color = this.associatedPawn.PawnColor;
            this.textPawnNickname.text = this.associatedPawn.Nickname;
            this.textPawnBalance.text = $"{UIManagerGame.Instance.Currency} {this.associatedPawn.Balance.Value}";

            this.associatedPawn.Balance.OnValueChanged += this.OnBalanceValueChanged;

            this.transform.SetParent(UIManagerGame.Instance.PanelPlayersList.transform);
            this.transform.localScale = Vector3.one;

            GameManager.Instance.AddPawnPanel(this);
        }

        public sealed override void OnNetworkDespawn()
        {
            this.associatedPawn.Balance.OnValueChanged -= this.OnBalanceValueChanged;
            GameManager.Instance.AddPawnPanel(this);

            // base.OnNetworkDespawn();
        }

        private void OnBalanceValueChanged(int previousValue, int newValue)
        {
            this.textPawnBalance.text = $"{UIManagerGame.Instance.Currency} {this.associatedPawn.Balance.Value}";
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
