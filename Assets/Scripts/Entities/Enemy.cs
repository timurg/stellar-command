using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : Ship
{
    [SerializeField] private float attackDistance = 3f; // Уменьшенная дистанция атаки
    [SerializeField] private float spawnEdgeOffset = 10f; // Смещение от краёв экрана при спавне
    [SerializeField] private float orbitRadiusMin = 2f; // Минимальный радиус орбиты
    [SerializeField] private float orbitRadiusMax = 5f; // Максимальный радиус орбиты
    [SerializeField] private float orbitAngularSpeed = Mathf.PI / 8f; // Угловая скорость орбиты
    [SerializeField] private float courseCorrectionInterval = 0.7f; // Интервал корректировки курса

    private float orbitRadius; // Случайный радиус для каждого Enemy
    private float orbitAngle = 0f; // Угол орбиты
    private Vector2 smoothedOrbitDirection = Vector2.zero; // Буфер для сглаживания направления
    private float courseCorrectionTimer = 0f; // Таймер для корректировки

    protected override void Awake()
    {
        base.Awake();
        orbitRadius = Random.Range(orbitRadiusMin, orbitRadiusMax); // Случайный радиус орбиты
        courseCorrectionTimer = courseCorrectionInterval; // Начальное значение таймера
        target = FindClosestShip(); // Поиск ближайшего Carrier как цели
    }

    protected override void Update()
    {
        base.Update();
        if (target == null || !IsAlive()) return;

        Collider2D targetCollider = target.GetComponent<Collider2D>();
        Vector2 closestPoint = targetCollider.ClosestPoint(transform.position); // Ближайшая точка на коллайдере цели
        float distanceToTarget = Vector2.Distance((Vector2)transform.position, closestPoint); // Расстояние до ближайшей точки

        if (distanceToTarget > attackDistance)
        {
            // Лететь к цели
            Direction = (closestPoint - (Vector2)transform.position).normalized; // Устанавливаем направление
        }
        else
        {
            // Орбитальное движение вокруг цели
            courseCorrectionTimer -= Time.deltaTime;
            if (courseCorrectionTimer <= 0f)
            {
                orbitAngle += orbitAngularSpeed * Time.deltaTime; // Обновляем угол
                Vector2 offset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
                Vector2 desiredPosition = (Vector2)target.transform.position + offset;
                Vector2 rawOrbitDirection = (desiredPosition - (Vector2)transform.position).normalized;
                smoothedOrbitDirection = Vector2.Lerp(smoothedOrbitDirection, rawOrbitDirection, 0.1f); // Сглаживание
                courseCorrectionTimer = courseCorrectionInterval; // Сбрасываем таймер
            }
            Direction = smoothedOrbitDirection; // Устанавливаем направление для орбиты
            ShootAtTarget(); // Стрельба по цели
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        EnemyPoolManager.Instance.ReturnEnemy(this); // Возвращаем в пул вместо Destroy
    }

    // Метод для поиска ближайшего Carrier как цели
    private Ship FindClosestShip()
    {
        Ship[] ships = FindObjectsOfType<Carrier>();
        Ship closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (var ship in ships)
        {
            if (ship.IsAlive())
            {
                float distance = Vector2.Distance(transform.position, ship.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = ship;
                }
            }
        }
        return closest;
    }

    protected override Entity SelectTarget()
    {
        return target; // Возвращаем текущую цель
    }

    public override void SetState(ShipState newState)
    {
        base.SetState(newState);
        // Дополнительная логика для Enemy
    }

    public void SpawnAtEdge()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        int edge = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;
        switch (edge)
        {
            case 0: spawnPos = new Vector2(-screenBounds.x - spawnEdgeOffset, Random.Range(-screenBounds.y, screenBounds.y)); break;
            case 1: spawnPos = new Vector2(screenBounds.x + spawnEdgeOffset, Random.Range(-screenBounds.y, screenBounds.y)); break;
            case 2: spawnPos = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), screenBounds.y + spawnEdgeOffset); break;
            case 3: spawnPos = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), -screenBounds.y - spawnEdgeOffset); break;
        }
        transform.position = spawnPos;
        SetAlive(true); // Восстанавливаем состояние
        Health = maxHealth; // Восстанавливаем здоровье
        shields = maxShields; // Восстанавливаем щиты
        SetState(ShipState.PATROL); // Устанавливаем начальное состояние
        orbitAngle = Random.Range(0f, 2f * Mathf.PI); // Случайный стартовый угол орбиты
        courseCorrectionTimer = 0f; // Сбрасываем таймер при спавне
    }
}