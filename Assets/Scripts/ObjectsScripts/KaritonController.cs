using UnityEngine;

public class KaritonController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 10f;
    public float turnSpeed = 150f;
    public Transform seatPoint;

    // Change this to 'false' if your cart front is the Blue arrow
    // Set to 'true' if your cart front is the Red arrow
    public bool meshFacesRight = true;

    [Header("State")]
    private GameObject player;
    private Rigidbody cartRb;
    private bool isInside = false;
    private bool canEnter = false;

    void Start()
    {
        cartRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (canEnter && Input.GetKeyDown(KeyCode.F))
        {
            if (!isInside) EnterCart();
            else ExitCart();
        }
    }

    void FixedUpdate()
    {
        if (isInside)
        {
            HandleMovement();
        }
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // FIX: Choose the correct direction based on your model's orientation
        Vector3 moveDir = meshFacesRight ? transform.right : transform.forward;

        // 1. Move Forward/Backward
        Vector3 movement = moveDir * moveInput * moveSpeed * Time.fixedDeltaTime;
        cartRb.MovePosition(cartRb.position + movement);

        // 2. Rotate
        float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        cartRb.MoveRotation(cartRb.rotation * turnRotation);
    }

    void EnterCart()
    {
        if (player == null) return;
        isInside = true;

        // A. SHUT DOWN PLAYER
        var moveScript = player.GetComponent<PlayerMovement>();
        if (moveScript != null) moveScript.enabled = false;

        if (player.GetComponent<CharacterController>())
            player.GetComponent<CharacterController>().enabled = false;

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true;
            playerRb.detectCollisions = false;
        }

        if (player.GetComponent<Collider>())
            player.GetComponent<Collider>().enabled = false;

        // B. ATTACH AND FIX ROTATION
        player.transform.SetParent(seatPoint); // Parent to SeatPoint directly for easier math
        player.transform.localPosition = Vector3.zero; // Snap to center

        // FIX: Force the player to be upright relative to the seat
        player.transform.localRotation = Quaternion.identity;
    }

    void ExitCart()
    {
        isInside = false;
        player.transform.SetParent(null);

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.detectCollisions = true;
        }

        if (player.GetComponent<CharacterController>())
            player.GetComponent<CharacterController>().enabled = true;

        var moveScript = player.GetComponent<PlayerMovement>();
        if (moveScript != null) moveScript.enabled = true;

        if (player.GetComponent<Collider>())
            player.GetComponent<Collider>().enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            canEnter = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canEnter = false;
        }
    }
}