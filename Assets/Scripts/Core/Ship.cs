using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Rendering;

/// <summary>
/// Abstract base class for all ships in the game (Carrier, Enemy, Interceptor).
/// Extends SpaceObject with shields, weapons, targeting, and state management.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>Ship is the base for all combat-capable space entities. Key design patterns:</para>
/// <list type="bullet">
///   <item>MOVEMENT: Inherited from SpaceObject. Set Direction property, never Rigidbody directly.</item>
///   <item>TARGETING: Each derived class MUST implement SelectTarget() abstractly.</item>
///   <item>WEAPONS: Auto-loaded from child objects via GetComponentsInChildren&lt;WeaponGun&gt;().</item>
///   <item>STATE MACHINE: ShipState enum controls behavior (HANGAR, PATROL, ATTACK, RETURN, DAMAGED).</item>
///   <item>DAMAGE: Shields absorb damage first, then health. Override TakeDamage() if needed.</item>
/// </list>
/// <para>Lifecycle: Awake() → OnEnable() (loads weapons) → Update() (regen + shooting) → ShootAtTarget()</para>
/// <para>Collisions between ships are automatically ignored via IgnoreShipCollisions().</para>
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Ship : SpaceObject
{
    /// <summary>Current shield points. Absorbs damage before health.</summary>
    [SerializeField] protected float shields = 500f;
    
    /// <summary>Maximum shield capacity.</summary>
    [SerializeField] protected float maxShields = 500f;

    /// <summary>List of weapons attached to this ship. Auto-populated from children.</summary>
    [SerializeField] protected List<WeaponGun> weapons = new List<WeaponGun>();
    
    /// <summary>Current target for weapons and AI behavior.</summary>
    [SerializeField] protected SpaceObject target;
    
    /// <summary>
    /// Ship behavioral states for AI and game logic.
    /// </summary>
    public enum ShipState
    {
        /// <summary>In hangar, refueling and repairing.</summary>
        HANGAR,
        /// <summary>Patrolling around carrier or designated area.</summary>
        PATROL,
        /// <summary>Actively attacking a target.</summary>
        ATTACK,
        /// <summary>Returning to carrier/hangar.</summary>
        RETURN,
        /// <summary>Damaged, needs repair in hangar.</summary>
        DAMAGED
    }
    
    [SerializeField] private ShipState state = ShipState.HANGAR;
    
    /// <summary>
    /// Gets minimum attack distance based on shortest-range weapon.
    /// Used for determining optimal attack positioning.
    /// </summary>
    public float MinAttackDistance
    {
        get
        {
            float min = float.MaxValue;
            bool hasAny = false;
            foreach (var w in weapons)
            {
                if (w == null) continue;
                hasAny = true;
                if (w.EffectiveRange < min) min = w.EffectiveRange;
            }
            return hasAny ? min : 0f;
        }
    }

    /// <summary>
    /// Gets maximum attack distance based on longest-range weapon.
    /// Used for target acquisition range checks.
    /// </summary>
    public float MaxAttackDistance
    {
        get
        {
            float max = 0f;
            foreach (var w in weapons)
            {
                if (w == null) continue;
                if (w.EffectiveRange > max) max = w.EffectiveRange;
            }
            return max;
        }
    }

    /// <summary>
    /// Checks if ALL weapons can fire at the given distance.
    /// </summary>
    /// <param name="distance">Distance to target.</param>
    /// <returns>True if all weapons are in range.</returns>
    public bool AllWeaponsInRange(float distance)
    {
        foreach (var w in weapons)
            if (w != null && distance > w.EffectiveRange)
                return false;
        return true;
    }

    /// <summary>
    /// Checks if ANY weapon can fire at the given distance.
    /// </summary>
    /// <param name="distance">Distance to target.</param>
    /// <returns>True if at least one weapon is in range.</returns>
    public bool AnyWeaponInRange(float distance)
    {
        foreach (var w in weapons)
            if (w != null && distance <= w.EffectiveRange)
                return true;
        return false;
    }

    /// <summary>
    /// Initializes ship. Called before Start().
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }
    /// <summary>
    /// Called when object becomes active. Sets up collision ignoring and loads weapons.
    /// </summary>
    protected void Start()
    {
    }

    /// <summary>
    /// Called when ship is enabled. Loads weapons from children and sets up collisions.
    /// </summary>
    protected void OnEnable()
    {
        IgnoreShipCollisions();
        LoadWeapons();
    }
    
    /// <summary>
    /// Auto-discovers and loads all WeaponGun components from child objects.
    /// Weapons inherit the ship's tag for friend/foe identification.
    /// </summary>
    void LoadWeapons()
    {
        foreach (var weapon in GetComponentsInChildren<WeaponGun>())
        {
            if (weapons.Contains(weapon)) continue;
            weapons.Add(weapon);
            weapon.tag = this.tag;
        }
    }

    /// <summary>
    /// Frame update. Handles shield regeneration and weapon firing.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (!IsAlive()) return;
        if (shields < maxShields) shields += Time.deltaTime * 10f;
        ShootAtTarget();
    }
    
    /// <summary>
    /// Sets the current target for this ship and its weapons.
    /// </summary>
    /// <param name="newTarget">The SpaceObject to target.</param>
    public void SetTarget(SpaceObject newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Calculates total damage per second from all equipped weapons.
    /// </summary>
    /// <returns>Combined DPS of all weapons.</returns>
    public float GetDamagePerSecond()
    {
        float dps = 0f;
        foreach (var weapon in weapons)
        {
            if (weapon != null)
            {
                dps += weapon.GetDamagePerSecond();
            }
        }
        return dps;
    }

    /// <summary>
    /// Gets the current target.
    /// </summary>
    /// <returns>Current target SpaceObject or null.</returns>
    public SpaceObject GetTarget()
    {
        return target;
    }

    /// <summary>
    /// Main combat loop. Acquires target via SelectTarget() and fires all weapons.
    /// Called every frame from Update().
    /// </summary>
    protected virtual void ShootAtTarget()
    {
        if (target == null || !target.IsAlive())
        {
            target = SelectTarget();
        }
        if (target != null && IsAlive())
        {
            foreach (var weapon in weapons)
            {
                if (weapon != null)
                {
                    weapon.SetTarget(target);
                    weapon.ShootIfReady();
                }
            }
        }
    }
    
    /// <summary>
    /// ABSTRACT: Each derived class must implement target selection logic.
    /// Called when current target is null or dead.
    /// </summary>
    /// <returns>Selected target or null if none available.</returns>
    protected abstract SpaceObject SelectTarget();
    
    /// <summary>
    /// Gets current ship state.
    /// </summary>
    public ShipState GetState() => state;
    
    /// <summary>
    /// Sets ship state. Override for custom state transition logic.
    /// </summary>
    /// <param name="newState">New state to set.</param>
    public virtual void SetState(ShipState newState)
    {
        state = newState;
        switch (newState)
        {
            case ShipState.DAMAGED:
                if (shields <= 0) SetAlive(false);
                break;
        }
    }
    
    /// <summary>
    /// Sets up Physics2D to ignore collisions between all ships.
    /// Called on enable to prevent ship-to-ship physics interference.
    /// </summary>
    private void IgnoreShipCollisions()
    {
        Ship[] ships = FindObjectsByType<Ship>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var otherShip in ships)
        {
            if (otherShip != this && otherShip.GetComponent<Collider2D>() != null)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherShip.GetComponent<Collider2D>(), true);
            }
        }
    }

    /// <summary>
    /// Applies damage. Shields absorb damage first, then health.
    /// </summary>
    /// <param name="damage">Amount of damage.</param>
    /// <param name="ignoreShields">If true, bypasses shields.</param>
    public override void TakeDamage(float damage, Vector2 hitPoint, bool ignoreShields = false)
    {
        var shieldEmitter = GetComponentInChildren<ShieldRippleEmitter>(true);
        if (shieldEmitter != null) shieldEmitter.AddHitWorld(hitPoint);
        if (!ignoreShields && shields > 0){
            shields -= damage;
        } else{
            Health -= damage;
        }
        if (Health <= 0 && IsAlive())
        {
            SetAlive(false);
            OnDeath();
        }
    }

    /// <summary>
    /// Current shields property with clamping.
    /// </summary>
    public new float Shields
    {
        get => shields;
        set => shields = Mathf.Clamp(value, 0, maxShields);
    }

    /// <summary>
    /// Maximum shields capacity property.
    /// </summary>
    public float MaxShields
    {
        get => maxShields;
        set => maxShields = value;
    }

    /// <summary>
    /// Checks if any weapon is ready to fire.
    /// </summary>
    /// <returns>True if at least one weapon can fire.</returns>
    public bool CanShoot()
    {
        return weapons.Any(w => w != null && w.CanFire());
    }

}