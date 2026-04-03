using UnityEngine;

public class KaritonController : MonoBehaviour
{
    public Transform seat;
    public float speed = 10f;
    public float turnSpeed = 100f;

    [Header("Controls")]
    public KeyCode interactButton = KeyCode.JoystickButton1; // B Button

    private bool isOccupied = false;
    private GameObject passenger;
    private bool playerIsNearby = false;
    private GameObject nearbyPlayer;

    void Update()
    {
        if (isOccupied)
        {
            HandleMovement();
            if (Input.GetKeyDown(interactButton)) ExitCart();
        }
        else if (playerIsNearby && Input.GetKeyDown(interactButton))
        {
            // 1. Get the Tag Controller to see if they are "It"
            if (nearbyPlayer.TryGetComponent(out PlayerTagController tagController))
            {
                // 2. Only allow if NOT "It" (not the Taya)
                if (!tagController.isIt)
                {
                    EnterCart(nearbyPlayer);
                }
                else
                {
                    Debug.Log("The Taya is not allowed in the Kariton!");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider foreignObject)
    {
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

        // 1. Disable scripts and physics to lock player in place
        if (passenger.TryGetComponent(out PlayerMovement pm)) pm.enabled = false;
        if (passenger.TryGetComponent(out CharacterController cc)) cc.enabled = false;

        // IMPORTANT: If player has Rigidbody, make it kinematic so they don't slide
        if (passenger.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;

        // 2. Force Animator to stay Idle
        if (passenger.TryGetComponent(out Animator anim))
        {
            anim.SetFloat("Speed", 0f);
            anim.SetFloat("Horizontal", 0f);
            anim.SetFloat("Vertical", 0f);
            anim.applyRootMotion = false; // Prevents animation from moving the player
        }

        // 3. Parenting
        passenger.transform.SetParent(seat);
        passenger.transform.localPosition = Vector3.zero;
        passenger.transform.localRotation = Quaternion.identity;
    }

    void HandleMovement()
    {
        float move = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;

        transform.Translate(move, 0, 0);
        transform.Rotate(0, turn, 0);
    }

    void ExitCart()
    {
        isOccupied = false;

        // Restore physics/animator settings
        if (passenger.TryGetComponent(out Animator anim)) anim.applyRootMotion = true;
        if (passenger.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;

        passenger.transform.SetParent(null);

        if (passenger.TryGetComponent(out PlayerMovement pm)) pm.enabled = true;
        if (passenger.TryGetComponent(out CharacterController cc)) cc.enabled = true;

        // Move player slightly outside so they don't get stuck in the cart
        passenger.transform.position += transform.forward * 2.0f + Vector3.up * 0.5f;
        passenger = null;
    }
}