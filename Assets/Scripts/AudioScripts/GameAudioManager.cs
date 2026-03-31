using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource menuMusic;
    [SerializeField] private AudioSource gameMusic;

    void Awake()
    {
        // 1. Force stop EVERYTHING first to clear the air
        menuMusic.Stop();
        gameMusic.Stop();

        // 2. Ensure the states are correct
        menuMusic.enabled = true;
        gameMusic.enabled = false;

        // 3. Manually trigger the menu music to be safe
        menuMusic.Play();
    }

    public void TransitionToGame()
    {
        if (menuMusic != null && gameMusic != null)
        {
            menuMusic.Stop();
            menuMusic.enabled = false;

            gameMusic.enabled = true;
            gameMusic.Play();

            Debug.Log("Switched to Game Music.");
        }
    }
}