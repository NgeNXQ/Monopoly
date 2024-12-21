using Unity.Netcode;

internal struct TradeCredentials : INetworkSerializable
{
    internal const int NODE_INDEX_PLACEHOLDER = -1;

    internal int SenderNetworkIndex;
    internal int ReceiverNetworkIndex;
    internal int SenderNodeIndex;
    internal int ReceiverNodeIndex;
    internal int SenderBalanceAmount;
    internal int ReceiverBalanceAmount;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref this.SenderNetworkIndex);
        serializer.SerializeValue(ref this.ReceiverNetworkIndex);
        serializer.SerializeValue(ref this.SenderNodeIndex);
        serializer.SerializeValue(ref this.ReceiverNodeIndex);
        serializer.SerializeValue(ref this.SenderBalanceAmount);
        serializer.SerializeValue(ref this.ReceiverBalanceAmount);
    }
}
