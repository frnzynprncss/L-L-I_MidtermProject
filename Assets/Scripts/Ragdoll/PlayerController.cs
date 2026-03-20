using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float pushSpeed = 2f;
    public float jumpForce = 7f;

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
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        movementInput = new Vector3(moveX, 0, moveZ).normalized;

        // SIMPLIFIED: Just check if E is held. 
        // This ensures the checkbox in the Animator actually ticks!
        isPushing = Input.GetKey(KeyCode.E);

        if (Input.GetButtonDown("Jump") && isGrounded)
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
            Vector3 targetPosition = rb.position + movementInput * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 15f * Time.fixedDeltaTime));
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

    private void OnTriggerStay(Collider other) { if (other.CompareTag("Ground")) isGrounded = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Ground")) isGrounded = false; }
}