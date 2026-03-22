using UnityEngine;
using UnityEngine.UI; // Required for accessing the Image component
using UnityEngine.EventSystems;

public class ButtonHoverSmooth : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    [SerializeField] private float lerpSpeed = 15f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    // Alpha Variables
    private Image buttonImage;
    private float targetAlpha = 0f; // Start at 0 as per your default

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        // Get the Image component on this object
        buttonImage = GetComponent<Image>();

        // Ensure the starting color matches your hidden default
        if (buttonImage != null)
        {
            Color c = buttonImage.color;
            c.a = 0f;
            buttonImage.color = c;
        }
    }

    void Update()
    {
        // Smooth Scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * lerpSpeed);

        // Smooth Alpha Fade
        if (buttonImage != null)
        {
            Color curColor = buttonImage.color;
            float newAlpha = Mathf.Lerp(curColor.a, targetAlpha, Time.deltaTime * lerpSpeed);
            buttonImage.color = new Color(curColor.r, curColor.g, curColor.b, newAlpha);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;
        targetAlpha = 1f; // 1f is equivalent to 255 (max alpha)
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        targetAlpha = 0f; // Back to invisible
    }
}