using UnityEngine;

public class TitleIdleAnimation : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatAmplitude = 15f; // How high it moves
    public float floatSpeed = 2f;      // How fast it moves

    [Header("Breathing Settings")]
    public float pulseAmount = 0.05f;  // How much it scales (0.05 = 5%)
    public float pulseSpeed = 1.5f;    // How fast it pulses

    [Header("Rotation Settings")]
    public float tiltAngle = 2f;       // Subtle rocking side-to-side
    public float tiltSpeed = 1f;

    private Vector3 startPosition;
    private Vector3 startScale;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
        startScale = rectTransform.localScale;
    }

    void Update()
    {
        // 1. Floating (Up and Down)
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        rectTransform.anchoredPosition = new Vector2(startPosition.x, newY);

        // 2. Breathing (Scaling)
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        rectTransform.localScale = startScale + new Vector3(scaleOffset, scaleOffset, 0);

        // 3. Rocking (Z-Rotation)
        float tilt = Mathf.Sin(Time.time * tiltSpeed) * tiltAngle;
        rectTransform.localRotation = Quaternion.Euler(0, 0, tilt);
    }
}