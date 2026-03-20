using UnityEngine;

public class TagSystem : MonoBehaviour
{
    public float pushForce = 15f;
    public float upwardLift = 2f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we hit has a Rigidbody (the other player)
        Rigidbody enemyRb = other.GetComponent<Rigidbody>();

        if (enemyRb != null)
        {
            // Calculate direction: From Me to the Enemy
            Vector3 pushDir = (other.transform.position - transform.position).normalized;

            // Flatten the Y so we don't push them into the dirt
            pushDir.y = 0;

            // Apply the "Tag" momentum
            enemyRb.velocity = (pushDir * pushForce) + (Vector3.up * upwardLift);

            Debug.Log("TAGGED! Pushing " + other.name);
        }
    }
}