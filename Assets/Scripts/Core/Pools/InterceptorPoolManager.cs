using UnityEngine;
using UnityEngine.Pool;

public class InterceptorPoolManager : EntityPoolManager<Interceptor>
{
    protected override void Awake()
    {
        base.Awake();
    }

}
