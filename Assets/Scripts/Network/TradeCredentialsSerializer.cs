using Unity.Netcode;

public struct TradeCredentials : INetworkSerializable
{
    public ulong SenderId;
    public ulong ReceiverId;

    public int SenderOffer;
    public int ReceiverOffer;

    public int SenderNodeIndex;
    public int ReceiverNodeIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref this.SenderId);
        serializer.SerializeValue(ref this.ReceiverId);
        serializer.SerializeValue(ref this.SenderOffer);
        serializer.SerializeValue(ref this.ReceiverOffer);
        serializer.SerializeValue(ref this.SenderNodeIndex);
        serializer.SerializeValue(ref this.ReceiverNodeIndex);
    }
}