using UnityEngine;

public class ProtonProjectile: ProjectileBase
{
    public override void Init(SpaceObject owner, Vector2 position, Vector2 dir, float speed, float damage)
    {
        base.Init(owner, position, dir, speed, damage);
        if (trail != null) trail.enabled = true;
    }
    protected TrailRenderer trail = null;
    protected override void Awake()
    {
        base.Awake();
        trail = GetComponent<TrailRenderer>();
        // Дополнительная инициализация, если нужна
    }

    protected override void Update()
    {
        base.Update();
        // Дополнительное поведение, если нужно
    }
    protected override void OnDeath()
    {
        if (trail != null) {
            trail.Clear();
            trail.enabled = false;
        }
        ProtonProjectilePoolManager.Instance.Return(this);
    }
}