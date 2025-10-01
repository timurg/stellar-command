using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Interceptor : Ship
{
    [SerializeField] private float patrolRadius = 150f; // Радиус патруля
    [SerializeField] private float attackRadius = 75f; // Радиус атаки
    private Carrier carrier; // Ссылка на Carrier
    protected override void Awake()
    {
        base.Awake();
        carrier = FindFirstObjectByType<Carrier>();
    }
    protected override void Update()
    {
        base.Update();
        if (carrier == null || !IsAlive()) return;
        Entity newTarget = SelectTarget();
        if (newTarget != null && GetState() == ShipState.HANGAR)
        {
            SetState(ShipState.PATROL); // Вылет при обнаружении цели
        }
    }
    protected override Entity SelectTarget()
    {
        // Выбираем ближайшего врага
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Entity closestTarget = null;
        float closestDistance = Mathf.Infinity;
        foreach (var enemy in enemies)
        {
            if (enemy.IsAlive())
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = enemy;
                }
            }
        }
        return closestTarget;
    }
    public override void SetState(ShipState newState)
    {
        base.SetState(newState);
        switch (newState)
        {
            case ShipState.PATROL:
                transform.position = carrier.transform.position; // Возвращение к Carrier
                break;
            case ShipState.RETURN:
                // Дополнительная логика возврата
                break;
        }
    }
}