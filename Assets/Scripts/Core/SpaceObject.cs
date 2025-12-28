using UnityEngine;

/// <summary>
/// Abstract base class for all physical objects in space (ships, projectiles, etc.).
/// Handles all movement physics, rotation, health, and damage systems.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>This is the CENTRAL class for all movement logic. ALL movement and rotation code MUST be here.</para>
/// <para><b>CRITICAL DESIGN PRINCIPLE:</b> Derived classes (Ship, Enemy, Interceptor) should NEVER directly 
/// modify Rigidbody.velocity or transform.position. They should ONLY set the Direction property, 
/// and SpaceObject.UpdateMovement() will apply physics centrally.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item>Rigidbody2D physics: mass, velocity, forces</item>
///   <item>Movement via Direction property (normalized vector)</item>
///   <item>Speed limiting via maxSpeed and GetMaxSpeed()</item>
///   <item>Smooth rotation towards movement direction</item>
///   <item>Health/damage system with TakeDamage() and OnDeath()</item>
///   <item>Alive state management for pooling compatibility</item>
/// </list>
/// <para>Movement pattern: Derived class sets Direction → FixedUpdate() calls UpdateMovement() → Physics applied</para>
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class SpaceObject : Entity
{
    /// <summary>Mass applied to Rigidbody2D on initialization.</summary>
    [SerializeField] protected float mass = 1f;
    
    /// <summary>Maximum health value. Health is reset to this on spawn.</summary>
    [SerializeField] protected float maxHealth = 1000f;
    
    /// <summary>Maximum movement speed. Can be overridden via GetMaxSpeed().</summary>
    [SerializeField] protected float maxSpeed = 10f;
    
    /// <summary>Acceleration force applied when moving.</summary>
    [SerializeField] protected float acceleration = 10f;

    /// <summary>If true, object rotates to face movement direction.</summary>
    [SerializeField] protected bool rotateToDirection = true;
    
    /// <summary>Alive state flag. Used by pooling system. Set via SetAlive().</summary>
    [SerializeField] protected bool alive = false;
    
    /// <summary>Current health points. Object dies when this reaches 0.</summary>
    public float Health { get; set; }
    
    /// <summary>
    /// Movement direction vector. Set this to control movement.
    /// Physics is applied in UpdateMovement() based on this value.
    /// DERIVED CLASSES SHOULD ONLY MODIFY THIS PROPERTY FOR MOVEMENT.
    /// </summary>
    public Vector2 Direction { get; set; }
    
    /// <summary>Cached Rigidbody2D reference.</summary>
    protected Rigidbody2D Rigidbody { get; private set; }

    /// <summary>Shield points (damage absorbed before health).</summary>
    public float Shields { get; protected set; } = 0f;
    
    /// <summary>Damage per second output (used for targeting priority).</summary>
    public float DPS { get; protected set; } = 0f;

    /// <summary>
    /// Initializes Rigidbody2D, sets mass and initial health.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Rigidbody = GetComponent<Rigidbody2D>();
        if (Rigidbody == null)
        {
            Debug.LogError("Rigidbody2D not found on Entity! Please add it.");
            return;
        }
        Rigidbody.mass = mass;
        Health = maxHealth;
    }

    /// <summary>
    /// Physics update. Calls UpdateMovement() if alive.
    /// </summary>
    protected void FixedUpdate()
    {
        if (!IsAlive()) return;
        UpdateMovement();
    }

    /// <summary>
    /// Central movement update method. Applies physics based on Direction property.
    /// Supports both Dynamic and Kinematic Rigidbody types.
    /// DO NOT OVERRIDE unless absolutely necessary - modify Direction instead.
    /// </summary>
    protected virtual void UpdateMovement()
    {
        if (Direction.magnitude > 0)
        {
            if (Rigidbody.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 force = Direction.normalized * acceleration;
                Rigidbody.AddForce(force * Time.fixedDeltaTime, ForceMode2D.Impulse);
                if (Rigidbody.linearVelocity.magnitude > GetMaxSpeed())
                {
                    Rigidbody.linearVelocity = Rigidbody.linearVelocity.normalized * GetMaxSpeed();
                }
            }
            else if (Rigidbody.bodyType == RigidbodyType2D.Kinematic)
            {
                Vector2 move = Direction.normalized * GetMaxSpeed() * Time.fixedDeltaTime;
                Rigidbody.MovePosition(Rigidbody.position + move);
            }
            UpdateRotation();
        }

    }
    
    /// <summary>
    /// Sets the Direction property to control movement.
    /// Preferred method for external movement commands.
    /// </summary>
    /// <param name="direction">Normalized direction vector.</param>
    public void Move(Vector2 direction)
    {
        Direction = direction;
    }

    /// <summary>
    /// Smoothly rotates object to face movement direction using Slerp.
    /// Override for custom rotation behavior.
    /// </summary>
    protected virtual void UpdateRotation()
    {
        if (Rigidbody.linearVelocity.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(Rigidbody.linearVelocity.y, Rigidbody.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), 0.2f);
        }
    }

    /// <summary>
    /// Frame update. Override in derived classes for per-frame logic.
    /// </summary>
    protected virtual void Update()
    {
        if (!IsAlive()) return;
    }

    /// <summary>
    /// Applies damage to this object. Triggers OnDeath() if health reaches 0.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    /// <param name="ignoreShields">If true, bypasses shields and damages health directly.</param>
    public virtual void TakeDamage(float damage, Vector2 hitPoint, bool ignoreShields = false)
    {
        Health -= damage;
        if (Health <= 0 && IsAlive())
        {
            SetAlive(false);
            OnDeath();
        }
    }

    /// <summary>
    /// Called when health reaches 0. Override for custom death behavior.
    /// For pooled objects, return to pool instead of Destroy().
    /// </summary>
    protected virtual void OnDeath()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Sets the alive state. Used by pooling system.
    /// </summary>
    /// <param name="state">True if object should be active and processing.</param>
    public virtual void SetAlive(bool state)
    {
        alive = state;
    }

    /// <summary>
    /// Returns current alive state.
    /// </summary>
    public bool IsAlive() => alive;

    /// <summary>
    /// Returns maximum speed. Override to apply state-based speed modifiers.
    /// </summary>
    public virtual float GetMaxSpeed() => maxSpeed;
}