using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("UI Prefabs")]
    [SerializeField] GameObject clickableBackground;
    [SerializeField] private GameObject playerInformationPanel;
    [SerializeField] private GameObject playerInformationPrefab;
    [SerializeField] private Transform roadInformationContainer;
    [SerializeField] private GameObject roadInformationPrefab;
    [SerializeField] private Transform castleContainer;
    [SerializeField] private GameObject castlePrefab;
    [SerializeField] private GameObject castleInformationPrefab;
    private GameObject activeInformationPrefab;
    [SerializeField] private Text instructionText;
    [SerializeField] private GameObject attackPanel;
    [SerializeField] private List<Slider> troopSliders;
    [SerializeField] private List<InputField> troopInputs;
    [SerializeField] private Button originButton;
    [SerializeField] private Button targetButton;
    [SerializeField] private Transform castleInformationPanel;
    [SerializeField] private Dropdown pathDropdown;
    [SerializeField] private Text timeText;

    // Private values
    private RoadManager roadManager;
    private RuntimeSpline runtimeSpline;
    private Outpost origin;
    private Outpost target;
    private bool information = false;
    private bool roadInformation = false;
    private List<Node> path = new List<Node>();

    [Header("UI Values")]
    [SerializeField] private int arrowResolution = 5;
    public bool originSelect = false;
    public bool targetSelect = false;
    public bool pathSelect = false;

    private void Start()
    {
        Instance = this;
        roadManager = GetComponent<RoadManager>();
        runtimeSpline = GetComponent<RuntimeSpline>();

        for (int i = 0; i < troopSliders.Count; i++)
        {
            troopSliders[i].onValueChanged.AddListener(OnSliderValueChanged);
            troopInputs[i].onValueChanged.AddListener(OnTroopInputChanged);

            // Default value if no origin selected
            troopSliders[i].maxValue = 0;
            troopInputs[i].ActivateInputField();
            troopInputs[i].SetTextWithoutNotify(0.ToString());
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
        if (attackPanel.activeInHierarchy == false)
        {
            clickableBackground.SetActive(true);
            Node node = roadManager.GetNode(int.Parse(castle.name));
            string username = GameManager.Instance.networkDict[node.outpost.ownerIndex].username.Value.ToString();

            if (node.outpost.ownerIndex == NetworkManager.Singleton.LocalClientId)
            {
                GameObject castleInformation = Instantiate(castleInformationPrefab, castleInformationPanel);
                castleInformation.transform.position = castle.transform.position;
                activeInformationPrefab = castleInformation;
                castle.GetComponent<CastleUI>().SetCastleInformation(castleInformation);
                StartCoroutine(ItemAnimation(castleInformation.transform));

                // Display username
                castleInformation.GetComponentsInChildren<Text>()[0].text = username;
            }
        }
    }

    public void OnClickBackground()
    {
        if (activeInformationPrefab != null)
        {
            Destroy(activeInformationPrefab);
            origin = null;
            target = null;
            GetComponent<SelectionOutline>().ClearSelection();
            attackPanel.SetActive(false);
            clickableBackground.SetActive(false);
        }
    }

    private IEnumerator ItemAnimation(Transform itemContainer)
    {
        foreach (var item in itemContainer.GetComponentsInChildren<Image>())
        {
            if (!item.GetComponent<Button>())
            {
                item.transform.localScale = Vector3.zero;
            }
        }
        foreach (var item in itemContainer.GetComponentsInChildren<Image>())
        {
            if (!item.GetComponent<Button>())
            {
                item.transform.DOScale(1f, 1f).SetEase(Ease.OutBounce);
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    public void EnableAttackUI()
    {
        attackPanel.SetActive(true);
        Destroy(activeInformationPrefab);
        pathDropdown.ClearOptions();

        List<string> options = new();

        options.Add("Auto");
        options.Add("Select");
        
        pathDropdown.AddOptions(options);
        pathDropdown.value = 0;
        pathDropdown.RefreshShownValue();

        timeText.text = 0.ToString();
    }

    public void ChangePathOption()
    {
        if (origin == null)
        {
            GameError.Instance.DisplayError("You haven't selected an origin.");
            pathDropdown.SetValueWithoutNotify(0);
            pathDropdown.RefreshShownValue();
            return;
        }
        if (target == null)
        {
            GameError.Instance.DisplayError("You haven't selected a target.");
            pathDropdown.SetValueWithoutNotify(0);
            pathDropdown.RefreshShownValue();
            return;
        }
        if (pathDropdown.value == 0)
        {
            CalculatePath(roadManager.GetNode(origin.roadIndex), roadManager.GetNode(target.roadIndex));
        }
        else
        {
            OnSelectPath();
        }       
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

    public void OnSelectPath()
    {
        attackPanel.SetActive(false);
        clickableBackground.SetActive(false);
        instructionText.gameObject.SetActive(true);
        instructionText.text = "Choose your path.";
        pathSelect = true;

        GetComponent<SelectionOutline>().ClearSelection();
    }

    public void ValidatePath(List<GameObject> roads)
    {
        List<Node> nodes = new List<Node>();
        foreach (GameObject road in roads)
        {
            nodes.Add(roadManager.GetNode(int.Parse(road.name.ToString())));
        }

        Node originNode = roadManager.GetNode(origin.roadIndex);
        Node targetNode = roadManager.GetNode(target.roadIndex);

        if (nodes.Contains(originNode) && nodes.Contains(targetNode))
        {
            if (roadManager.HasPath(nodes, originNode, targetNode))
            {
                GetComponent<SelectionOutline>().HighlightPath();
                attackPanel.SetActive(true);
                instructionText.gameObject.SetActive(false);
                pathSelect = false;
                CalculateTime(nodes);
            }
        }
    }

    public void SelectOrigin(GameObject outpost)
    {
        int roadIndex = int.Parse(outpost.name);
        Node originNode = roadManager.GetNode(roadIndex);
        origin = originNode.outpost;

        attackPanel.SetActive(true);
        instructionText.gameObject.SetActive(false);

        ulong clientId = origin.ownerIndex;
        string username = GameManager.Instance.networkDict[clientId].username.Value.ToString();
        originButton.GetComponentInChildren<Text>().text = username;

        troopSliders[0].maxValue = originNode.outpost.infantryCount;
        troopSliders[1].maxValue = originNode.outpost.artilleryCount;
        troopSliders[2].maxValue = originNode.outpost.bearCount;
        troopSliders[3].maxValue = originNode.outpost.mamoothCount;

        if (target != null)
        {
            CalculatePath(originNode, roadManager.GetNode(target.roadIndex));            
        }

        originSelect = false;
    }

    public void SelectTarget(GameObject outpost)
    {
        int roadIndex = int.Parse(outpost.name);
        Node targetNode = roadManager.GetNode(roadIndex);
        target = targetNode.outpost;

        attackPanel.SetActive(true);
        instructionText.gameObject.SetActive(false);
      
        ulong clientId = target.ownerIndex;
        string username = GameManager.Instance.networkDict[clientId].username.Value.ToString();
        targetButton.GetComponentInChildren<Text>().text = username;

        if (origin != null)
        {
            CalculatePath(roadManager.GetNode(origin.roadIndex), targetNode);
        }

        targetSelect = false;
    }

    private void OnSliderValueChanged(float newValue)
    {
        for (int i = 0; i < troopInputs.Count; i++)
        {
            if (int.TryParse(troopInputs[i].text, out int fieldValue))
            {
                if (origin == null)
                {
                    troopSliders[i].SetValueWithoutNotify(0);
                    troopInputs[i].SetTextWithoutNotify(0.ToString());
                    return;
                }

                if (fieldValue != troopSliders[i].value)
                {
                    troopInputs[i].SetTextWithoutNotify(troopSliders[i].value.ToString());
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
                    if (origin == null)
                    {
                        troopSliders[i].SetValueWithoutNotify(0);
                        troopInputs[i].SetTextWithoutNotify(0.ToString());
                        return;
                    }

                    if (value <= troopSliders[i].maxValue)
                    {
                        troopSliders[i].SetValueWithoutNotify(value);
                    }
                    else
                    {
                        troopInputs[i].SetTextWithoutNotify(troopSliders[i].maxValue.ToString());
                        troopSliders[i].value = troopSliders[i].maxValue;
                    }                                       
                }
            }
        }
    }

    private void CalculatePath(Node origin, Node target)
    {
        // Calculate the shortest path from origin to target
        path = roadManager.GetShortestPath(origin, target);
        GetComponent<SelectionOutline>().HighlightPath(path);
        //CreateArrowPoints(path);
        CalculateTime(path);
    }

    private void CreateArrowPoints(List<Node> path)
    {
        runtimeSpline.GetSplineValues(path, arrowResolution, out List<Vector3> verts, out List<Vector3> tangents);

        DisplayArrows(verts);

    }

    private void DisplayArrows(List<Vector3> arrowPoints)
    {
        foreach (Vector3 position in arrowPoints)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = new Vector3(5, 5, 5);
        }
    }

    private void CalculateTime(List<Node> nodes)
    {
        timeText.text = roadManager.GetPathTime(nodes).ToString();
    }
}
