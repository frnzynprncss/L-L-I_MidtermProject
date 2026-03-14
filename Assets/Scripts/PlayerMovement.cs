using UnityEngine;
using UnityEngine.InputSystem; // You must include this to use the new Input System!

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Vector2 moveInput;
    private Rigidbody rb;

    void Start()
    {
        // Grab the Rigidbody attached to the parent prefab
        rb = GetComponent<Rigidbody>();
    }

    // The Player Input component automatically calls this function 
    // because we created an action called "Move"
    void OnMove(InputValue value)
    {
        // Store the joystick/keyboard input (X and Y)
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        // Convert the 2D input (X, Y) into 3D movement (X, Z)
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;

        // Preserve the current Y velocity so gravity still works!
        movement.y = rb.velocity.y;

        // Apply the movement to the Rigidbody
        rb.velocity = movement;
    }
}