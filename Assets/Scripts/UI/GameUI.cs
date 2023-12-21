using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using UnityEditor.Experimental.GraphView;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    [SerializeField] private GameObject playerInformationPanel;
    [SerializeField] private GameObject playerInformationPrefab;

    private bool information = false;
    private bool roadInformation = false;
    private RoadManager roadManager;

    [SerializeField] private Transform roadInformationContainer;
    [SerializeField] private GameObject roadInformationPrefab;

    [SerializeField] private Transform castleContainer;
    [SerializeField] private GameObject castlePrefab;

    [SerializeField] private Transform gameOverlay;
    private GameObject activeInformationPrefab;

    [SerializeField] private Text instructionText;
    [SerializeField] private GameObject attackPanel;
    [SerializeField] private List<Slider> troopSliders;
    [SerializeField] private List<InputField> troopInputs;
    [SerializeField] private Button originButton;
    [SerializeField] private Button targetButton;
    public bool originSelect = false;
    public bool targetSelect = false;

    private void Start()
    {
        Instance = this;
        roadManager = GetComponent<RoadManager>();

        for (int i = 0; i < troopSliders.Count; i++)
        {
            troopSliders[i].onValueChanged.AddListener(OnSliderValueChanged);
            troopInputs[i].onValueChanged.AddListener(OnTroopInputChanged);
        }
    }

    public void BuildPlayerInformationPanel(Dictionary<ulong, PlayerNetwork> networkDict)
    {
        Transform playerInformationContainer = playerInformationPanel.GetComponentInChildren<VerticalLayoutGroup>().transform;
        foreach (var player in networkDict)
        {
            GameObject playerInformation = Instantiate(playerInformationPrefab, playerInformationContainer);
            Color color = new Color();

            switch (GameManager.Instance.colors[(int)player.Key])
            {
                case "Red":
                    color = new Color(1, 0, 0, 1);
                    break;
                case "Blue":
                    color = new Color(0, 0, 1, 1);
                    break;
                case "Green":
                    color = new Color(0, 1, 0, 1);
                    break;
                case "Yellow":
                    color = new Color(1, 1, 0, 1);
                    break;
                case "Magenta":
                    color = new Color(1, 0, 1, 1);
                    break;
                case "Cyan":
                    color = new Color(0, 1, 1, 1); 
                    break;
            }

            playerInformation.GetComponentsInChildren<Image>()[1].color = color;
            playerInformation.GetComponentsInChildren<Text>()[0].text = player.Value.username.Value.ToString();

            // Turn off diplomacy and chat for self.
            if (player.Key == NetworkManager.Singleton.LocalClientId)
            {
                playerInformation.GetComponentsInChildren<Image>()[4].enabled = false;
                playerInformation.GetComponentsInChildren<Image>()[5].enabled = false;
            }
        }

    }

    public void OnClickPlayers()
    {
        if (information == false) 
        {
            playerInformationPanel.GetComponent<RectTransform>().DOAnchorPosX(-62, 0.5f);
            information = true;
        }
        else
        {
            playerInformationPanel.GetComponent<RectTransform>().DOAnchorPosX(72.8f, 0.5f);
            information = false;
        }
    }

    public void OnClickChat()
    {
        Debug.LogWarning("Chat not implemented yet");
    }

    public void OnClickSettings()
    {
        PlayerData.Instance.ToggleSettings();
    }

    public void OnClickInfo()
    {
        if (roadInformation == false)
        {
            DisplayRoadTimes();
            roadInformation = true;
        }
        else
        {
            ClearRoadTimes();
            roadInformation = false;
        }
    }

    private void DisplayRoadTimes()
    {
        foreach (var road in roadManager.roadObjects)
        {
            Node node = road.Key;
            Vector3 uiPosition = roadManager.GetMiddleVertexPosition(node);

            GameObject roadInformation = Instantiate(roadInformationPrefab, roadInformationContainer);
            roadInformation.transform.position = uiPosition;
            roadInformation.GetComponent<Text>().text = node.time.ToString();
        }
    }

    private void ClearRoadTimes()
    {
        foreach (Text roadInformation in roadInformationContainer.GetComponentsInChildren<Text>())
        {
            Destroy(roadInformation.gameObject);
        }
    }

    public void BuildCastleUI(Node node, ulong clientId)
    {
        Vector3 uiPosition = roadManager.GetMiddleVertexPosition(node);

        GameObject castle = Instantiate(castlePrefab, castleContainer);
        castle.name = node.index.ToString();
        castle.transform.position = uiPosition;

        Color color = new Color();

        switch (GameManager.Instance.colors[(int)clientId])
        {
            case "Red":
                color = new Color(1, 0, 0, 1);
                break;
            case "Blue":
                color = new Color(0, 0, 1, 1);
                break;
            case "Green":
                color = new Color(0, 1, 0, 1);
                break;
            case "Yellow":
                color = new Color(1, 1, 0, 1);
                break;
            case "Magenta":
                color = new Color(1, 0, 1, 1);
                break;
            case "Cyan":
                color = new Color(0, 1, 1, 1);
                break;
        }

        castle.GetComponent<Image>().color = color;
    }

    public void DisplayCastleOptions(GameObject castle)
    {
        Node node = roadManager.GetNode(int.Parse(castle.name));
        string username = GameManager.Instance.networkDict[node.castle.ownerIndex].username.Value.ToString();
        if (node.castle.ownerIndex == NetworkManager.Singleton.LocalClientId)
        {
            // If castle is allied
            GameObject castleInformation = castle.transform.GetChild(0).gameObject;
            castleInformation.SetActive(true);
            activeInformationPrefab = castleInformation;
            StartCoroutine(ItemAnimation(castleInformation.transform));

            // Display username
            castleInformation.GetComponentsInChildren<Text>()[0].text = username;
        }
        else
        {
            // If castle is foe
            GameObject castleInformation = castle.transform.GetChild(0).gameObject;
            castleInformation.SetActive(true);
            activeInformationPrefab = castleInformation;
            StartCoroutine(ItemAnimation(castleInformation.transform));

            // Display username
            castleInformation.GetComponentsInChildren<Text>()[0].text = username;
        }
    }

    private IEnumerator ItemAnimation(Transform itemContainer)
    {
        foreach (var item in itemContainer.GetComponentsInChildren<Image>())
        {
            item.transform.localScale = Vector3.zero;
        }
        foreach (var item in itemContainer.GetComponentsInChildren<Image>())
        {
            item.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce);
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void DestroyCastleInformation()
    {
        if (activeInformationPrefab != null)
        {
            activeInformationPrefab.SetActive(false);
            activeInformationPrefab = null;
        }
    }

    public void EnableAttackUI()
    {
        attackPanel.SetActive(true);

    }

    public void OnPressOrigin()
    {
        attackPanel.SetActive(false);
        instructionText.gameObject.SetActive(true);
        instructionText.text = "Choose where you want to attack from.";
        originSelect = true;
    }

    public void OnPressTarget()
    {
        attackPanel.SetActive(false);
        instructionText.gameObject.SetActive(true);
        instructionText.text = "Choose your target.";
        targetSelect = true;
    }

    public void SelectOrigin(GameObject castle)
    {
        attackPanel.SetActive(true);
        instructionText.gameObject.SetActive(false);

        int roadIndex = int.Parse(castle.name);
        Node node = roadManager.GetNode(roadIndex);
        ulong clientId = node.castle.ownerIndex;
        string username = GameManager.Instance.networkDict[clientId].username.Value.ToString();
        originButton.GetComponentInChildren<Text>().text = username;

        troopSliders[0].maxValue = node.castle.infantryCount;
        troopSliders[1].maxValue = node.castle.artilleryCount;
        troopSliders[2].maxValue = node.castle.bearCount;
        troopSliders[3].maxValue = node.castle.mamoothCount;

        originSelect = false;
    }

    public void SelectTarget(GameObject castle)
    {
        attackPanel.SetActive(true);
        instructionText.gameObject.SetActive(false);

        int roadIndex = int.Parse(castle.name);
        Node node = roadManager.GetNode(roadIndex);
        ulong clientId = node.castle.ownerIndex;
        string username = GameManager.Instance.networkDict[clientId].username.Value.ToString();
        targetButton.GetComponentInChildren<Text>().text = username;

        targetSelect = false;
    }

    private void OnSliderValueChanged(float newValue)
    {
        for (int i = 0; i < troopInputs.Count; i++)
        {
            if (int.TryParse(troopInputs[i].text, out int value))
            {
                if (value != troopSliders[i].value)
                {
                    troopInputs[i].text = value.ToString();
                }
            }
        }
    }

    private void OnTroopInputChanged(string newValue)
    {
        for (int i = 0; i < troopSliders.Count; i++)
        {
            if (int.TryParse(troopInputs[i].text, out int value))
            {
                if (value != troopSliders[i].value)
                {
                    troopSliders[i].value = value;
                }
            }
        }
    }

    public void OnPressAttack()
    {
        
    }
}
