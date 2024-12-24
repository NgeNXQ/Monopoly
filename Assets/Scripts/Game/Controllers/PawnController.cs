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

    private protected const float TURN_DELAY = 0.25f;

    private protected bool IsInJail;
    private protected bool IsSkipTurn;
    private protected int TurnsInJailCount;

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

        if (NetworkManager.Singleton.IsHost)
            this.Balance.Value = GameManager.Instance.StartingBalance;

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

        const float MONOPOLY_PERCENTAGE_THRESHOLD = 0.5f;
        return (float)this.OwnedNodes.Where(node => node.AffiliatedMonopoly == monopolySet).Count() / monopolySet.NodesCount >= MONOPOLY_PERCENTAGE_THRESHOLD;
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SurrenderServerRpc(ServerRpcParams serverRpcParams)
    {
        MonopolyNode[] monopolyNodes = this.OwnedNodes.ToArray();

        foreach (MonopolyNode monopolyNode in monopolyNodes)
            monopolyNode.ResetOwnershipServerRpc(GameManager.Instance.SenderLocalClient);

        this.DeclineTradeServerRpc(TradeCredentials.Blank, GameManager.Instance.SenderLocalClient);

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
        if (this.IsInJail || this.IsSkipTurn)
            GameManager.Instance.SwitchPlayerForcefullyServerRpc(GameManager.Instance.SenderLocalClient);
        else
            GameManager.Instance.SwitchPawnServerRpc(GameManager.Instance.SenderLocalClient);
    }

    [ServerRpc]
    internal void SendTradeServerRpc(TradeCredentials credentials, ServerRpcParams serverRpcParams)
    {
        if (!credentials.AreValid)
            return;

#if UNITY_EDITOR || DEBUG
        MonopolyNode senderNode = null;
        MonopolyNode receiverNode = null;
        PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderNetworkIndex);
        PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverNetworkIndex);

        int senderBalance = credentials.SenderBalanceAmount;
        int receiverBalance = credentials.ReceiverBalanceAmount;

        if (credentials.SenderNodeIndex != TradeCredentials.PLACEHOLDER)
            senderNode = MonopolyBoard.Instance.GetNodeByIndex(credentials.SenderNodeIndex);

        if (credentials.ReceiverNodeIndex != TradeCredentials.PLACEHOLDER)
            receiverNode = MonopolyBoard.Instance.GetNodeByIndex(credentials.ReceiverNodeIndex);

        Debug.Log($"{sender.Nickname} sends offer to the {receiver.Nickname} ({senderNode?.name} and {senderBalance} for {receiverNode?.name} and {receiverBalance})");
#endif

        this.ReceiveTradeClientRpc(credentials, GameManager.Instance.TargetAllClients);
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
#if UNITY_EDITOR || DEBUG
        PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderNetworkIndex);
        PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverNetworkIndex);

        Debug.Log($"{sender?.Nickname} declined offer from {receiver?.Nickname}");
#endif

        credentials.Result = TradeResult.Failure;
        this.HandleTradeResponseClientRpc(credentials, GameManager.Instance.TargetAllClients);
    }

    [ServerRpc]
    private protected void AcceptTradeServerRpc(TradeCredentials credentials, ServerRpcParams serverRpcParams)
    {
        credentials.Result = TradeResult.Success;

        PawnController sender = GameManager.Instance.GetPawnController(credentials.SenderNetworkIndex);
        PawnController receiver = GameManager.Instance.GetPawnController(credentials.ReceiverNetworkIndex);

#if UNITY_EDITOR || DEBUG
        Debug.Log($"{sender.Nickname} accepted offer from {receiver.Nickname}");
#endif

        this.UpdateBalanceServerRpc(credentials.SenderNetworkIndex, sender.Balance.Value - credentials.SenderBalanceAmount, GameManager.Instance.SenderLocalClient);
        this.UpdateBalanceServerRpc(credentials.ReceiverNetworkIndex, receiver.Balance.Value + credentials.SenderBalanceAmount, GameManager.Instance.SenderLocalClient);

        this.UpdateBalanceServerRpc(credentials.SenderNetworkIndex, sender.Balance.Value + credentials.ReceiverBalanceAmount, GameManager.Instance.SenderLocalClient);
        this.UpdateBalanceServerRpc(credentials.ReceiverNetworkIndex, receiver.Balance.Value - credentials.ReceiverBalanceAmount, GameManager.Instance.SenderLocalClient);

        if (credentials.SenderNodeIndex != TradeCredentials.PLACEHOLDER)
        {
            MonopolyBoard.Instance.GetNodeByIndex(credentials.SenderNodeIndex).ResetOwnershipServerRpc(GameManager.Instance.SenderLocalClient);
            MonopolyBoard.Instance.GetNodeByIndex(credentials.SenderNodeIndex).UpdateOwnershipServerRpc(credentials.ReceiverNetworkIndex, GameManager.Instance.SenderLocalClient);
        }

        if (credentials.ReceiverNodeIndex != TradeCredentials.PLACEHOLDER)
        {
            MonopolyBoard.Instance.GetNodeByIndex(credentials.ReceiverNodeIndex).ResetOwnershipServerRpc(GameManager.Instance.SenderLocalClient);
            MonopolyBoard.Instance.GetNodeByIndex(credentials.ReceiverNodeIndex).UpdateOwnershipServerRpc(credentials.SenderNetworkIndex, GameManager.Instance.SenderLocalClient);
        }

        this.HandleTradeResponseClientRpc(credentials, GameManager.Instance.TargetAllClients);
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
