public interface IPoolManager<T> where T : Entity
{
    T Get();
    void Return(T obj);
}
