using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class UIAnimator
{
    public static IEnumerator Fade(this Graphic g, float targetAlpha, float duration, EaseType ease = EaseType.Linear)
    {
        if (!g) yield break;
        Color start = g.color;
        Color end = new Color(start.r, start.g, start.b, targetAlpha);
        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            g.color = Color.Lerp(start, end, Ease(t, ease));
            yield return null;
        }
        g.color = end;
    }

    public static IEnumerator ColorTo(this Graphic g, Color target, float duration)
    {
        if (!g) yield break;
        Color start = g.color;
        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            g.color = Color.Lerp(start, target, t);
            yield return null;
        }
        g.color = target;
    }

    public static IEnumerator MoveAnchored(this RectTransform rt, Vector2 target, float duration, EaseType ease = EaseType.Linear)
    {
        if (!rt) yield break;
        Vector2 start = rt.anchoredPosition;
        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            rt.anchoredPosition = Vector2.Lerp(start, target, Ease(t, ease));
            yield return null;
        }
        rt.anchoredPosition = target;
    }

    public static IEnumerator ScaleTo(this Transform t, Vector3 target, float duration, EaseType ease = EaseType.Linear)
    {
        if (!t) yield break;
        Vector3 start = t.localScale;
        float i = 0;
        while (i < 1f)
        {
            i += Time.unscaledDeltaTime / duration;
            t.localScale = Vector3.Lerp(start, target, Ease(i, ease));
            yield return null;
        }
        t.localScale = target;
    }

    private static float Ease(float t, EaseType type) => type switch
    {
        EaseType.OutCubic => 1f - Mathf.Pow(1f - t, 3f),
        EaseType.OutBack => 1f + 1.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f),
        EaseType.InBack => t * t * (2.70158f * t - 1.70158f),
        _ => t
    };
}

public enum EaseType { Linear, OutCubic, OutBack, InBack }