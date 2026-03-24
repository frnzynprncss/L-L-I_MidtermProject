using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleScaleHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    [Tooltip("The scale the object will reach on hover.")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);

    [Tooltip("How fast the scaling transition happens.")]
    [SerializeField] private float lerpSpeed = 15f;

    private Vector3 _originalScale;
    private Vector3 _targetScale;

    // Standard Property in case other scripts need to check the target scale
    public Vector3 TargetScale => _targetScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _targetScale = _originalScale;
    }

    private void Update()
    {
        // Smoothly interpolate to the target scale
        if (transform.localScale != _targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * lerpSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = _originalScale;
    }

    // Force the button back to normal (useful if the button is disabled while hovered)
    public void ResetScale()
    {
        _targetScale = _originalScale;
        transform.localScale = _originalScale;
    }
}