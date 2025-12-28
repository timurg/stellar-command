using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Laser weapon - instant beam attack without projectiles.
/// Uses LineRenderer for visual effect.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>LaserGun is a non-projectile (beam) weapon implementation.</para>
/// <para>Key behaviors:</para>
/// <list type="bullet">
///   <item>WEAPON TYPE: isProjectileWeapon = false (should be set in Inspector).</item>
///   <item>VISUAL: Uses LineRenderer for beam effect.</item>
///   <item>DAMAGE: Instant hit via OnShootNonProjectile().</item>
///   <item>FLASH: Beam visible for flashTimer seconds, then hides.</item>
///   <item>NO POOLING: No projectiles - direct damage to target.</item>
/// </list>
/// <para>GetProjectile/ReleaseProjectile throw NotImplementedException.</para>
/// <para>To create beam weapon: inherit WeaponGun, override OnShootNonProjectile.</para>
/// </remarks>
[RequireComponent(typeof(LineRenderer))]
public class LaserGun : WeaponGun
{
    /// <summary>Duration the beam remains visible after firing.</summary>
    [SerializeField] protected float flashTimer = 0.1f;

    protected LineRenderer lineRenderer;

    /// <summary>
    /// Initializes LineRenderer reference.
    /// </summary>
    protected void OnEnable()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LaserGun: No LineRenderer component found!");
            return;
        }
    }

    /// <summary>
    /// Not implemented - LaserGun uses beam, not projectiles.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public override ProjectileBase GetProjectile()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not implemented - LaserGun uses beam, not projectiles.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public override void ReleaseProjectile(ProjectileBase projectile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Fires laser beam - draws line and deals instant damage.
    /// </summary>
    /// <param name="owner">Weapon owner.</param>
    /// <param name="pos">Muzzle position.</param>
    /// <param name="direction">Firing direction.</param>
    /// <param name="target">Target to damage.</param>
    override protected void OnShootNonProjectile(SpaceObject owner, Vector2 pos, Vector2 direction, SpaceObject target)
    {
        lineRenderer.SetPosition(0, pos);
        lineRenderer.SetPosition(1, target.gameObject.transform.position);
        target.TakeDamage(damage, target.gameObject.transform.position, ignoresShields);
        lineRenderer.enabled = true;
        StartCoroutine(FlashEffect());


    }

    /// <summary>
    /// Coroutine to hide beam after flash duration.
    /// </summary>
    private IEnumerator FlashEffect()
    {
        yield return new WaitForSeconds(flashTimer);
        lineRenderer.enabled = false;
    }
}