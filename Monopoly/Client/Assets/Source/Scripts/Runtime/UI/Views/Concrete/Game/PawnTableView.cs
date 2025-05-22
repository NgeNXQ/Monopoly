using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;

namespace Monopoly.Client.Runtime.UI.Views.Concrete.Game
{
    internal sealed class PawnTableView : NetworkBehaviour
    {
        [SerializeField]
        private Image imagePawnColor;

        [SerializeField]
        private Button buttonInteract;

        [SerializeField]
        private TMP_Text textPawnBalance;

        [SerializeField]
        private TMP_Text textPawnNickname;

        internal PawnController Owner { get; private set; }

        private void Start()
        {
            this.transform.SetParent(UIManagerGame.Instance.PanelPlayersContainer.transform);
            this.transform.localScale = Vector3.one;
        }

        public override sealed void OnNetworkSpawn()
        {
            this.Owner = GameManager.Instance.GetPawnController(GameManager.Instance.PawnsCount - 1);

            this.Owner.Balance.OnValueChanged += this.OnBalanceValueChanged;

            this.imagePawnColor.color = this.Owner.PawnColor;
            this.textPawnNickname.text = this.Owner.Nickname;
            this.textPawnBalance.text = $"{UIManagerGame.Instance.Currency} {this.Owner.Balance.Value}";

            this.buttonInteract.onClick.AddListener(this.OnButtonInteractClicked);

            GameManager.Instance.RegisterPawnTable(this);
        }

        public override sealed void OnNetworkDespawn()
        {
            this.buttonInteract.onClick.RemoveListener(this.OnButtonInteractClicked);
            this.Owner.Balance.OnValueChanged -= this.OnBalanceValueChanged;
        }

        private void OnBalanceValueChanged(int previousValue, int newValue)
        {
            this.textPawnBalance.text = $"{UIManagerGame.Instance.Currency} {this.Owner.Balance.Value}";
        }

        private void OnButtonInteractClicked()
        {
            if (PlayerPawnController.LocalInstance == this.Owner)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OKCancel,
                    MessageBoxView.Icon.Question,
                    UIManagerGame.Instance.MessageConfirmSurrender,
                    this.OnSurrendered
                );
            }
            else
            {
                if (PlayerPawnController.LocalInstance.IsTrading)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Error,
                        UIManagerGame.Instance.MessageLimitedTradesCount
                    );
                    return;
                }

                if (PlayerPawnController.LocalInstance == GameManager.Instance.CurrentPawn)
                    PlayerPawnController.LocalInstance.HandleTradeInitialization(this.Owner);
            }
        }

        private void OnSurrendered()
        {
            if (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult == MessageBoxView.DialogResult.OK)
                PlayerPawnController.LocalInstance.Surrender();
        }
    }
}
