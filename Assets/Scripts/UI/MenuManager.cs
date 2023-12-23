using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public InputField usernameInput;
    public Text errorMessage;

    public void OnClickPlay()
    {
        if (usernameInput.text.ToCharArray().Length <= 15)
        {
            PlayerData.Instance.username = usernameInput.text;
            SceneManager.LoadScene("Lobby");      
        }
        else
        {
            errorMessage.text = "Username exceeded character limit of 15!";
        }
    }

    public void OnClickSettings()
    {
        PlayerData.Instance.ToggleSettings();
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
