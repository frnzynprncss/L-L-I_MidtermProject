using UnityEngine;
using System.Collections;

public class SafePlatform : MonoBehaviour
{
    [Header("Landing Spots")]
    public Transform[] landingSpots;
    public float horizontalFlySpeed = 5f;

    [Header("Levitation Cycle")]
    public float liftHeight = 5f;
    public float liftSpeed = 3f;
    public float minTimeAtBottom = 3f;
    public float maxTimeAtBottom = 8f;
    public float timeAtTop = 4f;

    [Header("Detection Settings")]
    public float antiCrushDistance = 1.5f;
    public Vector3 boxSize = new Vector3(2f, 0.5f, 2f);

    [Header("Anti-IT Forcefield")]
    public float repelForce = 25f;
    public float stunTime = 0.5f;

    // ==========================================
    // ---> NEW: ORB MAGNET SETTINGS <---
    // ==========================================
    [Header("Orb Magnet")]
    public float magnetRadius = 15f; // How far away to suck orbs from
    public float magnetSpeed = 15f;  // How fast orbs fly to the player
    public string orbTag = "Orb";    // Make sure your Orbs use this tag!

    private Vector3 currentGroundPosition;
    private Vector3 targetPosition;
    private bool isActive = false;
    private Collider platformCollider;

    // Tracks who is currently standing on the platform
    private GameObject currentRider;

    void Start()
    {
        currentGroundPosition = transform.position;
        targetPosition = currentGroundPosition;
        platformCollider = GetComponent<Collider>();
        StartCoroutine(PlatformCycleRoutine());
    }

    void Update()
    {
        if (targetPosition == currentGroundPosition && IsPlayerUnderneath())
        {
            return;
        }

        float currentSpeed = (targetPosition.y == transform.position.y) ? horizontalFlySpeed : liftSpeed;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

        // ==========================================
        // ---> NEW: MAGNETIZE ORBS IF RIDER IS SAFE <---
        // ==========================================
        if (currentRider != null)
        {
            PlayerTagController riderTag = currentRider.GetComponent<PlayerTagController>();

            // Double check that our rider hasn't suddenly become the IT!
            if (riderTag != null && !riderTag.isIt)
            {
                PullNearbyOrbs();
            }
            else
            {
                currentRider = null; // They are IT now, turn off the magnet!
            }
        }
    }

    // Sucks orbs through the air towards the player
    void PullNearbyOrbs()
    {
        // Draw an invisible sphere around the platform
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, magnetRadius);

        foreach (Collider col in nearbyObjects)
        {
            // If the object we found is an Orb...
            if (col.CompareTag(orbTag))
            {
                // Smoothly pull it directly into the player's body!
                col.transform.position = Vector3.MoveTowards(col.transform.position, currentRider.transform.position, magnetSpeed * Time.deltaTime);
            }
        }
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
        yield return new WaitForSeconds(Random.Range(0f, maxTimeAtBottom));

        while (true)
        {
            // 1. AT BOTTOM
            isActive = false;
            if (platformCollider != null) platformCollider.enabled = false;
            targetPosition = currentGroundPosition;

            float randomWaitTime = Random.Range(minTimeAtBottom, maxTimeAtBottom);
            yield return new WaitForSeconds(randomWaitTime);

            // 2. GOING UP
            isActive = true;
            if (platformCollider != null) platformCollider.enabled = true;
            targetPosition = currentGroundPosition + new Vector3(0, liftHeight, 0);

            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
                yield return null;

            // FLY TO A RANDOM SPOT
            if (landingSpots != null && landingSpots.Length > 0)
            {
                Transform chosenSpot = landingSpots[Random.Range(0, landingSpots.Length)];
                currentGroundPosition = chosenSpot.position;
                targetPosition = currentGroundPosition + new Vector3(0, liftHeight, 0);

                while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
                    yield return null;
            }

            // 3. AT TOP 
            yield return new WaitForSeconds(timeAtTop);

            // 4. GOING DOWN 
            targetPosition = currentGroundPosition;

            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
                yield return null;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerTagController tagController = collision.gameObject.GetComponent<PlayerTagController>();
            PlayerMovement movement = collision.gameObject.GetComponent<PlayerMovement>();

            if (tagController != null && movement != null)
            {
                if (tagController.isIt)
                {
                    // Repel the IT
                    if (isActive)
                    {
                        Vector3 pushDirection = (collision.transform.position - transform.position);
                        pushDirection.y = 0.2f;
                        movement.ApplyKnockback(pushDirection.normalized * repelForce, stunTime);
                    }

                    // If the IT jumped on us, they are definitely not a valid magnet rider
                    if (currentRider == collision.gameObject) currentRider = null;
                }
                else
                {
                    // ---> NEW: A normal player is standing on us! Log them as the rider. <---
                    currentRider = collision.gameObject;
                }
            }
        }
    }

    // ---> NEW: When the player jumps off, clear the rider to stop the magnet <---
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == currentRider)
        {
            currentRider = null;
        }
    }

    void OnDrawGizmos()
    {
        // Draw the anti-crush box
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down * antiCrushDistance, boxSize * 2);

        // Draw a blue sphere so you can see how big your magnet is in the editor!
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}