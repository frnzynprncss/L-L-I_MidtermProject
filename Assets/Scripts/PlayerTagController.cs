using UnityEngine;

public class PlayerTagController : MonoBehaviour
{
    [Header("Player State")]
    public bool isIt = false;

    [Header("3D Models (Assign Child Objects)")]
    public GameObject normalModel;
    public GameObject itModel;

    [Header("Tag Settings")]
    public float tagCooldown = 2f;
    private float currentCooldown = 0f;

    void Start()
    {
        // Make sure the right model is showing when the game starts
        UpdateForm();
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isIt && currentCooldown <= 0 && collision.gameObject.CompareTag("Player"))
        {
            PlayerTagController otherPlayer = collision.gameObject.GetComponent<PlayerTagController>();

            if (otherPlayer != null && !otherPlayer.isIt)
            {
                this.BecomeNormal();
                otherPlayer.BecomeIt();
            }
        }
    }

    public void BecomeIt()
    {
        isIt = true;
        currentCooldown = tagCooldown;
        UpdateForm();
    }

    public void BecomeNormal()
    {
        isIt = false;
        UpdateForm();
    }

    // This handles the actual model swapping
    void UpdateForm()
    {
        if (isIt)
        {
            // Turn off the normal model, turn on the "It" model
            normalModel.SetActive(false);
            itModel.SetActive(true);
        }
        else
        {
            // Turn off the "It" model, turn on the normal model
            itModel.SetActive(false);
            normalModel.SetActive(true);
        }
    }
}