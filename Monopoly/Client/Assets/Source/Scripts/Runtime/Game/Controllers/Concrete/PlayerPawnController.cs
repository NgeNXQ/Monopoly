using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Runtime.Game.Core;
using Monopoly.Client.Runtime.Game.Serializables;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.UI.Managers;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Game;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;
using Monopoly.Client.Scriptable.Objects.Cards;

namespace Monopoly.Client.Runtime.Game.Controllers.Concrete
{
    internal sealed class PlayerPawnController : PawnController
    {
        internal static PlayerPawnController LocalInstance { get; private set; }

        private bool isAbleToBuild;
        private ChanceCardScriptableObject currentChanceNode;

        internal bool IsAbleToTrade { get; private set; }

        internal MonopolyTile SelectedTile { get; set; }
        internal PawnController TradeReceiver { get; set; }

        public override sealed async void OnNetworkSpawn()
        {
            try
            {
                this.Nickname = LobbyManager.Instance.LocalLobby.Players[GameManager.Instance.PawnsCount - 1].Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
            }
            catch (LobbyServiceException)
            {
                await LobbyManager.Instance.DisconnectFromLobbyAsync();
            }

            if (base.OwnerClientId == NetworkManager.Singleton?.LocalClientId)
            {
                PlayerPawnController.LocalInstance = this;
                UIManagerGame.Instance.ButtonRollDiceClicked += this.OnButtonRollDiceClicked;
            }
        }

        internal void DeclareBankruptcy()
        {
            UIManagerGame.Instance.HideButtonRollDice();
            UIManagerGame.Instance.ShowButtonDisconnect();

            PlayerPawnController.LocalInstance.SurrenderServerRpc(GameManager.Instance.SenderLocalClient);
        }

        private void OnButtonRollDiceClicked()
        {
            this.IsAbleToTrade = false;
            this.isAbleToBuild = false;

            UIManagerGame.Instance.HidePanelTileManagement();
            UIManagerGame.Instance.HidePanelTradeSender();
            UIManagerGame.Instance.HidePanelNodeOffer();
            UIManagerGame.Instance.HideButtonRollDice();
            UIManagerGame.Instance.HidePanelTilePayment();
            UIManagerGame.Instance.HidePanelTradeReceiver();
            UIManagerGame.Instance.HidePanelChancePayment();

            GameManager.Instance.RollDice();
            UIManagerGame.Instance.ShowDiceAnimation();

            base.PerformDiceRolling();
        }

        internal override sealed async void PerformTurn()
        {
            await Awaitable.WaitForSecondsAsync(PawnController.TURN_DELAY);

            this.SelectedTile = null;
            this.TradeReceiver = null;
            this.isAbleToBuild = true;
            this.IsAbleToTrade = true;
            this.currentChanceNode = null;

            if (base.IsSkipTurn)
            {
                base.IsSkipTurn = false;
                base.CompleteTurn();
                return;
            }

            UIManagerGame.Instance.ShowButtonRollDice();
        }

        private protected override sealed void HandleJailLanding()
        {
            base.CompleteTurn();
        }

        private protected override sealed void HandleStartLanding()
        {
            base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value + GameManager.Instance.ExactCircleBonus, GameManager.Instance.SenderLocalClient);
            base.CompleteTurn();
        }

        private protected override sealed void HandleChanceLanding()
        {
            this.currentChanceNode = MonopolyBoard.Instance.GetChanceNode();

            if (this.currentChanceNode.ChanceType != ChanceCardScriptableObject.Type.Penalty)
                UIManagerGame.Instance.ShowPanelTileInformation(this.currentChanceNode.Description, this.OnChanceActionAccepted);
            else
                UIManagerGame.Instance.ShowPanelChancePayment(this.currentChanceNode.Description, this.OnChancePaymentAccepted);

            UIManagerGame.Instance.ShowPanelInfoServerRpc(this.currentChanceNode.Description, GameManager.Instance.SenderLocalClient);
        }

        private void OnChanceActionAccepted()
        {
            if (UIManagerGame.Instance.PanelTileInformation.PanelDialogResult == TileInformationPanel.DialogResult.Confirmed)
                base.PerformChanceAction(this.currentChanceNode);
        }

