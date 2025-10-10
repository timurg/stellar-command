using UnityEngine;

public class LaserProjectile: ProjectileBase
{
    protected override void Awake()
    {
        base.Awake();
        // Дополнительная инициализация, если нужна
    }

    protected override void Update()
    {
        base.Update();
        // Дополнительное поведение, если нужно
    }
    protected override void OnDeath()
    {
        //ProtonProjectilePoolManager.Instance.Return(this);
    }
}