using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

internal abstract class PawnController : NetworkBehaviour
{
    [Header("Visuals")]

    [Space]
    [SerializeField]
    private protected Image pawnImageToken;

    private protected bool IsInJail;
    private protected bool IsSkipTurn;
    private protected int TurnsInJailCount;

    internal Action OnBalanceUpdated;

    internal int NetworkIndex { get; private set; }
    internal string Nickname { get; private protected set; }

    internal Color PawnColor { get; private set; }
    internal MonopolyNode CurrentNode { get; private set; }
    internal NetworkVariable<int> Balance { get; private set; }
    internal List<MonopolyNode> OwnedNodes { get; private set; }
    internal int NetWorth => this.Balance.Value + this.OwnedNodes.Sum(node => node.WorthTotal);

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
        this.NetworkIndex = GameManager.Instance.PawnsCount;

        this.Balance = new NetworkVariable<int>(GameManager.Instance.StartingBalance, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        this.Balance.OnValueChanged += (previousValue, newValue) => this.OnBalanceUpdated?.Invoke();

        // if (NetworkManager.Singleton.IsHost)
        //     this.Balance.Value = GameManager.Instance.StartingBalance;

        if (NetworkManager.Singleton.IsHost)
        {
            if (GameManager.Instance.PawnsCount == 0)
                this.Balance.Value = GameManager.Instance.StartingBalance;
            else
                this.Balance.Value = 5000;
        }

        this.OwnedNodes = new List<MonopolyNode>();
        this.CurrentNode = MonopolyBoard.Instance.NodeStart;
        this.transform.position = MonopolyBoard.Instance.NodeStart.transform.position;
        this.PawnColor = GameManager.Instance.PawnsVisuals[this.NetworkIndex].PawnTokenColor;
        this.pawnImageToken.sprite = GameManager.Instance.PawnsVisuals[this.NetworkIndex].PawnTokenSprite;

        GameManager.Instance.AddPawnController(this);
    }

    private void MoveToken(int steps)
    {
        const float POSITION_THRESHOLD = 0.001f;

        Vector3 targetPosition;
        bool hasMovedOverStart = false;
        int currentNodeIndex = MonopolyBoard.Instance.GetIndexOfNode(this.CurrentNode);

        this.StartCoroutine(MoveCoroutine());

        IEnumerator MoveCoroutine()
        {
            while (steps != 0)
            {
                if (steps < 0)
                {
                    ++steps;
                    currentNodeIndex = Mathf.Abs(--currentNodeIndex + MonopolyBoard.Instance.NodesCount) % MonopolyBoard.Instance.NodesCount;
                }
                else
                {
                    --steps;
                    currentNodeIndex = ++currentNodeIndex % MonopolyBoard.Instance.NodesCount;
                }

                targetPosition = MonopolyBoard.Instance.GetNodeByIndex(currentNodeIndex).transform.position;

                if (MonopolyBoard.Instance.NodeStart == MonopolyBoard.Instance.GetNodeByIndex(currentNodeIndex))
                    hasMovedOverStart = true;

                yield return StartCoroutine(MoveStepCoroutine(targetPosition));
            }

            this.CurrentNode = MonopolyBoard.Instance.GetNodeByIndex(currentNodeIndex);

            if (hasMovedOverStart && this.CurrentNode != MonopolyBoard.Instance.NodeStart)
                this.UpdateBalanceServerRpc(this.NetworkIndex, this.Balance.Value + GameManager.Instance.CircleBonus, GameManager.Instance.SenderLocalClient);

            this.HandleLanding();
        }

        IEnumerator MoveStepCoroutine(Vector3 targetPosition)
        {
            while (Vector3.Distance(this.transform.position, targetPosition) > POSITION_THRESHOLD)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, GameManager.Instance.PawnMovementSpeed * Time.deltaTime);
                yield return null;
            }

