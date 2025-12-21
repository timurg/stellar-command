using UnityEngine;
using UnityEngine.Pool;

public class InterceptorPoolManager : EntityPoolManager<Interceptor>
{

    private AdmiralProtection admiralProtection;
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

    override protected void ActivateEntity(Interceptor entity)
    {
        base.ActivateEntity(entity);
        admiralProtection.AddProtector(entity); // Назначаем цель
    }

    override protected void DeactivateEntity(Interceptor entity)
    {
        base.DeactivateEntity(entity);
        admiralProtection.RemoveProtector(entity); // Убираем цель
    }

}
