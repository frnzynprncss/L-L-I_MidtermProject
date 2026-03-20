using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController_2 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float pushSpeed = 2f;
    public float jumpForce = 7f;

    [Header("Player 2 Specific Keys")]
    // Hardcoded for P2 (using Arrows/Right Ctrl as an example)
    public KeyCode pushKey = KeyCode.RightShift;
    public KeyCode jumpKey = KeyCode.RightControl;

    private Animator anim;
    private Rigidbody rb;
    private bool isGrounded;

    private Vector3 movementInput;
    private bool jumpRequested;
    private bool isPushing;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Prevent self-collision jitter
        var colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = i + 1; j < colliders.Length; j++)
            {
                Physics.IgnoreCollision(colliders[i], colliders[j]);
            }
        }
    }

    void Update()
    {
        // Hardcoded to look for "Horizontal2" and "Vertical2" in Input Manager
        float moveX = Input.GetAxisRaw("Horizontal2");
        float moveZ = Input.GetAxisRaw("Vertical2");
        movementInput = new Vector3(moveX, 0, moveZ).normalized;

        isPushing = Input.GetKey(pushKey);

        // Uses the specific P2 Jump Key
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            jumpRequested = true;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", movementInput.magnitude);
            anim.SetBool("isPushing", isPushing);
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
        if (jumpRequested) Jump();
    }

    void MovePlayer()
    {
        if (movementInput.magnitude >= 0.1f)
        {
            float currentSpeed = isPushing ? pushSpeed : moveSpeed;

            // NEW: Calculate target velocity based on input
            Vector3 targetVelocity = movementInput * currentSpeed;

            // Manually set the X and Z velocity, but keep the current Y (Gravity/Jumping)
            // This effectively "cancels out" the sliding force the moment you move
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

            // Rotation logic remains the same
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 15f * Time.fixedDeltaTime));
        }
        else if (isGrounded)
        {
            // If no input is given and we are on the ground, 
            // kill the horizontal velocity so we don't slide.
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        if (anim != null) anim.SetTrigger("Jump");
        jumpRequested = false;
        isGrounded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherRb = collision.collider.attachedRigidbody;
        if (otherRb != null && !otherRb.isKinematic && isPushing)
        {
            Vector3 pushDir = collision.contacts[0].normal * -1;
            pushDir.y = 0;
            otherRb.AddForce(pushDir * 5f, ForceMode.Impulse);
        }
    }

    private void OnTriggerStay(Collider other) { if (other.CompareTag("Ground")) isGrounded = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Ground")) isGrounded = false; }
}