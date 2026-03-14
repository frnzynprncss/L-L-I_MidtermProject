using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTagController : MonoBehaviour
{
    [Header("Player State")]
    public bool isIt = false;

    [Header("3D Models")]
    public GameObject normalModel;
    public GameObject itModel;

    [Header("Tag Mechanics")]
    public float tagRadius = 2.5f; // How far the "It" player's arms reach
    public float knockbackForce = 15f; // How hard they get pushed away
    public float tagCooldown = 2f;

    private float currentCooldown = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        UpdateForm();
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    // The Input System automatically calls this when you press the "Tag" button (Space/Gamepad A)
    void OnTag()
    {
        // If I am NOT it, or my cooldown isn't finished, I can't tag anyone. Stop here.
        if (!isIt || currentCooldown > 0) return;

        // Draw an invisible sphere around me. Who is inside it?
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, tagRadius);

        foreach (Collider hit in hitColliders)
        {
            // Did I hit a player that is NOT me?
            if (hit.CompareTag("Player") && hit.gameObject != this.gameObject)
            {
                PlayerTagController otherPlayer = hit.GetComponent<PlayerTagController>();

                // If they exist and aren't already "It"
                if (otherPlayer != null && !otherPlayer.isIt)
                {
                    // 1. Calculate which direction to knock them back (away from me)
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;

                    // Add a tiny bit of upward lift so they pop into the air nicely
                    pushDirection.y = 0.5f;

                    // 2. I become normal
                    this.BecomeNormal();

                    // 3. They become "It" and take the knockback force!
                    otherPlayer.BecomeIt(pushDirection * knockbackForce);

                    // Stop the loop so we don't accidentally tag two people at the exact same time
                    break;
                }
            }
        }
    }

    // Notice we added the "knockback" variable here
    public void BecomeIt(Vector3 knockbackAmount)
    {
        isIt = true;
        currentCooldown = tagCooldown;
        UpdateForm();

        // NEW: Grab the movement script and trigger the stun!
        PlayerMovement movementScript = GetComponent<PlayerMovement>();

        // If we have a movement script, and the knockback isn't zero (like when the game first starts)
        if (movementScript != null && knockbackAmount != Vector3.zero)
        {
            // Apply the force, and stun their joystick for 0.5 seconds
            movementScript.ApplyKnockback(knockbackAmount, 0.5f);
        }
    }

    public void BecomeNormal()
    {
        isIt = false;
        UpdateForm();
    }

    void UpdateForm()
    {
        if (isIt)
        {
            normalModel.SetActive(false);
            itModel.SetActive(true);
        }
        else
        {
            itModel.SetActive(false);
            normalModel.SetActive(true);
        }
    }

    // Optional: This draws a wireframe sphere in the Unity Editor so you can see how big your tagRadius is!
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tagRadius);
    }
}