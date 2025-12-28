using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Pool manager for Interceptor drones.
/// Registers interceptors with AdmiralProtection as protectors.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>InterceptorPoolManager manages interceptor lifecycle.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item>POOLING: Standard pool Get/Return for Interceptor objects.</item>
///   <item>ADMIRAL INTEGRATION: AddProtector on activate, RemoveProtector on deactivate.</item>
///   <item>CARRIER MANAGED: Carrier.InitxInterceptors() gets from this pool.</item>
/// </list>
/// <para>Access: InterceptorPoolManager.Instance</para>
/// </remarks>
public class InterceptorPoolManager : EntityPoolManager<Interceptor>
{

    private AdmiralProtection admiralProtection;

    /// <summary>
    /// Initializes pool and finds AdmiralProtection.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        admiralProtection = FindFirstObjectByType<AdmiralProtection>();
        if (admiralProtection == null)
        {
            Debug.LogError("InterceptorPoolManager: No AdmiralProtection found in scene!");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// Activates interceptor and registers as protector with AdmiralProtection.
    /// </summary>
    /// <param name="entity">Interceptor being activated.</param>
    override protected void ActivateEntity(Interceptor entity)
    {
        base.ActivateEntity(entity);
        admiralProtection.AddProtector(entity);
    }

    /// <summary>
    /// Deactivates interceptor and unregisters from AdmiralProtection.
    /// </summary>
    /// <param name="entity">Interceptor being deactivated.</param>
    override protected void DeactivateEntity(Interceptor entity)
    {
        base.DeactivateEntity(entity);
        admiralProtection.RemoveProtector(entity);
    }

}
