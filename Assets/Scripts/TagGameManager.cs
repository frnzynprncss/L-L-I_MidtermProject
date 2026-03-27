using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video; // Added for Video support
using UnityEngine.SceneManagement; // Added for Restart support

public class TagGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float matchTimeLimit = 60f;

    [Header("Cutscene References")]
    public VideoPlayer cutscenePlayer; // Drag Video Player here
    public GameObject videoUI;         // Drag Raw Image here
    public float resultDisplayTime = 5f; // How long to see text before video starts

    [Header("Lobby UI References")]
    public GameObject lobbyPanel;
    public TextMeshProUGUI playersJoinedText;
    public Button startButton;

    [Header("Lobby 3D Showcase")]
    public GameObject[] displayModels;
    public float rotationSpeed = 0f;
    private int lastPlayerCount = 0;

    [Header("In-Game UI References")]
    public TextMeshProUGUI resultsText;
    //public TextMeshProUGUI timerText;

    private bool matchIsActive = false;
    private PlayerInputManager inputManager;

    void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();

        lobbyPanel.SetActive(false);
        if (videoUI != null) videoUI.SetActive(false); // Ensure video is hidden
        if (resultsText != null) resultsText.text = "";
        //if (timerText != null) timerText.text = "00:00";

        startButton.onClick.AddListener(StartMatchButton_Clicked);

        // Setup the video restart listener
        if (cutscenePlayer != null)
        {
            cutscenePlayer.loopPointReached += OnVideoFinished;
        }
    }

    // ... (Keep your Update and StartMatchButton_Clicked exactly as they were) ...

    void Update()
    {
        if (lobbyPanel.activeSelf)
        {
            int currentCount = inputManager.playerCount;
            if (playersJoinedText != null) playersJoinedText.text = "PLAYERS JOINED: " + currentCount;

            if (currentCount > lastPlayerCount)
            {
                for (int i = lastPlayerCount; i < currentCount; i++)
                {
                    if (i < displayModels.Length) displayModels[i].SetActive(true);
                }
                lastPlayerCount = currentCount;
            }

            foreach (GameObject model in displayModels)
            {
                //if (model != null && model.activeSelf) model.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
                if (model != null && model.activeSelf) model.transform.Rotate(Vector3.up * 0f);
            }
        }
    }

    public void StartMatchButton_Clicked()
    {
        if (inputManager.playerCount == 0) return;
        lobbyPanel.SetActive(false);
        inputManager.DisableJoining();
        foreach (GameObject model in displayModels) { if (model != null) model.SetActive(false); }
        StartCoroutine(MatchRoutine());
    }

    IEnumerator MatchRoutine()
    {
        float delayTimer = 5f;
        while (delayTimer > 0)
        {
            if (resultsText != null) resultsText.text = "STARTING IN: " + Mathf.CeilToInt(delayTimer).ToString();
            delayTimer -= Time.deltaTime;
            yield return null;
        }

        PlayerTagController[] allPlayers = FindObjectsOfType<PlayerTagController>();

        if (allPlayers.Length > 0)
        {
            int randomIndex = Random.Range(0, allPlayers.Length);
            foreach (PlayerTagController player in allPlayers) player.BecomeNormal();
            allPlayers[randomIndex].BecomeIt(Vector3.zero);

            matchIsActive = true;
            if (resultsText != null) resultsText.text = "MATCH STARTED!";
            StartCoroutine(ClearResultsText(2f));

            float currentTime = matchTimeLimit;
            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                //UpdateTimerDisplay(currentTime);
                yield return null;
            }

            matchIsActive = false;
            //if (timerText != null) timerText.text = "00:00";
            CalculateAndDisplayRanks(allPlayers);
        }
    }

    //void UpdateTimerDisplay(float timeToDisplay)
    //{
    //    if (timerText == null) return;
    //    float secondsLeft = Mathf.CeilToInt(timeToDisplay);
    //    float minutes = Mathf.FloorToInt(secondsLeft / 60);
    //    float seconds = Mathf.FloorToInt(secondsLeft % 60);
    //    timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    //}

    IEnumerator ClearResultsText(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (matchIsActive && resultsText != null) resultsText.text = "";
    }

    void CalculateAndDisplayRanks(PlayerTagController[] players)
    {
        foreach (var player in players) player.GetComponent<PlayerInput>().DeactivateInput();

        var groupedPlayers = players.GroupBy(p => p.score)
                                    .OrderByDescending(g => g.Key)
                                    .ToList();

        string finalLeaderboard = "GAME OVER\n";
        int rank = 1;

        foreach (var group in groupedPlayers)
        {
            List<string> namesInThisRank = new List<string>();
            foreach (var player in group) namesInThisRank.Add(player.playerName);
            string combinedNames = string.Join(" & ", namesInThisRank);
            finalLeaderboard += "Rank " + rank + ": " + combinedNames + " - " + group.Key + " Points\n";
            rank++;
        }

        //if (resultsText != null) resultsText.text = finalLeaderboard;

        // NEW: Wait a few seconds so players see the text, then play video
        StartCoroutine(PlayVideoSequence());
    }

    IEnumerator PlayVideoSequence()
    {
        yield return new WaitForSeconds(resultDisplayTime); // Wait to read results

        if (videoUI != null && cutscenePlayer != null)
        {
            resultsText.text = ""; // Clear text so it's not over the video
            videoUI.SetActive(true);
            cutscenePlayer.Play();
        }
        else
        {
            // If no video is set, just restart after the delay
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Restart the whole game once the video ends
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}