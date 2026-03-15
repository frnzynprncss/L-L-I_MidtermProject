using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float turnSpeed = 0.15f;

    public Rigidbody rb;
    private Vector3 inputs = Vector3.zero;

    void Start()
    {
        // Automatically grab the Rigidbody if you forgot to drag it in
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Unity's "Horizontal" = A/D keys
        // Unity's "Vertical" = W/S keys
        inputs.x = Input.GetAxisRaw("Vertical");
        inputs.z = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        // Only move if a key is being pressed
        if (inputs != Vector3.zero)
        {
            // Move the Hips/Body Rigidbody
            Vector3 movement = inputs.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);

            // Rotate the body to face the direction you are walking
            Quaternion targetRotation = Quaternion.LookRotation(inputs);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed));
        }
    }
}