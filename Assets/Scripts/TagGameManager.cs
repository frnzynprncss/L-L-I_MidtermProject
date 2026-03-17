using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem; // NEW: We need this to count players and stop joining
using UnityEngine.UI; // NEW: We need this to interact with the Start Button

public class TagGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float matchTimeLimit = 60f;

    [Header("Lobby UI References")]
    public GameObject lobbyPanel;
    public TextMeshProUGUI playersJoinedText;
    public Button startButton;

    [Header("In-Game UI References")]
    public TextMeshProUGUI resultsText;
    public TextMeshProUGUI timerText;

    private bool matchIsActive = false;
    private PlayerInputManager inputManager;

    void Start()
    {
        // Grab the Input Manager sitting on this exact same GameObject
        inputManager = GetComponent<PlayerInputManager>();

        // 1. Setup the Lobby state
        lobbyPanel.SetActive(true);
        if (resultsText != null) resultsText.text = "";
        if (timerText != null) timerText.text = "00:00";

        // 2. Tell the Start Button what code to run when clicked!
        startButton.onClick.AddListener(StartMatchButton_Clicked);
    }

    void Update()
    {
        // Constantly update the text to show how many people pressed a button to join
        if (lobbyPanel.activeSelf && playersJoinedText != null)
        {
            playersJoinedText.text = "PLAYERS JOINED: " + inputManager.playerCount;
        }
    }

    // This runs the exact moment the UI Start Button is pressed
    public void StartMatchButton_Clicked()
    {
        // Don't let them start the game if nobody has spawned in yet!
        if (inputManager.playerCount == 0) return;

        // Hide the Lobby Panel
        lobbyPanel.SetActive(false);

        // Optional: Lock the lobby so nobody else can join mid-match
        inputManager.DisableJoining();

        // Officially start the match sequence
        StartCoroutine(MatchRoutine());
    }

    IEnumerator MatchRoutine()
    {
        // 1. PRE-GAME COUNTDOWN (Give them 3 seconds to get ready after clicking Start)
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

            // 2. THE ACTIVE MATCH TIMER
            float currentTime = matchTimeLimit;
            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay(currentTime);
                yield return null;
            }

            // 3. GAME OVER
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