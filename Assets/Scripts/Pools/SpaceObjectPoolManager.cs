/// <summary>
/// Pool manager for SpaceObject types. Adds alive state management.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>SpaceObjectPoolManager extends EntityPoolManager with alive state.</para>
/// <para>Key additions:</para>
/// <list type="bullet">
///   <item>ACTIVATE: Sets entity.SetAlive(true) when retrieved.</item>
///   <item>DEACTIVATE: Sets entity.SetAlive(false) when returned.</item>
///   <item>INHERITANCE: Base for Enemy, Interceptor, Projectile pools.</item>
/// </list>
/// <para>Use this for any SpaceObject that needs alive state tracked.</para>
/// </remarks>
/// <typeparam name="T">SpaceObject type being pooled.</typeparam>
public abstract class SpaceObjectPoolManager<T> : EntityPoolManager<T>, IPoolManager<T> where T : SpaceObject
{
    /// <summary>
    /// Activates entity and sets alive state to true.
    /// </summary>
    /// <param name="entity">Entity being activated.</param>
    protected override void ActivateEntity(T entity)
    {
        base.ActivateEntity(entity);
        entity.SetAlive(true);
    }

    /// <summary>
    /// Deactivates entity and sets alive state to false.
    /// </summary>
    /// <param name="entity">Entity being deactivated.</param>
    override protected void DeactivateEntity(T entity)
    {
        base.DeactivateEntity(entity);
        entity.SetAlive(false);
    }   
}