        private void OnChancePaymentAccepted()
        {
            if (base.NetWorth < this.currentChanceNode.Penalty)
            {
                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.NetWorth, GameManager.Instance.SenderLocalClient);

                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Error,
                    UIManagerGame.Instance.MessageBankrupt,
                    this.DeclareBankruptcy
                );
            }
            else
            {
                if (base.Balance.Value >= this.currentChanceNode.Penalty)
                {
                    UIManagerGame.Instance.HidePanelChancePayment();

                    base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - this.currentChanceNode.Penalty, GameManager.Instance.SenderLocalClient);
                    base.CompleteTurn();
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Warning,
                        UIManagerGame.Instance.MessageInsufficientFunds
                    );
                }
            }
        }

        private protected override sealed void HandleSendJailLanding()
        {
            UIManagerGlobal.Instance.ShowMessageBox(
                MessageBoxPanel.Type.OK,
                MessageBoxPanel.Icon.Warning,
                UIManagerGame.Instance.MessageSentJail
            );

            base.GoToJail();
        }

        private protected override sealed void HandlePropertyLanding()
        {
            if (base.CurrentTile.Owner == null)
            {
                UIManagerGame.Instance.ShowPanelTileProposal(base.CurrentTile, this.OnTileProposalShown);
                return;
            }

            if (base.CurrentTile.Owner == this || base.CurrentTile.IsMortgaged)
            {
                base.CompleteTurn();
                return;
            }

            if (base.NetWorth < base.CurrentTile.PriceRent)
            {
                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.NetWorth, GameManager.Instance.SenderLocalClient);
                base.UpdateBalanceServerRpc(base.CurrentTile.Owner.NetworkIndex, base.CurrentTile.Owner.Balance.Value + base.NetWorth, GameManager.Instance.SenderLocalClient);

                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Error,
                    UIManagerGame.Instance.MessageBankrupt,
                    this.DeclareBankruptcy
                );
            }
            else
            {
                UIManagerGame.Instance.ShowPanelTilePayment(base.CurrentTile, this.OnTilePaymentShown);
            }
        }

        private void OnTileProposalShown()
        {
            if (UIManagerGame.Instance.PanelTileProposal.PanelDialogResult == TileProposalPanel.DialogResult.Accepted)
            {
                if (base.Balance.Value >= base.CurrentTile.PricePurchase)
                {
                    UIManagerGame.Instance.HidePanelNodeOffer();

                    base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentTile.PricePurchase, GameManager.Instance.SenderLocalClient);
                    base.CurrentTile.UpdateOwnershipServerRpc(base.NetworkIndex, GameManager.Instance.SenderLocalClient);
                    base.CompleteTurn();
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Warning,
                        UIManagerGame.Instance.MessageInsufficientFunds
                    );
                }
            }
            else
            {
                UIManagerGame.Instance.HidePanelNodeOffer();
                base.CompleteTurn();
            }
        }

        private void OnTilePaymentShown()
        {
            if (base.Balance.Value >= base.CurrentTile.PriceRent)
            {
                UIManagerGame.Instance.HidePanelTilePayment();

                base.UpdateBalanceServerRpc(base.CurrentTile.Owner.NetworkIndex, base.CurrentTile.Owner.Balance.Value + base.CurrentTile.PriceRent, GameManager.Instance.SenderLocalClient);
                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - base.CurrentTile.PriceRent, GameManager.Instance.SenderLocalClient);
                base.CompleteTurn();
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Warning,
                    UIManagerGame.Instance.MessageInsufficientFunds
                );
            }
        }

        private protected override sealed void HandleFreeParkingLanding()
        {
            base.CompleteTurn();
        }

        internal void OnTileManagementShown()
        {
            if (UIManagerGame.Instance.PanelTileManagement.PanelDialogResult == TileManagementPanel.DialogResult.Upgrade)
                this.UpgradeProperty();
            else
                this.DowngradeProperty();
        }

        private void UpgradeProperty()
        {
            if (!this.isAbleToBuild)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Warning,
                    UIManagerGame.Instance.MessageAlreadyBuilt
                );

                return;
            }

            if (base.Balance.Value < this.SelectedTile.PriceUpgrade)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Warning,
                    UIManagerGame.Instance.MessageInsufficientFunds
                );

                return;
            }

            if (!this.SelectedTile.IsUpgradable)
            {
                if (this.SelectedTile.TileType == MonopolyTile.Type.Property)
                {
                    if (this.SelectedTile.Level == MonopolyTile.PROPERTY_MAX_LEVEL)
                    {
                        UIManagerGlobal.Instance.ShowMessageBox(
                            MessageBoxPanel.Type.OK,
                            MessageBoxPanel.Icon.Warning,
                            UIManagerGame.Instance.MessageCannotUpgradeMaxLevel
                        );
                    }
                    else if (!base.HasFullMonopoly(this.SelectedTile.AffiliatedMonopoly))
                    {
                        UIManagerGlobal.Instance.ShowMessageBox(
                            MessageBoxPanel.Type.OK,
                            MessageBoxPanel.Icon.Warning,
                            UIManagerGame.Instance.MessageCompleteMonopolyRequired
                        );
                    }
                    else
                    {
                        UIManagerGlobal.Instance.ShowMessageBox(
                            MessageBoxPanel.Type.OK,
                            MessageBoxPanel.Icon.Warning,
                            UIManagerGame.Instance.MessageOnlyEvenBuildingAllowed
                        );
                    }
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Warning,
                        UIManagerGame.Instance.MessageCannotUpgradeMaxLevel
                    );
                }
            }
            else
            {
                this.isAbleToBuild = false;
                UIManagerGame.Instance.HidePanelTileManagement();

                this.SelectedTile.Upgrade();
                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value - this.SelectedTile.PriceUpgrade, GameManager.Instance.SenderLocalClient);
            }
        }

        private void DowngradeProperty()
        {
            if (!this.SelectedTile.IsDowngradable)
            {
                if (this.SelectedTile.Level == MonopolyTile.PROPERTY_MIN_LEVEL)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Warning,
                        UIManagerGame.Instance.MessageCannotDowngradeMinLevel
                    );
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Warning,
                        UIManagerGame.Instance.MessageOnlyEvenBuildingAllowed
                    );
                }
            }
            else
            {
                UIManagerGame.Instance.HidePanelTileManagement();

                base.UpdateBalanceServerRpc(base.NetworkIndex, base.Balance.Value + this.SelectedTile.PriceDowngrade, GameManager.Instance.SenderLocalClient);
                this.SelectedTile.Downgrade();
            }
        }

        internal void OnTradeSenderShown()
        {
            if (TradeSenderPanel.Instance.PanelDialogResult == TradeSenderPanel.DialogResult.Offer)
            {
                TradeCredentials credentials = TradeSenderPanel.Instance.Credentials;

                if (credentials.AreValid)
                {
                    UIManagerGame.Instance.HidePanelTradeSender();
                    base.SendTradeServerRpc(credentials, GameManager.Instance.SenderLocalClient);
                }
                else
                {
                    this.TradeReceiver = null;
                    UIManagerGame.Instance.ShowButtonRollDice();

                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Error,
                        UIManagerGame.Instance.MessageWrongTradeCredentials
                    );
                }
            }
            else
            {
                this.TradeReceiver = null;
                UIManagerGame.Instance.ShowButtonRollDice();
                UIManagerGame.Instance.HidePanelTradeSender();
            }
        }

        private protected override sealed void RespondToTrade(TradeCredentials credentials)
        {
            UIManagerGame.Instance.ShowPanelTradeReceiver(credentials, () => this.OnTradeReceived(credentials));
        }

        private void OnTradeReceived(TradeCredentials credentials)
        {
            if (UIManagerGame.Instance.PanelTradeReceiver.PanelDialogResult == TradeReceiverPanel.DialogResult.Accept)
                base.AcceptTradeServerRpc(credentials, GameManager.Instance.SenderLocalClient);
            else
                base.DeclineTradeServerRpc(credentials, GameManager.Instance.SenderLocalClient);

            UIManagerGame.Instance.HidePanelTradeReceiver();
        }

        private protected override sealed void HandleTradeResponse(TradeCredentials credentials)
        {
            this.TradeReceiver = null;
            this.IsAbleToTrade = false;

            if (credentials.Result == TradeResult.Success)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Success,
                    UIManagerGame.Instance.MessageTradeAccepted);
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Failure,
                    UIManagerGame.Instance.MessageTradeDeclined
                );
            }

            UIManagerGame.Instance.ShowButtonRollDice();
        }
    }
}
