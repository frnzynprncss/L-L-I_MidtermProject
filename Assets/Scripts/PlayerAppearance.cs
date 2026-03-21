using UnityEngine;

public class PlayerAppearance : MonoBehaviour
{
    [Header("Player Models (Drag the child objects here)")]
    public GameObject normalModeObject;
    public GameObject itModeObject;

    private SkinnedMeshRenderer normalRenderer;
    private PlayerTagController tagController;

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
            Material[] sharedMaterials = normalRenderer.materials;
            sharedMaterials[1] = PlayerColorManager.Instance.GetNextColor();
            normalRenderer.materials = sharedMaterials;
        }
        else if (PlayerColorManager.Instance == null)
        {
            Debug.LogError("PlayerColorManager is missing from the scene!");
        }

        // Make sure the correct model is showing (Normal vs IT)
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (tagController != null && tagController.isIt)
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
    }
}