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
    public bool isLobbyPhase = false;

    [Header("1. Intro Video Settings")]
    public VideoPlayer introVideo;
    public RawImage introVideoScreenDisplay; // ---> CHANGED: Dedicated Intro Screen
    public float introVideoDuration = 6f;

    // ==========================================
    // ---> NEW: SKIP CUTSCENE SETTINGS <---
    // ==========================================
    [Header("1a. Intro Skip Settings")]
    [Tooltip("The exact name of your tag button in the Input Action Asset (e.g., 'Tag', 'Fire', 'Jump')")]
    public string skipActionName = "Tag";
    public float requiredSkipHoldTime = 2f;
    public GameObject skipUIContainer; // Drag a UI panel here that says "Hold [Button] to Skip"
    public Slider skipProgressBar;     // (Optional) Drag a UI Slider here to visually show the 2 seconds filling up!

    [Header("2. Intermission / Elimination Video")]
    public VideoPlayer eliminationVideo;
    public RawImage eliminationVideoScreenDisplay; // ---> NEW: Dedicated Elimination Screen
    public float eliminationVideoDuration = 4f;

    [Header("3. Round & Timer Settings")]
    public float roundDuration = 30f;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI roundTimerText;
    public TextMeshProUGUI winnerText;

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

        // Hide both video screens and the skip UI
        if (introVideoScreenDisplay != null) introVideoScreenDisplay.gameObject.SetActive(false);
        if (eliminationVideoScreenDisplay != null) eliminationVideoScreenDisplay.gameObject.SetActive(false);
        if (skipUIContainer != null) skipUIContainer.SetActive(false);

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (roundTimerText != null) roundTimerText.gameObject.SetActive(false);
        if (winnerText != null) winnerText.gameObject.SetActive(false);

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

        isLobbyPhase = false;

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

        // ---> CHANGED: Uses the new Skip System for the intro! <---
        yield return StartCoroutine(PlayIntroVideoWithSkip());

        if (DynamicCamera.Instance != null) DynamicCamera.Instance.isTracking = true;

        yield return StartCoroutine(PlayCountdownNumbers(3f));

        PickNextIt();

        if (countdownText != null) countdownText.text = "GO!";

        if (roundTimerText != null) roundTimerText.gameObject.SetActive(true);

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

        // ---> CHANGED: Elimination video now uses its own dedicated screen <---
        if (eliminationVideo != null && eliminationVideoScreenDisplay != null)
        {
            yield return StartCoroutine(PlayVideoByTimer(eliminationVideo, eliminationVideoDuration, eliminationVideoScreenDisplay));
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

        PickNextIt();

        yield return StartCoroutine(PlayCountdownNumbers(3f));

        if (countdownText != null) countdownText.text = "GO!";

        if (roundTimerText != null) roundTimerText.gameObject.SetActive(true);

        if (inGameHUDContainer != null) inGameHUDContainer.SetActive(true);

        currentRoundTimer = roundDuration;
        matchHasStarted = true;
        SetPlayerLock(false);

        yield return new WaitForSeconds(1f);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void PickNextIt()
    {
        PlayerTagController[] activePlayers = FindObjectsByType<PlayerTagController>(FindObjectsSortMode.None);
        if (activePlayers.Length == 0) return;

        foreach (PlayerTagController p in activePlayers)
        {
            p.BecomeNormal();
        }

        int lowestScore = int.MaxValue;
        foreach (PlayerTagController p in activePlayers)
        {
            if (p.score < lowestScore)
            {
                lowestScore = p.score;
            }
        }

        List<PlayerTagController> lowestScoringPlayers = new List<PlayerTagController>();
        foreach (PlayerTagController p in activePlayers)
        {
            if (p.score == lowestScore)
            {
                lowestScoringPlayers.Add(p);
            }
        }

        int randomWinner = Random.Range(0, lowestScoringPlayers.Count);
        lowestScoringPlayers[randomWinner].BecomeIt(Vector3.zero);
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
    // ---> NEW: INTRO VIDEO WITH SKIP LOGIC <---
    // ==========================================
    private IEnumerator PlayIntroVideoWithSkip()
    {
        if (introVideo != null && introVideoScreenDisplay != null)
        {
            AudioListener.volume = 0f;
            introVideoScreenDisplay.gameObject.SetActive(true);

            // Show the skip UI
            if (skipUIContainer != null) skipUIContainer.SetActive(true);
            if (skipProgressBar != null) skipProgressBar.value = 0f;

            introVideo.isLooping = false;
            introVideo.Stop();
            introVideo.time = 0;
            introVideo.Play();

            float elapsedVideoTime = 0f;
            float currentHoldTime = 0f;

            // Wait until the video ends naturally, OR the skip timer hits 2 seconds
            while (elapsedVideoTime < introVideoDuration)
            {
                elapsedVideoTime += Time.deltaTime;

                // Check if everyone is holding the button
                if (AreAllPlayersHoldingSkip())
                {
                    currentHoldTime += Time.deltaTime;
                }
                else
                {
                    currentHoldTime = 0f; // Reset if anyone lets go!
                }

                // Update the visual slider if you assigned one
                if (skipProgressBar != null)
                {
                    skipProgressBar.value = currentHoldTime / requiredSkipHoldTime;
                }

                // Did they hold it long enough? SKIP!
                if (currentHoldTime >= requiredSkipHoldTime)
                {
                    break;
                }

                yield return null;
            }

            // Cleanup
            introVideo.Stop();
            introVideoScreenDisplay.gameObject.SetActive(false);
            if (skipUIContainer != null) skipUIContainer.SetActive(false);
            AudioListener.volume = 1f;
        }
    }

    // Checks every active player to see if they are holding the specified button
    private bool AreAllPlayersHoldingSkip()
    {
        UnityEngine.InputSystem.PlayerInput[] allInputs = FindObjectsByType<UnityEngine.InputSystem.PlayerInput>(FindObjectsSortMode.None);

        // If no one is in the lobby, we can't skip
        if (allInputs.Length == 0) return false;

        foreach (var input in allInputs)
        {
            if (input.actions == null) return false;

            // Finds the exact action by the name you type in the Inspector
            UnityEngine.InputSystem.InputAction skipAction = input.actions[skipActionName];

            // If the action isn't currently being held down, stop checking and return false
            if (skipAction == null || !skipAction.IsPressed())
            {
                return false;
            }
        }

        // If we made it through the whole loop, EVERYONE is holding it!
        return true;
    }

    // ---> CHANGED: Now accepts a specific RawImage so it works for multiple screens <---
    private IEnumerator PlayVideoByTimer(VideoPlayer vp, float customDuration, RawImage displayScreen)
    {
        if (vp != null && displayScreen != null)
        {
            AudioListener.volume = 0f;

            displayScreen.gameObject.SetActive(true);
            vp.isLooping = false;
            vp.Stop();
            vp.time = 0;

            vp.Play();

            yield return new WaitForSeconds(customDuration);

            vp.Stop();
            displayScreen.gameObject.SetActive(false);

            AudioListener.volume = 1f;
        }
    }
}