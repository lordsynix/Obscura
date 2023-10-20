using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System;
using System.Linq;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class LobbyManager : MonoBehaviour
{
    public GameObject creatorPanel;
    public InputField roomNameInput;
    public Text errorText;
    public Slider playerCountSlider;
    private string mapSelection;

    private LobbyEventCallbacks callbacks;
    private ILobbyEvents events;

    private List<GameObject> roomsList = new List<GameObject>();
    public GameObject roomItemPrefab;
    public Transform roomContainer;
    private float refreshTime;

    private void Start()
    {
        Authentication();
    }

    // Authentication
    #region
    public async void Authentication()
    {
        await Authenticate();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as player id: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task Authenticate()
    {
        var options = new InitializationOptions();

#if UNITY_EDITOR
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif

        await UnityServices.InitializeAsync(options);
    }
    #endregion

    private void Update()
    {
        RefreshLobbies();
    }

    private void RefreshLobbies()
    {
        if (PlayerData.Instance.currentLobby == null)
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                refreshTime -= Time.deltaTime;
                if (refreshTime <= 0)
                {
                    refreshTime = 1.1f;
                    ListLobbies();
                }
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            foreach (GameObject room in roomsList)
            {
                Destroy(room);
            }
            roomsList.Clear();

            foreach (Lobby lobby in queryResponse.Results)
            {
                RoomItem roomItem = Instantiate(roomItemPrefab, roomContainer).GetComponent<RoomItem>();
                roomItem.roomName.text = lobby.Name;
                roomItem.mapName.text = PlayerData.Instance.currentLobby.Data["Map"].Value;
                roomItem.hostName.text = PlayerData.Instance.currentLobby.Data["Host"].Value;
                roomItem.SetPlayerCount(lobby.Players.Count, lobby.MaxPlayers);

                roomItem.roomId = lobby.Id;
                roomsList.Add(roomItem.gameObject);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void OnClickMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OnClickCreate()
    {
        creatorPanel.SetActive(true);
    }

    public void OnClickCancel()
    {
        roomNameInput.text = string.Empty;
        creatorPanel.SetActive(false);
    }

    public void OnClickConfirm()
    {
        if (roomNameInput.text.ToCharArray().Length <= 15)
        {
            CreateLobby();
        }
        else
        {
            errorText.text = "Room name exceeded character limit of 15!";
        }
    }

    private async void CreateLobby()
    {
        string lobbyName = roomNameInput.text;
        int maxPlayers = (int)playerCountSlider.value;
        string map = mapSelection;

        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
        {
            Data = new Dictionary<string, DataObject>()
            {
                { "ConnectionKey", new DataObject(
                    visibility: DataObject.VisibilityOptions.Member,
                    value: "null",
                    index: DataObject.IndexOptions.S1)
                },

                { "Map", new DataObject(
                    visibility: DataObject.VisibilityOptions.Public,
                    value: map,
                    index: DataObject.IndexOptions.S2) 
                },

                { "Host", new DataObject(
                    visibility: DataObject.VisibilityOptions.Public,
                    value: PlayerData.Instance.username,
                    index: DataObject.IndexOptions.S3)
                },
            },

            Player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "Username", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerData.Instance.username) }
                }
            }
        };

        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            PlayerData.Instance.currentLobby = lobby;
            SceneManager.LoadScene("Game");
        }
        catch (LobbyServiceException e)
        {
            ErrorManager.Instance.DisplayError(100, "Couldn't create Lobby. There might be something wrong with your internet connection.");
            Debug.LogWarning(e);
        }
    }

    public async void JoinLobby(string roomId)
    {
        JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions()
        {
            Player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "Username", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerData.Instance.username) }
                }
            }
        };

        try
        {
            await LobbyService.Instance.JoinLobbyByIdAsync(roomId, joinLobbyByIdOptions);
            PlayerData.Instance.currentLobby = await LobbyService.Instance.GetLobbyAsync(roomId);

            SceneManager.LoadScene("Game");
        }
        catch (LobbyServiceException e)
        {
            ErrorManager.Instance.DisplayError(101, "Couldn't join Lobby. There might be something wrong with your internet connection.");
            Debug.LogWarning(e);
        }
    }
}
