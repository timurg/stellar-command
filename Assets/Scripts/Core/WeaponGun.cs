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
    


    protected float shootTimer = 0f;
    protected SpaceObject target;

    public float EffectiveRange => effectiveRange;

    protected virtual void Update()
    {
        if (shootTimer > 0) shootTimer -= Time.deltaTime;
    }

    protected void FixedUpdate()
    {
        if (shootTimer > 0) shootTimer -= Time.fixedDeltaTime;
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
        if (shootTimer > 0) return;
        Vector2 targetPos = target != null ? (Vector2)target.transform.position : (Vector2)transform.position;
        float distanceToTarget = Vector2.Distance(muzzle.position, targetPos);
        if (distanceToTarget > effectiveRange) return;

        Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
        Vector2 dir = (targetPos - origin).normalized;
        
        

        
        //var projectileBase = projectile.GetComponent<ProjectileBase>();
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

        shootTimer = shootCooldown;
    }
}
