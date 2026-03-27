using UnityEngine;
using System.Collections.Generic;

public class GameHUDManager : MonoBehaviour
{
    public static GameHUDManager Instance;

    [Header("UI Setup")]
    public GameObject playerHUDPrefab; // The UI panel prefab you will make
    public Transform hudContainer;     // The layout group that organizes them

    // This list keeps track of the panels so we can update them later
    private List<PlayerHUDPanel> playerPanels = new List<PlayerHUDPanel>();

    void Awake()
    {
        Instance = this;
    }

    // Called automatically by the player when they spawn
    public int AddPlayerHUD(Color assignedColor)
    {
        // 1. Spawn a new UI panel inside the container
        GameObject newPanelObj = Instantiate(playerHUDPrefab, hudContainer);
        PlayerHUDPanel newPanel = newPanelObj.GetComponent<PlayerHUDPanel>();

        // 2. Set its starting color
        newPanel.Setup(assignedColor);
        playerPanels.Add(newPanel);

        // 3. Return the index number (0 for Player 1, 1 for Player 2, etc.)
        // so the player knows which HUD panel belongs to them!
        return playerPanels.Count - 1;
    }

    // We will use this later when you hook up the score logic
    public void UpdatePlayerScore(int playerIndex, int score)
    {
        if (playerIndex >= 0 && playerIndex < playerPanels.Count)
        {
            playerPanels[playerIndex].UpdateScore(score);
        }
    }

    // Updates the HUD color to red when they get tagged
    public void UpdatePlayerItStatus(int playerIndex, bool isIt)
    {
        if (playerIndex >= 0 && playerIndex < playerPanels.Count)
        {
            playerPanels[playerIndex].SetItStatus(isIt);
        }
    }

    // ==========================================
    // ---> NEW: HIDE ELIMINATED PLAYER HUD <---
    // ==========================================
    public void HidePlayerHUD(int playerIndex)
    {
        // Note: If your list of UI panels is named something other than "playerPanels", 
        // just change that word here to match your script!
        if (playerIndex >= 0 && playerIndex < playerPanels.Count)
        {
            playerPanels[playerIndex].gameObject.SetActive(false);
        }
    }
}