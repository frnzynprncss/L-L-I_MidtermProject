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

    [Header("Speed Settings")]
    public float normalSpeed = 6f; // How fast normal players run
    public float itSpeed = 8f;     // How fast the IT runs

    [Header("Tag & Shove Mechanics")]
    public float tagRadius = 2.5f;
    public float knockbackForce = 15f;
    public float tagCooldown = 2f;

    [Tooltip("How many seconds the player is frozen after being shoved.")]
    public float stunDuration = 3f;

    private float currentCooldown = 0f;
    private Rigidbody rb;

    private PlayerMovement movementScript;
    private PlayerAppearance appearanceScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<PlayerMovement>();
        appearanceScript = GetComponent<PlayerAppearance>();

        // Name the player based on their gamepad/keyboard index
        playerName = "Player " + (GetComponent<PlayerInput>().playerIndex + 1);

        UpdateForm();

        // Set their starting speed immediately
        if (movementScript != null)
        {
            movementScript.SetSpeed(isIt ? itSpeed : normalSpeed);
        }

        // Tell the Dynamic Camera to track this player
        if (DynamicCamera.Instance != null)
        {
            DynamicCamera.Instance.AddPlayer(this.transform);
        }
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    // The Orb will call this when collected
    public void AddScore(int amount)
    {
        // 1. Increase the internal score
        score += amount;

        // 2. Tell the Appearance script to update the UI panel
        if (appearanceScript != null)
        {
            appearanceScript.AddScoreToHUD(score);
        }
    }

    void OnTag()
    {
        if (currentCooldown > 0) return;

        if (movementScript != null && movementScript.anim != null)
        {
            movementScript.anim.SetTrigger("Tag");
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, tagRadius);

        foreach (Collider hit in hitColliders)
        {
            PlayerTagController targetPlayer = hit.GetComponent<PlayerTagController>();

            if (targetPlayer != null && targetPlayer.gameObject != this.gameObject)
            {
                PlayerMovement targetMovement = targetPlayer.GetComponent<PlayerMovement>();

                Vector3 pushDirection = (targetPlayer.transform.position - transform.position).normalized;
                pushDirection.y = 0.5f;

                if (isIt)
                {
                    if (!targetPlayer.isIt)
                    {
                        this.BecomeNormal();
                        targetPlayer.BecomeIt(pushDirection * knockbackForce);
                        break;
                    }
                }
                else
                {
                    if (!targetPlayer.isIt)
                    {
                        targetMovement.ApplyKnockback(pushDirection * knockbackForce, stunDuration);
                        currentCooldown = tagCooldown;
                        break;
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

        // Tell the Appearance script we are IT so it turns the HUD Red
        if (appearanceScript != null)
        {
            appearanceScript.UpdateVisuals();
        }

        // Give them the IT speed boost!
        if (movementScript != null)
        {
            movementScript.SetSpeed(itSpeed);
        }

        if (movementScript != null && knockbackAmount != Vector3.zero)
        {
            movementScript.ApplyKnockback(knockbackAmount, stunDuration);
        }
    }

    public void BecomeNormal()
    {
        isIt = false;
        UpdateForm();

        // Tell the Appearance script we are Normal so it turns the HUD back to color
        if (appearanceScript != null)
        {
            appearanceScript.UpdateVisuals();
        }

        // Slow them back down to normal speed!
        if (movementScript != null)
        {
            movementScript.SetSpeed(normalSpeed);
        }
    }

    void UpdateForm()
    {
        Animator activeAnim = null;

        if (isIt)
        {
            normalModel.SetActive(false);
            itModel.SetActive(true);

            activeAnim = itModel.GetComponent<Animator>();
        }
        else
        {
            itModel.SetActive(false);
            normalModel.SetActive(true);

            activeAnim = normalModel.GetComponent<Animator>();
        }

        if (movementScript != null && activeAnim != null)
        {
            movementScript.SetActiveAnimator(activeAnim);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tagRadius);
    }
}