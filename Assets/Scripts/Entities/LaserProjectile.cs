using UnityEngine;

/// <summary>
/// Laser projectile stub - placeholder for laser-based projectile.
/// Currently not implemented (LaserGun uses beam, not projectiles).
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>LaserProjectile is a stub/placeholder class.</para>
/// <para>Current state:</para>
/// <list type="bullet">
///   <item>NOT USED: LaserGun uses beam (LineRenderer), not projectiles.</item>
///   <item>STUB: OnDeath() is empty - needs implementation if used.</item>
///   <item>POOLING: Would need LaserProjectilePoolManager if activated.</item>
/// </list>
/// <para>If implementing: create pool manager and return to pool in OnDeath.</para>
/// </remarks>
public class LaserProjectile: ProjectileBase
{
    /// <summary>
    /// Initializes laser projectile.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Update loop - delegates to base class.
    /// </summary>
    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Death handler - STUB, needs pool implementation.
    /// </summary>
    protected override void OnDeath()
    {
        // TODO: Implement pooling if LaserProjectile is used
        // LaserProjectilePoolManager.Instance.Return(this);
    }
}