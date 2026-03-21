using UnityEngine;
using System.Collections;

public class SafePlatform : MonoBehaviour
{
    [Header("Levitation Cycle")]
    public float liftHeight = 5f;
    public float liftSpeed = 3f;

    // CHANGED: Replaced the single timeAtBottom with a min and max range
    public float minTimeAtBottom = 3f;
    public float maxTimeAtBottom = 8f;

    public float timeAtTop = 4f;

    [Header("Detection Settings")]
    public float antiCrushDistance = 1.5f;
    public Vector3 boxSize = new Vector3(2f, 0.5f, 2f);

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
        if (targetPosition == startPosition && IsPlayerUnderneath())
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, liftSpeed * Time.deltaTime);
    }

    bool IsPlayerUnderneath()
    {
        RaycastHit hit;
        if (Physics.BoxCast(transform.position, boxSize, Vector3.down, out hit, transform.rotation, antiCrushDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator PlatformCycleRoutine()
    {
        // NEW: Add a random delay right at the start so all platforms instantly desync when the game loads
        yield return new WaitForSeconds(Random.Range(0f, maxTimeAtBottom));

        while (true)
        {
            // 1. AT BOTTOM
            isActive = false;
            if (platformCollider != null) platformCollider.enabled = false;
            targetPosition = startPosition;

            // CHANGED: Pick a random wait time for this specific cycle
            float randomWaitTime = Random.Range(minTimeAtBottom, maxTimeAtBottom);
            yield return new WaitForSeconds(randomWaitTime);

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
                pushDirection.y = 0.2f;
                movement.ApplyKnockback(pushDirection.normalized * repelForce, stunTime);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down * antiCrushDistance, boxSize * 2);
    }
}