using UnityEngine;

public class KaritonController : MonoBehaviour
{
    public Transform seat;
    public float speed = 10f;
    public float turnSpeed = 100f;

    private bool isOccupied = false;
    private GameObject passenger;
    private bool playerIsNearby = false;
    private GameObject nearbyPlayer;

    void Update()
    {
        // 1. Logic for when someone is ALREADY RIDING
        if (isOccupied)
        {
            HandleMovement();
            if (Input.GetKeyDown(KeyCode.F)) ExitCart();
        }
        // 2. Logic for when someone is NEARBY and wants to board
        else if (playerIsNearby && Input.GetKeyDown(KeyCode.F))
        {
            EnterCart(nearbyPlayer);
        }
    }

    private void OnTriggerEnter(Collider foreignObject)
    {
        // Check if the thing entering the zone is the Player
        if (foreignObject.CompareTag("Player"))
        {
            playerIsNearby = true;
            nearbyPlayer = foreignObject.gameObject;
        }
    }

    private void OnTriggerExit(Collider foreignObject)
    {
        if (foreignObject.CompareTag("Player"))
        {
            playerIsNearby = false;
            nearbyPlayer = null;
        }
    }

    public void EnterCart(GameObject player)
    {
        passenger = player;
        isOccupied = true;

        // 1. Disable player scripts
        if (passenger.TryGetComponent(out PlayerMovement pm)) pm.enabled = false;
        if (passenger.TryGetComponent(out CharacterController cc)) cc.enabled = false;

        // 2. STOP THE ANIMATION
        // Most Unity characters use a parameter called "Speed" or "Forward"
        if (passenger.TryGetComponent(out Animator anim))
        {
            // Reset common animation parameters to 0 so the player stays static/idle
            anim.SetFloat("Speed", 0f);
            anim.SetFloat("Horizontal", 0f);
            anim.SetFloat("Vertical", 0f);
            // If you use a Boolean for walking, set it to false:
            // anim.SetBool("isWalking", false);
        }

        // 3. Parenting logic
        passenger.transform.SetParent(seat);
        passenger.transform.localPosition = Vector3.zero;
        passenger.transform.localRotation = Quaternion.identity;
    }

    void HandleMovement()
    {
        float move = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;

        // Change this line: 
        // Move on the X axis (first parameter) instead of the Z axis (third parameter)
        transform.Translate(move, 0, 0);

        // Rotation usually stays on the Y axis, but if it spins like a top 
        // on its side, we may need to adjust this too.
        transform.Rotate(0, turn, 0);
    }

    void ExitCart()
    {
        isOccupied = false;

        // 1. Unparent the player
        passenger.transform.SetParent(null);

        // 2. Re-enable player scripts immediately
        if (passenger.TryGetComponent(out PlayerMovement pm)) pm.enabled = true;
        if (passenger.TryGetComponent(out CharacterController cc)) cc.enabled = true;

        // 3. Simple Offset: Move the player 2 units to the SIDE of the cart
        // Using 'transform.forward' here because your cart moves on its 'right' axis
        // This should put the player next to the cart instead of inside it.
        passenger.transform.position += transform.forward * 2.0f + Vector3.up * 0.5f;

        // 4. Clear the passenger reference
        passenger = null;
    }
}