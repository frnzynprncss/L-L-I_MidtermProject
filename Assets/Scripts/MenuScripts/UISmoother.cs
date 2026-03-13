using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UISmoother : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.5f;
    public Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f);

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine currentRoutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        // Start hidden
        canvasGroup.alpha = 0;
        rectTransform.localScale = startScale;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowPanel()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(Fade(1, Vector3.one, true));
    }

    public void HidePanel()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(Fade(0, startScale, false));
    }

    IEnumerator Fade(float targetAlpha, Vector3 targetScale, bool isShowing)
    {
        float startAlpha = canvasGroup.alpha;
        Vector3 initialScale = rectTransform.localScale;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float lerpTime = time / duration;

            // This SmoothStep is what makes it feel "premium"
            float smoothedTime = Mathf.SmoothStep(0f, 1f, lerpTime);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothedTime);
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, smoothedTime);

            yield return null;
        }

        // Ensure final values are set
        canvasGroup.alpha = targetAlpha;
        rectTransform.localScale = targetScale;

        // Toggle interaction so you can't click invisible buttons
        canvasGroup.interactable = isShowing;
        canvasGroup.blocksRaycasts = isShowing;
    }
}