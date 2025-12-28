# Stellar Command — Архитектура и гайд для AI-агента

Полная документация по архитектуре проекта для быстрого понимания кодовой базы и безопасного внесения изменений.

---

## 1. Общее описание проекта

**Stellar Command** — Unity 2D космическая стратегия. Основной код в `Assets/Scripts/`.

**Стек:**
- Unity (2D, URP)
- C#, стандартный Unity API
- Rigidbody2D для физики
- ObjectPool (Unity) для пуллинга объектов

---

## 2. Иерархия классов (Core Architecture)

```
MonoBehaviour
 └── Entity (абстрактный) — базовая сущность игры с уникальным Id
      │
      ├── Admiral (абстрактный) — командные/управляющие сущности
      │    └── AdmiralProtection — система распределения целей и защиты
      │
      ├── SpaceObject (абстрактный) — объект в космосе с физикой
      │    │   Содержит: масса, здоровье, скорость, ускорение, Direction
      │    │   ВСЕ методы движения и ротации — ТОЛЬКО здесь!
      │    │
      │    ├── Ship (абстрактный) — корабль с оружием и состояниями
      │    │    ├── Carrier — авианосец, управляет ангаром истребителей
      │    │    ├── Enemy — враг, орбитальное движение вокруг цели
      │    │    └── Interceptor — истребитель с топливом и патрулём
      │    │
      │    └── ProjectileBase (абстрактный) — снаряд
      │         ├── ProtonProjectile — протонный снаряд
      │         └── LaserProjectile — лазерный снаряд (заглушка)
      │
      └── WeaponGun (абстрактный) — оружие (турель)
           ├── ProtonGun — протонная пушка (стреляет снарядами)
           └── LaserGun — лазер (луч, без снарядов)
```

---

## 3. Детальное описание базовых классов

### 3.1 Entity (`Assets/Scripts/Core/Entity.cs`)

**Назначение:** Корневой класс для ВСЕХ игровых объектов. Генерирует уникальный `Id`.

```csharp
public abstract class Entity : MonoBehaviour
{
    public int Id { get; private set; }         // Уникальный ID
    private static int nextId = 0;

    protected virtual void Awake()
    {
        Id = nextId++;
    }
}
```

**Правила:**
- Все игровые объекты наследуются от `Entity`
- `Id` присваивается автоматически при создании

---

### 3.2 SpaceObject (`Assets/Scripts/Core/SpaceObject.cs`)

**Назначение:** Любой объект в космосе с координатами, массой, скоростью и ускорением. **ВСЕ методы движения и ротации ТОЛЬКО здесь!**

**Ключевые поля:**
```csharp
[SerializeField] protected float mass = 1f;           // Масса (Rigidbody2D)
[SerializeField] protected float maxHealth = 1000f;   // Максимальное здоровье
[SerializeField] protected float maxSpeed = 10f;      // Максимальная скорость
[SerializeField] protected float acceleration = 10f;  // Ускорение
[SerializeField] protected bool rotateToDirection = true; // Авто-ротация
[SerializeField] protected bool alive = false;        // Жив ли объект

public float Health { get; set; }
public Vector2 Direction { get; set; }  // ГЛАВНОЕ СВОЙСТВО — направление движения
protected Rigidbody2D Rigidbody { get; private set; }
public float Shields { get; protected set; } = 0f;
public float DPS { get; protected set; } = 0f;
```

**Ключевые методы:**

| Метод | Описание |
|-------|----------|
| `Awake()` | Инициализация Rigidbody2D, установка массы и здоровья |
| `FixedUpdate()` | Вызывает `UpdateMovement()` если объект жив |
| `UpdateMovement()` | **ЦЕНТРАЛЬНЫЙ МЕТОД ДВИЖЕНИЯ** — применяет физику на основе `Direction` |
| `UpdateRotation()` | Плавная ротация к направлению движения (Slerp) |
| `Move(Vector2 direction)` | Устанавливает `Direction` для движения |
| `TakeDamage(float, bool)` | Получение урона, проверка смерти |
| `OnDeath()` | Вызывается при смерти (по умолчанию — `Destroy`) |
| `SetAlive(bool)` / `IsAlive()` | Управление состоянием жизни |
| `GetMaxSpeed()` | Виртуальный метод — можно переопределить для модификаторов |

