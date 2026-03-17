using UnityEngine;

// This line automatically adds a Rigidbody and Animator if they are missing
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private Animator anim;
    private Rigidbody rb;
    private bool isGrounded;

    private Vector3 movementInput;
    private bool jumpRequested;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Safety check: Make sure components exist
        if (rb == null) Debug.LogError("Rigidbody is missing from " + gameObject.name);
        if (anim == null) Debug.LogError("Animator is missing from " + gameObject.name);

        // Smooths out movement physics
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // Get Input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        movementInput = new Vector3(moveX, 0, moveZ).normalized;

        // Only allow jump request if we are grounded according to the "Ground" tag
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }

        // Update Animator
        if (anim != null)
        {
            anim.SetFloat("Speed", movementInput.magnitude);
        }
    }

    void FixedUpdate()
    {
        MovePlayer();

        if (jumpRequested)
        {
            Jump();
        }
    }

    void MovePlayer()
    {
        if (movementInput.magnitude >= 0.1f)
        {
            Vector3 targetPosition = rb.position + movementInput * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 15f * Time.fixedDeltaTime));
        }
    }

    void Jump()
    {
        // Reset vertical velocity for a consistent jump height
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (anim != null)
        {
            anim.SetTrigger("Jump");
        }

        jumpRequested = false;
        isGrounded = false; // Immediately set to false so they can't double jump
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}