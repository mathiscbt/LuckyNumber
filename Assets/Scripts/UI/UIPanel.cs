using System.Collections;
using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    protected void FadeIn(float duration)
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        canvasGroup.alpha = 0f;
        fadeRoutine = StartCoroutine(FadeRoutine(duration));
    }

    private IEnumerator FadeRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        fadeRoutine = null;
    }
}
