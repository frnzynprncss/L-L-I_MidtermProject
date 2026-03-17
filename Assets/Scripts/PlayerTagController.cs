using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTagController : MonoBehaviour
{
    [Header("Player State")]
    public bool isIt = false;

    [Header("Score System")]
    public int score = 0;
    public string playerName = "Player";

    [Header("3D Models")]
    public GameObject normalModel;
    public GameObject itModel;

    [Header("Tag & Shove Mechanics")]
    public float tagRadius = 2.5f;
    public float knockbackForce = 15f;
    public float tagCooldown = 2f;

    private float currentCooldown = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Automatically name the player based on their gamepad/keyboard index!
        playerName = "Player " + (GetComponent<PlayerInput>().playerIndex + 1);

        UpdateForm();
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    void OnTag()
    {
        // If my cooldown isn't finished, I can't tag OR shove anyone.
        if (currentCooldown > 0) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, tagRadius);

        foreach (Collider hit in hitColliders)
        {
            // Did I hit a player that is NOT me?
            if (hit.CompareTag("Player") && hit.gameObject != this.gameObject)
            {
                PlayerTagController targetPlayer = hit.GetComponent<PlayerTagController>();
                PlayerMovement targetMovement = hit.GetComponent<PlayerMovement>();

                if (targetPlayer != null && targetMovement != null)
                {
                    // Calculate which direction to push them
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;
                    pushDirection.y = 0.5f; // Upward pop

                    // SCENARIO 1: I am "It"
                    if (isIt)
                    {
                        // I can only tag Normal players
                        if (!targetPlayer.isIt)
                        {
                            this.BecomeNormal();
                            targetPlayer.BecomeIt(pushDirection * knockbackForce);
                            break;
                        }
                    }
                    // SCENARIO 2: I am a Normal player
                    else
                    {
                        // I can only shove OTHER Normal players. Ignore the "It" player!
                        if (!targetPlayer.isIt)
                        {
                            // Trigger the stun and push them away, but DO NOT change their state!
                            targetMovement.ApplyKnockback(pushDirection * knockbackForce, 0.5f);

                            // Put the shove on a cooldown so players can't spam the button
                            currentCooldown = tagCooldown;
                            break;
                        }
                    }
                }
            }
        }
    }

    public void BecomeIt(Vector3 knockbackAmount)
    {
        isIt = true;
        currentCooldown = tagCooldown;
        UpdateForm();

        PlayerMovement movementScript = GetComponent<PlayerMovement>();

        if (movementScript != null && knockbackAmount != Vector3.zero)
        {
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tagRadius);
    }
}