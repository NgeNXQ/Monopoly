using System;
using Unity.Netcode;

namespace Monopoly.Client.Runtime.Game.Models
{
    internal enum TradeResult : byte
    {
        None,
        Accepted,
        Declined
    }

    internal struct TradeCredentialsSerializable : INetworkSerializable, IEquatable<TradeCredentialsSerializable>
    {
        internal const int PLACEHOLDER = -1;

        internal static readonly TradeCredentialsSerializable Blank = new TradeCredentialsSerializable()
        {
            Result = TradeResult.None,
            SenderPawnId = PLACEHOLDER,
            ReceiverPawnId = PLACEHOLDER,
            SenderTileIndex = PLACEHOLDER,
            ReceiverTileIndex = PLACEHOLDER,
            SenderBalanceAmount = PLACEHOLDER,
            ReceiverBalanceAmount = PLACEHOLDER,
        };

        internal TradeResult Result;
        internal int SenderPawnId;
        internal int ReceiverPawnId;
        internal int SenderTileIndex;
        internal int ReceiverTileIndex;
        internal int SenderBalanceAmount;
        internal int ReceiverBalanceAmount;

        internal bool AreValid
        {
            get
            {
                if (this.SenderBalanceAmount < 0)
                    return false;

                if (this.ReceiverBalanceAmount < 0)
                    return false;

                if (this.SenderTileIndex == TradeCredentialsSerializable.PLACEHOLDER && this.ReceiverTileIndex == TradeCredentialsSerializable.PLACEHOLDER)
                    return false;

                return true;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Result);
            serializer.SerializeValue(ref SenderPawnId);
            serializer.SerializeValue(ref ReceiverPawnId);
            serializer.SerializeValue(ref SenderTileIndex);
            serializer.SerializeValue(ref ReceiverTileIndex);
            serializer.SerializeValue(ref SenderBalanceAmount);
            serializer.SerializeValue(ref ReceiverBalanceAmount);
        }

        public bool Equals(TradeCredentialsSerializable other)
        {
            if (this.Result != other.Result)
                return false;

            if (this.SenderPawnId != other.SenderPawnId)
                return false;

            if (this.ReceiverPawnId != other.ReceiverPawnId)
                return false;

            if (this.SenderTileIndex != other.SenderTileIndex)
                return false;

            if (this.ReceiverTileIndex != other.ReceiverTileIndex)
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
            return HashCode.Combine(Result, SenderPawnId, ReceiverPawnId, SenderTileIndex, ReceiverTileIndex, SenderBalanceAmount, ReceiverBalanceAmount);
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
