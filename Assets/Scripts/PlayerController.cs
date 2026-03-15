using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float turnSpeed = 10f; // Increased for better responsiveness

    public Rigidbody rb;
    private Vector3 inputs = Vector3.zero;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // This forces the physics engine to treat the body as an upright pillar
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Optional: Make the body harder to knock over by increasing mass
        rb.mass = 20f;
    }

    void Update()
    {
        // Corrected mapping: X is Horizontal, Z is Vertical
        inputs.x = Input.GetAxisRaw("Horizontal");
        inputs.z = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        if (inputs.magnitude > 0.1f)
        {
            // Movement: Use .velocity instead of .linearVelocity
            Vector3 moveDirection = inputs.normalized;
            Vector3 targetVelocity = moveDirection * speed;

            // Apply velocity while keeping the current gravity (y)
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

            // Rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
        else
        {
            // Stop movement when no input is given
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
}