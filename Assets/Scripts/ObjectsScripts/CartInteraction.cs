using UnityEngine;

public class CartInteraction : MonoBehaviour
{
    public Transform seatPoint;    // Drag the 'SeatPoint' object here
    private GameObject player;
    private bool isInside = false;
    private bool canEnter = false;

    void Update()
    {
        if (canEnter && Input.GetKeyDown(KeyCode.F))
        {
            if (!isInside) EnterCart();
            else ExitCart();
        }
    }

    void EnterCart()
    {
        isInside = true;

        // 1. Snap player to the seat position
        player.transform.position = seatPoint.position;
        player.transform.rotation = seatPoint.rotation;

        // 2. Make the player a child of the cart so they move together
        player.transform.SetParent(transform);

        // 3. Disable player movement scripts (replace 'PlayerMovement' with your script name)
        player.GetComponent<PlayerMovement>().enabled = false;
    }

    void ExitCart()
    {
        isInside = false;
        player.transform.SetParent(null); // Unparent
        player.GetComponent<PlayerMovement>().enabled = true;
    }

    // Detect if player is near the cart
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            canEnter = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canEnter = false;
        }
    }
}