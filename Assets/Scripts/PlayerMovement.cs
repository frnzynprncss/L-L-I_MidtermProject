using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Vector2 moveInput;
    private Rigidbody rb;

    // Timer to track how long the player is stunned by a knockback
    private float stunTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // --- THE SPAWN OVERRIDE (Brought back from the dead!) ---
        PlayerInput playerInput = GetComponent<PlayerInput>();
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        if (playerInput != null && spawnPoints.Length > 0)
        {
            int index = playerInput.playerIndex % spawnPoints.Length;
            transform.position = spawnPoints[index].transform.position;
        }
        // --------------------------------------------------------
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        // If we are currently stunned, let physics take over and ignore the joystick
        if (stunTimer > 0)
        {
            stunTimer -= Time.fixedDeltaTime;
            return;
        }

        // Standard movement
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        movement.y = rb.velocity.y;
        rb.velocity = movement;
    }

    // Called from the Tag script
    public void ApplyKnockback(Vector3 force, float stunDuration)
    {
        stunTimer = stunDuration;
        rb.velocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);
    }
}