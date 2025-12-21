using UnityEngine;

public class ExplosionFX: Entity
{
    protected void StopExplosion()
    {
        var component = GetComponent<Animator>();
        component.enabled = false;
        Destroy(gameObject, 0.5f); // Удаляем объект через короткое
    }
}