**Логика движения в `UpdateMovement()`:**
```csharp
protected virtual void UpdateMovement()
{
    if (Direction.magnitude > 0)
    {
        if (Rigidbody.bodyType == RigidbodyType2D.Dynamic)
        {
            // Применяем силу через AddForce
            Vector2 force = Direction.normalized * acceleration;
            Rigidbody.AddForce(force * Time.fixedDeltaTime, ForceMode2D.Impulse);
            // Ограничиваем скорость
            if (Rigidbody.linearVelocity.magnitude > GetMaxSpeed())
            {
                Rigidbody.linearVelocity = Rigidbody.linearVelocity.normalized * GetMaxSpeed();
            }
        }
        else if (Rigidbody.bodyType == RigidbodyType2D.Kinematic)
        {
            // Для Kinematic — прямое задание позиции
            Vector2 move = Direction.normalized * GetMaxSpeed() * Time.fixedDeltaTime;
            Rigidbody.MovePosition(Rigidbody.position + move);
        }
        UpdateRotation();
    }
}
```

**КРИТИЧЕСКИ ВАЖНО:**
- Наследники **НИКОГДА** не должны напрямую менять `Rigidbody.velocity` или `transform.position`
- Наследники только вычисляют и устанавливают свойство `Direction`
- Вся физика применяется централизованно в `SpaceObject.UpdateMovement()`

---

### 3.3 Ship (`Assets/Scripts/Core/Ship.cs`)

**Назначение:** Базовый класс для всех кораблей. Добавляет щиты, оружие, состояния и выбор цели.

**Ключевые поля:**
```csharp
[SerializeField] protected float shields = 500f;
[SerializeField] protected float maxShields = 500f;
[SerializeField] protected List<WeaponGun> weapons = new List<WeaponGun>();
[SerializeField] protected SpaceObject target;

public enum ShipState
{
    HANGAR,    // В ангаре (refuel/repair)
    PATROL,    // Патруль вокруг Carrier
    ATTACK,    // Атака врага
    RETURN,    // Возврат к Carrier
    DAMAGED    // Повреждён, в ангаре на ремонт
}
```

**Ключевые методы:**

| Метод | Описание |
|-------|----------|
| `SelectTarget()` | **АБСТРАКТНЫЙ** — каждый наследник реализует свою логику выбора цели |
| `ShootAtTarget()` | Итерирует по `weapons`, устанавливает цель и вызывает `ShootIfReady()` |
| `SetTarget(SpaceObject)` / `GetTarget()` | Управление текущей целью |
| `GetState()` / `SetState(ShipState)` | Управление состоянием корабля |
| `TakeDamage(float, bool)` | Переопределено — сначала снимает щиты |
| `IgnoreShipCollisions()` | Игнорирует коллизии между кораблями |
| `MinAttackDistance` / `MaxAttackDistance` | Дистанция атаки на основе оружия |
| `AllWeaponsInRange(float)` / `AnyWeaponInRange(float)` | Проверка дальности оружия |
| `CanShoot()` | Есть ли готовое к стрельбе оружие |
| `GetDamagePerSecond()` | Суммарный DPS всего оружия |

**Цикл жизни Ship:**
```
Update() → base.Update()
         → Реген щитов
         → ShootAtTarget() → SelectTarget() если нет цели
                           → weapon.SetTarget(target)
                           → weapon.ShootIfReady()
```

**Автозагрузка оружия:**
```csharp
void LoadWeapons()
{
    foreach (var weapon in GetComponentsInChildren<WeaponGun>())
    {
        if (weapons.Contains(weapon)) continue;
        weapons.Add(weapon);
        weapon.tag = this.tag; // Наследуем тег корабля
    }
}
```

---

### 3.4 WeaponGun (`Assets/Scripts/Core/WeaponGun.cs`)

**Назначение:** Абстрактный класс оружия (турели). Управляет прицеливанием, кулдауном и стрельбой.

**Ключевые поля:**
```csharp
[SerializeField] protected float damage = 20f;
[SerializeField] protected bool ignoresShields = false;
[SerializeField] protected float shootCooldown = 1f;
[SerializeField] protected float effectiveRange = 3f;
[SerializeField] protected bool isProjectileWeapon = true;  // true = снаряды, false = луч
[SerializeField] protected Transform muzzle;                 // Точка вылета
[SerializeField] protected float turnSpeed = 900f;           // Скорость поворота
[SerializeField] protected float lockAngleTolerance = 5f;    // Допуск угла захвата
```

