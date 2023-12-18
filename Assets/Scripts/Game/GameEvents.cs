using UnityEngine;
using UnityEngine.UI;

public class GameEvents : MonoBehaviour
{
    private SelectionOutline selection;
    [SerializeField] private GameObject eventInfo;

    private void Start()
    {
        selection = GetComponent<SelectionOutline>();
    }

    public void EnableCastlePlacement()
    {
        eventInfo.SetActive(true);
        Text infoText = eventInfo.GetComponentInChildren<Text>();
        infoText.text = "Build your castle";
        selection.interaction = true;
        selection.build = true;
    }

    public void DisplayTurnInformation(string username, bool build)
    {
        eventInfo.SetActive(true);
        Text infoText = eventInfo.GetComponentInChildren<Text>();

        if (build)
        {
            infoText.text = $"{username} is placing their castle.";
        }
        else
        {
            infoText.text = $"{username} is playing their turn.";
        }
    }

    public void EnableInteraction()
    {
        eventInfo.SetActive(true);
        Text infoText = eventInfo.GetComponentInChildren<Text>();
        infoText.text = "It's your turn.";
        selection.interaction = true;
    }
}
