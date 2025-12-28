using UnityEngine;

/// <summary>
/// Interface for object pool managers. Defines Get/Return contract.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>IPoolManager is the core pooling contract for all pooled entities.</para>
/// <para>Key principles:</para>
/// <list type="bullet">
///   <item>GET: Retrieves object from pool (activates it).</item>
///   <item>RETURN: Returns object to pool (deactivates it).</item>
///   <item>NEVER DESTROY: Pooled objects are recycled, not destroyed.</item>
///   <item>CONSTRAINT: T must inherit from Entity.</item>
/// </list>
/// <para>All projectiles, enemies, and interceptors use pooling.</para>
/// </remarks>
/// <typeparam name="T">Entity type being pooled.</typeparam>
public interface IPoolManager<T> where T : Entity
{
    /// <summary>
    /// Gets an object from pool at optional position.
    /// </summary>
    /// <param name="position">Optional spawn position.</param>
    /// <returns>Activated entity from pool.</returns>
    T Get(Vector2? position = null);

    /// <summary>
    /// Returns object to pool for reuse.
    /// </summary>
    /// <param name="obj">Entity to return.</param>
    void Return(T obj);
}