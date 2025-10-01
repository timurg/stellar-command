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
    [SerializeField] private float courseCorrectionInterval = 2f; // Интервал корректировки курса
    private float orbitRadius; // Случайный радиус для каждого Enemy
    private float orbitAngle = 0f; // Угол орбиты
    private Vector2 smoothedOrbitDirection = Vector2.zero; // Буфер для сглаживания направления
    private float courseCorrectionTimer = 0f; // Таймер для корректировки
    private Vector2[] orbitPathPoints; // Ключевые точки траектории (сплайн)
    private int currentPointIndex = 0; // Индекс текущей точки траектории
    protected override void Awake()
    {
        base.Awake();
        target = FindClosestShip(); // Инициализация: Находим ближайший Ship
        if (target == null)
        {
            Debug.LogError("Enemy: No Ship found in scene!");
        }
        orbitRadius = Random.Range(orbitRadiusMin, orbitRadiusMax); // Случайный радиус орбиты
        courseCorrectionTimer = courseCorrectionInterval; // Начальное значение таймера
        orbitPathPoints = new Vector2[0]; // Инициализация пустого массива
    }
    protected override void Update()
    {
        base.Update();
        if (target == null || !IsAlive()) return;
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        Vector2 closestPoint = targetCollider.ClosestPoint(transform.position); // Ближайшая точка на коллайдере цели
        Vector2 direction = (closestPoint - (Vector2)transform.position).normalized;
        float distanceToTarget = Vector2.Distance((Vector2)transform.position, closestPoint); // Расстояние до ближайшей точки
        Debug.Log($"Distance to Target Closest Point: {distanceToTarget}, Direction: {direction}");
        if (distanceToTarget > attackDistance)
        {
            Move(direction); // Лететь к цели
        }
        else
        {
            OrbitAroundTarget(distanceToTarget, closestPoint); // Орбитальное движение вокруг цели
            ShootAtTarget(); // Стрельба по цели
        }
    }
    protected override void OnDeath()
    {
        base.OnDeath();
        EnemyPoolManager.Instance.ReturnEnemy(this); // Возвращаем в пул вместо Destroy
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
        courseCorrectionTimer = courseCorrectionInterval; // Сбрасываем таймер при спавне
    }
    private void OrbitAroundTarget(float distanceToTarget, Vector2 closestPoint)
    {
        if (distanceToTarget <= attackDistance) // Используем attackDistance как границу орбиты
        {
            courseCorrectionTimer -= Time.deltaTime; // Уменьшаем таймер
            if (courseCorrectionTimer <= 0f)
            {
                // Корректировка курса каждые 0.7 секунды
                orbitAngle += orbitAngularSpeed * Time.deltaTime; // Обновляем угол
                Vector2 offset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
                Vector2 desiredPosition = (Vector2)target.transform.position + offset;
                Vector2 rawOrbitDirection = (desiredPosition - (Vector2)transform.position).normalized;
                smoothedOrbitDirection = Vector2.Lerp(smoothedOrbitDirection, rawOrbitDirection, 0.1f); // Сглаживание
                courseCorrectionTimer = courseCorrectionInterval; // Сбрасываем таймер
            }
            Move(smoothedOrbitDirection); // Двигаемся по сглаженной орбите
        }
        else
        {
            Move((closestPoint - (Vector2)transform.position).normalized); // Подходим ближе
        }
    }
    // Метод для поиска ближайшего Ship как цели
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
}