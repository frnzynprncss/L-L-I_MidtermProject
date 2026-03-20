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

    // The Circuit Breaker to prevent the UI input glitch!
    public bool canMove = true;

    private Vector2 moveInput;
    private Rigidbody rb;
    private float stunTimer = 0f;

    void Start()
    {
        // Physics brain is safely on the Parent!
        rb = GetComponent<Rigidbody>();

        // Automatically find the Animator on the active child object if it's empty
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }

        // Spawn Logic
        PlayerInput playerInput = GetComponent<PlayerInput>();
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        if (playerInput != null && spawnPoints.Length > 0)
        {
            int index = playerInput.playerIndex % spawnPoints.Length;
            transform.position = spawnPoints[index].transform.position;
        }
    }

    void OnMove(InputValue value)
    {
        // Only accept new joystick inputs if the game says we are allowed to move
        if (canMove)
        {
            moveInput = value.Get<Vector2>();
        }
    }

    void OnJump(InputValue value)
    {
        if (canMove && value.isPressed && stunTimer <= 0 && IsGrounded())
        {
            // Reset Y velocity so jumping on moving platforms is consistent
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Trigger the Jump animation
            if (anim != null) anim.SetTrigger("Jump");
        }
    }



    void FixedUpdate()
    {
        // 1. FREEZE CHECK (For pre-game countdowns)
        if (!canMove)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0); // Keep falling, but stop running
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        // 2. KNOCKBACK STUN
        if (stunTimer > 0)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        // 3. NORMAL MOVEMENT
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        movement.y = rb.velocity.y; // Keep current jumping/falling momentum
        rb.velocity = movement;

        // 4. FACING DIRECTION
        if (moveInput != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0f, moveInput.y));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 15f);
        }

        // 5. RUNNING ANIMATION
        if (anim != null)
        {
            anim.SetFloat("Speed", moveInput.magnitude);
        }
    }

    bool IsGrounded()
    {
        // Invisible laser pointing down to check for the floor
        return Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, groundCheckDistance, groundLayer);
    }

    public void ApplyKnockback(Vector3 force, float stunDuration)
    {
        stunTimer = stunDuration;
        rb.velocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);

        // NEW: Play the knockback/stun animation!
        if (anim != null)
        {
            anim.SetTrigger("Knockback");
        }
    }

    // Called by the PlayerTagController when you swap between the Normal and It models
    public void SetActiveAnimator(Animator newAnim)
    {
        anim = newAnim;
    }

    // Called by the TagGameManager right when you hit Start to wipe any stuck inputs
    public void ResetInput()
    {
        moveInput = Vector2.zero;
    }
}