using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Monopoly.Client.Shared.Factories;
using Monopoly.Client.Runtime.Game.Managers;
using Monopoly.Client.Runtime.UI.Managers.Game;
using Monopoly.Client.Runtime.Game.Models;
using Monopoly.Client.Runtime.Game.Gameplay.Boards;
using Monopoly.Client.Runtime.Game.Gameplay.Groups;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;
using Monopoly.Client.Runtime.Game.Controllers.Strategies.Shared;
using Monopoly.Client.Scriptable.Objects.Cards.Common;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Unity.Services.Lobbies.Models.Extensions;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Concrete;

namespace Monopoly.Client.Runtime.Game.Controllers.Pawns.Common
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
        private protected int TurnsInJailCount;

        internal int PawnId { get; private set; }
        internal Color PawnColor { get; private set; }
        internal Tile CurrentTile { get; private set; }
        internal NetworkVariable<int> Balance { get; private set; }
        internal List<PropertyTile> OwnedTiles { get; private set; }
        internal int NetWorth => this.Balance.Value + this.OwnedTiles.Sum(tile => tile.Worth);

        internal bool IsBot { get; private protected set; }
        internal bool IsPlayer { get; private protected set; }
        internal string Nickname { get; private protected set; }
        internal ISimpleFactory<ITileStrategy, Tile> FactoryTileStrategy { get; private protected set; }
        internal ISimpleFactory<ICardStrategy, CardScriptableObject> FactoryCardStrategy { get; private protected set; }

        internal abstract void PerformTurn();
        internal abstract void RespondToTrade(TradeCredentialsSerializable credentials);
        internal abstract void HandleTradeResponse(TradeCredentialsSerializable credentials);

        private void Awake()
        {
            this.Balance = new NetworkVariable<int>(
                GameManager.Instance.InitialBalance,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

            if (NetworkManager.Singleton.IsHost)
                this.Balance.Value = GameManager.Instance.InitialBalance;

            this.OwnedTiles = new List<PropertyTile>();
            this.PawnId = GameManager.Instance.PawnsCount;
            this.PawnColor = GameManager.Instance.Tokens[this.PawnId].ColorToken;

            GameManager.Instance.RegisterPawnController(this);
        }

        private void Start()
        {
            this.CurrentTile = Board.Instance.TileStart;
            this.transform.position = Board.Instance.TileStart.transform.position;
            this.pawnImageToken.sprite = GameManager.Instance.Tokens[this.PawnId].SpriteToken;
        }

        internal int GetBalance()
        {
            return this.Balance.Value;
        }

        internal void TransactDumpBalance(int amount)
        {
            this.TransactDumpBalanceRemotelyRpc(amount);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void TransactDumpBalanceRemotelyRpc(int amount)
        {
            this.Balance.Value -= amount;
        }

        internal void TransactTakeBalance(int amount)
        {
            this.TransactTakeBalanceRemotelyRpc(amount);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void TransactTakeBalanceRemotelyRpc(int amount)
        {
            this.Balance.Value += amount;
        }

        internal void TransactSendBalance(PawnController receiver, int amount)
        {
            this.TransactSendBalanceRemotelyRpc(this.PawnId, receiver.PawnId, amount);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void TransactSendBalanceRemotelyRpc(int idSender, int idReceiver, int amount)
        {
            PawnController sender = GameManager.Instance.GetPawnController(idSender);
            PawnController receiver = GameManager.Instance.GetPawnController(idReceiver);

            if (receiver == null)
                return;

            if (sender == null)
            {
                receiver.Balance.Value += amount;
                return;
            }

            sender.Balance.Value -= amount;
            receiver.Balance.Value += amount;
        }

        private protected void RollDice()
        {
            GameManager.Instance.RollDice();
            UIManagerGame.Instance.ShowDiceAnimation();

            if (this.IsInJail)
            {
                ++this.TurnsInJailCount;

                if (GameManager.Instance.HasRolledDouble || this.TurnsInJailCount > GameManager.Instance.MaxJailTurns)
                    this.ReleaseFromJail();
                else
                    this.CompleteTurn();
            }
            else
            {
                this.MoveToken(GameManager.Instance.TotalRollResult);
            }
        }

        private void ReleaseFromJail()
        {
            this.IsInJail = false;
            this.TurnsInJailCount = 0;
            this.MoveToken(GameManager.Instance.TotalRollResult);
        }

        internal void CompleteTurn()
        {
            // if (this.IsInJail || this.IsSkipTurn)
            if (!this.IsInJail)
                GameManager.Instance.SwitchPawnSoftly();
            else
                GameManager.Instance.SwitchPawnForcefully();
        }

        internal void Surrender()
        {
            if (this == PlayerPawnController.LocalInstance)
            {
                string gameMode = LobbyManager.Instance.LocalLobby?.GetData(LobbyManager.KEY_LOBBY_DATA_GAME_MODE);

                if (gameMode == null)
                    return;

                if (gameMode.Equals(LobbyManager.VALUE_LOBBY_DATA_GAME_MODE_RANKED, StringComparison.OrdinalIgnoreCase))
                {
                    int trophies = int.Parse(GameCoordinator.Instance.LocalPlayer.GetData(GameCoordinator.KEY_PLAYER_DATA_TROPHIES));
                    GameCoordinator.Instance.ServiceAccount.PutAccountTrophies($"{trophies - new System.Random().Next(5, 15)}");
                }
            }

            IList<PropertyTile> ownedTiles = this.OwnedTiles.ToList();

            foreach (PropertyTile ownedTile in ownedTiles)
                ownedTile.ResetOwnership();

            if (GameManager.Instance.CurrentPawn == this)
                GameManager.Instance.SwitchPawnForcefully();

            GameManager.Instance.DespawnPawnItemsRemotelyRpc(this.PawnId);
        }

        internal void MoveToJail()
        {
            this.IsInJail = true;
            this.TurnsInJailCount = 0;
            this.MoveToken(Board.Instance.GetDistance(this.CurrentTile, Board.Instance.TileJail));
        }

        internal void MoveToken(int steps)
        {
            if (this == null)
                return;

            this.StartCoroutine(this.MoveCoroutine(steps));
        }

        private IEnumerator MoveCoroutine(int steps)
        {
            Vector3 targetPosition;
            bool hasMovedOverStart = false;
            int currentNodeIndex = Board.Instance.GetIndexOfTile(this.CurrentTile);

            while (steps != 0)
            {
                if (steps > 0)
                {
                    --steps;
                    currentNodeIndex = ++currentNodeIndex % Board.Instance.TilesCount;

                }
                else
                {
                    ++steps;
                    currentNodeIndex = Mathf.Abs(--currentNodeIndex + Board.Instance.TilesCount) % Board.Instance.TilesCount;
                }

                targetPosition = Board.Instance.GetTileByIndex(currentNodeIndex).transform.position;

                if (Board.Instance.TileStart == Board.Instance.GetTileByIndex(currentNodeIndex))
                    hasMovedOverStart = true;

                yield return StartCoroutine(this.MoveStepCoroutine(targetPosition));
            }

            this.CurrentTile = Board.Instance.GetTileByIndex(currentNodeIndex);

            if (hasMovedOverStart && this.CurrentTile != Board.Instance.TileStart)
                this.TransactTakeBalance(GameManager.Instance.CircleDefaultBonus);

            this.CurrentTile.HandleLanding(this);
        }

        private IEnumerator MoveStepCoroutine(Vector3 targetPosition)
        {
            const float POSITION_THRESHOLD = 0.001f;

            while (Vector3.Distance(this.transform.position, targetPosition) > POSITION_THRESHOLD)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, this.movementSpeed * Time.deltaTime);
                yield return null;
            }

            this.transform.position = targetPosition;
        }

        internal bool HasFullMonopoly(Group group)
        {
            if (group == null)
                throw new System.NullReferenceException($"{nameof(group)} cannot be null.");

            return this.OwnedTiles.Where(tile => tile.Monopoly == group).Count() == group.Tiles.Length;
        }

        internal bool HasPartialMonopoly(Group group)
        {
            if (group == null)
                throw new System.NullReferenceException($"{nameof(group)} cannot be null.");

            const float MONOPOLY_PERCENTAGE_THRESHOLD = 0.5f;
            return (float)this.OwnedTiles.Where(tile => tile.Monopoly == group).Count() / group.Tiles.Length >= MONOPOLY_PERCENTAGE_THRESHOLD;
        }

        internal void SendTrade(TradeCredentialsSerializable credentials)
        {
            this.SendTradeRemotelyRpc(credentials);
        }

        [Rpc(SendTo.Server, RequireOwnership = true)]
        private void SendTradeRemotelyRpc(TradeCredentialsSerializable credentials)
        {
            Debug.Log("SendTradeRemotelyRpc");

            if (!credentials.AreValid)
            {
                credentials.Result = TradeResult.Declined;
                this.HandleTradeResponseLocallyRpc(credentials);
                return;
            }

            PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderPawnId);

            if (sender == null)
                return;

            PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverPawnId);

            if (receiver == null)
            {
                credentials.Result = TradeResult.Declined;
                this.HandleTradeResponseLocallyRpc(credentials);
                return;
            }

            this.ReceiveTradeLocallyRpc(credentials);
        }

        [Rpc(SendTo.Everyone)]
        private void ReceiveTradeLocallyRpc(TradeCredentialsSerializable credentials)
        {
            Debug.Log("ReceiveTradeLocallyRpc");

            PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverPawnId);

            if (receiver?.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                receiver.RespondToTrade(credentials);

            if (NetworkManager.Singleton.IsHost)
            {
                if (receiver == null)
                {
                    credentials.Result = TradeResult.Declined;
                    this.HandleTradeResponseLocallyRpc(credentials);
                }
            }
        }

        private protected void AcceptTrade(TradeCredentialsSerializable credentials)
        {
            this.AcceptTradeRemotelyRpc(credentials);
        }

        [Rpc(SendTo.Server, RequireOwnership = true)]
        private void AcceptTradeRemotelyRpc(TradeCredentialsSerializable credentials)
        {
            Debug.Log("AcceptTradeRemotelyRpc");

            credentials.Result = TradeResult.Accepted;

            PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderPawnId);

            if (sender == null)
                return;

            PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverPawnId);

            if (receiver == null)
            {
                credentials.Result = TradeResult.Declined;
                this.HandleTradeResponseLocallyRpc(credentials);
                return;
            }

            if (credentials.SenderBalanceAmount > 0)
                sender.TransactSendBalance(receiver, credentials.SenderBalanceAmount);

            if (credentials.ReceiverBalanceAmount > 0)
                receiver.TransactSendBalance(sender, credentials.ReceiverBalanceAmount);

            if (credentials.SenderTileIndex != TradeCredentialsSerializable.PLACEHOLDER)
            {
                PropertyTile senderTile = Board.Instance.GetTileByIndex(credentials.SenderTileIndex) as PropertyTile;

                senderTile.ResetOwnership();
                senderTile.UpdateOwnership(receiver);
            }

            if (credentials.ReceiverTileIndex != TradeCredentialsSerializable.PLACEHOLDER)
            {
                PropertyTile receiverTile = Board.Instance.GetTileByIndex(credentials.ReceiverTileIndex) as PropertyTile;

                receiverTile.ResetOwnership();
                receiverTile.UpdateOwnership(sender);
            }

            this.HandleTradeResponseLocallyRpc(credentials);
        }

        private protected void DeclineTrade(TradeCredentialsSerializable credentials)
        {
            this.DeclineTradeRemotelyRpc(credentials);
        }

        [Rpc(SendTo.Server, RequireOwnership = true)]
        private protected void DeclineTradeRemotelyRpc(TradeCredentialsSerializable credentials)
        {
            Debug.Log("DeclineTradeRemotelyRpc");

            credentials.Result = TradeResult.Declined;
            this.HandleTradeResponseLocallyRpc(credentials);
        }

        [Rpc(SendTo.Everyone)]
        private void HandleTradeResponseLocallyRpc(TradeCredentialsSerializable credentials)
        {
            Debug.Log("HandleTradeResponseLocallyRpc");

            PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderPawnId);

            if (sender?.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                sender.HandleTradeResponse(credentials);
        }
    }
}
