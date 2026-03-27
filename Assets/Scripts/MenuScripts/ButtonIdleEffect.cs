using UnityEngine;

public class ButtonIdleEffect : MonoBehaviour
{
    [Header("Idle Animation Settings")]
    [Tooltip("How much the button grows and shrinks during idle.")]
    [SerializeField] private float pulseAmount = 0.05f;

    [Tooltip("How fast the pulsing happens.")]
    [SerializeField] private float pulseSpeed = 2f;

    private Vector3 _baseScale;

    void Start()
    {
        // Store the scale the button starts with
        _baseScale = transform.localScale;
    }

    void Update()
    {
        // Use Sine wave to calculate a smooth oscillating value
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        // Apply the pulse to the base scale
        transform.localScale = _baseScale + new Vector3(pulse, pulse, pulse);
    }

    // Call this if you change the button's size via other scripts 
    // to update what "Idle" looks like
    public void UpdateBaseScale(Vector3 newScale)
    {
        _baseScale = newScale;
    }
}