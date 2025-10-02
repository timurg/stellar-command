using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Entity : MonoBehaviour
{
    [SerializeField] protected float mass = 1f; // Масса (для Rigidbody2D)
    [SerializeField] protected float maxHealth = 1000f; // Максимальное здоровье
    [SerializeField] protected float maxSpeed = 100f; // Максимальная скорость
    [SerializeField] protected float acceleration = 200f; // Ускорение

    [SerializeField] protected bool rotateToDirection = true; // Флаг для ротации (доступен в наследниках)
    [SerializeField] protected bool alive = true; // Объект жив
    protected float Health { get; set; }
    public Vector2 Direction { get; set; } // Направление движения (нормализованный вектор)
    protected Rigidbody2D Rigidbody { get; private set; }
    

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

    protected void FixedUpdate()
    {
        if (!IsAlive()) return;
        UpdateMovement();
    }

    // Универсальное обновление движения и ротации
    protected virtual void UpdateMovement()
    {
        if (Direction.magnitude > 0)
        {
            Vector2 force = Direction.normalized * acceleration;
            Rigidbody.AddForce(force * Time.fixedDeltaTime, ForceMode2D.Impulse);
            if (Rigidbody.linearVelocity.magnitude > maxSpeed)
            {
                Rigidbody.linearVelocity = Rigidbody.linearVelocity.normalized * maxSpeed;
            }
            if (this is Carrier){
                Debug.Log("Carrier speed: " + Rigidbody.linearVelocity.magnitude + " Direction.magnitude: " + Direction.magnitude);
            }
            UpdateRotation();
        }
        
    }
    protected void Move(Vector2 direction)
    {
        // Управление движением только через Direction
        Direction = direction;
    }

    // Вращение по направлению движения (можно переопределять)
    protected virtual void UpdateRotation()
    {
        if (Rigidbody.linearVelocity.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(Rigidbody.linearVelocity.y, Rigidbody.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), 0.2f);
        }
    }

    protected virtual void Update()
    {
        if (!IsAlive()) return;
       // UpdateMovement();
    }

    public virtual void TakeDamage(float damage, bool ignoreShields = false)
    {
        Health -= damage;
        if (Health <= 0 && IsAlive())
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
        alive = state;
    }

    public bool IsAlive() => alive; // Публичный метод для проверки
}