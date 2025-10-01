using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Entity : MonoBehaviour
{
    [SerializeField] protected float mass = 1f; // Масса (для Rigidbody2D)
    [SerializeField] protected float maxHealth = 1000f; // Максимальное здоровье
    protected float Health { get; set; }
    protected Vector2 Direction { get; set; } // Направление движения (нормализованный вектор)
    protected Rigidbody2D Rigidbody { get; private set; }
    protected bool Alive { get; private set; } = true; // Остаётся private set

    protected virtual void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        if (Rigidbody == null)
        {
            Debug.LogError("Rigidbody2D not found on Entity! Please add it.");
            return;
        }
        Rigidbody.mass = mass;
        Health = maxHealth;
    }

    protected virtual void FixedUpdate()
    {
        if (!Alive) return;
        if (Direction.magnitude > 0) // Движение по Direction, если не нулевое
        {
            Rigidbody.linearVelocity = Direction.normalized * Rigidbody.linearVelocity.magnitude; // Поддерживаем скорость, меняем направление
        }
    }

    protected virtual void Update()
    {
        if (!Alive) return;
    }

    public virtual void TakeDamage(float damage, bool ignoreShields = false)
    {
        Health -= damage;
        if (Health <= 0 && Alive)
        {
            SetAlive(false); // Используем метод для изменения состояния
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        Destroy(gameObject);
    }

    // Защищённый метод для установки Alive
    protected virtual void SetAlive(bool state)
    {
        Alive = state;
    }

    public bool IsAlive() => Alive; // Публичный метод для проверки
}