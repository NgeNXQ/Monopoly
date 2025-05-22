using UnityEngine;
using UnityEngine.EventSystems;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.UI.Handlers
{
    internal sealed class TileTouchHandler : MonoBehaviour, IPointerClickHandler
    {
        private bool isShown;
        private PropertyTile tile;

        private void Awake()
        {
            this.tile = this.GetComponent<PropertyTile>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (this.tile.Owner == null)
                return;

            if (PlayerPawnController.LocalInstance.IsTrading)
            {
                if (this.tile.Owner != PlayerPawnController.LocalInstance && this.tile.Owner != PlayerPawnController.LocalInstance.TradeReceiver)
                    return;

                if (!this.tile.IsTradable)
                    return;

                if (this.tile.Owner == PlayerPawnController.LocalInstance)
                    UIManagerGame.Instance.ViewTradeSender.SenderTile = this.tile;
                else if (this.tile.Owner == PlayerPawnController.LocalInstance.TradeReceiver)
                    UIManagerGame.Instance.ViewTradeSender.ReceiverTile = this.tile;

                return;
            }

            if (PlayerPawnController.LocalInstance.IsAbleToBuild)
            {
                if (this.tile.Owner != PlayerPawnController.LocalInstance)
                    return;

                if (this.isShown)
                {
                    this.isShown = false;
                    UIManagerGame.Instance.HideViewTileManagement();
                    return;
                }

                if (!this.isShown)
                {
                    this.isShown = true;
                    UIManagerGame.Instance.ShowViewTileManagement(this.tile, this.OnViewTileManagementShown);
                    return;
                }

                return;
            }
        }

        private void OnViewTileManagementShown()
        {
            if (UIManagerGame.Instance.ViewTileManagement.PanelDialogResult == TileManagementView.DialogResult.Upgrade)
                this.UpgradeProperty();
            else
                this.DowngradeProperty();
        }

        private void UpgradeProperty()
        {
            // if (!PlayerPawnController.LocalInstance.IsAbleToBuild)
            // {
            //     UIManagerGlobal.Instance.ShowMessageBox(
            //         MessageBoxView.Type.OK,
            //         MessageBoxView.Icon.Warning,
            //         UIManagerGame.Instance.MessageAlreadyBuilt
            //     );

            //     return;
            // }

            if (PlayerPawnController.LocalInstance.GetBalance() < this.tile.PricePurchase)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Warning,
                    UIManagerGame.Instance.MessageInsufficientFunds
                );

                return;
            }

            if (!this.tile.IsUpgradable)
            {
                if (this.tile.Level == PropertyTile.LEVEL_MAX)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Warning,
                        UIManagerGame.Instance.MessageCannotUpgradeMaxLevel
                    );

                    return;
                }

                if (!PlayerPawnController.LocalInstance.HasFullMonopoly(this.tile.Monopoly))
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Warning,
                        UIManagerGame.Instance.MessageCompleteMonopolyRequired
                    );

                    return;
                }

                if (this.tile.Level >= this.tile.Monopoly.GetLevel(this.tile.Owner))
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Warning,
                        UIManagerGame.Instance.MessageOnlyEvenBuildingAllowed
                    );

                    return;
                }
            }

            this.isShown = false;
            UIManagerGame.Instance.HideViewTileManagement();

            PlayerPawnController.LocalInstance.TransactDumpBalance(this.tile.PricePurchase);
            this.tile.Upgrade();
        }

        private void DowngradeProperty()
        {
            if (!this.tile.IsDowngradable)
            {
                if (this.tile.Level == PropertyTile.LEVEL_MIN)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Warning,
                        UIManagerGame.Instance.MessageCannotDowngradeMinLevel
                    );

                    return;
                }

                if (this.tile.Level < this.tile.Monopoly.GetLevel(this.tile.Owner))
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Warning,
                        UIManagerGame.Instance.MessageOnlyEvenBuildingAllowed
                    );

                    return;
                }
            }
            else
            {
                this.isShown = false;
                UIManagerGame.Instance.HideViewTileManagement();

                PlayerPawnController.LocalInstance.TransactTakeBalance(this.tile.PricePurchase);
                this.tile.Downgrade();
            }
        }
    }
}
