using UnityEngine;
using UnityEngine.InputSystem;

public class KaritonController : MonoBehaviour
{
    [Header("Settings")]
    public float cartSpeed = 12f;
    public float interactionRadius = 3.5f;
    public Transform seatPoint;

    [Header("References")]
    public GameObject player;
    private PlayerMovement playerScript;
    private Rigidbody rb;
    private bool isOccupied = false;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = 15f; // Heavier cart is more stable
            rb.drag = 1.5f;
            // Stops the cart from falling over sideways
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void Update()
    {
        // 1. Find Player if missing
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerScript = player.GetComponent<PlayerMovement>();
            return;
        }

        // 2. Interaction
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (isOccupied) ExitCart();
            else if (Vector3.Distance(transform.position, player.transform.position) <= interactionRadius) EnterCart();
        }

        // 3. Driving Input
        if (isOccupied)
        {
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
            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            // Move the cart (Preserve current gravity)
            rb.velocity = new Vector3(direction.x * cartSpeed, rb.velocity.y, direction.z * cartSpeed);

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            }

            // Force player to stay in the seat every physics frame
            if (seatPoint != null)
            {
                player.transform.position = seatPoint.position;
                player.transform.rotation = seatPoint.rotation;
            }
        }
    }

    public void EnterCart()
    {
        isOccupied = true;

        if (playerScript != null)
        {
            playerScript.ResetInput();
            playerScript.canMove = false;
        }

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        Collider playerCol = player.GetComponent<Collider>();

        // Disable Player physics so they don't fight the cart
        if (playerRb != null)
        {
            playerRb.isKinematic = true;
            playerRb.velocity = Vector3.zero;
        }

        // CRITICAL: Disable collider so the cart doesn't "fly"
        if (playerCol != null) playerCol.enabled = false;

        player.transform.SetParent(this.transform);

        if (seatPoint != null)
        {
            player.transform.position = seatPoint.position;
            player.transform.rotation = seatPoint.rotation;
        }
    }

    public void ExitCart()
    {
        isOccupied = false;
        player.transform.SetParent(null);

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        Collider playerCol = player.GetComponent<Collider>();

        if (playerRb != null) playerRb.isKinematic = false;
        if (playerCol != null) playerCol.enabled = true; // Walk again!
        if (playerScript != null) playerScript.canMove = true;

        rb.velocity = Vector3.zero;
    }
}