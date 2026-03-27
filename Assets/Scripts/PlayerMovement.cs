using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float jumpForce = 8f;
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;

    [Header("Animation")]
    public Animator anim;
    public bool canMove = true;

    // ---> NEW: Cutscene Lock <---
    public bool isPlayingCutscene = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip knockbackSound;

    private Vector2 moveInput;
    private Rigidbody rb;
    private float stunTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        if (playerInput != null && spawnPoints.Length > 0)
        {
            int index = playerInput.playerIndex % spawnPoints.Length;
            transform.position = spawnPoints[index].transform.position;
        }

        if (MatchFlowManager.Instance != null)
        {
            isPlayingCutscene = !MatchFlowManager.Instance.matchHasStarted;
        }
    }

    void OnMove(InputValue value)
    {
        // Ignore the controller if we are in a cutscene
        if (canMove && !isPlayingCutscene)
        {
            moveInput = value.Get<Vector2>();
        }
    }

    void OnJump(InputValue value)
    {
        // Ignore the controller if we are in a cutscene
        if (canMove && !isPlayingCutscene && value.isPressed && stunTimer <= 0 && IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            if (anim != null) anim.SetTrigger("Jump");

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }
    }

    void FixedUpdate()
    {
        // HANDLE STUNS
        if (stunTimer > 0)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0) canMove = true;
            return;
        }

        // ---> THE BIG FIX IS HERE <---
        // Only run movement logic if we are allowed to move.
        // If we are in the cart (canMove is false), we do NOTHING.
        if (canMove && !isPlayingCutscene)
        {
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
            movement.y = rb.velocity.y;
            rb.velocity = movement;

            if (moveInput != Vector2.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0f, moveInput.y));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 15f);
            }

            if (anim != null) anim.SetFloat("Speed", moveInput.magnitude);
        }
        else
        {
            // If we are in the cart, ensure the animator stops running
            if (anim != null) anim.SetFloat("Speed", 0f);

            // Do NOT set rb.velocity = Vector3.zero here because the body is Kinematic!
            // Unity will throw the error if you touch velocity while kinematic.
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, groundCheckDistance, groundLayer);
    }

    public void ApplyKnockback(Vector3 force, float stunDuration)
    {
        stunTimer = stunDuration;
        canMove = false;
        moveInput = Vector2.zero;

        rb.velocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);

        if (anim != null) anim.SetTrigger("Knockback");

        if (audioSource != null && knockbackSound != null)
        {
            audioSource.PlayOneShot(knockbackSound);
        }
    }

    public void SetActiveAnimator(Animator newAnim)
    {
        anim = newAnim;
    }

    public void ResetInput()
    {
        moveInput = Vector2.zero;
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}