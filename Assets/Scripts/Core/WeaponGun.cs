using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Abstract base class for all weapon turrets attached to ships.
/// Handles aiming, cooldowns, projectile spawning, and firing logic.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>WeaponGun is the base for all ship-mounted weapons. Key patterns:</para>
/// <list type="bullet">
///   <item>PROJECTILE WEAPONS: Override GetProjectile() to return from pool, ReleaseProjectile() to return to pool.</item>
///   <item>BEAM WEAPONS: Set isProjectileWeapon=false in Inspector, override OnShootNonProjectile().</item>
///   <item>AIMING: FixedUpdate() rotates turret towards target. Lock-on uses lockAngleTolerance.</item>
///   <item>COOLDOWN: shootCooldown controls fire rate. shootTimer counts down.</item>
///   <item>OWNERSHIP: Weapon inherits tag from parent Ship for friend/foe identification.</item>
/// </list>
/// <para>Firing flow: Ship.ShootAtTarget() → SetTarget() → ShootIfReady() → OnShootProjectile/OnShootNonProjectile()</para>
/// <para>NEVER call Destroy() on projectiles - always return to pool via ReleaseProjectile().</para>
/// </remarks>
public abstract class WeaponGun : Entity
{
    /// <summary>Damage dealt per shot.</summary>
    [SerializeField] protected float damage = 20f;
    
    /// <summary>If true, damage bypasses shields.</summary>
    [SerializeField] protected bool ignoresShields = false;
    
    /// <summary>Time between shots in seconds.</summary>
    [SerializeField] protected float shootCooldown = 1f;
    
    /// <summary>Maximum effective firing range.</summary>
    [SerializeField] protected float effectiveRange = 3f;
    
    /// <summary>True for projectile weapons, false for beam/instant weapons.</summary>
    [SerializeField] protected bool isProjectileWeapon = true;
    
    /// <summary>Transform for projectile spawn point. Uses transform.position if null.</summary>
    [SerializeField] protected Transform muzzle;
    
    /// <summary>Turret rotation speed in degrees per second.</summary>
    [SerializeField] protected float turnSpeed = 900f;
    
    /// <summary>Angle tolerance for target lock-on in degrees.</summary>
    [SerializeField] protected float lockAngleTolerance = 5f;

    /// <summary>Cooldown timer. Weapon can fire when <= 0.</summary>
    protected float shootTimer = 0f;
    
    /// <summary>Current target for aiming and firing.</summary>
    protected SpaceObject target;

    /// <summary>Gets the effective firing range.</summary>
    public float EffectiveRange => effectiveRange;

    /// <summary>
    /// Calculates damage per second based on damage and cooldown.
    /// </summary>
    /// <returns>DPS value.</returns>
    public float GetDamagePerSecond()
    {
        return damage / shootCooldown;
    }

    /// <summary>
    /// Physics update. Handles cooldown timer and turret aiming.
    /// </summary>
    protected void FixedUpdate()
    {
        if (shootTimer > 0) shootTimer -= Time.fixedDeltaTime;
        if (target != null)
        {
            Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
            Collider2D targetCollider = target.GetComponent<Collider2D>();
            Vector2 targetPos = targetCollider != null ? targetCollider.ClosestPoint(origin) : (Vector2)target.transform.position;
            Vector2 dir = (targetPos - origin).normalized;
            float angle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Sets the target for this weapon to aim at.
    /// </summary>
    /// <param name="target">Target SpaceObject.</param>
    public virtual void SetTarget(SpaceObject target)
    {
        this.target = target;
    }

    /// <summary>
    /// ABSTRACT: Returns a projectile from the object pool.
    /// Implement in derived classes to return specific projectile types.
    /// </summary>
    /// <returns>Projectile instance from pool.</returns>
    public abstract ProjectileBase GetProjectile();
    
    /// <summary>
    /// ABSTRACT: Returns a projectile to the object pool.
    /// Called when projectile hits or expires.
    /// </summary>
    /// <param name="projectile">Projectile to return.</param>
    public abstract void ReleaseProjectile(ProjectileBase projectile);

    /// <summary>
    /// Called when firing a projectile weapon. Override for custom projectile initialization.
    /// </summary>
    /// <param name="owner">Ship that owns this weapon.</param>
    /// <param name="pos">Spawn position.</param>
    /// <param name="direction">Fire direction.</param>
    /// <param name="target">Current target.</param>
    /// <param name="projectile">Projectile instance.</param>
    protected virtual void OnShootProjectile(SpaceObject owner, Vector2 pos, Vector2 direction, SpaceObject target, ProjectileBase projectile)
    {
        projectile.tag = owner.tag;
        projectile.Init(owner, pos, direction, projectile.GetMaxSpeed(), damage);
    }
    
    /// <summary>
    /// Called when firing a non-projectile weapon (beam, instant-hit).
    /// Override for custom beam/laser effects.
    /// </summary>
    /// <param name="owner">Ship that owns this weapon.</param>
    /// <param name="pos">Fire origin position.</param>
    /// <param name="direction">Fire direction.</param>
    /// <param name="target">Current target.</param>
    protected virtual void OnShootNonProjectile(SpaceObject owner, Vector2 pos, Vector2 direction, SpaceObject target)
    {
        Debug.Log("Shooting non-projectile weapon");
    }

    /// <summary>
    /// Attempts to fire the weapon. Checks cooldown, range, and lock-on angle.
    /// </summary>
    public virtual void ShootIfReady()
    {
        if (target == null) return;
        if (shootTimer > 0) return;

        Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        Vector2 targetPos = targetCollider != null ? targetCollider.ClosestPoint(origin) : (Vector2)target.transform.position;
        float distanceToTarget = Vector2.Distance(origin, targetPos);
        if (distanceToTarget > effectiveRange) return;

        Vector2 dir = (targetPos - origin).normalized;
        Vector2 currentDir = (Vector2)transform.up;

        float cosTolerance = Mathf.Cos(lockAngleTolerance * Mathf.Deg2Rad);
        if (Vector2.Dot(currentDir, dir) < cosTolerance) return;

        shootTimer = shootCooldown;

        var owner = GetComponentInParent<SpaceObject>();
        if (owner == null)
        {
            Debug.LogError("Owner not found");
            return;
        }

        if (isProjectileWeapon)
        {
            OnShootProjectile(owner, origin, dir, target, GetProjectile());
        }
        else
        {
            OnShootNonProjectile(owner, origin, dir, target);
        }        
    }

    /// <summary>
    /// Checks if weapon can fire (cooldown ready and target in range).
    /// </summary>
    /// <returns>True if weapon can fire.</returns>
    public bool CanFire()
    {
        if (shootTimer > 0 || target == null) return false;
        Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        Vector2 closest = targetCollider != null ? targetCollider.ClosestPoint(origin) : (Vector2)target.transform.position;
        return Vector2.Distance(origin, closest) <= effectiveRange;
    }
}
