using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    public Text nameField;
    public string id;
    public GameObject kickButton;
    public GameObject loadingIcon;
    public GameObject checkmarkIcon;
    public GameObject xIcon;

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

    public void FailConnection()
    {
        loadingIcon.SetActive(false);
        xIcon.SetActive(true);
    }

    public void SucceedConnection()
    {
        loadingIcon.SetActive(false);
        checkmarkIcon.SetActive(true);
    }

}