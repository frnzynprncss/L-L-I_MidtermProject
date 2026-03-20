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
    public KeyCode pushKey = KeyCode.RightShift;
    public KeyCode jumpKey = KeyCode.RightControl;

    private Animator anim;
    private Rigidbody rb;
    private bool isGrounded;

    private Vector3 movementInput;
    private bool jumpRequested;
    private bool isPushing;

    // fall anim
    private float fallTimer = 0f;
    public float fallDuration = 1.5f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        /*var colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = i + 1; j < colliders.Length; j++)
            {
                Physics.IgnoreCollision(colliders[i], colliders[j]);
            }
        } */
    }

    void Update()
    {
        // 1. FALL TIMER LOGIC
        if (fallTimer > 0)
        {
            fallTimer -= Time.deltaTime;
            movementInput = Vector3.zero;

            if (anim != null)
            {
                anim.SetFloat("Speed", 0);
                anim.SetBool("isFalling", true);
            }
            return;
        }
        else
        {
            if (anim != null) anim.SetBool("isFalling", false);
        }

        float moveX = Input.GetAxisRaw("Horizontal2");
        float moveZ = Input.GetAxisRaw("Vertical2");
        movementInput = new Vector3(moveX, 0, moveZ).normalized;

        isPushing = Input.GetKey(pushKey);

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
            Vector3 targetVelocity = movementInput * currentSpeed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 15f * Time.fixedDeltaTime));
        }
        else if (isGrounded)
        {
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

    private void OnCollisionStay(Collision collision)
    {
        // Checks every frame you are touching the other player
        if (Input.GetKey(KeyCode.RightShift))
        {
            Rigidbody otherRb = collision.collider.attachedRigidbody;

            if (otherRb != null && !otherRb.isKinematic)
            {
                Debug.Log("SUCCESS! Pushing " + otherRb.gameObject.name);

                Vector3 pushDir = (otherRb.position - transform.position).normalized;
                pushDir.y = 0;
                otherRb.AddForce(pushDir * 10f, ForceMode.Impulse);

                otherRb.SendMessage("SetFallTimer", fallDuration, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void OnTriggerStay(Collider other) { if (other.CompareTag("Ground")) isGrounded = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Ground")) isGrounded = false; }

    public void SetFallTimer(float duration)
    {
        fallTimer = duration;
        if (anim != null) anim.SetBool("isFalling", true);
    }
}