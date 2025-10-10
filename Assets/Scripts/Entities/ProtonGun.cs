using UnityEngine;

public class ProtonGun : WeaponGun
{
    public override ProjectileBase GetProjectile()
    {
        return ProtonProjectilePoolManager.Instance.Get();
    }

    public override void ReleaseProjectile(ProjectileBase projectile)
    {
        ProtonProjectilePoolManager.Instance.Return(projectile as ProtonProjectile);
    }
}