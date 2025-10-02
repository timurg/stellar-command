using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Interceptor : Ship
{
    [SerializeField] private float patrolRadius = 150f; // Радиус патруля
    [SerializeField] private float attackRadius = 75f; // Радиус атаки
    [SerializeField] private float fuelConsumptionRate = 10f; // Расход топлива в секунду вне ангара
    [SerializeField] private float lowFuelThreshold = 20f; // Порог топлива для возврата
    [SerializeField] private float lowShieldsThreshold = 20f; // Порог щитов для возврата

    private Carrier carrier; // Ссылка на Carrier
    private float fuel = 400f; // Текущее топливо
    private float maxFuel = 100f; // Максимальное топливо
    private float patrolAngle = 0f; // Угол патруля
    private float orbitAngle = 0f; // Угол орбиты для атаки

    protected override void Awake()
    {
        base.Awake();
        carrier = FindFirstObjectByType<Carrier>();
        if (carrier == null)
        {
            Debug.LogError("Interceptor: No Carrier found in scene!");
        }
        SetState(ShipState.HANGAR); // Начальное состояние
        fuel = maxFuel; // Полное топливо
        orbitAngle = Random.Range(0f, 2f * Mathf.PI); // Случайный стартовый угол
    }

    protected override void Update()
    {
        base.Update();
        if (carrier == null || !IsAlive()) return;

        // Управление состоянием
        HandleState(Time.deltaTime);
    }

    private void HandleState(float deltaTime)
    {
        switch (GetState())
        {
            case ShipState.HANGAR:
                transform.position = carrier.transform.position; // В ангаре
                //fuel = Mathf.Min(maxFuel, fuel + carrier.fuelRegenRate * deltaTime); // Реген топлива
                //shields = Mathf.Min(maxShields, shields + carrier.fuelRegenRate * deltaTime / 2); // Реген щитов
                if (SelectTarget() != null) // Проверка наличия цели
                {
                    SetState(ShipState.PATROL); // Вылет при появлении цели
                }
                break;
            case ShipState.PATROL:
                Patrol(deltaTime);
                break;
            case ShipState.ATTACK:
                Attack(deltaTime);
                break;
            case ShipState.RETURN:
                Return(deltaTime);
                break;
            case ShipState.DAMAGED:
                SetState(ShipState.HANGAR); // Авто-возврат на ремонт
                break;
        }
    }

    private void Patrol(float deltaTime)
    {
        patrolAngle += Mathf.PI / 10f * deltaTime; // Угловая скорость патруля
        Vector2 patrolPos = (Vector2)carrier.transform.position + new Vector2(Mathf.Cos(patrolAngle), Mathf.Sin(patrolAngle)) * patrolRadius;
        Vector2 direction = (patrolPos - (Vector2)transform.position).normalized;
        Move(direction); // Движение по патрулю
        fuel -= fuelConsumptionRate * deltaTime; // Расход топлива
        if (fuel <= lowFuelThreshold || shields <= lowShieldsThreshold)
        {
            SetState(ShipState.RETURN); // Возврат при низком топливе/щитах
        }
        Entity newTarget = SelectTarget();
        if (newTarget != null)
        {
            SetTarget(newTarget); // Устанавливаем цель
            SetState(ShipState.ATTACK); // Переход в атаку
        }
        Debug.Log($"Patrolling. Fuel: {fuel}, Shields: {shields}, Position: {transform.position}, Direction: {Direction}");
    }

    private void Attack(float deltaTime)
    {
        if (target == null || !target.IsAlive())
        {
            SetState(ShipState.PATROL); // Возврат к патрулю, если цель уничтожена
            return;
        }
        Vector2 closestPoint = target.GetComponent<Collider2D>().ClosestPoint(transform.position);
        Vector2 direction = (closestPoint - (Vector2)transform.position).normalized;
        float distanceToTarget = Vector2.Distance((Vector2)transform.position, closestPoint);
        if (distanceToTarget > attackRadius)
        {
            Move(direction); // Подход к цели
        }
        else
        {
            //orbitAngle += orbitAngularSpeed * deltaTime; // Орбитальное движение вокруг цели
            Vector2 offset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * attackRadius;
            Vector2 desiredPosition = (Vector2)target.transform.position + offset;
            Vector2 orbitDirection = (desiredPosition - (Vector2)transform.position).normalized;
            Move(orbitDirection); // Движение по орбите
            ShootAtTarget(); // Стрельба
        }
        fuel -= fuelConsumptionRate * deltaTime; // Расход топлива
        if (fuel <= lowFuelThreshold || shields <= lowShieldsThreshold)
        {
            SetState(ShipState.RETURN); // Возврат при низком топливе/щитах
        }
        Debug.Log($"Attacking. Fuel: {fuel}, Shields: {shields}, Position: {transform.position}, Direction: {Direction}");
    }

    private void Return(float deltaTime)
    {
        Vector2 direction = ((Vector2)carrier.transform.position - (Vector2)transform.position).normalized;
        Move(direction); // Движение назад к Carrier
        if (Vector2.Distance(transform.position, carrier.transform.position) < 1f) // Порог возврата
        {
            SetState(ShipState.HANGAR); // В ангар
        }
        Debug.Log($"Returning. Fuel: {fuel}, Shields: {shields}, Position: {transform.position}, Direction: {Direction}");
    }

    protected override Entity SelectTarget()
    {
        // Выбираем ближайшего врага
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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
        Debug.Log($"Selected target: {(closestTarget != null ? closestTarget.name : "None")}");
        return closestTarget;
    }

    public void Deploy(Vector2 startPos, Carrier parentCarrier)
    {
        transform.position = startPos;
        SetState(ShipState.PATROL); // Вылет
        carrier = parentCarrier; // Установка Carrier
        fuel = maxFuel; // Полное топливо
        shields = maxShields; // Полные щиты
        gameObject.SetActive(true);
    }

    public override void SetState(ShipState newState)
    {
        base.SetState(newState);
        // Дополнительная логика для Interceptor
    }

    public float Fuel
    {
        get { return fuel; }
        set { fuel = Mathf.Clamp(value, 0, maxFuel); }
    }
}