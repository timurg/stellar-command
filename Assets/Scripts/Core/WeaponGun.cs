using UnityEngine;
using UnityEngine.AI;

public abstract class WeaponGun : Entity
{
    [SerializeField] protected float damage = 20f;
    [SerializeField] protected bool ignoresShields = false;
    [SerializeField] protected float shootCooldown = 1f;
    [SerializeField] protected float effectiveRange = 3f;
    [SerializeField] protected bool isProjectileWeapon = true;
    [SerializeField] protected Transform muzzle;
    [SerializeField] protected float turnSpeed = 900f; // Degrees per second, enough to turn 180 degrees in 0.2 seconds
    [SerializeField] protected float lockAngleTolerance = 5f; // Degrees tolerance for lock-on


    protected float shootTimer = 0f;
    protected SpaceObject target;

    public float EffectiveRange => effectiveRange;

    public float GetDamagePerSecond()
    {
        return damage / shootCooldown;
    }

     protected void FixedUpdate()
    {
        if (shootTimer > 0) shootTimer -= Time.fixedDeltaTime;
        if (target != null)
        {
            Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
            Vector2 targetPos = (Vector2)target.transform.position;
            Vector2 dir = (targetPos - origin).normalized;
            float angle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }
    }

    public virtual void SetTarget(SpaceObject target)
    {
        this.target = target;
    }

    public abstract ProjectileBase GetProjectile();
    public abstract void ReleaseProjectile(ProjectileBase projectile);

    protected virtual void OnShootProjectile(SpaceObject owner, Vector2 pos, Vector2 direction, SpaceObject target, ProjectileBase projectile)
    {
        projectile.tag = owner.tag; // Наследуем тег владельца
        projectile.Init(owner, pos, direction, projectile.GetMaxSpeed(), damage);
    }
    
    protected virtual void OnShootNonProjectile(SpaceObject owner, Vector2 pos, Vector2 direction, SpaceObject target)
    {
        // Реализация для оружия, не использующего снаряды (например, лазеры)
        // Можно добавить эффекты, звуки и т.д.
        Debug.Log("Shooting non-projectile weapon");
    }

    public virtual void ShootIfReady()
    {
        if (target == null) return;
        if (shootTimer > 0) return;

        Vector2 targetPos = (Vector2)target.transform.position;
        Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
        float distanceToTarget = Vector2.Distance(origin, targetPos);
        if (distanceToTarget > effectiveRange) return;

        Vector2 dir = (targetPos - origin).normalized;
        Vector2 currentDir = (Vector2)transform.up;

        float cosTolerance = Mathf.Cos(lockAngleTolerance * Mathf.Deg2Rad);
        if (Vector2.Dot(currentDir, dir) < cosTolerance) return; // Not locked on yet

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

    public bool CanFire()
    {
        return shootTimer <= 0 && target != null && Vector2.Distance((Vector2)(muzzle != null ? muzzle.position : transform.position), (Vector2)target.transform.position) <= effectiveRange;
    }
}
