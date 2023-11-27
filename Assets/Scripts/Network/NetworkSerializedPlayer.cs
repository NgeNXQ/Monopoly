using Unity.Netcode;

public struct SerializedPlayer : INetworkSerializable
{
    public float PositionX;
    public float PositionY;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PositionX);
        serializer.SerializeValue(ref PositionY);
    }
}