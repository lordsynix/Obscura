using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.UIElements;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;

public class RoomManager : MonoBehaviour
{
    private LobbyEventCallbacks callbacks;
    private ILobbyEvents events;

    private List<GameObject> playerList = new List<GameObject>();
    public GameObject playerItemPrefab;
    public Transform playerItemParent;

    private float refreshTimer;
    public List<Text> settings = new();
    private int[] playerSettings = new int[] { 1, 2, 3, 4, 5, 6 };
    private int[] timeSettings = new int[] { 20, 40, 60, 80, 100, 120, 140, 200, 300 , 600};
    private string[] positionSettings = new string[] { "Auto", "Select", "Random" };
    private int currentPlayerSetting = 3;
    private int currentTimeSetting = 5;
    private int currentPositionSetting = 1;
    private float updateTime = 0;
    private float updateDelay = 0.5f;
    private UpdateLobbyOptions queuedUpdate;

    public GameObject roomPanel;

    void Start()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            if (PlayerData.Instance.currentLobby != null)
            {
                SubscribeToEvents();
                currentPlayerSetting = PlayerData.Instance.currentLobby.MaxPlayers - 1;
                settings[0].text = PlayerData.Instance.currentLobby.Players.Count.ToString() + "/" + PlayerData.Instance.currentLobby.MaxPlayers.ToString();
                settings[1].text = timeSettings[currentTimeSetting].ToString();
                settings[2].text = positionSettings[currentPositionSetting].ToString();
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

        if (queuedUpdate != null)
        {
            if (updateTime + updateDelay < Time.time)
            {
                UpdateLobby(queuedUpdate);
                queuedUpdate = null;
            }
        }

        if (PlayerData.Instance.currentLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            settings[0].text = PlayerData.Instance.currentLobby.Players.Count.ToString() + "/" + playerSettings[currentPlayerSetting].ToString();
            settings[1].text = timeSettings[currentTimeSetting].ToString() + "s";
            settings[2].text = positionSettings[currentPositionSetting];
        }
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

    private async void UpdateLobby(UpdateLobbyOptions updateLobbyOptions)
    {
        await LobbyService.Instance.UpdateLobbyAsync(PlayerData.Instance.currentLobby.Id, queuedUpdate);
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
        if (changes.MaxPlayers.Changed)
        {
            settings[0].text = PlayerData.Instance.currentLobby.Players.Count.ToString() + "/" + changes.MaxPlayers.Value.ToString();
        }
        if (changes.Data.Changed)
        {
            if (changes.Data.Value.ContainsKey("ConnectionKey"))
            {
                string key = changes.Data.Value["ConnectionKey"].Value.Value;
                PlayerData.Instance.connectionKey = key;
                JoinRelay();
                roomPanel.SetActive(false);
            }
            if (changes.Data.Value.ContainsKey("Time"))
            {
                settings[1].text = changes.Data.Value["Time"].Value.Value + "s";
            }
            if (changes.Data.Value.ContainsKey("Position"))
            {
                settings[2].text = changes.Data.Value["Position"].Value.Value;
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

        CreateRelay();
        PlayerData.Instance.currentLobby = await LobbyService.Instance.UpdateLobbyAsync(PlayerData.Instance.currentLobby.Id, updateOptions);   
        roomPanel.SetActive(false);
    }

    public void ChangeSettings(string setting)
    {
        string[] settingString = setting.Split(',');
        int settingIndex = int.Parse(settingString[0]);
        int sign = int.Parse(settingString[1]);
        if (AuthenticationService.Instance.PlayerId == PlayerData.Instance.currentLobby.HostId)
        {
            if (settingIndex == 0)
            {                
                currentPlayerSetting += sign;
                if (currentPlayerSetting < 0)
                {
                    currentPlayerSetting = playerSettings.Length - 1;
                }
                if (currentPlayerSetting > playerSettings.Length - 1)
                {
                    currentPlayerSetting = 0;
                }
                if (playerSettings[currentPlayerSetting] < PlayerData.Instance.currentLobby.Players.Count)
                {
                    currentPlayerSetting = PlayerData.Instance.currentLobby.Players.Count - 1;
                }
            }
            if (settingIndex == 1)
            {
                currentTimeSetting += sign;
                if (currentTimeSetting < 0)
                {
                    currentTimeSetting = timeSettings.Length - 1;
                }
                if (currentTimeSetting > timeSettings.Length - 1)
                {
                    currentTimeSetting = 0;
                }
            }
            if (settingIndex == 2)
            {
                currentPositionSetting += sign;
                if (currentPositionSetting < 0)
                {
                    currentPositionSetting = positionSettings.Length - 1;
                }
                if (currentPositionSetting > positionSettings.Length - 1)
                {
                    currentPositionSetting = 0;
                }
            }                        

            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions();
            updateOptions.Data = PlayerData.Instance.currentLobby.Data;

            updateOptions.MaxPlayers = playerSettings[currentPlayerSetting];

            if (updateOptions.Data.ContainsKey("Time"))
            {
                updateOptions.Data.Remove("Time");
            }
            updateOptions.Data.Add("Time",
                new DataObject(
                    visibility: DataObject.VisibilityOptions.Member,
                    value: timeSettings[currentTimeSetting].ToString(),
                    index: DataObject.IndexOptions.N1));

            if (updateOptions.Data.ContainsKey("Position"))
            {
                updateOptions.Data.Remove("Position");
            }
            updateOptions.Data.Add("Position",
                new DataObject(
                    visibility: DataObject.VisibilityOptions.Member,
                    value: positionSettings[currentPositionSetting],
                    index: DataObject.IndexOptions.S4));

            updateTime = Time.time;
            queuedUpdate = updateOptions;
        }          
    }

    private void CreateRelay()
    {
        try
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                PlayerData.Instance.allocation.RelayServer.IpV4,
                (ushort)PlayerData.Instance.allocation.RelayServer.Port,
                PlayerData.Instance.allocation.AllocationIdBytes,
                PlayerData.Instance.allocation.Key,
                PlayerData.Instance.allocation.ConnectionData
                );

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinRelay()
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(PlayerData.Instance.connectionKey);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
