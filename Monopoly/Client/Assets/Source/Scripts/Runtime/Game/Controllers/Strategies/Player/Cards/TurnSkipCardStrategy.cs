// using System;
// using Monopoly.Client.Runtime.UI.Managers.Game;
// using Monopoly.Client.Runtime.UI.Views.Concrete.Game;
// using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;
// using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
// using Monopoly.Client.Scriptable.Objects.Cards.Common;
// using Monopoly.Client.Scriptable.Objects.Cards.Concrete;

// namespace Monopoly.Client.Runtime.Game.Controllers.Strategies.Player.Cards
// {
//     internal sealed class TurnSkipCardStrategy : ICardStrategy
//     {
//         public Type CardType { get; } = typeof(SkipCard);

//         public void Execute(PawnController pawn, CardScriptableObject card)
//         {
//             UIManagerGame.Instance.ShowCardInformationToEveryoneNotMe(card);
//             UIManagerGame.Instance.ShowViewCardPayment(card, () => this.OnViewCardInformationShown(pawn, card));
//         }

//         private void OnViewCardInformationShown(PawnController pawn, CardScriptableObject card)
//         {
//             if (UIManagerGame.Instance.ViewCardInformation.PanelDialogResult == CardInformationView.DialogResult.Confirmed)
//                 this.AcceptTeleportationForward(pawn, card);
//         }

//         private void AcceptTeleportationForward(PawnController pawn, CardScriptableObject card)
//         {
//             // pawn.IsSkippingTurn = true;
//             pawn.CompleteTurn();
//             // GameManager.Instance.RollDice();
//             // UIManagerGame.Instance.ShowDiceAnimation();
//             // pawn.MoveToken(GameManager.Instance.TotalRollResult);
//         }
//     }
// }
