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
            PlayerPrefs.SetString("Username", usernameInput.text);
            SceneManager.LoadScene("Lobby");      
        }
        else
        {
            errorMessage.text = "Username exceeded character limit of 15!";
        }
    }
}
