using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    private RoomManager roomManager;
    private float maxConnectionTime = 15;

    private void Start()
    {
        Instance = this;
        roomManager = GetComponent<RoomManager>();
    }

    public IEnumerator CheckConnection()
    {
        DisplayLoading();
        yield return new WaitForSeconds(maxConnectionTime);
        if (NetworkManager.Singleton.IsServer)
        {
            KickInactivePlayers();
        }
    }

    private void DisplayLoading()
    {
        foreach (var player in roomManager.playerDict)
        {
            player.Value.GetComponent<PlayerItem>().kickButton.SetActive(false);
            player.Value.GetComponent<PlayerItem>().loadingIcon.SetActive(true);
        }
    }

    private void KickInactivePlayers()
    {
        
        foreach (var player in roomManager.playerDict)
        {
            bool active = false;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject.GetComponent<PlayerNetwork>().id == player.Key)
                {
                    active = true;
                    Debug.Log("Connected " + client.PlayerObject.GetComponent<PlayerNetwork>().username);
                }
            }  
            if (active == false)
            {
                StartCoroutine(KickDelay(player.Key));               
            }
        }

        StartCoroutine(StartDelay());
    }

    private IEnumerator KickDelay(string id)
    {
        DisplayKickClientRpc(id);
        yield return new WaitForSeconds(3);
        roomManager.KickPlayer(id);
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(5);
        StartGameClientRpc();
    }

    [ClientRpc] private void DisplayKickClientRpc(string id)
    {
        roomManager.playerDict[id].GetComponent<PlayerItem>().FailConnection();
    }

    [ClientRpc] private void DisplaySuccessionClientRpc(string id)
    {
        roomManager.playerDict[id].GetComponent<PlayerItem>().SucceedConnection();
    }

    [ClientRpc] private void StartGameClientRpc()
    {
        roomManager.roomPanel.SetActive(false);
    }

    public void ConnectedToServer(string id)
    {
        roomManager.playerDict[id].GetComponent<PlayerItem>().SucceedConnection();
    }
}
