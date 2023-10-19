using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public Text roomName;
    public Text mapName;
    public Text hostName;
    public Text playerCountText;
    LobbyManager lobbyManager;
    public string roomId;

    private void Start()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();
    }

    public void SetPlayerCount(int count, int maxCount)
    {
        playerCountText.text = count.ToString() + "/" + maxCount.ToString();
    }

    public void OnClickItem()
    {
        lobbyManager.JoinLobby(roomId);
    }

}