using UnityEngine;
using System.Collections;

public class SafePlatform : MonoBehaviour
{
    [Header("Levitation Cycle")]
    public float liftHeight = 5f;
    public float liftSpeed = 3f;
    public float timeAtBottom = 5f;
    public float timeAtTop = 4f;

    [Header("Detection Settings")]
    public float antiCrushDistance = 1.5f; // How far below to look for players
    public Vector3 boxSize = new Vector3(2f, 0.5f, 2f); // The width of the "safety check"

    [Header("Anti-IT Forcefield")]
    public float repelForce = 25f;
    public float stunTime = 0.5f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isActive = false;
    private Collider platformCollider;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition;
        platformCollider = GetComponent<Collider>();
        StartCoroutine(PlatformCycleRoutine());
    }

    void Update()
    {
        // SAFETY CHECK: If we are moving DOWN, check if a player is underneath
        if (targetPosition == startPosition && IsPlayerUnderneath())
        {
            // STOP MOVING! Wait for the player to leave.
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, liftSpeed * Time.deltaTime);
    }

    bool IsPlayerUnderneath()
    {
        // This creates an invisible box below the cloud to see if a player is trapped
        RaycastHit hit;
        if (Physics.BoxCast(transform.position, boxSize, Vector3.down, out hit, transform.rotation, antiCrushDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true; // A player is in the way!
            }
        }
        return false;
    }

    IEnumerator PlatformCycleRoutine()
    {
        while (true)
        {
            // 1. AT BOTTOM
            isActive = false;
            if (platformCollider != null) platformCollider.enabled = false;
            targetPosition = startPosition;
            yield return new WaitForSeconds(timeAtBottom);

            // 2. GOING UP
            isActive = true;
            if (platformCollider != null) platformCollider.enabled = true;
            targetPosition = startPosition + new Vector3(0, liftHeight, 0);

            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
                yield return null;

            // 3. AT TOP
            yield return new WaitForSeconds(timeAtTop);

            // 4. GOING DOWN
            targetPosition = startPosition;

            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
                yield return null;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (isActive && collision.gameObject.CompareTag("Player"))
        {
            PlayerTagController tagController = collision.gameObject.GetComponent<PlayerTagController>();
            PlayerMovement movement = collision.gameObject.GetComponent<PlayerMovement>();

            if (tagController != null && movement != null && tagController.isIt)
            {
                Vector3 pushDirection = (collision.transform.position - transform.position);
                pushDirection.y = 0.2f; // Always nudge slightly UP, never down
                movement.ApplyKnockback(pushDirection.normalized * repelForce, stunTime);
            }
        }
    }

    // Visualizes the safety box in the Scene view so you can see it working
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down * antiCrushDistance, boxSize * 2);
    }
}