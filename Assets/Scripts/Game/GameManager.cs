using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    private RoomManager roomManager;
    private RoadManager roadManager;
    private GameEvents gameEvents;

    public GameObject gameOverlayPanel;
    private float maxConnectionTime = 15;
    private bool started = false;

    public Dictionary<ulong, PlayerNetwork> networkDict = new();
    private List<ulong> moveQueue = new List<ulong>();
    private int currentTurnIndex = 0;

    public Text countdownText;
    public Text actionText;

    public List<string> colors = new List<string>() { "Red", "Blue", "Green", "Yellow", "Magenta", "Cyan" };

    private void Start()
    {
        Instance = this;
        roomManager = GetComponent<RoomManager>();
        roadManager = GetComponent<RoadManager>();
        gameEvents = GetComponent<GameEvents>();
    }

    private void Update()
    {
        if (IsServer)
        {
            if (started == false)
            {
                if (NetworkManager.Singleton.ConnectedClientsList.Count == roomManager.playerDict.Count)
                {
                    started = true;
                    StartCoroutine(StartDelay());
                }
            }
        }        
    }

    public IEnumerator CheckConnection()
    {
        DisplayLoading();
        yield return new WaitForSeconds(maxConnectionTime);
        if (NetworkManager.Singleton.IsServer)
        {
            KickInactivePlayers();
        }
    }

    private void DisplayLoading()
    {
        foreach (var player in roomManager.playerDict)
        {
            player.Value.GetComponent<PlayerItem>().kickButton.SetActive(false);
            player.Value.GetComponent<PlayerItem>().loadingIcon.SetActive(true);
        }
    }

    private void KickInactivePlayers()
    {
        
        foreach (var player in roomManager.playerDict)
        {
            bool active = false;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject.GetComponent<PlayerNetwork>().id == player.Key)
                {
                    active = true;
                    Debug.Log("Connected " + client.PlayerObject.GetComponent<PlayerNetwork>().username.Value);
                }
            }  
            if (active == false)
            {
                StartCoroutine(KickDelay(player.Key));               
            }
        }

        if (started == false)
        {
            started = true;
            StartCoroutine(StartDelay());
        }
    }

    private IEnumerator KickDelay(string id)
    {
        DisplayKickClientRpc(id);
        yield return new WaitForSeconds(3);
        roomManager.KickPlayer(id);
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(3);
        StartGameClientRpc();
    }

    [ClientRpc] private void DisplayKickClientRpc(string id)
    {
        roomManager.playerDict[id].GetComponent<PlayerItem>().FailConnection();
    }

    public void ConnectedToServer(string id)
    {
        roomManager.playerDict[id].GetComponent<PlayerItem>().SucceedConnection();
    }

    [ClientRpc] private void StartGameClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            networkDict.Add(player.GetComponent<NetworkObject>().OwnerClientId, player.GetComponent<PlayerNetwork>());
        }
        GameUI.Instance.BuildPlayerInformationPanel(networkDict);
        roomManager.roomPanel.SetActive(false);
        gameOverlayPanel.SetActive(true);
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        int countdown = 5;
        while (countdown > 0)
        {
            actionText.text = countdown.ToString();
            countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1);
            countdown--;
        }
    
        countdownText.text = "0";
        actionText.text = "0";
        yield return new WaitForSeconds(1);

        if (PlayerData.Instance.currentLobby.Data["Position"].Value == "Select")
        {
            actionText.text = "Build your castle!";
            yield return new WaitForSeconds(1);
        }

        if (IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                moveQueue.Add(client.Key);
            }
            moveQueue.Shuffle();

            // Checking if lobby settings are on select or random.
            if (PlayerData.Instance.currentLobby.Data["Position"].Value == "Select")
            {
                // First player in queue has to select territory.
                StartTerritorySelectionClientRpc(moveQueue[0]);
            }
            else
            {
                // Territory is randomly selected by computer, then first player in queue starts turn.
                RandomSelectTerritory();
                StartTurnClientRpc(moveQueue[0]);
            }
        }

    }

    private void RandomSelectTerritory()
    {

    }

    [ClientRpc] private void StartTerritorySelectionClientRpc(ulong clientId)
    {
        actionText.text = $"{networkDict[clientId].username.Value}s Turn!";

        // Allow player to interact with the map when it's their turn.
        if (NetworkManager.LocalClientId == clientId)
        {
            gameEvents.EnableCastlePlacement();
        }
        else
        {
            gameEvents.DisplayTurnInformation(networkDict[clientId].username.Value.ToString(), true);
        }

        StartCoroutine(TerritorySelectTimer());
    }

    private IEnumerator TerritorySelectTimer()
    {
        yield return new WaitForSeconds(1);
        actionText.text = string.Empty;

        // Give players time to think based on lobby settings
        int countdown = int.Parse(PlayerData.Instance.currentLobby.Data["Time"].Value);
        while (countdown > 0)
        {
            countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1);
            countdown--;
        }
        
        countdownText.text = "0";
        yield return new WaitForSeconds(1);

        if (IsServer)
        {
            currentTurnIndex++;
            if (currentTurnIndex > moveQueue.Count - 1)
            {
                currentTurnIndex = 0;
                StartTurnClientRpc(moveQueue[currentTurnIndex]);
            }
            else
            {
                StartTerritorySelectionClientRpc(moveQueue[currentTurnIndex]);
            }            
        }
    }

    [ClientRpc] private void StartTurnClientRpc(ulong clientId)
    {
        actionText.text = $"{networkDict[clientId].username.Value}s Turn!";

        // Allow player to interact with the map when it's their turn.
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            gameEvents.EnableInteraction();
        }
        else
        {
            gameEvents.DisplayTurnInformation(networkDict[clientId].username.Value.ToString(), false);
        }

        StartCoroutine(TurnTimer());
    }

    private IEnumerator TurnTimer()
    {
        yield return new WaitForSeconds(1);
        actionText.text = string.Empty;

        // Give players time to think based on lobby settings
        int countdown = int.Parse(PlayerData.Instance.currentLobby.Data["Time"].Value);
        while (countdown > 0)
        {
            countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1);
            countdown--;
        }
        yield return new WaitForSeconds(1);
        countdownText.text = "0";
        if (IsServer)
        {
            currentTurnIndex++;
            if (currentTurnIndex > moveQueue.Count - 1)
            {
                currentTurnIndex = 0;
            }
            StartTurnClientRpc(moveQueue[currentTurnIndex]);
        }
    }

    [ServerRpc(RequireOwnership = false)] public void BuildCastleServerRpc(int roadIndex, ulong clientId)
    {
        if (roadManager.RoadOccupied(roadIndex))
        {
            DisplayErrorClientRpc("This road is already occupied!", clientId);
        }
        else
        {
            BuildCastleClientRpc(roadIndex, clientId);
        }
    }

    [ClientRpc] private void BuildCastleClientRpc(int roadIndex, ulong clientId)
    {
        roadManager.BuildCastle(roadIndex, clientId);
    }

    [ClientRpc] private void DisplayErrorClientRpc(string error, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            GameError.Instance.DisplayError(error);
            gameEvents.EnableCastlePlacement();
        }
    }

    public void DeselectCastle()
    {
        GameUI.Instance.DestroyCastleInformation();
    }

    [ServerRpc(RequireOwnership = false)] public void AttackServerRpc(int roadIndex, ulong attackerClientId)
    {
        
        if (moveQueue[currentTurnIndex] == attackerClientId)
        {
            AttackClientRpc(attackerClientId);
        }
        // If it's not attackers turn, return error.
        else
        {
            DisplayErrorClientRpc("It is not your turn.", attackerClientId);
        }
    }

    [ClientRpc] private void AttackClientRpc(ulong attackerClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == attackerClientId)
        {
            GameUI.Instance.EnableAttackUI();
        }
    }
}