**Ключевые методы:**

| Метод | Описание |
|-------|----------|
| `SetTarget(SpaceObject)` | Установка цели для прицеливания |
| `ShootIfReady()` | Проверяет условия и стреляет |
| `GetProjectile()` | **АБСТРАКТНЫЙ** — возвращает снаряд из пула |
| `ReleaseProjectile(ProjectileBase)` | **АБСТРАКТНЫЙ** — возвращает снаряд в пул |
| `OnShootProjectile(...)` | Виртуальный — инициализация снаряда при выстреле |
| `OnShootNonProjectile(...)` | Виртуальный — для оружия без снарядов (лазеры) |
| `CanFire()` | Готово ли оружие к стрельбе |
| `GetDamagePerSecond()` | DPS оружия |

**Логика прицеливания (FixedUpdate):**
```csharp
protected void FixedUpdate()
{
    if (shootTimer > 0) shootTimer -= Time.fixedDeltaTime;
    if (target != null)
    {
        // Вычисляем направление к цели
        Vector2 dir = (targetPos - origin).normalized;
        float angle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle);
        // Плавный поворот турели
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
    }
}
```

**Логика стрельбы (`ShootIfReady()`):**
1. Проверка наличия цели
2. Проверка кулдауна
3. Проверка дальности
4. Проверка угла захвата (lock-on)
5. Вызов `OnShootProjectile()` или `OnShootNonProjectile()`

---

### 3.5 ProjectileBase (`Assets/Scripts/Core/ProjectileBase.cs`)

**Назначение:** Базовый класс снаряда. Наследует `SpaceObject` для физики.

**Ключевые поля:**
```csharp
[SerializeField] protected float damage;
[SerializeField] protected bool ignoresShields;
[SerializeField] protected float timeToLive = 5f;
protected float liveTimer = 0f;
public Entity Owner { get; set; }
```

**Ключевые методы:**

| Метод | Описание |
|-------|----------|
| `Init(owner, position, dir, speed, damage)` | Инициализация снаряда при выстреле |
| `Update()` | Отсчёт времени жизни, вызов `OnDeath()` при истечении |
| `OnTriggerEnter2D(Collider2D)` | Обработка попадания, нанесение урона |
| `OnDeath()` | **ПЕРЕОПРЕДЕЛЯТЬ** — возврат в пул (НЕ Destroy!) |

**Логика попадания:**
```csharp
protected virtual void OnTriggerEnter2D(Collider2D other)
{
    if (!IsAlive()) return;
    var entity = other.GetComponent<SpaceObject>();
    if (entity != null)
    {
        if (entity.tag == Owner.tag) return; // Не бьём своих
        entity.TakeDamage(damage, ignoresShields);
        OnDeath();
    }
}
```

**КРИТИЧЕСКИ ВАЖНО:** В `OnDeath()` **НИКОГДА** не вызывать `Destroy()` — только возврат в пул!

---

## 4. Конкретные реализации (Entities)

### 4.1 Carrier (`Assets/Scripts/Entities/Carrier.cs`)

Авианосец — главный корабль игрока.

**Особенности:**
- Управляет ангаром `Interceptor`
- Регенерирует топливо и щиты истребителей в ангаре
- Деплой истребителей по кулдауну
- Использует `AdmiralProtection` для выбора целей

**Ключевые поля:**
```csharp
[SerializeField] private int maxInterceptors = 5;
[SerializeField] private float fuelRegenRate = 2f;
[SerializeField] private float deployCooldown = 1f;
[SerializeField] public GameObject hangarObject;  // Точка ангара
private readonly List<Interceptor> hangar = new();
```

**SelectTarget() реализация:**
```csharp
protected override SpaceObject SelectTarget()
{
    if (weapons.Any())
    {
        if (target != null && target.IsAlive()) return target;
        var enemy = admiralProtection.getTargetForProtectable(this);
        if (enemy != null) return enemy;
    }
    return null;
}
```

---

### 4.2 Enemy (`Assets/Scripts/Entities/Enemy.cs`)

Враг — противник, атакующий Carrier.

**Особенности:**
- Спавнится на краях экрана
- Сближается с целью до дистанции атаки
- Орбитальное движение вокруг цели при атаке
- Возвращается в пул при смерти

