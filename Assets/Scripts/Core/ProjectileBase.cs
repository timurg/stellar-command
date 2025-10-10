using NUnit.Framework.Internal;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class ProjectileBase : SpaceObject
{
    [SerializeField] protected float damage;
    [SerializeField] protected bool ignoresShields;
    [SerializeField] protected float timeToLive = 5f; // Время жизни снаряда
    protected float liveTimer = 0f; // Время жизни снаряда

    public Entity Owner { get; set; } // Владелец снаряда (кто его выстрелил)

    protected override void Awake()
    {
        base.Awake();
        // Дополнительная инициализация, если нужна
    }
    public virtual void Init(SpaceObject owner, Vector2 position, Vector2 dir, float speed, float damage)
    {
        this.Owner = owner;
        transform.position = position;
        this.Direction = dir.normalized;
        this.maxSpeed = speed;
        if (damage > 0f) this.damage = damage;
        gameObject.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();
        if (!IsAlive()) return;
        //transform.position += (Vector3)(Direction * speed * Time.deltaTime);
        liveTimer += Time.deltaTime;
        if (liveTimer >= timeToLive)
        {
            //Debug.Log("Projectile time to live expired, returning to pool.");
            liveTimer = 0f;
            OnDeath();
        }        
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive()) return;

        var entity = other.GetComponent<SpaceObject>();
        if (entity != null)
        {
            //Debug.Log($"Projectile {entity.GetType().Name} tag {this.tag}, dealing {damage} damage to tag {entity.tag} My type is {this.GetType().FullName}, other type is {other.GetType().FullName}, owner is null {Owner==null}, .");
            if (entity.tag == Owner.tag) return; // Не наносим урон своей команде

            entity.TakeDamage(damage, ignoresShields);
            OnDeath();
        }
    }

    
 
}
