using UnityEngine;

public class PushImpact : MonoBehaviour
{
    public float pushStrength = 10f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if we hit someone with a Rigidbody (the other player)
        Rigidbody victimRb = other.GetComponent<Rigidbody>();

        if (victimRb != null && other.gameObject != transform.root.gameObject)
        {
            // Calculate direction from me to the victim
            Vector3 pushDir = other.transform.position - transform.position;
            pushDir.y = 0; // Keep the push horizontal

            // Apply the force!
            victimRb.AddForce(pushDir.normalized * pushStrength, ForceMode.Impulse);
            Debug.Log("Pushed: " + other.name);
        }
    }
}