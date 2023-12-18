using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> username = new NetworkVariable<FixedString64Bytes>(string.Empty, writePerm: NetworkVariableWritePermission.Owner);
    public string id;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            username.Value = PlayerData.Instance.username;
            id = AuthenticationService.Instance.PlayerId;
            RequestInfoServerRpc();
        }       
    }

    private void Update()
    {
        if (id != string.Empty)
        {
            GameManager.Instance.ConnectedToServer(id);
        }
    }

    [ServerRpc(RequireOwnership = false)] private void UpdateInfoServerRpc(string id)
    {
        UpdateInfoClientRpc(id);
    }

    [ClientRpc] private void UpdateInfoClientRpc(string id)
    {
        this.id = id;
    }

    [ServerRpc] private void RequestInfoServerRpc()
    {
        SendInfoClientRpc();
    }

    [ClientRpc] private void SendInfoClientRpc()
    {
        id = AuthenticationService.Instance.PlayerId;
        UpdateInfoServerRpc(id);
    }
}
