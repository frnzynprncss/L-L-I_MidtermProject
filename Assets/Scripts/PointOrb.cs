using UnityEngine;

public class PointOrb : MonoBehaviour
{
    public int pointValue = 1;

    // ---> NEW: The slot for your sound effect! <---
    public AudioClip collectSound;

    void OnTriggerEnter(Collider other)
    {
        // 1. Log whatever touched the orb to the Unity Console so we can see it!
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
                player.score += pointValue;

                // ---> NEW: Play the sound in the air before destroying the orb! <---
                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
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