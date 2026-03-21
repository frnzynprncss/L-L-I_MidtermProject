using UnityEngine;

public class PlayerIDAssigner : MonoBehaviour
{
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            PlayerTagController controller = players[i].GetComponent<PlayerTagController>();
            if (controller != null)
            {
                controller.playerID = i;
                // Ensure HUDManager exists in the scene to avoid null reference errors
                if (HUDManager.instance != null)
                {
                    HUDManager.instance.UpdateHUD(i, controller.score, controller.isIt);
                }
            }
        }
    }
}