            this.transform.position = targetPosition;
        }
    }

    private void HandleLanding()
    {
        switch (this.CurrentNode.NodeType)
        {
            case MonopolyNode.Type.Tax:
                this.HandleChanceLanding();
                break;
            case MonopolyNode.Type.Jail:
                this.HandleJailLanding();
                break;
            case MonopolyNode.Type.Start:
                this.HandleStartLanding();
                break;
            case MonopolyNode.Type.Chance:
                this.HandleChanceLanding();
                break;
            case MonopolyNode.Type.SendJail:
                this.HandleSendJailLanding();
                break;
            case MonopolyNode.Type.Property:
                this.HandlePropertyLanding();
                break;
            case MonopolyNode.Type.Gambling:
                this.HandlePropertyLanding();
                break;
            case MonopolyNode.Type.Transport:
                this.HandlePropertyLanding();
                break;
            case MonopolyNode.Type.FreeParking:
                this.HandleFreeParkingLanding();
                break;
        }
    }

    internal void GoToJail()
    {
        this.IsInJail = true;
        this.TurnsInJailCount = 0;
        this.MoveToken(MonopolyBoard.Instance.GetDistance(this.CurrentNode, MonopolyBoard.Instance.NodeJail));
    }

    internal bool HasFullMonopoly(MonopolySet monopolySet)
    {
        if (monopolySet == null)
            throw new System.NullReferenceException($"{nameof(monopolySet)} cannot be null.");

        return this.OwnedNodes.Where(node => node.AffiliatedMonopoly == monopolySet).Count() == monopolySet.NodesInSet.Count;
    }

    internal bool HasPartialMonopoly(MonopolySet monopolySet)
    {
        if (monopolySet == null)
            throw new System.NullReferenceException($"{nameof(monopolySet)} cannot be null.");

        return this.OwnedNodes.Where(node => node.AffiliatedMonopoly == monopolySet).Count() > 1;
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SurrenderServerRpc(ServerRpcParams serverRpcParams)
    {
        MonopolyNode[] monopolyNodes = this.OwnedNodes.ToArray();

        foreach (MonopolyNode monopolyNode in monopolyNodes)
            monopolyNode.ResetOwnershipServerRpc(GameManager.Instance.SenderLocalClient);

        GameManager.Instance.GetPawnPanel(this.NetworkIndex).GetComponent<NetworkObject>().Despawn();
        GameManager.Instance.GetPawnController(this.NetworkIndex).GetComponent<NetworkObject>().Despawn();
        GameManager.Instance.RemoveSurrenderedPawn(this.NetworkIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private protected void UpdateBalanceServerRpc(int networkIndex, int newBalance, ServerRpcParams serverRpcParams)
    {
        GameManager.Instance.GetPawnController(networkIndex).Balance.Value = newBalance;
    }

    private protected void PerformChanceAction(ChanceNodeSO chanceNode)
    {
        switch (chanceNode.ChanceType)
        {
            case ChanceNodeSO.Type.Reward:
                this.UpdateBalanceServerRpc(this.NetworkIndex, this.Balance.Value + chanceNode.Reward, GameManager.Instance.SenderLocalClient);
                this.CompleteTurn();
                break;
            case ChanceNodeSO.Type.SkipTurn:
                this.IsSkipTurn = true;
                this.CompleteTurn();
                break;
            case ChanceNodeSO.Type.SendJail:
                this.GoToJail();
                break;
            case ChanceNodeSO.Type.MoveForward:
                GameManager.Instance.RollDice();
                UIManagerMonopolyGame.Instance.ShowDiceAnimation();
                this.MoveToken(GameManager.Instance.TotalRollResult);
                break;
            case ChanceNodeSO.Type.MoveBackwards:
                GameManager.Instance.RollDice();
                UIManagerMonopolyGame.Instance.ShowDiceAnimation();
                this.MoveToken(-GameManager.Instance.TotalRollResult);
                break;
        }
    }

    private protected void PerformDiceRolling()
    {
        if (this.IsInJail)
        {
            ++this.TurnsInJailCount;

            if (GameManager.Instance.HasRolledDouble || this.TurnsInJailCount > GameManager.Instance.MaxTurnsInJail)
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
        if (this.IsSkipTurn || this.IsInJail)
            GameManager.Instance.SwitchPlayerForcefullyServerRpc(GameManager.Instance.SenderLocalClient);
        else
            GameManager.Instance.SwitchPawnServerRpc(GameManager.Instance.SenderLocalClient);
    }

    [ServerRpc]
    internal void SendTradeServerRpc(TradeCredentials credentials, ServerRpcParams serverRpcParams)
    {
        this.ReceiveTradeClientRpc(credentials, GameManager.Instance.TargetAllClients);
    }

    [ClientRpc]
    private protected void ReceiveTradeClientRpc(TradeCredentials credentials, ClientRpcParams clientRpcParams)
    {
        if (this.NetworkIndex == credentials.ReceiverNetworkIndex)
            this.RespondToTrade(credentials);
    }

    [ServerRpc]
    private protected void AcceptTradeServerRpc(TradeCredentials credentials, ServerRpcParams serverRpcParams)
    {
        PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderNetworkIndex);
        PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverNetworkIndex);

        this.UpdateBalanceServerRpc(credentials.SenderNetworkIndex, sender.Balance.Value - credentials.SenderBalanceAmount, GameManager.Instance.SenderLocalClient);
        this.UpdateBalanceServerRpc(credentials.ReceiverNetworkIndex, receiver.Balance.Value + credentials.SenderBalanceAmount, GameManager.Instance.SenderLocalClient);

        this.UpdateBalanceServerRpc(credentials.SenderNetworkIndex, sender.Balance.Value + credentials.ReceiverBalanceAmount, GameManager.Instance.SenderLocalClient);
        this.UpdateBalanceServerRpc(credentials.ReceiverNetworkIndex, receiver.Balance.Value - credentials.ReceiverBalanceAmount, GameManager.Instance.SenderLocalClient);

        if (credentials.SenderNodeIndex == TradeCredentials.NODE_INDEX_PLACEHOLDER)
            MonopolyBoard.Instance.GetNodeByIndex(credentials.SenderNodeIndex).UpdateOwnershipServerRpc(credentials.ReceiverNetworkIndex, GameManager.Instance.SenderLocalClient);

        if (credentials.ReceiverNodeIndex == TradeCredentials.NODE_INDEX_PLACEHOLDER)
            MonopolyBoard.Instance.GetNodeByIndex(credentials.ReceiverNodeIndex).UpdateOwnershipServerRpc(credentials.SenderNetworkIndex, GameManager.Instance.SenderLocalClient);

        if (this.NetworkIndex == credentials.ReceiverNetworkIndex)
            this.HandleTradeResponse(credentials);
    }

    [ServerRpc(RequireOwnership = false)]
    private protected void DeclineTradeServerRpc(TradeCredentials credentials, ServerRpcParams serverRpcParams)
    {
        if (this.NetworkIndex == credentials.ReceiverNetworkIndex)
            this.HandleTradeResponse(credentials);
    }

}
