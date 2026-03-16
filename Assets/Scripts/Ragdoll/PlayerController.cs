using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Animator anim;
    private Rigidbody rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Get input from WASD or Arrow Keys
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized;

        // Apply movement to Rigidbody
        if (movement.magnitude >= 0.1f)
        {
            rb.MovePosition(transform.position + movement * moveSpeed * Time.deltaTime);

            // Make the player face the direction they are walking
            transform.forward = movement;
        }

        // --- THE ANIMATION PART ---
        // We send the movement "amount" to the Animator
        // If movement is 0, it goes to Idle. If it's > 0.1, it walks.
        float currentSpeed = movement.magnitude;
        anim.SetFloat("Speed", currentSpeed);
    }
}