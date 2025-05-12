using System;
using Unity.Netcode;
using Monopoly.Client.Runtime.Game.Managers;

namespace Monopoly.Client.Runtime.Game.Serializables
{
    enum TradeResult : byte
    {
        None,
        Success,
        Failure
    }

    internal struct TradeCredentialsSerializable : INetworkSerializable, IEquatable<TradeCredentialsSerializable>
    {
        internal const int PLACEHOLDER = -1;

        internal static readonly TradeCredentialsSerializable Blank = new TradeCredentialsSerializable()
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

        public bool Equals(TradeCredentialsSerializable other)
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
            return obj is TradeCredentialsSerializable other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Result, SenderNetworkIndex, ReceiverNetworkIndex, SenderNodeIndex, ReceiverNodeIndex, SenderBalanceAmount, ReceiverBalanceAmount);
        }

        public static bool operator ==(TradeCredentialsSerializable current, TradeCredentialsSerializable other)
        {
            return current.Equals(other);
        }

        public static bool operator !=(TradeCredentialsSerializable current, TradeCredentialsSerializable other)
        {
            return !current.Equals(other);
        }
    }
}
