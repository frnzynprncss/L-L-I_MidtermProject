using UnityEngine;

public class PointOrb : MonoBehaviour
{
    public int pointValue = 10;

    private void OnTriggerEnter(Collider other)
    {
        // Look for the script on the player or its parent
        PlayerTagController player = other.GetComponentInParent<PlayerTagController>();

        if (player != null)
        {
            // 1. Check if Taya (Taya cannot collect points)
            if (player.isIt) return;

            // 2. Add Score to the player object
            player.score += pointValue;

            // 3. Update the HUD
            if (HUDManager.instance != null)
            {
                HUDManager.instance.UpdateHUD(player.playerID, player.score, player.isIt);
            }
            else
            {
                Debug.LogWarning("HUDManager instance is missing! Points added but HUD not updated.");
            }

            Destroy(gameObject);
        }
    }
}