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

    public bool matchHasStarted = false;
    private float currentRoundTimer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (lobbyUIPanel != null) lobbyUIPanel.SetActive(false);
        if (videoScreenDisplay != null) videoScreenDisplay.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        // Make sure the round timer is completely hidden at the start!
        if (roundTimerText != null) roundTimerText.gameObject.SetActive(false);

        if (winnerText != null) winnerText.gameObject.SetActive(false);

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
        StartCoroutine(MatchStartSequence());
    }

    IEnumerator MatchStartSequence()
    {
        SetPlayerLock(true);

        // 1. PLAY INTRO VIDEO (Using the new safe method!)
        // 1. PLAY INTRO VIDEO 
        yield return StartCoroutine(PlayVideoByTimer(introVideo, introVideoDuration));

        // 2. CAMERA SWOOP & COUNTDOWN
        if (DynamicCamera.Instance != null) DynamicCamera.Instance.isTracking = true;

        // This just counts the numbers 3, 2, 1
        yield return StartCoroutine(PlayCountdownNumbers(3f));

        // 3. MATCH START!
        PickRandomIt();

        if (countdownText != null) countdownText.text = "GO!";

        if (roundTimerText != null) roundTimerText.gameObject.SetActive(true);
        currentRoundTimer = roundDuration;
        matchHasStarted = true;
        SetPlayerLock(false);

        // Hide the "GO!" text after 1 second
        yield return new WaitForSeconds(1f);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    IEnumerator RoundOverSequence()
    {
        // Freeze the game and hide the timer immediately
        matchHasStarted = false;
        SetPlayerLock(true);
        if (roundTimerText != null) roundTimerText.gameObject.SetActive(false);

        PlayerTagController[] allPlayers = FindObjectsByType<PlayerTagController>(FindObjectsSortMode.None);
        List<PlayerTagController> survivors = new List<PlayerTagController>();

        foreach (PlayerTagController player in allPlayers)
        {
            if (player.isIt)
            {
                // Tell the HUD to hide their portrait before destroying them
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

        yield return null; // Wait 1 frame to ensure the IT is completely deleted

        // ==========================================
        // ---> CHANGED: PLAY VIDEO BEFORE THE WINNER CHECK! <---
        // ==========================================
        if (eliminationVideo != null && videoScreenDisplay != null)
        {
            yield return StartCoroutine(PlayVideoByTimer(eliminationVideo, eliminationVideoDuration));
        }

        // ==========================================
        // ---> NOW WE CHECK IF THE GAME IS OVER <---
        // ==========================================
        if (survivors.Count == 1)
        {
            if (roundTimerText != null) roundTimerText.gameObject.SetActive(false);
            if (winnerText != null)
            {
                winnerText.gameObject.SetActive(true);
                winnerText.text = survivors[0].playerName + " WINS!";
            }

            // The game is over, so we stop the loop here!
            yield break;
        }

        // If there are still more than 1 survivor, the game continues!
        PickRandomIt();

        // Do the 3, 2, 1 numbers again
        yield return StartCoroutine(PlayCountdownNumbers(3f));

        if (countdownText != null) countdownText.text = "GO!";

        if (roundTimerText != null) roundTimerText.gameObject.SetActive(true);
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

    

    // ==========================================
    // ---> THE MANUAL TIMER VIDEO FIX <---
    // ==========================================
    private IEnumerator PlayVideoByTimer(VideoPlayer vp, float customDuration)
    {
        if (vp != null && videoScreenDisplay != null)
        {
            // 1. Show screen and reset the video
            videoScreenDisplay.gameObject.SetActive(true);
            vp.isLooping = false;
            vp.Stop();
            vp.time = 0;

            // 2. Hit play
            vp.Play();

            // 3. IGNORE UNITY! Just wait for the exact amount of seconds you typed in the Inspector
            yield return new WaitForSeconds(customDuration);

            // 4. Time is up! Kill the video and hide the screen immediately
            vp.Stop();
            videoScreenDisplay.gameObject.SetActive(false);
        }
    }
}