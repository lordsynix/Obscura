using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class GameError : MonoBehaviour
{
    [SerializeField] private Transform errorContainer;
    [SerializeField] private GameObject errorPanelPrefab;

    public static GameError Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void DisplayError(string errorMessage)
    {
        GameObject errorObject = Instantiate(errorPanelPrefab, errorContainer);
        errorObject.GetComponent<Text>().text = errorMessage;
        StartCoroutine(FadeText(errorObject, errorObject.GetComponent<Text>()));
    }

    private IEnumerator FadeText(GameObject errorObject, Text errorText)
    {
        yield return new WaitForSeconds(0.5f);
        errorText.DOFade(0, 2f);
        yield return new WaitForSeconds(2);
        Destroy(errorObject);
    }
}
