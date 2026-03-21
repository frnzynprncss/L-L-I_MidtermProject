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

    //added these
    [Header("Player Info")]
    public string playerName = "Player"; // Added for the Leaderboard
    public int playerID;
    public int score = 0;
    public bool isIt = false;

    // ---> WE ADDED THIS HERE SO ALL FUNCTIONS CAN SEE IT! <---
    private PlayerMovement movementScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Grab the movement script once when the game starts
        movementScript = GetComponent<PlayerMovement>();

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
        if (currentCooldown > 0) return;

        // Play the shoving/tagging animation immediately!
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
                        targetMovement.ApplyKnockback(pushDirection * knockbackForce, 0.5f);
                        currentCooldown = tagCooldown;
                        break;
                    }
                }
            }
        }
    }

    public void BecomeIt(Vector3 spawnPos)
    {
        isIt = true;
        // Add visual changes here (e.g., change color to Red)
        Debug.Log(playerName + " is IT!");
    }

    public void BecomeNormal()
    {
        isIt = false;
        // Add visual changes here (e.g., change color to Blue)
        Debug.Log(playerName + " is a Runner!");
    }

    void UpdateForm()
    {
        Animator activeAnim = null;

        if (isIt)
        {
            normalModel.SetActive(false);
            itModel.SetActive(true);

            // Grab the Animator from the newly activated IT model
            activeAnim = itModel.GetComponent<Animator>();
        }
        else
        {
            itModel.SetActive(false);
            normalModel.SetActive(true);

            // Grab the Animator from the newly activated Normal model
            activeAnim = normalModel.GetComponent<Animator>();
        }

        // Send the active Animator to the PlayerMovement script!
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