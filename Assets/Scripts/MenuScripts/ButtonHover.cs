using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSmooth : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    [SerializeField] private float lerpSpeed = 15f; // Higher = faster transition

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        // Set both to the starting size
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // This line does the magic. It smoothly moves the scale 
        // from where it is now toward the target scale.
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * lerpSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}