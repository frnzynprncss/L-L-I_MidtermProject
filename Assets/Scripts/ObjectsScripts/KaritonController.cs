using UnityEngine;
using UnityEngine.InputSystem;

public class KaritonController : MonoBehaviour
{
    [Header("Settings")]
    public float cartSpeed = 12f; // Faster than the player's 7f
    public float interactionRadius = 2.5f;
    public Transform seatPoint; // Create an empty GameObject on the cart for this

    [Header("References")]
    private GameObject player;
    private PlayerMovement playerScript;
    private Rigidbody rb;
    private bool isOccupied = false;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Find the player in the scene
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerScript = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Check for "F" key press
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (isOccupied)
            {
                Dismount();
            }
            else if (Vector3.Distance(transform.position, player.transform.position) <= interactionRadius)
            {
                Mount();
            }
        }

        // If occupied, capture the movement input from the Input System
        if (isOccupied)
        {
            // We read the WASD/LeftStick input directly for the cart
            moveInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) moveInput.y = 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y = -1;
            if (Keyboard.current.aKey.isPressed) moveInput.x = -1;
            if (Keyboard.current.dKey.isPressed) moveInput.x = 1;
        }
    }

    void FixedUpdate()
    {
        if (isOccupied)
        {
            // Move the Kariton
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized * cartSpeed;
            rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

            // Rotate the Kariton to face movement direction
            if (moveInput != Vector2.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0f, moveInput.y));
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            }
        }
    }

    void Mount()
    {
        isOccupied = true;

        // 1. Disable Player Movement
        playerScript.canMove = false;
        playerScript.ResetInput();

        // 2. Parent Player to Cart
        player.transform.SetParent(transform);

        // 3. Position Player on Seat
        if (seatPoint != null)
        {
            player.transform.position = seatPoint.position;
            player.transform.rotation = seatPoint.rotation;
        }

        // 4. Handle Physics
        player.GetComponent<Rigidbody>().isKinematic = true;
    }

    void Dismount()
    {
        isOccupied = false;
        playerScript.canMove = true;
        player.transform.SetParent(null);
        player.GetComponent<Rigidbody>().isKinematic = false;

        // Add this to prevent the "non-kinematic" cart from sliding away
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Visual aid in Editor to see the interaction range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}