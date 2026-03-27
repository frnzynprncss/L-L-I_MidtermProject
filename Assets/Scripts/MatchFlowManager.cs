using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MatchFlowManager : MonoBehaviour
{
    public static MatchFlowManager Instance;

    [Header("0. Lobby Settings")]
    public GameObject lobbyUIPanel;
    public bool isLobbyPhase = true; // ---> NEW: Tells the players if they should be invisible

    [Header("1. Intro Video Settings")]
    public VideoPlayer introVideo;
    public RawImage videoScreenDisplay;
    public float introVideoDuration = 6f;

    [Header("2. Intermission / Elimination Video")]
    public VideoPlayer eliminationVideo;
    public float eliminationVideoDuration = 4f;

    [Header("3. Round & Timer Settings")]
    public float roundDuration = 30f;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI roundTimerText;
    public TextMeshProUGUI winnerText;

    // ---> NEW: Drag your main Game HUD container here! <---
    [Header("4. In-Game HUD Settings")]
    public GameObject inGameHUDContainer;

    public bool matchHasStarted = false;
    private float currentRoundTimer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        isLobbyPhase = false;

        if (lobbyUIPanel != null) lobbyUIPanel.SetActive(false);
        if (videoScreenDisplay != null) videoScreenDisplay.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (roundTimerText != null) roundTimerText.gameObject.SetActive(false);
        if (winnerText != null) winnerText.gameObject.SetActive(false);

        // Hide the main game HUD while we are in the lobby!
        if (inGameHUDContainer != null) inGameHUDContainer.SetActive(false);

        if (DynamicCamera.Instance != null) DynamicCamera.Instance.SnapToCinematicView();

        SetPlayerLock(true);
    }

    void Update()
    {
        if (matchHasStarted)
        {
            currentRoundTimer -= Time.deltaTime;

            if (roundTimerText != null)
            {
                roundTimerText.text = Mathf.Ceil(currentRoundTimer).ToString();
            }

            if (currentRoundTimer <= 0)
            {
                StartCoroutine(RoundOverSequence());
            }
        }
    }

    public void OnStartButtonPressed()
    {
        if (lobbyUIPanel != null) lobbyUIPanel.SetActive(false);

        // ---> NEW: The lobby is officially over! <---
        isLobbyPhase = false;

        // Force all players to turn their 3D models on behind the video
        PlayerAppearance[] allAppearances = FindObjectsByType<PlayerAppearance>(FindObjectsSortMode.None);
        foreach (PlayerAppearance p in allAppearances)
        {
            p.UpdateVisuals();
        }

        StartCoroutine(MatchStartSequence());
    }

    IEnumerator MatchStartSequence()
    {
        SetPlayerLock(true);

        yield return StartCoroutine(PlayVideoByTimer(introVideo, introVideoDuration));

        if (DynamicCamera.Instance != null) DynamicCamera.Instance.isTracking = true;

        yield return StartCoroutine(PlayCountdownNumbers(3f));

        PickRandomIt();

        if (countdownText != null) countdownText.text = "GO!";

        if (roundTimerText != null) roundTimerText.gameObject.SetActive(true);

        // ---> NEW: Turn on the Game HUD so we can see the player scorecards! <---
        if (inGameHUDContainer != null) inGameHUDContainer.SetActive(true);

        currentRoundTimer = roundDuration;
        matchHasStarted = true;
        SetPlayerLock(false);

        yield return new WaitForSeconds(1f);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    IEnumerator RoundOverSequence()
    {
        matchHasStarted = false;
        SetPlayerLock(true);
        if (roundTimerText != null) roundTimerText.gameObject.SetActive(false);

        // Hide the player HUDs while the video plays to keep it cinematic
        if (inGameHUDContainer != null) inGameHUDContainer.SetActive(false);

        PlayerTagController[] allPlayers = FindObjectsByType<PlayerTagController>(FindObjectsSortMode.None);
        List<PlayerTagController> survivors = new List<PlayerTagController>();

        foreach (PlayerTagController player in allPlayers)
        {
            if (player.isIt)
            {
                PlayerAppearance appearance = player.GetComponent<PlayerAppearance>();
                if (appearance != null)
                {
                    appearance.RemoveFromHUD();
                }

                Destroy(player.gameObject);
            }
            else
            {
                survivors.Add(player);
            }
        }

        yield return null;

        if (eliminationVideo != null && videoScreenDisplay != null)
        {
            yield return StartCoroutine(PlayVideoByTimer(eliminationVideo, eliminationVideoDuration));
        }

        if (survivors.Count == 1)
        {
            if (roundTimerText != null) roundTimerText.gameObject.SetActive(false);
            if (winnerText != null)
            {
                winnerText.gameObject.SetActive(true);
                winnerText.text = survivors[0].playerName + " WINS!";
            }
            yield break;
        }

        PickRandomIt();

        yield return StartCoroutine(PlayCountdownNumbers(3f));

        if (countdownText != null) countdownText.text = "GO!";

        if (roundTimerText != null) roundTimerText.gameObject.SetActive(true);

        // Bring the player HUDs back for the next round!
        if (inGameHUDContainer != null) inGameHUDContainer.SetActive(true);

        currentRoundTimer = roundDuration;
        matchHasStarted = true;
        SetPlayerLock(false);

        yield return new WaitForSeconds(1f);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void PickRandomIt()
    {
        PlayerTagController[] activePlayers = FindObjectsByType<PlayerTagController>(FindObjectsSortMode.None);
        if (activePlayers.Length == 0) return;

        foreach (PlayerTagController p in activePlayers)
        {
            p.BecomeNormal();
        }

        int randomWinner = Random.Range(0, activePlayers.Length);
        activePlayers[randomWinner].BecomeIt(Vector3.zero);
    }

    private IEnumerator PlayCountdownNumbers(float duration)
    {
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        float timer = duration;
        while (timer > 0)
        {
            if (countdownText != null) countdownText.text = Mathf.Ceil(timer).ToString();
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }
    }

    private void SetPlayerLock(bool isLocked)
    {
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (PlayerMovement player in allPlayers)
        {
            player.isPlayingCutscene = isLocked;
        }
    }

    private IEnumerator PlayVideoByTimer(VideoPlayer vp, float customDuration)
    {
        if (vp != null && videoScreenDisplay != null)
        {
            videoScreenDisplay.gameObject.SetActive(true);
            vp.isLooping = false;
            vp.Stop();
            vp.time = 0;

            vp.Play();

            yield return new WaitForSeconds(customDuration);

            vp.Stop();
            videoScreenDisplay.gameObject.SetActive(false);
        }
    }
}