using UnityEngine;
using System.Collections;

public class UISmoother : MonoBehaviour
{
    [Header("Settings")]
    public CanvasGroup canvasGroup;
    public float duration = 0.3f;

    private void Awake()
    {
        // Automatically grab the CanvasGroup if you forgot to drag it in
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowPanel()
    {
        StopAllCoroutines();
        // We ensure the object is active, but the REAL magic is the Alpha
        gameObject.SetActive(true);
        StartCoroutine(Fade(canvasGroup.alpha, 1, true));
    }

    public void HidePanel()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(canvasGroup.alpha, 0, false));
    }

    private IEnumerator Fade(float start, float end, bool isShowing)
    {
        // If we are showing the panel, make it clickable immediately
        if (isShowing)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = end;

        // If we are hiding the panel, disable clicks ONLY after it's invisible
        if (!isShowing)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            // IMPORTANT: We do NOT call SetActive(false) here. 
            // We keep it "Active" but invisible so the script stays awake!
        }
    }
}