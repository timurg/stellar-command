using UnityEngine;

/// <summary>
/// Proton projectile - physical projectile fired by ProtonGun.
/// Returns to pool on death, manages TrailRenderer effect.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>ProtonProjectile is a pooled projectile with trail effect.</para>
/// <para>Key behaviors:</para>
/// <list type="bullet">
///   <item>POOLING: Returns to ProtonProjectilePoolManager on death.</item>
///   <item>TRAIL: Enables TrailRenderer on Init, clears on death.</item>
///   <item>PHYSICS: Movement handled by ProjectileBase (via Direction).</item>
///   <item>COLLISION: OnTriggerEnter2D in base class handles damage.</item>
/// </list>
/// <para>CRITICAL: OnDeath returns to pool - NEVER calls Destroy!</para>
/// </remarks>
public class ProtonProjectile: ProjectileBase
{
    protected TrailRenderer trail = null;

    /// <summary>
    /// Initializes projectile and enables trail effect.
    /// </summary>
    /// <param name="owner">Firing entity.</param>
    /// <param name="position">Spawn position.</param>
    /// <param name="dir">Direction of travel.</param>
    /// <param name="speed">Movement speed.</param>
    /// <param name="damage">Damage on hit.</param>
    public override void Init(SpaceObject owner, Vector2 position, Vector2 dir, float speed, float damage)
    {
        base.Init(owner, position, dir, speed, damage);
        if (trail != null) trail.enabled = true;
    }

    /// <summary>
    /// Initializes TrailRenderer reference.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        trail = GetComponent<TrailRenderer>();
    }

    /// <summary>
    /// Update loop - delegates to base class.
    /// </summary>
    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Returns projectile to pool - clears and disables trail.
    /// NEVER calls Destroy - uses pooling!
    /// </summary>
    protected override void OnDeath()
    {
        if (trail != null) {
            trail.Clear();
            trail.enabled = false;
        }
        ProtonProjectilePoolManager.Instance.Return(this);
    }
}