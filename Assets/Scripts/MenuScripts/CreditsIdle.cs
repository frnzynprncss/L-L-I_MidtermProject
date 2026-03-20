using UnityEngine;

public class CreditsIdle : MonoBehaviour
{
    [Header("Movement Settings")]
    public float waveAmplitude = 10f; // Height of the bob
    public float waveSpeed = 2f;      // Speed of the bob

    [Header("Subtle Pulse")]
    public float scalePulse = 0.02f; // Very tiny scale change
    public float pulseSpeed = 1.5f;

    private Vector3 startPos;
    private Vector3 startScale;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
        startScale = rectTransform.localScale;
    }

    void Update()
    {
        // Smooth vertical wave
        float newY = startPos.y + Mathf.Sin(Time.time * waveSpeed) * waveAmplitude;
        rectTransform.anchoredPosition = new Vector2(startPos.x, newY);

        // Subtle "breathing" scale
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * scalePulse;
        rectTransform.localScale = startScale + new Vector3(scaleOffset, scaleOffset, 0);
    }
}