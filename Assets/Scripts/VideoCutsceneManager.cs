using UnityEngine;
using UnityEngine.Video; // Required for the VideoPlayer
using UnityEngine.UI;    // Required for the RawImage

public class VideoCutsceneManager : MonoBehaviour
{
    [Header("Video Setup")]
    public VideoPlayer introVideo;
    public RawImage videoScreenDisplay; // The UI element showing the video

    void Start()
    {
        // 1. Lock the camera immediately
        if (DynamicCamera.Instance != null)
        {
            DynamicCamera.Instance.isTracking = false;
        }

        // 2. Lock all players as soon as they spawn
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (PlayerMovement player in allPlayers)
        {
            player.isPlayingCutscene = true;
        }

        // 3. Listen for the exact moment the video finishes
        if (introVideo != null)
        {
            introVideo.loopPointReached += EndCutscene;
        }
        else
        {
            Debug.LogError("No Video Player assigned! Match won't start.");
        }
    }

    // This method fires automatically the millisecond the video ends
    void EndCutscene(VideoPlayer vp)
    {
        // 1. Hide the RawImage UI so we can see the game behind it
        if (videoScreenDisplay != null)
        {
            videoScreenDisplay.gameObject.SetActive(false);
        }

        // 2. Unlock all the players so they can run!
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (PlayerMovement player in allPlayers)
        {
            player.isPlayingCutscene = false;
        }

        // 3. Tell the camera to start tracking
        if (DynamicCamera.Instance != null)
        {
            DynamicCamera.Instance.isTracking = true;
        }

        // 4. Clean up the event listener so it doesn't cause errors later
        vp.loopPointReached -= EndCutscene;
    }
}