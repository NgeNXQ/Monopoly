using System;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Scriptable.Objects.Cards.Concrete;

namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Player.Cards
{
    internal sealed class PenaltyCardPlayerStrategy : ICardStrategy
    {
        public Type CardType { get; } = typeof(PenaltyCard);

        public void Execute(PawnController pawn, CardScriptableObject card)
        {
            string description = $"{card.Description} {((PenaltyCard)card).Penalty}";

            UIManagerGame.Instance.ShowCardInformationToEveryoneNotMe(description);
            UIManagerGame.Instance.ShowViewCardPayment(description, () => this.OnViewCardPaymentShown(pawn, card));
        }

        private void OnViewCardPaymentShown(PawnController pawn, CardScriptableObject card)
        {
            if (UIManagerGame.Instance.ViewCardPayment.PanelDialogResult == CardPaymentView.DialogResult.Confirmed)
                this.AcceptCardPayment(pawn, card);
        }

        private void AcceptCardPayment(PawnController pawn, CardScriptableObject card)
        {
            PenaltyCard penaltyCard = card as PenaltyCard;

            if (pawn.NetWorth < penaltyCard.Penalty)
            {
                pawn.TransactDumpBalance(pawn.NetWorth);

                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Error,
                    UIManagerGame.Instance.MessageBankrupt,
                    pawn.Surrender
                );
            }
            else
            {
                if (pawn.GetBalance() >= penaltyCard.Penalty)
                {
                    UIManagerGame.Instance.HideViewCardPayment();
                    pawn.TransactDumpBalance(penaltyCard.Penalty);
                    pawn.CompleteTurn();
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Warning,
                        UIManagerGame.Instance.MessageInsufficientFunds
                    );
                }
            }
        }
    }
}
