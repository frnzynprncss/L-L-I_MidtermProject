using UnityEngine;

public class LimbFollower : MonoBehaviour
{
    public Transform targetBone; // Drag the matching bone from Animation_Target here
    private ConfigurableJoint joint;
    private Quaternion startingRotation;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        startingRotation = transform.localRotation;
    }

    void FixedUpdate()
    {
        // This math tells the physical joint to rotate to match the animation bone
        joint.targetRotation = Quaternion.Inverse(targetBone.localRotation) * startingRotation;
    }
}