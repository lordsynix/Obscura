using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorManager : MonoBehaviour
{
    public static ErrorManager Instance;

    public GameObject errorPanel;
    public Text errorCodeText;
    public Text errorText;

    private void Awake()
    {
        Instance = this;
    }

    public void DisplayError(int error, string message)
    {
        errorPanel.SetActive(true);
        errorCodeText.text = "Error: " + error.ToString();
        errorText.text = message;
    }

    public void OnClickIUnderstand()
    {
        errorPanel.SetActive(false);
    }

    public void OnClickTroubleshoot()
    {
        Application.OpenURL("https://packentertainment.net/troubleshooting");
    }
}
