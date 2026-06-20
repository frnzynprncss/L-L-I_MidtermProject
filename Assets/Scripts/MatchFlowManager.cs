using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MatchFlowManager : MonoBehaviour
{
    public static MatchFlowManager Instance;

    public GameObject RestartButton;
    public GameObject GameTitle;
    public GameObject MainMenu_btns;
    public GameObject Timeline_2;
    public GameObject Timeline_1;

    public GameObject OrbManager;

    [Header("0. Lobby Settings")]
    public GameObject lobbyUIPanel;
    public bool isLobbyPhase = false;

    [Header("1. Intro Video Settings")]
    public VideoPlayer introVideo;
    public RawImage introVideoScreenDisplay;
    public float introVideoDuration = 6f;

    [Header("1a. Intro Skip Settings")]
    public string skipActionName = "Tag";
    public float requiredSkipHoldTime = 2f;
    public GameObject skipUIContainer;
    public Slider skipProgressBar;

    [Header("2. Intermission / Elimination Video")]
    public VideoPlayer eliminationVideo;
    public RawImage eliminationVideoScreenDisplay;
    public float eliminationVideoDuration = 4f;

    
    [Tooltip("The text that says 'Player X is eliminated!'")]
    public TextMeshProUGUI eliminationNotificationText;

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
        Timeline_1.SetActive(false);
        if (introVideoScreenDisplay != null) introVideoScreenDisplay.gameObject.SetActive(false);
        if (eliminationVideoScreenDisplay != null) eliminationVideoScreenDisplay.gameObject.SetActive(false);
        if (skipUIContainer != null) skipUIContainer.SetActive(false);

        // ---> NEW: Hide the elimination text at the start of the game <---
        if (eliminationNotificationText != null) eliminationNotificationText.gameObject.SetActive(false);

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

        // Hide the player HUDs while the video plays
        if (inGameHUDContainer != null) inGameHUDContainer.SetActive(false);

        PlayerTagController[] allPlayers = FindObjectsByType<PlayerTagController>(FindObjectsSortMode.None);
        List<PlayerTagController> survivors = new List<PlayerTagController>();

        // ---> NEW: A blank string to store the loser's name <---
        string eliminatedPlayerName = "SOMEONE";

        foreach (PlayerTagController player in allPlayers)
        {
            if (player.isIt)
            {
                // ---> NEW: Grab their name right before we destroy them! <---
                eliminatedPlayerName = player.playerName;

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

        // ---> NEW: Update the text and show it on screen! <---
        if (eliminationNotificationText != null)
        {
            eliminationNotificationText.text = eliminatedPlayerName + " WAS ELIMINATED!";
            eliminationNotificationText.gameObject.SetActive(true);
        }

        // Play the video while the text is on screen
        if (eliminationVideo != null && eliminationVideoScreenDisplay != null)
        {
            yield return StartCoroutine(PlayVideoByTimer(eliminationVideo, eliminationVideoDuration, eliminationVideoScreenDisplay));
        }

        // ---> NEW: The video finished, so hide the text again! <---
        if (eliminationNotificationText != null)
        {
            eliminationNotificationText.gameObject.SetActive(false);
        }

        // Check for winner
        if (survivors.Count == 1)
        {
            if (roundTimerText != null) roundTimerText.gameObject.SetActive(false);
            if (winnerText != null)
            {
                winnerText.gameObject.SetActive(true);
                winnerText.text = survivors[0].playerName + " WINS!";
                StartCoroutine(RestartPanel());
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

        foreach (PlayerTagController p in activePlayers) p.BecomeNormal();

        int lowestScore = int.MaxValue;
        foreach (PlayerTagController p in activePlayers)
        {
            if (p.score < lowestScore) lowestScore = p.score;
        }

        List<PlayerTagController> lowestScoringPlayers = new List<PlayerTagController>();
        foreach (PlayerTagController p in activePlayers)
        {
            if (p.score == lowestScore) lowestScoringPlayers.Add(p);
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

    private IEnumerator PlayIntroVideoWithSkip()
    {
        if (introVideo != null && introVideoScreenDisplay != null)
        {
            AudioListener.volume = 0f;
            introVideoScreenDisplay.gameObject.SetActive(true);

            if (skipUIContainer != null) skipUIContainer.SetActive(true);
            if (skipProgressBar != null) skipProgressBar.value = 0f;

            introVideo.isLooping = false;
            introVideo.Stop();
            introVideo.time = 0;
            introVideo.Play();

            float elapsedVideoTime = 0f;
            float currentHoldTime = 0f;

            while (elapsedVideoTime < introVideoDuration)
            {
                elapsedVideoTime += Time.deltaTime;

                if (AreAllPlayersHoldingSkip())
                {
                    currentHoldTime += Time.deltaTime;
                }
                else
                {
                    currentHoldTime = 0f;
                }

                if (skipProgressBar != null)
                {
                    skipProgressBar.value = currentHoldTime / requiredSkipHoldTime;
                }

                if (currentHoldTime >= requiredSkipHoldTime)
                {
                    break;
                }

                yield return null;
            }

            introVideo.Stop();
            introVideoScreenDisplay.gameObject.SetActive(false);
            if (skipUIContainer != null) skipUIContainer.SetActive(false);
            AudioListener.volume = 1f;

            OrbManager.SetActive(true);
        }


    }

    private bool AreAllPlayersHoldingSkip()
    {
        UnityEngine.InputSystem.PlayerInput[] allInputs = FindObjectsByType<UnityEngine.InputSystem.PlayerInput>(FindObjectsSortMode.None);

        if (allInputs.Length == 0) return false;

        foreach (var input in allInputs)
        {
            if (input.actions == null) return false;

            UnityEngine.InputSystem.InputAction skipAction = input.actions[skipActionName];

            if (skipAction == null || !skipAction.IsPressed())
            {
                return false;
            }
        }

        return true;
    }

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

    private IEnumerator RestartPanel()
    {
        yield return new WaitForSeconds(2f);
        RestartButton.SetActive(true);
    }

}