using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Pool manager for ProtonProjectile instances.
/// Simple implementation - no additional logic beyond base class.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>ProtonProjectilePoolManager pools proton projectiles.</para>
/// <para>Usage:</para>
/// <list type="bullet">
///   <item>GET: ProtonProjectilePoolManager.Instance.Get(position)</item>
///   <item>RETURN: ProtonProjectilePoolManager.Instance.Return(projectile)</item>
///   <item>CALLER: ProtonGun.GetProjectile() and ProtonProjectile.OnDeath()</item>
/// </list>
/// <para>Inherits from EntityPoolManager - no custom behavior needed.</para>
/// </remarks>
public class ProtonProjectilePoolManager : EntityPoolManager<ProtonProjectile>, IPoolManager<ProtonProjectile>
{
    
}