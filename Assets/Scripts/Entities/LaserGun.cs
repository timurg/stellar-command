using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserGun : WeaponGun
{
    [SerializeField] protected float flashTimer = 0.1f;

    protected LineRenderer lineRenderer;

    protected void OnEnable()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LaserGun: No LineRenderer component found!");
            return;
        }
    }
    public override ProjectileBase GetProjectile()
    {
        throw new NotImplementedException();
    }

    public override void ReleaseProjectile(ProjectileBase projectile)
    {
        throw new NotImplementedException();
    }

    override protected void OnShootNonProjectile(SpaceObject owner, Vector2 pos, Vector2 direction, SpaceObject target)
    {
        lineRenderer.SetPosition(0, pos);
        lineRenderer.SetPosition(1, target.gameObject.transform.position);
        target.TakeDamage(damage, ignoresShields);
        lineRenderer.enabled = true;
        StartCoroutine(FlashEffect());


    }

    private IEnumerator FlashEffect()
    {
        yield return new WaitForSeconds(flashTimer);
        lineRenderer.enabled = false;
    }
}