using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Fighter drone launched from Carrier. Has fuel system and state machine.
/// Patrols around Carrier, attacks enemies, returns when low on fuel/shields.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>Interceptor is a fuel-based drone controlled by state machine.</para>
/// <para>State Machine:</para>
/// <list type="bullet">
///   <item>HANGAR: In carrier, regenerating fuel/shields. Deploys when target available.</item>
///   <item>PATROL: Circular patrol around Carrier. Searches for targets.</item>
///   <item>ATTACK: Approach target, then orbit while shooting.</item>
///   <item>RETURN: Low fuel/shields - return to carrier hangar.</item>
///   <item>DAMAGED: Auto-transitions to HANGAR for repair.</item>
/// </list>
/// <para>Key mechanics:</para>
/// <list type="bullet">
///   <item>FUEL: Consumed outside hangar. Low fuel triggers RETURN.</item>
///   <item>TARGETING: Uses AdmiralProtection.getTargetForProtector().</item>
///   <item>SPEED: GetMaxSpeed() returns different values per state.</item>
///   <item>POOLING: Managed by InterceptorPoolManager (not destroyed).</item>
/// </list>
/// <para>Movement: Sets Direction via Move() - physics handled by SpaceObject.</para>
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Interceptor : Ship
{
    /// <summary>Radius of patrol orbit around Carrier.</summary>
    [SerializeField] private float patrolRadius = 15f;
    
    /// <summary>Fuel consumption rate per second outside hangar.</summary>
    [SerializeField] private float fuelConsumptionRate = 10f;
    
    /// <summary>Fuel threshold for returning to hangar.</summary>
    [SerializeField] private float lowFuelThreshold = 20f;
    
    /// <summary>Shield threshold for returning to hangar.</summary>
    [SerializeField] private float lowShieldsThreshold = 20f;

    private Carrier carrier;
    
    /// <summary>Current fuel level.</summary>
    [SerializeField] private float fuel = 100f;
    
    /// <summary>Maximum fuel capacity.</summary>
    [SerializeField] private float maxFuel = 100f;
    
    /// <summary>Current angle in patrol orbit.</summary>
    [SerializeField] private float patrolAngle = 0f;
    
    /// <summary>Current angle in attack orbit.</summary>
    [SerializeField] private float orbitAngle = 0f;

    private AdmiralProtection myAdmiralProtection;

    /// <summary>
    /// Current fuel level (clamped to 0-maxFuel).
    /// </summary>
    public float Fuel
    {
        get { return fuel; }
        set { fuel = Mathf.Clamp(value, 0, maxFuel); }
    }

    /// <summary>
    /// Reference to parent Carrier.
    /// </summary>
    public Carrier Carrier
    {
        get { return carrier; }
        set { carrier = value; }
    }

    /// <summary>
    /// Maximum fuel capacity.
    /// </summary>
    public float MaxFuel
    {
        get { return maxFuel; }
    }   

    private float savedMaxSpeed;

    /// <summary>
    /// Initializes interceptor, finds AdmiralProtection, sets initial state.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        myAdmiralProtection = FindFirstObjectByType<AdmiralProtection>();
        if (myAdmiralProtection == null)    
        {
            Debug.LogWarning("Interceptor: No AdmiralProtection component found, proceeding without it.");
        }
        savedMaxSpeed = maxSpeed;
        SetState(ShipState.HANGAR);
        Fuel = maxFuel;
        orbitAngle = Random.Range(0f, 2f * Mathf.PI);
    }

    /// <summary>
    /// Update loop - delegates to state machine.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (carrier == null || !IsAlive()) return;
        HandleState(Time.deltaTime);
    }

    /// <summary>
    /// State machine handler - executes behavior for current state.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    private void HandleState(float deltaTime)
    {
        switch (GetState())
        {
            case ShipState.HANGAR:
                transform.position = carrier.hangarObject.transform.position;
                if (SelectTarget() != null && Fuel >= maxFuel)
                {
                    SetState(ShipState.PATROL);
                }
                break;
            case ShipState.PATROL:
                Patrol(deltaTime);
                break;
            case ShipState.ATTACK:
                Attack(deltaTime);
                break;
            case ShipState.RETURN:
                Return(deltaTime);
                break;
            case ShipState.DAMAGED:
                SetState(ShipState.HANGAR);
                break;
        }
    }

    /// <summary>
    /// Returns speed modifier based on current state.
    /// HANGAR=0, RETURN=50%, DAMAGED=30%, PATROL=50%, ATTACK=100%.
    /// </summary>
    /// <returns>Modified max speed.</returns>
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

    /// <summary>
    /// Patrol behavior - circular orbit around Carrier, search for targets.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    private void Patrol(float deltaTime)
    {
        patrolAngle += Mathf.PI / 10f * deltaTime;
        Vector2 patrolPos = (Vector2)carrier.transform.position + new Vector2(Mathf.Cos(patrolAngle), Mathf.Sin(patrolAngle)) * patrolRadius;
        Vector2 direction = (patrolPos - (Vector2)transform.position).normalized;
        Move(direction);
        Fuel -= fuelConsumptionRate * deltaTime;
        if (Fuel <= lowFuelThreshold || Shields <= lowShieldsThreshold)
        {
            SetState(ShipState.RETURN);
        }
        var newTarget = SelectTarget();
        if (newTarget != null)
        {
            SetTarget(newTarget);
            SetState(ShipState.ATTACK);
        }
    }

    /// <summary>
    /// Attack behavior - approach target, orbit and shoot when in range.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    private void Attack(float deltaTime)
    {
        if (target == null || !target.IsAlive())
        {
            SetState(ShipState.PATROL);
            return;
        }
        Vector2 closestPoint = target.GetComponent<Collider2D>().ClosestPoint(transform.position);
        Vector2 direction = (closestPoint - (Vector2)transform.position).normalized;
        float distanceToTarget = Vector2.Distance((Vector2)transform.position, closestPoint);
        if (!AllWeaponsInRange(distanceToTarget))
        {
            Move(direction);
        }
        else
        {
            Vector2 offset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * MinAttackDistance;
            Vector2 desiredPosition = (Vector2)target.transform.position + offset;
            Vector2 orbitDirection = (desiredPosition - (Vector2)transform.position).normalized;
            Move(orbitDirection);
            ShootAtTarget();
        }
        Fuel -= fuelConsumptionRate * deltaTime;
        if (Fuel <= lowFuelThreshold || Shields <= lowShieldsThreshold)
        {
            SetState(ShipState.RETURN);
        }
    }

    /// <summary>
    /// Return behavior - move toward carrier hangar.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    private void Return(float deltaTime)
    {
        Vector2 direction = ((Vector2)carrier.hangarObject.transform.position - (Vector2)transform.position).normalized;
        Move(direction);
        if (Vector2.Distance(transform.position, carrier.hangarObject.transform.position) < 1f)
        {
            SetState(ShipState.HANGAR);
        }
    }

    /// <summary>
    /// Selects target using AdmiralProtection system.
    /// </summary>
    /// <returns>Target from AdmiralProtection or current target if alive.</returns>
    protected override SpaceObject SelectTarget()
    {
        if (target != null && target.IsAlive()) return target;
        if (myAdmiralProtection != null)
        {
            var admiralTarget = myAdmiralProtection.getTargetForProtector(this);
            if (admiralTarget != null) return admiralTarget;
        }
        return null;
    }

    /// <summary>
    /// Deploys interceptor from hangar - sets position, state, and refuels.
    /// </summary>
    /// <param name="startPos">Spawn position.</param>
    /// <param name="parentCarrier">Parent Carrier reference.</param>
    public void Deploy(Vector2 startPos, Carrier parentCarrier)
    {
        transform.position = startPos;
        SetState(ShipState.PATROL);
        carrier = parentCarrier;
        Fuel = maxFuel;
        shields = maxShields;
        gameObject.SetActive(true);
        SetAlive(true);
    }

    /// <summary>
    /// Sets state with hangar-specific logic (resets target).
    /// </summary>
    /// <param name="newState">New ship state.</param>
    public override void SetState(ShipState newState)
    {
        base.SetState(newState);
        if (newState == ShipState.HANGAR)
        {
            Debug.Log($"Interceptor {Id} returning to hangar. Fuel: {Fuel}/{maxFuel}, Shields: {Shields}/{maxShields}");
            target = null;
        }
    }

    
}