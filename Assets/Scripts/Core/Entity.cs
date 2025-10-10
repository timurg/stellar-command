using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public int Id { get; private set; }
    private static int nextId = 0;

    protected virtual void Awake()
    {
        Id = nextId++;
    }
}