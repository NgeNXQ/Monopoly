using Unity.Netcode;

internal interface INetworkControlUI
{
    [ServerRpc]
    public void ShowServerRpc(ServerRpcParams serverRpcParams);

    [ClientRpc]
    public void ShowClientRpc(ClientRpcParams clientRpcParams);
}
