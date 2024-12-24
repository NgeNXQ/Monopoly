using System;
using Unity.Netcode;

enum TradeResult : byte
{
    None,
    Success,
    Failure
}

internal struct TradeCredentials : INetworkSerializable, IEquatable<TradeCredentials>
{
    internal const int PLACEHOLDER = -1;

    internal static readonly TradeCredentials Blank = new TradeCredentials()
    {
        Result = TradeResult.None,
        SenderNetworkIndex = PLACEHOLDER,
        ReceiverNetworkIndex = PLACEHOLDER,
        SenderNodeIndex = PLACEHOLDER,
        ReceiverNodeIndex = PLACEHOLDER,
        SenderBalanceAmount = PLACEHOLDER,
        ReceiverBalanceAmount = PLACEHOLDER,
    };

    internal TradeResult Result;
    internal int SenderNetworkIndex;
    internal int ReceiverNetworkIndex;
    internal int SenderNodeIndex;
    internal int ReceiverNodeIndex;
    internal int SenderBalanceAmount;
    internal int ReceiverBalanceAmount;

    internal bool AreValid
    {
        get
        {
            if (SenderNetworkIndex == ReceiverNetworkIndex)
                return false;

            if (SenderNodeIndex == PLACEHOLDER && ReceiverNodeIndex == PLACEHOLDER)
                return false;

            if (GameManager.Instance.GetPawnController(ReceiverNetworkIndex) == null)
                return false;

            return true;
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Result);
        serializer.SerializeValue(ref SenderNetworkIndex);
        serializer.SerializeValue(ref ReceiverNetworkIndex);
        serializer.SerializeValue(ref SenderNodeIndex);
        serializer.SerializeValue(ref ReceiverNodeIndex);
        serializer.SerializeValue(ref SenderBalanceAmount);
        serializer.SerializeValue(ref ReceiverBalanceAmount);
    }

    public bool Equals(TradeCredentials other)
    {
        if (this.Result != other.Result)
            return false;

        if (this.SenderNetworkIndex != other.SenderNetworkIndex)
            return false;

        if (this.ReceiverNetworkIndex != other.ReceiverNetworkIndex)
            return false;

        if (this.SenderNodeIndex != other.SenderNodeIndex)
            return false;

        if (this.ReceiverNodeIndex != other.ReceiverNodeIndex)
            return false;

        if (this.SenderBalanceAmount != other.SenderBalanceAmount)
            return false;

        if (this.ReceiverBalanceAmount != other.ReceiverBalanceAmount)
            return false;

        return true;
    }

    public override bool Equals(object obj)
    {
        return obj is TradeCredentials other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Result, SenderNetworkIndex, ReceiverNetworkIndex, SenderNodeIndex, ReceiverNodeIndex, SenderBalanceAmount, ReceiverBalanceAmount);
    }

    public static bool operator ==(TradeCredentials current, TradeCredentials other)
    {
        return current.Equals(other);
    }

    public static bool operator !=(TradeCredentials current, TradeCredentials other)
    {
        return !current.Equals(other);
    }
}
