using UnityEngine;

public class StandingBalance : MonoBehaviour
{
    [Header("Balance Settings")]
    public float balanceStrength = 500f; // Force used to stand up
    public float balanceDamper = 25f;    // Prevents "shaking" or overshooting

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // High angular drag helps stop the character from spinning like a top
        rb.angularDrag = 10f;
    }

    void FixedUpdate()
    {
        // 1. Calculate the rotation difference between "Up" and our current rotation
        Quaternion deltaRotation = Quaternion.FromToRotation(transform.up, Vector3.up);

        // 2. Convert that rotation into a torque vector (Axis * Angle)
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        // 3. Apply the force
        // We multiply by Mathf.Deg2Rad to keep the numbers manageable
        if (angle > 0.1f) // Only apply if we are actually tilted
        {
            Vector3 balanceTorque = axis * (angle * Mathf.Deg2Rad * balanceStrength);

            // Subtract current angular velocity to act as a 'Damper'
            rb.AddTorque(balanceTorque - (rb.angularVelocity * balanceDamper));
        }
    }
}