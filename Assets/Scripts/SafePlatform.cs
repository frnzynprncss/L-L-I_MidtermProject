using UnityEngine;
using System.Collections;

public class SafePlatform : MonoBehaviour
{
    [Header("Levitation Cycle")]
    public float liftHeight = 5f;
    public float liftSpeed = 3f;
    public float timeAtBottom = 5f;
    public float timeAtTop = 4f;

    [Header("Anti-IT Forcefield")]
    public float repelForce = 25f; // Made it slightly stronger!
    public float stunTime = 0.5f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isActive = false;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition;

        StartCoroutine(PlatformCycleRoutine());
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, liftSpeed * Time.deltaTime);
    }

    IEnumerator PlatformCycleRoutine()
    {
        while (true)
        {
            // 1. RESTING AT BOTTOM
            isActive = false; // The forcefield is OFF. The IT player can safely walk here.
            targetPosition = startPosition;
            yield return new WaitForSeconds(timeAtBottom);

            // 2. GOING UP 
            isActive = true; // The forcefield is ON!
            targetPosition = startPosition + new Vector3(0, liftHeight, 0);

            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                yield return null;
            }

            // 3. HANG TIME (Still active, IT cannot board)
            yield return new WaitForSeconds(timeAtTop);

            // 4. GOING DOWN (Still active)
            targetPosition = startPosition;

            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                yield return null;
            }
        }
    }

    // NEW: We use hard physical collisions instead of an invisible trigger
    void OnCollisionStay(Collision collision)
    {
        // If the platform is active (moving or up) and a player physically touches it...
        if (isActive && collision.gameObject.CompareTag("Player"))
        {
            PlayerTagController tagController = collision.gameObject.GetComponent<PlayerTagController>();
            PlayerMovement movement = collision.gameObject.GetComponent<PlayerMovement>();

            // If we found their scripts, and that player happens to be IT...
            if (tagController != null && movement != null && tagController.isIt)
            {
                // Calculate a push direction horizontally AWAY from the center of the platform
                Vector3 pushDirection = (collision.transform.position - transform.position);
                pushDirection.y = 0; // Keep the math flat first
                pushDirection = pushDirection.normalized;

                // If they are on top of the platform, pop them up. If they hit their head underneath, push them down!
                float verticalPop = (collision.transform.position.y > transform.position.y) ? 0.5f : -0.5f;
                pushDirection.y = verticalPop;

                // Violently shove them away!
                movement.ApplyKnockback(pushDirection * repelForce, stunTime);
            }
        }
    }
}