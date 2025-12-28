/// <summary>
/// Pool manager for ExplosionFX visual effects.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>ExplosionFXPoolManager pools explosion effects.</para>
/// <para>Usage:</para>
/// <list type="bullet">
///   <item>GET: ExplosionFXPoolManager.Instance.Get()</item>
///   <item>CALLER: Enemy.OnDeath() spawns explosion effect.</item>
///   <item>RETURN: ExplosionFX returns itself after animation completes.</item>
/// </list>
/// <para>Access: ExplosionFXPoolManager.Instance</para>
/// </remarks>
public class ExplosionFXPoolManager : EntityPoolManager<ExplosionFX>
{
    /// <summary>
    /// Initializes pool.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }

}