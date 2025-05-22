using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.Game.Models;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Player.Factories;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Unity.Services.Lobbies.Models.Extensions;

namespace Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete
{
    internal sealed class PlayerPawnController : PawnController
    {
        internal static PlayerPawnController LocalInstance { get; private set; }

        internal bool IsTrading { get; private set; }
        internal bool IsAbleToBuild { get; private set; }
        internal bool IsAbleToTrade { get; private set; }
        internal PawnController TradeReceiver { get; private set; }

        public override sealed async void OnNetworkSpawn()
        {
            try
            {
                base.IsBot = false;
                base.IsPlayer = true;
                base.FactoryTileStrategy = new PlayerTileStrategyFactory();
                base.FactoryCardStrategy = new PlayerCardStrategyFactory();
                this.Nickname = LobbyManager.Instance.LocalLobby.Players[GameManager.Instance.PawnsCount - 1].GetData(GameCoordinator.KEY_PLAYER_DATA_NICKNAME);

                if (base.OwnerClientId == NetworkManager.Singleton?.LocalClientId)
                    PlayerPawnController.LocalInstance = this;
            }
            catch (LobbyServiceException)
            {
                await LobbyManager.Instance.DisconnectFromLobbyAsync();
            }
            finally
            {
                if (base.OwnerClientId == NetworkManager.Singleton?.LocalClientId)
                    UIManagerGame.Instance.ButtonRollDiceClicked += this.OnButtonRollDiceClicked;
            }
        }

        public sealed override void OnNetworkDespawn()
        {
            if (base.OwnerClientId == NetworkManager.Singleton?.LocalClientId)
                UIManagerGame.Instance.ButtonRollDiceClicked -= this.OnButtonRollDiceClicked;
        }

        internal override sealed async void PerformTurn()
        {
            await Awaitable.WaitForSecondsAsync(base.turnDelay);

            this.IsTrading = false;
            this.TradeReceiver = null;
            this.IsAbleToBuild = true;
            this.IsAbleToTrade = false;

            // if (base.IsSkipTurn)
            // {
            //     base.IsSkipTurn = false;
            //     base.CompleteTurn();
            //     return;
            // }

            UIManagerGame.Instance.ShowButtonRollDice();
        }

        private void OnButtonRollDiceClicked()
        {
            UIManagerGame.Instance.HideAllControls();

            base.RollDice();
        }

        internal void HandleTradeInitialization(PawnController receiver)
        {
            this.IsTrading = true;
            this.TradeReceiver = receiver;

            UIManagerGame.Instance.HideButtonRollDice();

            UIManagerGame.Instance.ShowViewTradeSender(
                PlayerPawnController.LocalInstance,
                receiver,
                this.OnViewTradeSenderShown
            );
        }

        private void OnViewTradeSenderShown()
        {
            if (UIManagerGame.Instance.ViewTradeSender.ViewDialogResult == TradeSenderView.DialogResult.Offer)
            {
                if (UIManagerGame.Instance.ViewTradeSender.Credentials.AreValid)
                {
                    UIManagerGame.Instance.HideViewTradeSender();
                    base.SendTrade(UIManagerGame.Instance.ViewTradeSender.Credentials);
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Error,
                        UIManagerGame.Instance.MessageWrongTradeCredentials
                    );
                }
            }
            else
            {
                this.IsTrading = false;
                this.TradeReceiver = null;

                UIManagerGame.Instance.ShowButtonRollDice();
                UIManagerGame.Instance.HideViewTradeSender();
            }
        }

        internal override sealed void RespondToTrade(TradeCredentialsSerializable credentials)
        {
            UIManagerGame.Instance.ShowViewTradeReceiver(credentials, () => this.OnViewTradeReceiverShown(credentials));
        }

        private void OnViewTradeReceiverShown(TradeCredentialsSerializable credentials)
        {
            if (UIManagerGame.Instance.ViewTradeReceiver.ViewDialogResult == TradeReceiverView.DialogResult.Accept)
            {
                UIManagerGame.Instance.HideViewTradeReceiver();
                base.AcceptTrade(credentials);
            }
            else
            {
                UIManagerGame.Instance.HideViewTradeReceiver();
                base.DeclineTrade(credentials);
            }
        }

        internal override sealed void HandleTradeResponse(TradeCredentialsSerializable credentials)
        {
            this.IsTrading = false;
            this.TradeReceiver = null;
            this.IsAbleToTrade = false;

            if (credentials.Result == TradeResult.Accepted)
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Success,
                    UIManagerGame.Instance.MessageTradeAccepted);
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Failure,
                    UIManagerGame.Instance.MessageTradeDeclined
                );
            }

            UIManagerGame.Instance.ShowButtonRollDice();
        }
    }
}
