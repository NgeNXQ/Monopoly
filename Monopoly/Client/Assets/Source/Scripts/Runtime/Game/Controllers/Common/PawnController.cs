using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Monopoly.Client.Runtime.Game.Core;
using Monopoly.Client.Runtime.Game.Board;
using Monopoly.Client.Runtime.Game.Tiles.Common;
using Monopoly.Client.Runtime.Game.Tiles.Properties.Common;
using Monopoly.Client.Runtime.Game.Serializables;
using Monopoly.Client.Runtime.UI.Managers;
using Monopoly.Client.Scriptable.Objects.Cards.Chance;

namespace Monopoly.Client.Runtime.Game.Controllers.Common
{
    internal abstract class PawnController : NetworkBehaviour
    {
        [SerializeField]
        private protected Image pawnImageToken;

        [SerializeField, Range(0.0f, 10.0f)]
        private protected float turnDelay = 0.25f;

        [SerializeField, Range(0.0f, 100.0f)]
        private protected float movementSpeed = 35.0f;

        private protected bool IsInJail;
        private protected bool IsSkipTurn;
        private protected int TurnsInJailCount;

        internal Color PawnColor { get; private set; }
        internal int NetworkIndex { get; private set; }
        internal string Nickname { get; private protected set; }
        internal NetworkVariable<int> Balance { get; private set; }
        internal List<PropertyMonopolyTile> OwnedTiles { get; private set; }
        internal int NetWorth => this.Balance.Value + this.OwnedTiles.Sum(tile => tile.Worth);

        internal MonopolyTile CurrentTile { get; private set; }

        internal abstract void PerformTurn();
        private protected abstract void HandleJailLanding();
        private protected abstract void HandleStartLanding();
        private protected abstract void HandleChanceLanding();
        private protected abstract void HandleSendJailLanding();
        private protected abstract void HandlePropertyLanding();
        private protected abstract void HandleFreeParkingLanding();
        private protected abstract void RespondToTrade(TradeCredentials credentials);
        private protected abstract void HandleTradeResponse(TradeCredentials credentials);

