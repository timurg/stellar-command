using UnityEngine;

/// <summary>
/// Proton cannon - projectile-based weapon that fires ProtonProjectile.
/// Uses object pooling for projectile management.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>ProtonGun is a projectile weapon implementation.</para>
/// <para>Key behaviors:</para>
/// <list type="bullet">
///   <item>PROJECTILE TYPE: isProjectileWeapon = true (set in base class).</item>
///   <item>POOLING: Gets projectiles from ProtonProjectilePoolManager.</item>
///   <item>RELEASE: Returns projectiles to pool, clears TrailRenderer.</item>
///   <item>FIRING: Base class handles timing, aiming, and calling GetProjectile().</item>
/// </list>
/// <para>To add new projectile weapon: inherit WeaponGun, implement Get/ReleaseProjectile.</para>
/// </remarks>
public class ProtonGun : WeaponGun
{
    /// <summary>
    /// Gets a proton projectile from pool and activates it.
    /// </summary>
    /// <returns>Active ProtonProjectile from pool.</returns>
    public override ProjectileBase GetProjectile()
    {
        var projectile = ProtonProjectilePoolManager.Instance.Get(gameObject.transform.position);
        projectile.SetAlive(true);
        return projectile;
    }

    /// <summary>
    /// Returns projectile to pool and clears its trail effect.
    /// </summary>
    /// <param name="projectile">Projectile to return.</param>
    public override void ReleaseProjectile(ProjectileBase projectile)
    {
        projectile.gameObject.SetActive(false);
        projectile.gameObject.GetComponent<TrailRenderer>().Clear();

        ProtonProjectilePoolManager.Instance.Return(projectile as ProtonProjectile);
    }
}