**Логика движения:**
```csharp
protected override void Update()
{
    if (!AllWeaponsInRange(distanceToTarget))
    {
        // Сближение
        Direction = (closestPoint - (Vector2)transform.position).normalized;
    }
    else
    {
        // Орбитальное движение
        orbitAngle += orbitAngularSpeed * Time.deltaTime;
        Vector2 offset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
        Vector2 desiredPosition = target.transform.position + offset;
        Direction = (desiredPosition - transform.position).normalized;
        ShootAtTarget();
    }
}
```

---

### 4.3 Interceptor (`Assets/Scripts/Entities/Interceptor.cs`)

Истребитель — дрон Carrier.

**Особенности:**
- Имеет топливо (`Fuel`) и расходует его вне ангара
- Состояния: HANGAR → PATROL → ATTACK → RETURN → HANGAR
- Возвращается при низком топливе/щитах
- Патрулирует вокруг Carrier
- Атакует орбитально вокруг цели

**Машина состояний:**

| Состояние | Поведение |
|-----------|-----------|
| HANGAR | В ангаре, регенерация, ожидание цели |
| PATROL | Круговое движение вокруг Carrier, поиск цели |
| ATTACK | Сближение + орбита вокруг цели, стрельба |
| RETURN | Возврат к ангару при низком топливе/щитах |
| DAMAGED | Авто-переход в HANGAR |

**Переопределение скорости:**
```csharp
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
```

---

### 4.4 AdmiralProtection (`Assets/Scripts/Entities/AdmiralProtection.cs`)

Система распределения целей между защитниками.

**Назначение:**
- Централизованный менеджер целей
- Распределяет врагов между истребителями
- Приоритизация целей по Health + Shields + DPS + дистанция
- Матрица защитников (кто кого атакует)

**Ключевые методы:**
```csharp
public void AddEnemy(SpaceObject enemy)      // Регистрация врага
public void RemoveEnemy(SpaceObject enemy)   // Удаление врага
public void Protect(SpaceObject obj)         // Добавить в защищаемые
public SpaceObject getTargetForProtector(Ship protector)    // Цель для истребителя
public SpaceObject getTargetForProtectable(Ship protectable) // Цель для Carrier
```

**Алгоритм выбора цели:**
```csharp
private SpaceObject GetPriorityTargetForProtector(Ship protector)
{
    // Выбираем ближайшую и слабейшую цель
    // Учитываем: дистанция, Health, Shields, DPS
    // Проверяем, не назначен ли уже другой защитник
    float score = distance - (enemy.Health + enemy.Shields + enemy.DPS);
    return bestTarget; // минимальный score
}
```

---

## 5. Система оружия

### 5.1 ProtonGun (`Assets/Scripts/Entities/ProtonGun.cs`)

Протонная пушка — стреляет снарядами.

```csharp
public override ProjectileBase GetProjectile()
{
    var projectile = ProtonProjectilePoolManager.Instance.Get(transform.position);
    projectile.SetAlive(true);
    return projectile;
}

public override void ReleaseProjectile(ProjectileBase projectile)
{
    projectile.gameObject.SetActive(false);
    projectile.GetComponent<TrailRenderer>().Clear();
    ProtonProjectilePoolManager.Instance.Return(projectile as ProtonProjectile);
}
```

### 5.2 LaserGun (`Assets/Scripts/Entities/LaserGun.cs`)

Лазер — луч без снарядов, использует LineRenderer.

```csharp
[RequireComponent(typeof(LineRenderer))]
public class LaserGun : WeaponGun
{
    [SerializeField] protected float flashTimer = 0.1f;
    protected LineRenderer lineRenderer;

    override protected void OnShootNonProjectile(SpaceObject owner, Vector2 pos, Vector2 direction, SpaceObject target)
    {
        lineRenderer.SetPosition(0, pos);
        lineRenderer.SetPosition(1, target.transform.position);
        target.TakeDamage(damage, ignoresShields);
        lineRenderer.enabled = true;
        StartCoroutine(FlashEffect());
    }
}
```

---

## 6. Система пуллинга (Object Pooling)

### Иерархия пулов:
```
IPoolManager<T> (интерфейс)
 └── EntityPoolManager<T> — базовый пул для Entity
      └── SpaceObjectPoolManager<T> — пул для SpaceObject (управляет alive)
           ├── EnemyPoolManager
           ├── InterceptorPoolManager
           └── ProtonProjectilePoolManager
```

