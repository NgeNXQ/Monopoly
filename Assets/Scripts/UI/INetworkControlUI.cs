using Unity.Netcode;

internal interface INetworkControlUI
{
    [ServerRpc]
    public void ShowServerRpc(ServerRpcParams serverRpcParams);

    [ClientRpc]
    public void ShowClientRpc(ClientRpcParams clientRpcParams);

    [ServerRpc]
    public void HideServerRpc(ServerRpcParams serverRpcParams);

    [ClientRpc]
    public void HideClientRpc(ClientRpcParams clientRpcParams);
}
