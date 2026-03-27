using UnityEngine;

public class PlayerAppearance : MonoBehaviour
{
    [Header("Player Models (Drag the child objects here)")]
    public GameObject normalModeObject;
    public GameObject itModeObject;

    private SkinnedMeshRenderer normalRenderer;
    private PlayerTagController tagController;
    private int myPlayerIndex = -1;

    void Awake()
    {
        tagController = GetComponent<PlayerTagController>();

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
        if (PlayerColorManager.Instance != null && normalRenderer != null)
        {
            Material assignedMat = PlayerColorManager.Instance.GetNextColor();

            Material[] sharedMaterials = normalRenderer.materials;
            sharedMaterials[1] = assignedMat;
            normalRenderer.materials = sharedMaterials;

            if (GameHUDManager.Instance != null && assignedMat != null)
            {
                myPlayerIndex = GameHUDManager.Instance.AddPlayerHUD(assignedMat.color);
            }
        }
        else if (PlayerColorManager.Instance == null)
        {
            Debug.LogError("PlayerColorManager is missing from the scene!");
        }

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        // ==========================================
        // ---> NEW: INVISIBILITY CLOAK IN LOBBY <---
        // ==========================================
        // If the game is still on the main menu, turn everything off!
        if (MatchFlowManager.Instance != null && MatchFlowManager.Instance.isLobbyPhase)
        {
            if (normalModeObject != null) normalModeObject.SetActive(false);
            if (itModeObject != null) itModeObject.SetActive(false);
            return; // Stop the code here!
        }

        bool isCurrentlyIt = (tagController != null && tagController.isIt);

        if (isCurrentlyIt)
        {
            if (normalModeObject != null) normalModeObject.SetActive(false);
            if (itModeObject != null) itModeObject.SetActive(true);
        }
        else
        {
            if (normalModeObject != null) normalModeObject.SetActive(true);
            if (itModeObject != null) itModeObject.SetActive(false);
        }

        if (GameHUDManager.Instance != null && myPlayerIndex != -1)
        {
            GameHUDManager.Instance.UpdatePlayerItStatus(myPlayerIndex, isCurrentlyIt);
        }
    }

    public void AddScoreToHUD(int newTotalScore)
    {
        if (GameHUDManager.Instance != null && myPlayerIndex != -1)
        {
            GameHUDManager.Instance.UpdatePlayerScore(myPlayerIndex, newTotalScore);
        }
    }

    public void RemoveFromHUD()
    {
        if (GameHUDManager.Instance != null && myPlayerIndex != -1)
        {
            GameHUDManager.Instance.HidePlayerHUD(myPlayerIndex);
        }
    }
}