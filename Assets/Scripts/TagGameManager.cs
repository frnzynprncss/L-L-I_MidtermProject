using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class TagGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float matchTimeLimit = 60f;

    [Header("Cutscene References")]
    public VideoPlayer cutscenePlayer;
    public GameObject videoUI;
    public float resultDisplayTime = 5f;

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

    private bool matchIsActive = false;
    private PlayerInputManager inputManager;

    void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();

        lobbyPanel.SetActive(false);
        if (videoUI != null) videoUI.SetActive(false);
        if (resultsText != null) resultsText.text = "";

        startButton.onClick.AddListener(StartMatchButton_Clicked);

        if (cutscenePlayer != null)
        {
            cutscenePlayer.loopPointReached += OnVideoFinished;
        }
    }

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
            if (resultsText != null) resultsText.text = "";
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

            // ---> REMOVED: "MATCH STARTED" text logic used to be here <---

            // Clear the text immediately instead of waiting for a Coroutine
            if (resultsText != null) resultsText.text = "";

            float currentTime = matchTimeLimit;
            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                yield return null;
            }

            matchIsActive = false;
            CalculateAndDisplayRanks(allPlayers);
        }
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

        // NEW: Wait a few seconds so players see the text, then play video
        StartCoroutine(PlayVideoSequence());
    }

    IEnumerator PlayVideoSequence()
    {
        yield return new WaitForSeconds(resultDisplayTime);

        if (videoUI != null && cutscenePlayer != null)
        {
            resultsText.text = "";
            videoUI.SetActive(true);
            cutscenePlayer.Play();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}