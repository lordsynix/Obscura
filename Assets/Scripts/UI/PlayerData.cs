using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData : MonoBehaviour
{
    public string username;
    public static PlayerData Instance;
    public Lobby currentLobby;

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
