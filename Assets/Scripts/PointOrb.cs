using UnityEngine;

public class PointOrb : MonoBehaviour
{
    public int pointValue = 1;

    // The slot for your sound effect!
    public AudioClip collectSound;

    // Optional: Adjust this slider in the Inspector if 1.0 is still too quiet
    [Range(0.1f, 2.0f)]
    public float volume = 1.0f;

    void OnTriggerEnter(Collider other)
    {
        // 1. Log whatever touched the orb to the Unity Console
        Debug.Log("Orb was touched by: " + other.gameObject.name);

        // 2. Use GetComponentInParent just in case a child 3D model bumped the orb
        PlayerTagController player = other.GetComponentInParent<PlayerTagController>();

        // 3. Did we successfully find a player script?
        if (player != null)
        {
            // 4. Are they currently a normal player?
            if (!player.isIt)
            {
                Debug.Log(player.gameObject.name + " collected an orb! +1 Point.");

                // Update the player's score
                player.AddScore(pointValue);

                // ---> OPTION C: Play at Camera Position for max volume <---
                if (collectSound != null && Camera.main != null)
                {
                    // By using Camera.main.transform.position, the sound is 
                    // right in the "ears" of the player.
                    AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position, volume);
                }

                // Destroy the orb so it disappears
                Destroy(gameObject);
            }
            else
            {
                Debug.Log(player.gameObject.name + " is IT! They cannot collect this.");
            }
        }
    }
}