using UnityEngine;

public class PlayerAppearance : MonoBehaviour
{
    [Header("Player Models (Drag the child objects here)")]
    public GameObject normalModeObject;
    public GameObject itModeObject;

    private SkinnedMeshRenderer normalRenderer;
    private PlayerTagController tagController;

    // Keeps track of which HUD belongs to this specific player
    private int myPlayerIndex = -1;

    void Awake()
    {
        tagController = GetComponent<PlayerTagController>();

        // Find the renderer on the Normal Mode child
        if (normalModeObject != null)
        {
            normalRenderer = normalModeObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }
        else
        {
            Debug.LogError("You forgot to assign the Normal Mode Object on " + gameObject.name);
        }
    }

    void Start()
    {
        // Because PlayerInputManager spawned this prefab, Start() runs immediately.
        // Grab the next color in line from the Manager right now!
        if (PlayerColorManager.Instance != null && normalRenderer != null)
        {
            Material assignedMat = PlayerColorManager.Instance.GetNextColor();

            Material[] sharedMaterials = normalRenderer.materials;
            sharedMaterials[1] = assignedMat;
            normalRenderer.materials = sharedMaterials;

            // Tell the HUD Manager we joined, pass it the color we just grabbed, and save our ID!
            if (GameHUDManager.Instance != null && assignedMat != null)
            {
                myPlayerIndex = GameHUDManager.Instance.AddPlayerHUD(assignedMat.color);
            }
        }
        else if (PlayerColorManager.Instance == null)
        {
            Debug.LogError("PlayerColorManager is missing from the scene!");
        }

        // Make sure the correct model and HUD status is showing right from the start
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        // Check if the player is currently IT
        bool isCurrentlyIt = (tagController != null && tagController.isIt);

        if (isCurrentlyIt)
        {
            // Player is IT: Hide the normal model, show the IT model
            normalModeObject.SetActive(false);
            itModeObject.SetActive(true);
        }
        else
        {
            // Player is Normal: Show the normal model, hide the IT model
            normalModeObject.SetActive(true);
            itModeObject.SetActive(false);
        }

        // Tell the HUD to turn red (or go back to their normal color)
        if (GameHUDManager.Instance != null && myPlayerIndex != -1)
        {
            GameHUDManager.Instance.UpdatePlayerItStatus(myPlayerIndex, isCurrentlyIt);
        }
    }

    // We need a way to let the Orb system give this specific player points on the HUD
    public void AddScoreToHUD(int newTotalScore)
    {
        if (GameHUDManager.Instance != null && myPlayerIndex != -1)
        {
            GameHUDManager.Instance.UpdatePlayerScore(myPlayerIndex, newTotalScore);
        }
    }

    // ==========================================
    // ---> NEW: REMOVE FROM HUD METHOD <---
    // ==========================================
    public void RemoveFromHUD()
    {
        // We just use the myPlayerIndex you already set up in Start()!
        if (GameHUDManager.Instance != null && myPlayerIndex != -1)
        {
            GameHUDManager.Instance.HidePlayerHUD(myPlayerIndex);
        }
    }
}