using UnityEngine;
using UnityEngine.UI; // Required for Raw Image
using UnityEngine.Video; // Required for Video Player
using System.Collections;

public class EndGameDisplay : MonoBehaviour
{
    public GameObject videoUIObject; // Drag your Raw Image here
    public VideoPlayer myVideoPlayer; // Drag your Video Player here

    // Call this function when the player reaches "Impyerno" or the game ends
    public void ShowEndingVideo()
    {
        // 1. Show the UI element
        videoUIObject.SetActive(true);

        // 2. Play the video
        myVideoPlayer.Play();

        Debug.Log("Playing the Langit Lupa results video!");
    }

    IEnumerator PlayVideoWithDelay()
    {
        // Wait for 2 seconds while the "Lupa!" text is on screen
        yield return new WaitForSeconds(2f);

        videoUIObject.SetActive(true);
        myVideoPlayer.Play();
    }
}