### Интерфейс IPoolManager:
```csharp
public interface IPoolManager<T> where T : Entity
{
    T Get(Vector2? position = null);
    void Return(T obj);
}
```

### EntityPoolManager — базовый менеджер:
```csharp
public abstract class EntityPoolManager<T> : MonoBehaviour, IPoolManager<T> where T : Entity
{
    public static EntityPoolManager<T> Instance { get; private set; }  // Синглтон

    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 10;

    private ObjectPool<T> entityPool;  // Unity ObjectPool

    public virtual T Get(Vector2? position = null) { ... }
    public virtual void Return(T entity) { ... }
}
```

### SpaceObjectPoolManager — управление alive:
```csharp
public abstract class SpaceObjectPoolManager<T> : EntityPoolManager<T> where T : SpaceObject
{
    protected override void ActivateEntity(T entity)
    {
        base.ActivateEntity(entity);
        entity.SetAlive(true);
    }

    protected override void DeactivateEntity(T entity)
    {
        base.DeactivateEntity(entity);
        entity.SetAlive(false);
    }
}
```

**ПРАВИЛА ПУЛЛИНГА:**
1. Все снаряды, враги, истребители используют пуллинг
2. **НИКОГДА** не вызывать `Destroy()` — только `Pool.Return()`
3. При возврате в пул: `SetAlive(false)`, `gameObject.SetActive(false)`
4. При получении из пула: `SetAlive(true)`, `gameObject.SetActive(true)`

---

## 7. Принципы выбора цели (Targeting)

### Абстрактный контракт:
Каждый `Ship` обязан реализовать `SelectTarget()`:
```csharp
protected abstract SpaceObject SelectTarget();
```

### Реализации по классам:

| Класс | Логика выбора цели |
|-------|-------------------|
| **Carrier** | Через `AdmiralProtection.getTargetForProtectable()` |
| **Interceptor** | Через `AdmiralProtection.getTargetForProtector()` |
| **Enemy** | `FindClosestShip()` — ближайший Carrier |

### AdmiralProtection — централизованная система:

**Приоритизация:**
```csharp
float score = distance - (enemy.Health + enemy.Shields + enemy.DPS);
// Чем ближе и слабее враг — тем выше приоритет (меньше score)
```

**Матрица защитников:**
- Один защитник на одну цель
- Если цель уже назначена другому — пропускаем
- При смерти цели — переназначение

---

## 8. Структура директорий

```
Assets/Scripts/
├── Core/                    # Базовые абстрактные классы
│   ├── Entity.cs           # Корневой класс
│   ├── SpaceObject.cs      # Объект с физикой
│   ├── Ship.cs             # Корабль с оружием
│   ├── WeaponGun.cs        # Турель
│   ├── ProjectileBase.cs   # Снаряд
│   ├── Admiral.cs          # Командная сущность
│   └── Explosion.cs        # Эффект взрыва
│
├── Entities/                # Конкретные реализации
│   ├── Carrier.cs          # Авианосец
│   ├── Enemy.cs            # Враг
│   ├── Interceptor.cs      # Истребитель
│   ├── AdmiralProtection.cs # Система защиты
│   ├── ProtonGun.cs        # Протонная пушка
│   ├── LaserGun.cs         # Лазер
│   ├── ProtonProjectile.cs # Протонный снаряд
│   └── LaserProjectile.cs  # Лазерный снаряд
│
├── Pools/                   # Пул-менеджеры
│   ├── IPoolManager.cs     # Интерфейс
│   ├── EntityPoolManager.cs
│   ├── SpaceObjectPoolManager.cs
│   ├── EnemyPoolManager.cs
│   ├── InterceptorPoolManager.cs
│   └── ProtonProjectilePoolManager.cs
│
├── Controllers/             # Контроллеры игры
└── Models/                  # Модели данных
```

---

## 9. Практические гайды

### 9.1 Как добавить новый тип корабля

1. Создать класс, наследующий `Ship`:
```csharp
public class MyShip : Ship
{
    protected override SpaceObject SelectTarget()
    {
        // Реализовать логику выбора цели
        return FindClosestEnemy();
    }

    protected override void Update()
    {
        base.Update();
        // Вычислить Direction на основе поведения
        Direction = CalculateDirection();
    }
}
```

