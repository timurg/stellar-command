using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Weapon : Entity
{
    [SerializeField] protected float damage = 20f; // Дамаг
    [SerializeField] protected bool ignoresShields = false; // Игнорирует щиты (для Rocket)
    [SerializeField] protected float speed = 300f; // Скорость (для PhotonBall/Rocket)
    [SerializeField] protected float shootCooldown = 1f; // Кулдаун стрельбы для оружия
    [SerializeField] protected float shootDistanse = 1f; //Минимальная дистанция стрельбы
    protected float shootTimer = 0f; // Таймер для управления стрельбой

    protected override void Awake()
    {
        base.Awake();
        if (Rigidbody == null)
        {
            Debug.LogError("Rigidbody2D not found on Weapon! Please add it.");
            return;
        }
        Rigidbody.bodyType = RigidbodyType2D.Kinematic; // Для управляемого движения
        Rigidbody.gravityScale = 0;
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true; // Для OnTriggerEnter2D вместо столкновений
        }
    }
    protected override void Update()
    {
        // Обновляем таймер стрельбы
        if (shootTimer > 0) shootTimer -= Time.deltaTime;
        // Базовое движение (переопределяется в наследниках)
        transform.position += transform.right * speed * Time.deltaTime;
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity != null)
        {
            entity.TakeDamage(damage, ignoresShields);
            // Взрыв: Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject); // Уничтожить снаряд
        }
    }
    public float Damage => damage;
    public bool IgnoresShields => ignoresShields;
    public abstract bool IsGuided(); // Для Rocket
    public abstract void SetTarget(Vector2 targetPos); // Для guided
    // Публичный метод для активации стрельбы
    public virtual void ShootIfReady()
    {
        if (shootTimer <= 0)
        {
            // Логика спавна снаряда (реализуется в наследниках)
            shootTimer = shootCooldown;
        }
    }
}