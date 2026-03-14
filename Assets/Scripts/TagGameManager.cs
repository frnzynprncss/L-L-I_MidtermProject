using UnityEngine;
using System.Collections; // We need this to use "Coroutines" (timers)

public class TagGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("How many seconds to wait for players to join before picking an It.")]
    public float startDelay = 5f;

    void Start()
    {
        // Start the countdown timer as soon as the level loads
        StartCoroutine(AssignItRoutine());
    }

    // A Coroutine is a special function that can pause itself (perfect for timers)
    IEnumerator AssignItRoutine()
    {
        // 1. Wait for the specified amount of seconds
        yield return new WaitForSeconds(startDelay);

        // 2. Find every player that has currently spawned into the scene
        PlayerTagController[] allPlayers = FindObjectsOfType<PlayerTagController>();

        // 3. Make sure at least somebody is playing!
        if (allPlayers.Length > 0)
        {
            // First, force absolutely everyone to be in their "Normal" form just in case
            foreach (PlayerTagController player in allPlayers)
            {
                player.BecomeNormal();
            }

            // Pick a random number from 0 up to the total amount of players
            int randomIndex = Random.Range(0, allPlayers.Length);

            // Force that randomly selected player to become "It"!
            // We pass Vector3.zero so they don't get knocked back by the system picking them
            allPlayers[randomIndex].BecomeIt(Vector3.zero);

            Debug.Log("Player at index " + randomIndex + " was randomly chosen as IT!");
        }
        else
        {
            Debug.LogWarning("No players found! Did nobody press join?");
        }
    }
}