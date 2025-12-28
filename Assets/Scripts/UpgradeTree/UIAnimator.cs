using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Static extension methods for UI coroutine-based animations.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>UIAnimator provides simple tween animations via coroutines.</para>
/// <para>Available methods:</para>
/// <list type="bullet">
///   <item>Fade(Graphic) - Animates alpha</item>
///   <item>ColorTo(Graphic) - Animates color</item>
///   <item>MoveAnchored(RectTransform) - Animates anchored position</item>
///   <item>ScaleTo(Transform) - Animates local scale</item>
/// </list>
/// <para>All methods return IEnumerator for use with StartCoroutine().</para>
/// </remarks>
public static class UIAnimator
{
    /// <summary>
    /// Fades graphic alpha to target value.
    /// </summary>
    /// <param name="g">Target graphic.</param>
    /// <param name="targetAlpha">Target alpha (0-1).</param>
    /// <param name="duration">Animation duration in seconds.</param>
    /// <param name="ease">Easing function type.</param>
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

    /// <summary>
    /// Animates graphic color to target color.
    /// </summary>
    /// <param name="g">Target graphic.</param>
    /// <param name="target">Target color.</param>
    /// <param name="duration">Animation duration in seconds.</param>
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

    /// <summary>
    /// Animates RectTransform anchored position.
    /// </summary>
    /// <param name="rt">Target RectTransform.</param>
    /// <param name="target">Target anchored position.</param>
    /// <param name="duration">Animation duration in seconds.</param>
    /// <param name="ease">Easing function type.</param>
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

    /// <summary>
    /// Animates Transform local scale.
    /// </summary>
    /// <param name="t">Target Transform.</param>
    /// <param name="target">Target scale.</param>
    /// <param name="duration">Animation duration in seconds.</param>
    /// <param name="ease">Easing function type.</param>
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

    /// <summary>
    /// Applies easing function to time value.
    /// </summary>
    private static float Ease(float t, EaseType type) => type switch
    {
        EaseType.OutCubic => 1f - Mathf.Pow(1f - t, 3f),
        EaseType.OutBack => 1f + 1.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f),
        EaseType.InBack => t * t * (2.70158f * t - 1.70158f),
        _ => t
    };
}

/// <summary>
/// Easing function types for UI animations.
/// </summary>
public enum EaseType { Linear, OutCubic, OutBack, InBack }