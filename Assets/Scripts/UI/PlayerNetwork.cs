using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public string username = string.Empty;
    public string id;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            username = PlayerPrefs.GetString("Username");
            id = AuthenticationService.Instance.PlayerId;            ;
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

    [ServerRpc(RequireOwnership = false)] private void UpdateInfoServerRpc(string username, string id)
    {
        UpdateInfoClientRpc(username, id);
    }

    [ClientRpc] private void UpdateInfoClientRpc(string username, string id)
    {
        this.username = username;
        this.id = id;
    }

    [ServerRpc] private void RequestInfoServerRpc()
    {
        SendInfoClientRpc();
    }

    [ClientRpc] private void SendInfoClientRpc()
    {
        username = PlayerPrefs.GetString("Username");
        id = AuthenticationService.Instance.PlayerId;
        UpdateInfoServerRpc(username, id);
    }
}
