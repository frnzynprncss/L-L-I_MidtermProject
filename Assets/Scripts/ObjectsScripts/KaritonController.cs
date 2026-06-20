using UnityEngine;

public class KaritonController : MonoBehaviour
{
    public Transform seat;
    public float speed = 10f;
    public float turnSpeed = 100f;

    private bool isOccupied = false;
    private GameObject passenger;
    private Vector2 moveInput;

    public void RequestRide(GameObject player)
    {
        if (isOccupied)
        {
            // If the player calling this IS the passenger, they want to EXIT
            if (player == passenger) ExitCart();
            return;
        }

        if (player.TryGetComponent(out PlayerTagController tagController))
        {
            if (tagController.isIt) return;
        }

        EnterCart(player);
    }

    private void EnterCart(GameObject player)
    {
        passenger = player;
        isOccupied = true;

        if (passenger.TryGetComponent(out PlayerMovement pm)) pm.canMove = false;
        if (passenger.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;

        // DISABLING COLLIDER: This stops the "falling out of bounds" glitch
        if (passenger.TryGetComponent(out Collider col)) col.enabled = false;

        passenger.transform.SetParent(seat);
        passenger.transform.localPosition = Vector3.zero;
        passenger.transform.localRotation = Quaternion.identity;
    }

    private void ExitCart()
    {
        // Re-enable everything so they can walk again
        if (passenger.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
        if (passenger.TryGetComponent(out Collider col)) col.enabled = true;
        if (passenger.TryGetComponent(out PlayerMovement pm)) pm.canMove = true;

        passenger.transform.SetParent(null);

        // Spawn the player slightly behind the cart so they don't get stuck
        passenger.transform.position += transform.forward * -2.5f + Vector3.up * 0.5f;

        passenger = null;
        isOccupied = false;
        moveInput = Vector2.zero;
    }

    public void SetMoveInput(Vector2 input) => moveInput = input;

    void Update()
    {
        if (isOccupied)
        {
            // FIX: Changed from (move, 0, 0) to (0, 0, move) for forward movement
            float move = moveInput.y * speed * Time.deltaTime;
            float turn = moveInput.x * turnSpeed * Time.deltaTime;
            transform.Translate(0, 0, move);
            transform.Rotate(0, turn, 0);
        }
    }
}