using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Interceptor : Ship
{
    [SerializeField] private float patrolRadius = 15f; // Радиус патруля
    [SerializeField] private float fuelConsumptionRate = 10f; // Расход топлива в секунду вне ангара
    [SerializeField] private float lowFuelThreshold = 20f; // Порог топлива для возврата
    [SerializeField] private float lowShieldsThreshold = 20f; // Порог щитов для возврата

    private Carrier carrier; // Ссылка на Carrier
    [SerializeField] private float fuel = 100f; // Текущее топливо
    [SerializeField] private float maxFuel = 100f; // Максимальное топливо
    [SerializeField] private float patrolAngle = 0f; // Угол патруля
    [SerializeField] private float orbitAngle = 0f; // Угол орбиты для атаки

    private AdmiralProtection myAdmiralProtection; // Ссылка на AdmiralProtection (если нужна)

    public float Fuel
    {
        get { return fuel; }
        set
        {
            fuel = Mathf.Clamp(value, 0, maxFuel);
           // Debug.Log($"Interceptor {Id} fuel set to {fuel}/{maxFuel}.");
        }
    }

    public Carrier Carrier
    {
        get { return carrier; }
        set { carrier = value; }
    }

    public float MaxFuel
    {
        get { return maxFuel; }
    }   

    private float savedMaxSpeed;

    protected override void Awake()
    {
        base.Awake();
        /*
        carrier = FindFirstObjectByType<Carrier>();
        if (carrier == null)
        {
            Debug.LogError("Interceptor: No Carrier found in scene!");
        }
        */

        myAdmiralProtection = FindFirstObjectByType<AdmiralProtection>();
        if (myAdmiralProtection == null)    
        {
            Debug.LogWarning("Interceptor: No AdmiralProtection component found, proceeding without it.");
        }
        savedMaxSpeed = maxSpeed;
        SetState(ShipState.HANGAR); // Начальное состояние
        Fuel = maxFuel; // Полное топливо
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
                transform.position = carrier.hangarObject.transform.position; // В ангаре
                //fuel = Mathf.Min(maxFuel, fuel + carrier.fuelRegenRate * deltaTime); // Реген топлива
                //shields = Mathf.Min(maxShields, shields + carrier.fuelRegenRate * deltaTime / 2); // Реген щитов
                if (SelectTarget() != null && Fuel >= maxFuel) // Проверка наличия цели
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

    public override float GetMaxSpeed()
    {
        switch (GetState())
        {
            case ShipState.HANGAR: return 0f;
            case ShipState.RETURN: return base.GetMaxSpeed() * 0.5f;
            case ShipState.DAMAGED: return base.GetMaxSpeed() * 0.3f;
            case ShipState.PATROL: return base.GetMaxSpeed() * 0.5f;
            default: return base.GetMaxSpeed();
        }
    }

    private void Patrol(float deltaTime)
    {
        patrolAngle += Mathf.PI / 10f * deltaTime; // Угловая скорость патруля
        Vector2 patrolPos = (Vector2)carrier.transform.position + new Vector2(Mathf.Cos(patrolAngle), Mathf.Sin(patrolAngle)) * patrolRadius;
        Vector2 direction = (patrolPos - (Vector2)transform.position).normalized;
        Move(direction); // Движение по патрулю
        Fuel -= fuelConsumptionRate * deltaTime; // Расход топлива
        if (Fuel <= lowFuelThreshold || Shields <= lowShieldsThreshold)
        {
            SetState(ShipState.RETURN); // Возврат при низком топливе/щитах
        }
        var newTarget = SelectTarget();
        if (newTarget != null)
        {
            SetTarget(newTarget); // Устанавливаем цель
            SetState(ShipState.ATTACK); // Переход в атаку
        }
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
        if (!AllWeaponsInRange(distanceToTarget))
        {
            Move(direction); // Подход к цели
        }
        else
        {
            Vector2 offset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * MinAttackDistance;
            Vector2 desiredPosition = (Vector2)target.transform.position + offset;
            Vector2 orbitDirection = (desiredPosition - (Vector2)transform.position).normalized;
            Move(orbitDirection); // Движение по орбите
            ShootAtTarget(); // Стрельба
        }
        Fuel -= fuelConsumptionRate * deltaTime; // Расход топлива
        if (Fuel <= lowFuelThreshold || Shields <= lowShieldsThreshold)
        {
            SetState(ShipState.RETURN); // Возврат при низком топливе/щитах
        }
    }

    private void Return(float deltaTime)
    {
        Vector2 direction = ((Vector2)carrier.hangarObject.transform.position - (Vector2)transform.position).normalized;
        Move(direction);
        if (Vector2.Distance(transform.position, carrier.hangarObject.transform.position) < 1f) // Порог возврата
        {
            SetState(ShipState.HANGAR); // В ангар
        }
    }

    protected override SpaceObject SelectTarget()
    {
        if (target != null && target.IsAlive()) return target;
        if (myAdmiralProtection != null)
        {
            var admiralTarget = myAdmiralProtection.getTargetForProtector(this);
            if (admiralTarget != null) return admiralTarget;
        }
        return null;
    }

    public void Deploy(Vector2 startPos, Carrier parentCarrier)
    {
        transform.position = startPos;
        SetState(ShipState.PATROL); // Вылет
        carrier = parentCarrier; // Установка Carrier
        Fuel = maxFuel; // Полное топливо
        shields = maxShields; // Полные щиты
        gameObject.SetActive(true);
        SetAlive(true);
    }

    public override void SetState(ShipState newState)
    {
        base.SetState(newState);
        if (newState == ShipState.HANGAR)
        {
            Debug.Log($"Interceptor {Id} returning to hangar. Fuel: {Fuel}/{maxFuel}, Shields: {Shields}/{maxShields}");
            target = null; // Сброс цели
        }
    }

    
}