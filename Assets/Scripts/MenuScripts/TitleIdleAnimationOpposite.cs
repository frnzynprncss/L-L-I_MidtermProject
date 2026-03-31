using UnityEngine;

public class TitleIdleAnimationOpposite : MonoBehaviour
{
    [Header("Target Object")]
    [Tooltip("Drag the Main Camera here. If empty, it targets the object this script is on.")]
    public Transform targetTransform;

    [Header("Floating Settings")]
    public float floatAmplitude = 0.2f;
    public float floatSpeed = 2f;

    [Header("Breathing Settings")]
    public float pulseAmount = 0.02f;
    public float pulseSpeed = 1.5f;

    [Header("Rotation Settings")]
    public float tiltAngle = 1.5f;
    public float tiltSpeed = 1f;

    private Vector3 startPosition;
    private Vector3 startScale;
    private Quaternion startRotation;
    private bool isAnimating = true;

    void Start()
    {
        // If nothing is assigned, use the current object (the Camera)
        if (targetTransform == null)
        {
            targetTransform = this.transform;
        }

        // CAPTURE the starting values so we know where to stay
        startPosition = targetTransform.localPosition;
        startScale = targetTransform.localScale;
        startRotation = targetTransform.localRotation;
    }

    public void StopAnimation()
    {
        isAnimating = false;

        // Reset to the exact values captured at Start
        if (targetTransform != null)
        {
            targetTransform.localPosition = startPosition;
            targetTransform.localScale = startScale;
            targetTransform.localRotation = startRotation;
        }
    }

    void Update()
    {
        if (!isAnimating || targetTransform == null) return;

        // 1. Floating (Moves Up/Down relative to startPosition)
        float floatPhase = (Time.time * floatSpeed) + Mathf.PI;
        float offsetY = Mathf.Sin(floatPhase) * floatAmplitude;
        targetTransform.localPosition = startPosition + new Vector3(0, offsetY, 0);

        // 2. Breathing (Scales relative to startScale)
        float pulsePhase = (Time.time * pulseSpeed) + Mathf.PI;
        float scaleOffset = Mathf.Sin(pulsePhase) * pulseAmount;
        targetTransform.localScale = startScale + new Vector3(scaleOffset, scaleOffset, scaleOffset);

        // 3. Rocking (Rotates relative to startRotation)
        float tiltPhase = (Time.time * tiltSpeed) + Mathf.PI;
        float tilt = Mathf.Sin(tiltPhase) * tiltAngle;
        targetTransform.localRotation = startRotation * Quaternion.Euler(0, 0, tilt);
    }
}