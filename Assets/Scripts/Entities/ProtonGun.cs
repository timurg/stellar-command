using UnityEngine;

public class ProtonGun : WeaponGun
{
    public override ProjectileBase GetProjectile()
    {
        var projectile = ProtonProjectilePoolManager.Instance.Get(gameObject.transform.position);
        projectile.SetAlive(true);
        return projectile;
    }

    public override void ReleaseProjectile(ProjectileBase projectile)
    {
        projectile.gameObject.SetActive(false);
        projectile.gameObject.GetComponent<TrailRenderer>().Clear();

        ProtonProjectilePoolManager.Instance.Return(projectile as ProtonProjectile);
    }
}