2. **НЕ трогать Rigidbody напрямую** — только `Direction`

3. Создать префаб с компонентами: `Rigidbody2D`, `Collider2D`, `SpriteRenderer`

4. Если нужен пуллинг — создать `MyShipPoolManager : SpaceObjectPoolManager<MyShip>`

### 9.2 Как добавить новое оружие

1. Создать класс, наследующий `WeaponGun`:
```csharp
public class MyGun : WeaponGun
{
    public override ProjectileBase GetProjectile()
    {
        return MyProjectilePoolManager.Instance.Get(muzzle.position);
    }

    public override void ReleaseProjectile(ProjectileBase projectile)
    {
        MyProjectilePoolManager.Instance.Return(projectile as MyProjectile);
    }
}
```

2. Если оружие без снарядов (лучевое):
```csharp
public class MyBeam : WeaponGun
{
    // isProjectileWeapon = false в Inspector
    
    protected override void OnShootNonProjectile(SpaceObject owner, Vector2 pos, Vector2 dir, SpaceObject target)
    {
        target.TakeDamage(damage, ignoresShields);
        // Визуальные эффекты
    }
}
```

3. На префабе корабля создать дочерний объект с компонентом оружия

4. Назначить `muzzle` (точка вылета) и параметры в Inspector

### 9.3 Как добавить новый снаряд

1. Создать класс снаряда:
```csharp
public class MyProjectile : ProjectileBase
{
    protected override void OnDeath()
    {
        // ОБЯЗАТЕЛЬНО возврат в пул, НЕ Destroy!
        MyProjectilePoolManager.Instance.Return(this);
    }
}
```

2. Создать пул-менеджер:
```csharp
public class MyProjectilePoolManager : SpaceObjectPoolManager<MyProjectile> { }
```

3. Создать GameObject в сцене с компонентом `MyProjectilePoolManager`

4. Назначить префаб снаряда в Inspector

---

## 10. Anti-patterns (чего НЕ делать)

| ❌ Нельзя | ✅ Правильно |
|----------|-------------|
| `Rigidbody.velocity = ...` в наследниках | `Direction = ...` |
| `transform.position += ...` | `Move(direction)` |
| `Destroy(projectile)` | `Pool.Return(projectile)` |
| Прямой вызов `Rigidbody.AddForce()` в Ship | Установить `Direction`, физика в `SpaceObject` |
| Создание синглтонов для взаимодействия | Использовать ссылки через `GetComponent` / Inspector |
| Ручное добавление оружия в список | `GetComponentsInChildren<WeaponGun>()` в `OnEnable` |

---

## 11. Тестирование

**Нет автоматических тестов.** Проверка через Play Mode:

### Чеклист перед PR:

- [ ] Снаряды возвращаются в пул (нет `Destroy`)
- [ ] Корабли не застревают / не улетают
- [ ] Турели поворачиваются к цели
- [ ] Враги спавнятся и атакуют
- [ ] Истребители возвращаются в ангар
- [ ] Нет NullReferenceException в консоли
- [ ] Коллизии между кораблями игнорируются

### Сцены для тестирования:
- Основная игровая сцена в `Assets/Scenes/`

---

## 12. Полезные grep-паттерны

```bash
# Найти все реализации SelectTarget
SelectTarget

# Найти работу с пулами
PoolManager|\.Get\(|\.Return\(

# Найти управление состоянием
SetState|GetState|ShipState

# Найти работу с Rigidbody (проверить, что только в SpaceObject)
Rigidbody\.velocity|AddForce|MovePosition

# Найти Destroy (не должно быть для снарядов/врагов)
Destroy\(

# Найти настройку оружия
WeaponGun|ShootIfReady|SetTarget
```

---

## 13. Быстрый старт для AI-агента

1. **Перед любыми изменениями** — прочитай `SpaceObject.cs` и `Ship.cs`
2. **Движение** — только через `Direction`, никогда напрямую Rigidbody
3. **Новые сущности** — наследуй правильный базовый класс
4. **Пуллинг обязателен** — для снарядов, врагов, истребителей
5. **Тестируй в Play Mode** — особенно стрельбу и движение

---

_Обновляй этот файл при изменении архитектуры. Последнее обновление: декабрь 2025._
