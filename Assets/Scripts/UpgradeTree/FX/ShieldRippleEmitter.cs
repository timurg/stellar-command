
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShieldRippleEmitter : MonoBehaviour
{
    [Header("Buffer")]
    [Range(1, 16)] public int maxImpulses = 16;

    [Header("Amplitude")]
    public float baseAmplitude = 1.0f;
    public float amplitudePerDamage = 0.0f;
    public float minAmplitude = 0.05f;

    private SpriteRenderer _sr;
    private MaterialPropertyBlock _mpb;

    // xy = UV, z = startTime, w = amplitude
    private readonly Vector4[] _impulses = new Vector4[16];
    private int _next;

    private static readonly int ImpulsesId = Shader.PropertyToID("_Impulses");

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();

        // “Пустые” слоты: amp=0
        for (int i = 0; i < 16; i++)
            _impulses[i] = new Vector4(0.5f, 0.5f, 0f, 0f);

        PushToMaterial();
    }

    /// <summary>
    /// Добавить попадание в WORLD точке.
    /// damageOrIntensity — сила попадания (можете передавать урон).
    /// </summary>
    public void AddHitWorld(Vector3 worldPoint, float damageOrIntensity = 1.0f)
    {
        if (_sr.sprite == null) return;

        Vector2 uv = WorldToSpriteUV(worldPoint);
        float amp = Mathf.Max(minAmplitude, baseAmplitude + damageOrIntensity * amplitudePerDamage);

        AddHitUV(uv, amp);
    }

    /// <summary>
    /// Добавить попадание в UV (0..1).
    /// </summary>
    public void AddHitUV(Vector2 uv, float amplitude)
    {
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        int slot = _next;
        _impulses[slot] = new Vector4(uv.x, uv.y, Time.time, amplitude);

        _next = (_next + 1) % Mathf.Clamp(maxImpulses, 1, 16);

        PushToMaterial();
    }

    private void PushToMaterial()
    {
        _sr.GetPropertyBlock(_mpb);
        _mpb.SetVectorArray(ImpulsesId, _impulses);
        _sr.SetPropertyBlock(_mpb);
    }

    /// <summary>
    /// Перевод WORLD точки в UV спрайта.
    /// Важно: текстура щита должна быть “квадратной маской” (круг в альфе внутри квадрата),
    /// тогда UV соответствует ожидаемой геометрии.
    /// </summary>
    private Vector2 WorldToSpriteUV(Vector3 worldPoint)
    {
        Vector3 local = transform.InverseTransformPoint(worldPoint);

        Bounds b = _sr.sprite.bounds; // локальные bounds спрайта в units
        float u = (local.x - b.min.x) / b.size.x;
        float v = (local.y - b.min.y) / b.size.y;

        return new Vector2(u, v);
    }
}