        private void Awake()
        {
            this.Balance = new NetworkVariable<int>(
                GameManager.Instance.InitialBalance,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

            if (NetworkManager.Singleton.IsHost)
                this.Balance.Value = GameManager.Instance.InitialBalance;

            this.OwnedTiles = new List<PropertyMonopolyTile>();
            this.CurrentTile = MonopolyBoard.Instance.TileStart;
            this.NetworkIndex = GameManager.Instance.PawnsCount;
            this.transform.position = MonopolyBoard.Instance.TileStart.transform.position;
            this.PawnColor = GameManager.Instance.PawnsVisuals[this.NetworkIndex].PawnTokenColor;
            this.pawnImageToken.sprite = GameManager.Instance.PawnsVisuals[this.NetworkIndex].PawnTokenSprite;

            GameManager.Instance.AddPawnController(this);
        }

        private void MoveToken(int steps)
        {
            // const float POSITION_THRESHOLD = 0.001f;

            // Vector3 targetPosition;
            // bool hasMovedOverStart = false;
            // int currentNodeIndex = MonopolyBoard.Instance.GetIndexOfTile(this.CurrentTile);

            // this.StartCoroutine(MoveCoroutine());

            // IEnumerator MoveCoroutine()
            // {
            //     while (steps != 0)
            //     {
            //         if (steps < 0)
            //         {
            //             ++steps;
            //             currentNodeIndex = Mathf.Abs(--currentNodeIndex + MonopolyBoard.Instance.TilesCount) % MonopolyBoard.Instance.TilesCount;
            //         }
            //         else
            //         {
            //             --steps;
            //             currentNodeIndex = ++currentNodeIndex % MonopolyBoard.Instance.TilesCount;
            //         }

            //         targetPosition = MonopolyBoard.Instance.GetTileByIndex(currentNodeIndex).transform.position;

            //         if (MonopolyBoard.Instance.TileStart == MonopolyBoard.Instance.GetTileByIndex(currentNodeIndex))
            //             hasMovedOverStart = true;

            //         yield return StartCoroutine(MoveStepCoroutine(targetPosition));
            //     }

            //     this.CurrentTile = MonopolyBoard.Instance.GetTileByIndex(currentNodeIndex);
            //     // this.CurrentTile.HandleLanding();

            //     // if (hasMovedOverStart && this.CurrentTile != MonopolyBoard.Instance.NodeStart)
            //     //     this.UpdateBalanceServerRpc(this.NetworkIndex, this.Balance.Value + GameManager.Instance.CircleBonus, GameManager.Instance.SenderLocalClient);

            //     this.HandleLanding();
            // }

            // IEnumerator MoveStepCoroutine(Vector3 targetPosition)
            // {
            //     while (Vector3.Distance(this.transform.position, targetPosition) > POSITION_THRESHOLD)
            //     {
            //         this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, this.movementSpeed * Time.deltaTime);
            //         yield return null;
            //     }

            //     this.transform.position = targetPosition;
            // }
        }

        // private void HandleLanding()
        // {
        //     switch (this.CurrentTile)
        //     {
        //         case PropertyMonopolyTile:
        //             this.HandlePropertyLanding();
        //             break;
        //         // case MonopolyTile.Type.Tax:
        //         //     this.HandleChanceLanding();
        //         //     break;
        //         // case MonopolyTile.Type.Jail:
        //         //     this.HandleJailLanding();
        //         //     break;
        //         // case MonopolyTile.Type.Start:
        //         //     this.HandleStartLanding();
        //         //     break;
        //         // case MonopolyTile.Type.Chance:
        //         //     this.HandleChanceLanding();
        //         //     break;
        //         // case MonopolyTile.Type.SendJail:
        //         //     this.HandleSendJailLanding();
        //         //     break;
        //         // case MonopolyTile.Type.Property:
        //         //     this.HandlePropertyLanding();
        //         //     break;
        //         // case MonopolyTile.Type.Gambling:
        //         //     this.HandlePropertyLanding();
        //         //     break;
        //         // case MonopolyTile.Type.Transport:
        //         //     this.HandlePropertyLanding();
        //         //     break;
        //         // case MonopolyTile.Type.FreeParking:
        //         //     this.HandleFreeParkingLanding();
        //         //     break;
        //     }
        // }

        internal void GoToJail()
        {
            this.IsInJail = true;
            this.TurnsInJailCount = 0;
            this.MoveToken(MonopolyBoard.Instance.GetDistance(this.CurrentTile, MonopolyBoard.Instance.TileJailVisit));
        }

        internal bool HasFullMonopoly(MonopolySet monopolySet)
        {
            if (monopolySet == null)
                throw new System.NullReferenceException($"{nameof(monopolySet)} cannot be null.");

            return this.OwnedTiles.Where(tile => tile.Monopoly == monopolySet).Count() == monopolySet.Tiles.Count;
        }

        internal bool HasPartialMonopoly(MonopolySet monopolySet)
        {
            if (monopolySet == null)
                throw new System.NullReferenceException($"{nameof(monopolySet)} cannot be null.");

            const float MONOPOLY_PERCENTAGE_THRESHOLD = 0.5f;
            return (float)this.OwnedTiles.Where(tile => tile.Monopoly == monopolySet).Count() / monopolySet.Tiles.Count >= MONOPOLY_PERCENTAGE_THRESHOLD;
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SurrenderServerRpc(ServerRpcParams serverRpcParams)
        {
            MonopolyTile[] MonopolyTiles = this.OwnedTiles.ToArray();

            // foreach (MonopolyTile monopolyTile in MonopolyTiles)
            //     monopolyTile.ResetOwnershipServerRpc(GameManager.Instance.SenderLocalClient);

            // this.DeclineTradeServerRpc(TradeCredentials.Blank, GameManager.Instance.SenderLocalClient);

            GameManager.Instance.GetPawnPanel(this.NetworkIndex).GetComponent<NetworkObject>().Despawn();
            GameManager.Instance.GetPawnController(this.NetworkIndex).GetComponent<NetworkObject>().Despawn();
            GameManager.Instance.RemoveSurrenderedPawn(this.NetworkIndex);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        internal void UpdateBalanceRpc(int networkIndex, int newBalance)
        {
            GameManager.Instance.GetPawnController(networkIndex).Balance.Value = newBalance;
        }

        private protected void PerformChanceAction(CardChanceScriptableObject chanceCard)
        {
            chanceCard.Effect.Apply(this);

            // switch (chanceNode.ChanceType)
            // {
            //     case ChanceCardScriptableObject.Type.Reward:
            //         this.UpdateBalanceServerRpc(this.NetworkIndex, this.Balance.Value + chanceNode.Reward, GameManager.Instance.SenderLocalClient);
            //         this.CompleteTurn();
            //         break;
            //     case ChanceCardScriptableObject.Type.SkipTurn:
            //         this.IsSkipTurn = true;
            //         this.CompleteTurn();
            //         break;
            //     case ChanceCardScriptableObject.Type.SendJail:
            //         this.GoToJail();
            //         break;
            //     case ChanceCardScriptableObject.Type.MoveForward:
            //         GameManager.Instance.RollDice();
            //         UIManagerGame.Instance.ShowDiceAnimation();
            //         this.MoveToken(GameManager.Instance.TotalRollResult);
            //         break;
            //     case ChanceCardScriptableObject.Type.MoveBackwards:
            //         GameManager.Instance.RollDice();
            //         UIManagerGame.Instance.ShowDiceAnimation();
            //         this.MoveToken(-GameManager.Instance.TotalRollResult);
            //         break;
            // }
        }

        private protected void PerformDiceRolling()
        {
            if (this.IsInJail)
            {
                ++this.TurnsInJailCount;

                if (GameManager.Instance.HasRolledDouble || this.TurnsInJailCount > GameManager.Instance.MaxJailTurns)
                {
                    this.ReleaseFromJail();
                    this.MoveToken(GameManager.Instance.TotalRollResult);
                }
                else
                {
                    this.CompleteTurn();
                }
            }
            else
            {
                this.MoveToken(GameManager.Instance.TotalRollResult);
            }
        }

        private protected void ReleaseFromJail()
        {
            this.IsInJail = false;
            this.TurnsInJailCount = 0;
        }

        private protected void CompleteTurn()
        {
            // if (this.IsInJail || this.IsSkipTurn)
            //     GameManager.Instance.SwitchPlayerForcefullyServerRpc(GameManager.Instance.SenderLocalClient);
            // else
            //     GameManager.Instance.SwitchPawnServerRpc(GameManager.Instance.SenderLocalClient);
        }

        [ServerRpc]
        internal void SendTradeServerRpc(TradeCredentials credentials, ServerRpcParams serverRpcParams)
        {
            if (!credentials.AreValid)
                return;

// #if UNITY_EDITOR || DEBUG
//             MonopolyTile senderNode = null;
//             MonopolyTile receiverNode = null;
//             PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderNetworkIndex);
//             PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverNetworkIndex);

//             int senderBalance = credentials.SenderBalanceAmount;
//             int receiverBalance = credentials.ReceiverBalanceAmount;

//             if (credentials.SenderNodeIndex != TradeCredentials.PLACEHOLDER)
//                 senderNode = MonopolyBoard.Instance.GetNodeByIndex(credentials.SenderNodeIndex);

//             if (credentials.ReceiverNodeIndex != TradeCredentials.PLACEHOLDER)
//                 receiverNode = MonopolyBoard.Instance.GetNodeByIndex(credentials.ReceiverNodeIndex);

//             Debug.Log($"{sender.Nickname} sends offer to the {receiver.Nickname} ({senderNode?.name} and {senderBalance} for {receiverNode?.name} and {receiverBalance})");
// #endif

            // this.ReceiveTradeClientRpc(credentials, GameManager.Instance.TargetAllClients);
        }

        [ClientRpc]
        private void ReceiveTradeClientRpc(TradeCredentials credentials, ClientRpcParams clientRpcParams)
        {
            if (!credentials.AreValid)
                return;

            PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverNetworkIndex);

            if (receiver.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                receiver.RespondToTrade(credentials);
        }

        [ServerRpc(RequireOwnership = false)]
        private protected void DeclineTradeServerRpc(TradeCredentials credentials, ServerRpcParams serverRpcParams)
        {
// #if UNITY_EDITOR || DEBUG
//             PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderNetworkIndex);
//             PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverNetworkIndex);

//             Debug.Log($"{sender?.Nickname} declined offer from {receiver?.Nickname}");
// #endif

            credentials.Result = TradeResult.Failure;
            // this.HandleTradeResponseClientRpc(credentials, GameManager.Instance.TargetAllClients);
        }

        [ServerRpc]
        private protected void AcceptTradeServerRpc(TradeCredentials credentials, ServerRpcParams serverRpcParams)
        {
            credentials.Result = TradeResult.Success;

            PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderNetworkIndex);
            PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverNetworkIndex);

// #if UNITY_EDITOR || DEBUG
//             Debug.Log($"{sender.Nickname} accepted offer from {receiver.Nickname}");
// #endif

            // this.UpdateBalanceServerRpc(credentials.SenderNetworkIndex, sender.Balance.Value - credentials.SenderBalanceAmount, GameManager.Instance.SenderLocalClient);
            // this.UpdateBalanceServerRpc(credentials.ReceiverNetworkIndex, receiver.Balance.Value + credentials.SenderBalanceAmount, GameManager.Instance.SenderLocalClient);

            // this.UpdateBalanceServerRpc(credentials.SenderNetworkIndex, sender.Balance.Value + credentials.ReceiverBalanceAmount, GameManager.Instance.SenderLocalClient);
            // this.UpdateBalanceServerRpc(credentials.ReceiverNetworkIndex, receiver.Balance.Value - credentials.ReceiverBalanceAmount, GameManager.Instance.SenderLocalClient);

            // if (credentials.SenderNodeIndex != TradeCredentials.PLACEHOLDER)
            // {
            //     MonopolyBoard.Instance.GetTileByIndex(credentials.SenderNodeIndex).ResetOwnershipServerRpc(GameManager.Instance.SenderLocalClient);
            //     MonopolyBoard.Instance.GetTileByIndex(credentials.SenderNodeIndex).UpdateOwnershipServerRpc(credentials.ReceiverNetworkIndex, GameManager.Instance.SenderLocalClient);
            // }

            // if (credentials.ReceiverNodeIndex != TradeCredentials.PLACEHOLDER)
            // {
            //     MonopolyBoard.Instance.GetTileByIndex(credentials.ReceiverNodeIndex).ResetOwnershipServerRpc(GameManager.Instance.SenderLocalClient);
            //     MonopolyBoard.Instance.GetTileByIndex(credentials.ReceiverNodeIndex).UpdateOwnershipServerRpc(credentials.SenderNetworkIndex, GameManager.Instance.SenderLocalClient);
            // }

            // this.HandleTradeResponseClientRpc(credentials, GameManager.Instance.TargetAllClients);
        }

        [ClientRpc]
        private void HandleTradeResponseClientRpc(TradeCredentials credentials, ClientRpcParams clientRpcParams)
        {
            if (!credentials.AreValid)
                return;

            PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderNetworkIndex);

            if (sender.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                sender.HandleTradeResponse(credentials);
        }
    }
}
