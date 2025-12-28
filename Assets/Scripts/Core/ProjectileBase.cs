using NUnit.Framework.Internal;
using UnityEngine;

/// <summary>
/// Abstract base class for all projectiles (bullets, missiles, etc.).
/// Inherits from SpaceObject for physics-based movement.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>ProjectileBase handles projectile lifecycle from spawn to impact/expiration.</para>
/// <para>Key design patterns:</para>
/// <list type="bullet">
///   <item>POOLING: NEVER use Destroy(). Override OnDeath() to return to pool.</item>
///   <item>INITIALIZATION: Init() sets owner, position, direction, speed, damage.</item>
///   <item>LIFETIME: timeToLive controls max lifetime. liveTimer counts up.</item>
///   <item>COLLISION: OnTriggerEnter2D handles hits. Ignores same-tag objects (friendly fire).</item>
///   <item>MOVEMENT: Uses Direction from SpaceObject. Set in Init(), physics in UpdateMovement().</item>
/// </list>
/// <para>Lifecycle: Pool.Get() → Init() → Update() → OnTriggerEnter2D/timeout → OnDeath() → Pool.Return()</para>
/// <para>CRITICAL: Always return to pool in OnDeath(), never Destroy().</para>
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class ProjectileBase : SpaceObject
{
    /// <summary>Damage dealt on hit.</summary>
    [SerializeField] protected float damage;

    /// <summary>If true, damage bypasses shields.</summary>
    [SerializeField] protected bool ignoresShields;

    /// <summary>Maximum lifetime in seconds before auto-destruction.</summary>
    [SerializeField] protected float timeToLive = 5f;

    /// <summary>Current lifetime counter.</summary>
    protected float liveTimer = 0f;

    /// <summary>The entity that fired this projectile. Used for friendly-fire prevention.</summary>
    public Entity Owner { get; set; }

    /// <summary>
    /// Initializes projectile components.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Initializes projectile for firing. Called by WeaponGun.
    /// </summary>
    /// <param name="owner">Entity that fired this projectile.</param>
    /// <param name="position">Spawn position.</param>
    /// <param name="dir">Movement direction (will be normalized).</param>
    /// <param name="speed">Movement speed.</param>
    /// <param name="damage">Damage on hit.</param>
    public virtual void Init(SpaceObject owner, Vector2 position, Vector2 dir, float speed, float damage)
    {
        this.Owner = owner;
        transform.position = position;
        this.Direction = dir.normalized;
        this.maxSpeed = speed;
        if (damage > 0f) this.damage = damage;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Frame update. Handles lifetime expiration.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (!IsAlive()) return;
        liveTimer += Time.deltaTime;
        if (liveTimer >= timeToLive)
        {
            liveTimer = 0f;
            OnDeath();
        }
    }

    /// <summary>
    /// Handles collision with other objects. Deals damage and triggers OnDeath().
    /// Ignores objects with same tag as owner (friendly fire prevention).
    /// </summary>
    /// <param name="other">Collider that was hit.</param>
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive()) return;

        var entity = other.GetComponent<SpaceObject>();
        if (entity == null) return;
        if (entity.tag == Owner.tag) return;

        var myCol = GetComponent<Collider2D>();
        Vector2 hitPoint;

        if (myCol != null)
        {
            var dist = myCol.Distance(other);
            hitPoint = dist.pointB;
        }
        else
        {
            hitPoint = other.ClosestPoint(transform.position);
        }

        entity.TakeDamage(damage, hitPoint, ignoresShields);
        OnDeath();
    }

}
