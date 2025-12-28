using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Base pool manager for Entity objects. Uses Unity's ObjectPool.
/// Implements singleton pattern for global access.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>EntityPoolManager is the base class for all object pools.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>SINGLETON: Static Instance property for global access.</item>
///   <item>UNITY POOL: Uses UnityEngine.Pool.ObjectPool internally.</item>
///   <item>PREFAB BASED: Instantiates from assigned prefab.</item>
///   <item>VIRTUAL HOOKS: Override Create/Activate/Deactivate for custom behavior.</item>
/// </list>
/// <para>Lifecycle: Awake (init pool) → Get (activate) → Return (deactivate).</para>
/// <para>Derived classes: SpaceObjectPoolManager adds alive state management.</para>
/// </remarks>
/// <typeparam name="T">Entity type being pooled.</typeparam>
public abstract class EntityPoolManager<T> : MonoBehaviour, IPoolManager<T> where T : Entity
{
    /// <summary>Singleton instance for global access.</summary>
    public static EntityPoolManager<T> Instance { get; private set; }

    private ObjectPool<T> entityPool;

    /// <summary>Prefab to instantiate for pool objects.</summary>
    [SerializeField] private GameObject entityPrefab;
    
    /// <summary>Initial number of objects to pre-create.</summary>
    [SerializeField] private int initialPoolSize = 10;
    
    /// <summary>Maximum pool capacity.</summary>
    [SerializeField] private int maxPoolSize = 10;

    /// <summary>
    /// Initializes singleton and creates object pool.
    /// </summary>
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (entityPrefab == null)
        {
            Debug.LogError("EntityPoolManager: Prefab not assigned!");
            enabled = false;
            return;
        }

        entityPool = new ObjectPool<T>(
            CreateEntity,
            ActivateEntity,
            DeactivateEntity,
            DestroyEntity,
            true,
            initialPoolSize,
            maxPoolSize
        );
    }

    /// <summary>
    /// Creates new entity from prefab. Called when pool needs more objects.
    /// </summary>
    /// <returns>New inactive entity.</returns>
    protected virtual T CreateEntity()
    {
        GameObject obj = Instantiate(entityPrefab, transform);
        obj.SetActive(false);
        var entity = obj.GetComponent<T>();
        return entity;
    }

    /// <summary>
    /// Called when entity is retrieved from pool.
    /// Override to add custom activation logic.
    /// </summary>
    /// <param name="entity">Entity being activated.</param>
    protected virtual void ActivateEntity(T entity)
    {
    }

    /// <summary>
    /// Called when entity is returned to pool.
    /// </summary>
    /// <param name="entity">Entity being deactivated.</param>
    protected virtual void DeactivateEntity(T entity)
    {
        entity.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when pool destroys excess objects.
    /// </summary>
    /// <param name="entity">Entity to destroy.</param>
    protected virtual void DestroyEntity(T entity)
    {
        Destroy(entity.gameObject);
    }

    /// <summary>
    /// Gets entity from pool at optional position.
    /// </summary>
    /// <param name="position">Optional spawn position.</param>
    /// <returns>Active entity from pool.</returns>
    public virtual T Get(Vector2? position = null)
    {
        var entity = entityPool.Get();
        if (position.HasValue){
            entity.gameObject.transform.position = position.Value;
        }
        entity.gameObject.SetActive(true);
        return entity;
    }

    /// <summary>
    /// Returns entity to pool for reuse.
    /// </summary>
    /// <param name="entity">Entity to return.</param>
    public virtual void Return(T entity)
    {
        try
        {
            entityPool.Release(entity);
        }
        catch (InvalidOperationException)
        {
            // Silently handle double-return attempts
        }
    }
}