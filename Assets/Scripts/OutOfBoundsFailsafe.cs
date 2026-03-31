using UnityEngine;

public class OutOfBoundsFailsafe : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Drag an empty GameObject here to act as the safe drop-in point!")]
    public Transform respawnPoint;

    // If you forget to assign a respawn point, it defaults to the center of the map, 5 units in the air
    public Vector3 fallbackRespawn = new Vector3(0f, 5f, 0f);

    void OnTriggerEnter(Collider other)
    {
        // Did a player touch the invisible net?
        if (other.CompareTag("Player"))
        {
            // 1. Figure out where to send them
            Vector3 safeSpot = (respawnPoint != null) ? respawnPoint.position : fallbackRespawn;

            // 2. Instantly teleport them there
            other.transform.position = safeSpot;

            // 3. CRITICAL: Reset their Rigidbody falling speed! 
            // If we don't do this, they will teleport with all their falling momentum and slam into the ground!
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log(other.gameObject.name + " fell out of bounds and was respawned!");
        }
    }
}