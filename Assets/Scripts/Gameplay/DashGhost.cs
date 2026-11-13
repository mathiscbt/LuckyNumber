using System.Collections.Generic;
using UnityEngine;

public class DashGhost : MonoBehaviour
{
    private List<SpriteRenderer> renderers;
    private Color baseColor;
    private float fadeDuration;
    private float elapsed;

    public void Init(List<SpriteRenderer> spriteRenderers, Color color, float duration)
    {
        renderers = spriteRenderers;
        baseColor = color;
        fadeDuration = Mathf.Max(0.01f, duration);
        elapsed = 0f;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / fadeDuration);
        Color c = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(baseColor.a, 0f, t));

        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null) renderers[i].color = c;
        }

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
