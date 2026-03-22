using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement; // To restart the level

public class LangitLupaManager : MonoBehaviour
{
    public VideoPlayer cutscenePlayer;
    public GameObject videoUI;
    public float gameTimer = 10f; // Set your round time here
    private bool gameEnded = false;

    void Start()
    {
        videoUI.SetActive(false); // Hide video at start
        // Tell Unity: "When the video finishes, run the 'OnVideoFinished' function"
        cutscenePlayer.loopPointReached += OnVideoFinished;
    }

    void Update()
    {
        if (!gameEnded)
        {
            gameTimer -= Time.deltaTime;
            if (gameTimer <= 0)
            {
                StartCutscene();
            }
        }
    }

    void StartCutscene()
    {
        gameEnded = true;
        videoUI.SetActive(true); // Show the UI/Raw Image
        cutscenePlayer.Play();   // Start the video

        // Optional: Pause game time or disable player movement here
        // Time.timeScale = 0; 
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video ended! Restarting game...");

        // Option A: Restart the whole scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Option B: Just hide the video and reset the timer manually
        /*
        videoUI.SetActive(false);
        gameTimer = 10f;
        gameEnded = false;
        */
    }
}