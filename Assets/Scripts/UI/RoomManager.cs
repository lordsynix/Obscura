using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    private LobbyEventCallbacks callbacks;
    private ILobbyEvents events;

    private List<GameObject> playerList = new List<GameObject>();
    public GameObject playerItemPrefab;
    public Transform playerItemParent;

    private float refreshTimer;
    public List<GameObject> settings = new();

    void Start()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            if (PlayerData.Instance.currentLobby != null)
            {
                SubscribeToEvents();
            }
            else
            {
                SceneManager.LoadScene("Menu");
            }
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }

    private async void SubscribeToEvents()
    {
        if (PlayerData.Instance.currentLobby.HostId != AuthenticationService.Instance.PlayerId)
        {
            callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            callbacks.KickedFromLobby += KickedFromLobby;

            events = await Lobbies.Instance.SubscribeToLobbyEventsAsync(PlayerData.Instance.currentLobby.Id, callbacks);
        }
    }

    private void Update()
    {
        RefreshPlayers();
    }

    private void RefreshPlayers()
    {
        if (PlayerData.Instance.currentLobby != null)
        {
            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0)
            {
                refreshTimer = 1.1f;
                ListPlayers();
            }
        }
    }

    private async void ListPlayers()
    {
        try
        {
            PlayerData.Instance.currentLobby = await Lobbies.Instance.GetLobbyAsync(PlayerData.Instance.currentLobby.Id);
            ClearList(playerList);

            foreach (Player player in PlayerData.Instance.currentLobby.Players)
            {
                GameObject playerItem = Instantiate(playerItemPrefab, playerItemParent);
                playerItem.GetComponent<PlayerItem>().SetPlayerData(player.Data["Username"].Value, player.Id);
                playerList.Add(playerItem);

                if (PlayerData.Instance.currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    if (player.Id != AuthenticationService.Instance.PlayerId)
                    {
                        playerItem.GetComponent<PlayerItem>().EnableKick();
                    }
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void ClearList(List<GameObject> list)
    {
        foreach (GameObject go in list)
        {
            Destroy(go);
        }
        playerList.Clear();
    }

    public async void KickPlayer(string id)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(PlayerData.Instance.currentLobby.Id, id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            ErrorManager.Instance.DisplayError(103, "This Lobby doesn't exist anymore");
            SceneManager.LoadScene("Lobby");
        }
        if (changes.Data.Changed)
        {
            if (changes.Data.Value.ContainsKey("ConnectionKey"))
            {
                string key = changes.Data.Value["ConnectionKey"].Value.Value;
                PlayerData.Instance.connectionKey = key;
            }
        }
    }

    private void KickedFromLobby()
    {
        PlayerData.Instance.currentLobby = null;
        ErrorManager.Instance.DisplayError(104, "You were kicked from the lobby by the host");
        SceneManager.LoadScene("Lobby");
    }

    public async void OnClickStart()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(PlayerData.Instance.currentLobby.MaxPlayers);
        PlayerData.Instance.allocation = allocation;

        UpdateLobbyOptions updateOptions = new UpdateLobbyOptions();
        updateOptions.Data = PlayerData.Instance.currentLobby.Data;
        PlayerData.Instance.currentLobby.Data.Remove("ConnectionKey");

        updateOptions.Data.Add("ConnectionKey",

            new DataObject(
                visibility: DataObject.VisibilityOptions.Member,
                value: await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId),
                index: DataObject.IndexOptions.S1));

        PlayerData.Instance.currentLobby = await LobbyService.Instance.UpdateLobbyAsync(PlayerData.Instance.currentLobby.Id, updateOptions);
    }
}
