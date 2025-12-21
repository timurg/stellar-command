using UnityEngine;

public interface IPoolManager<T> where T : Entity
{
    T Get(Vector2? position = null);
    void Return(T obj);
}
