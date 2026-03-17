using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TagGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float matchTimeLimit = 60f;

    [Header("Lobby UI References")]
    public GameObject lobbyPanel;
    public TextMeshProUGUI playersJoinedText;
    public Button startButton;

    [Header("Lobby 3D Showcase")]
    public GameObject[] displayModels; // Drag your 4 hidden studio models here!
    public float rotationSpeed = 60f;
    private int lastPlayerCount = 0;

    [Header("In-Game UI References")]
    public TextMeshProUGUI resultsText;
    public TextMeshProUGUI timerText;

    private bool matchIsActive = false;
    private PlayerInputManager inputManager;

    void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();

        lobbyPanel.SetActive(true);
        if (resultsText != null) resultsText.text = "";
        if (timerText != null) timerText.text = "00:00";

        startButton.onClick.AddListener(StartMatchButton_Clicked);
    }

    void Update()
    {
        if (lobbyPanel.activeSelf)
        {
            int currentCount = inputManager.playerCount;

            if (playersJoinedText != null)
            {
                playersJoinedText.text = "PLAYERS JOINED: " + currentCount;
            }

            // If a new player joined, turn on their display model!
            if (currentCount > lastPlayerCount)
            {
                for (int i = lastPlayerCount; i < currentCount; i++)
                {
                    if (i < displayModels.Length)
                    {
                        displayModels[i].SetActive(true);
                    }
                }
                lastPlayerCount = currentCount;
            }

            // Make the active display models spin continuously
            foreach (GameObject model in displayModels)
            {
                if (model != null && model.activeSelf)
                {
                    model.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
                }
            }
        }
    }

    public void StartMatchButton_Clicked()
    {
        if (inputManager.playerCount == 0) return;

        lobbyPanel.SetActive(false);
        inputManager.DisableJoining();

        // Turn off the display models to save performance during the actual game
        foreach (GameObject model in displayModels)
        {
            if (model != null) model.SetActive(false);
        }

        StartCoroutine(MatchRoutine());
    }

    IEnumerator MatchRoutine()
    {
        float delayTimer = 3f;
        while (delayTimer > 0)
        {
            if (resultsText != null)
            {
                resultsText.text = "STARTING IN: " + Mathf.CeilToInt(delayTimer).ToString();
            }
            delayTimer -= Time.deltaTime;
            yield return null;
        }

        PlayerTagController[] allPlayers = FindObjectsOfType<PlayerTagController>();

        if (allPlayers.Length > 0)
        {
            int randomIndex = Random.Range(0, allPlayers.Length);

            foreach (PlayerTagController player in allPlayers)
            {
                player.BecomeNormal();
            }

            allPlayers[randomIndex].BecomeIt(Vector3.zero);

            matchIsActive = true;
            if (resultsText != null) resultsText.text = "MATCH STARTED!";

            StartCoroutine(ClearResultsText(2f));

            float currentTime = matchTimeLimit;
            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay(currentTime);
                yield return null;
            }

            matchIsActive = false;
            if (timerText != null) timerText.text = "00:00";
            CalculateAndDisplayRanks(allPlayers);
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timerText == null) return;
        float secondsLeft = Mathf.CeilToInt(timeToDisplay);
        float minutes = Mathf.FloorToInt(secondsLeft / 60);
        float seconds = Mathf.FloorToInt(secondsLeft % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    IEnumerator ClearResultsText(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (matchIsActive && resultsText != null)
        {
            resultsText.text = "";
        }
    }

    void CalculateAndDisplayRanks(PlayerTagController[] players)
    {
        foreach (var player in players)
        {
            player.GetComponent<PlayerInput>().DeactivateInput();
        }

        var groupedPlayers = players.GroupBy(p => p.score)
                                    .OrderByDescending(g => g.Key)
                                    .ToList();

        string finalLeaderboard = "GAME OVER\n\n";
        int rank = 1;

        foreach (var group in groupedPlayers)
        {
            List<string> namesInThisRank = new List<string>();
            foreach (var player in group)
            {
                namesInThisRank.Add(player.playerName);
            }
            string combinedNames = string.Join(" & ", namesInThisRank);
            finalLeaderboard += "Rank " + rank + ": " + combinedNames + " - " + group.Key + " Points\n";
            rank++;
        }

        if (resultsText != null)
        {
            resultsText.text = finalLeaderboard;
        }
    }
}