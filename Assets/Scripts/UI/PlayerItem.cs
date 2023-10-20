using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    public Text nameField;
    public string id;
    public GameObject kickButton;

    private RoomManager roomManager;

    private void Start()
    {
        roomManager = FindObjectOfType<RoomManager>();
    }

    public void SetPlayerData(string name, string newId)
    {
        nameField.text = name;
        id = newId;
    }

    public void EnableKick()
    {
        kickButton.SetActive(true);
    }

    public void OnClickKick()
    {
        roomManager.KickPlayer(id);
    }

}