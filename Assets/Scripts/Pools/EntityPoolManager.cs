using System;
using UnityEngine;
using UnityEngine.Pool;

public abstract class EntityPoolManager<T> : MonoBehaviour, IPoolManager<T> where T : Entity
{
    public static EntityPoolManager<T> Instance { get; private set; }

    private ObjectPool<T> entityPool;

    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 10;

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

        // Предварительно создаём объекты
        for (int i = 0; i < initialPoolSize; i++)
        {
            //var entity = entityPool.Get();
            //entityPool.Release(entity);
        }
    }

    protected virtual T CreateEntity()
    {
        GameObject obj = Instantiate(entityPrefab, transform);
        obj.SetActive(false);
        var entity = obj.GetComponent<T>();
        //entity.SetAlive(false);
        return entity;
    }

    protected virtual void ActivateEntity(T entity)
    {
        
        //entity.SetAlive(true);
    }

    protected virtual void DeactivateEntity(T entity)
    {
        entity.gameObject.SetActive(false);
    }

    protected virtual void DestroyEntity(T entity)
    {
        Destroy(entity.gameObject);
    }

    public virtual T Get(Vector2? position = null)
    {
        var entity = entityPool.Get();
        if (position.HasValue){
            entity.gameObject.transform.position = position.Value;
        }
        entity.gameObject.SetActive(true);
        //entity.Deploy(position, carrier);
        return entity;
    }

    public virtual void Return(T entity)
    {
        try
        {
            entityPool.Release(entity);
        }
        catch (InvalidOperationException)
        {
            //Debug.LogError("InvalidOperationException: " + entity.GetType().FullName);
        }
    }
}