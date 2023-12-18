using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData : MonoBehaviour
{
    public string username = string.Empty;
    public static PlayerData Instance;
    public Lobby currentLobby;
    public string connectionKey = string.Empty;
    public Allocation allocation;

    private void Awake()
    {
        Instance = this;
    }  

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("Menu");
    }
}
