using UnityEngine;

/// <summary>
/// Enemy ship that attacks the player's Carrier.
/// Spawns at screen edges, approaches target, and orbits while attacking.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>Enemy is an AI-controlled hostile ship with orbital combat behavior.</para>
/// <para>Key behaviors:</para>
/// <list type="bullet">
///   <item>SPAWNING: Uses SpawnAtEdge() to appear at random screen edge.</item>
///   <item>APPROACH: Moves toward target until all weapons are in range.</item>
///   <item>ORBITAL COMBAT: Once in range, orbits target while shooting.</item>
///   <item>TARGETING: FindClosestShip() finds nearest Carrier.</item>
///   <item>POOLING: Returns to EnemyPoolManager on death (never Destroy!).</item>
/// </list>
/// <para>Movement pattern: Approach → Orbit at random radius → Shoot</para>
/// <para>Direction is set based on current behavior phase.</para>
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : Ship
{
    /// <summary>Offset from screen edges when spawning.</summary>
    [SerializeField] private float spawnEdgeOffset = 10f;
    
    /// <summary>Minimum orbit radius around target.</summary>
    [SerializeField] private float orbitRadiusMin = 2f;
    
    /// <summary>Maximum orbit radius around target.</summary>
    [SerializeField] private float orbitRadiusMax = 5f;
    
    /// <summary>Angular speed for orbital movement (radians/sec).</summary>
    [SerializeField] private float orbitAngularSpeed = Mathf.PI / 8f;
    
    /// <summary>Interval for course correction during orbit.</summary>
    [SerializeField] private float courseCorrectionInterval = 0.7f;

    private float orbitRadius;
    private float orbitAngle = 0f;
    private Vector2 smoothedOrbitDirection = Vector2.zero;
    private float courseCorrectionTimer = 0f;

    /// <summary>
    /// Initializes enemy with random orbit radius and finds initial target.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        orbitRadius = Random.Range(orbitRadiusMin, orbitRadiusMax);
        courseCorrectionTimer = courseCorrectionInterval;
        target = FindClosestShip();
    }

    /// <summary>
    /// Update loop - handles approach and orbital movement behavior.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (target == null || !IsAlive()) return;

        Collider2D targetCollider = target.GetComponent<Collider2D>();
        Vector2 closestPoint = targetCollider.ClosestPoint(transform.position);
        float distanceToTarget = Vector2.Distance((Vector2)transform.position, closestPoint);

        if (!AllWeaponsInRange(distanceToTarget))
        {
            // Approach phase - move toward target
            Direction = (closestPoint - (Vector2)transform.position).normalized;
        }
        else
        {
            // Orbital combat phase
            courseCorrectionTimer -= Time.deltaTime;
            if (courseCorrectionTimer <= 0f)
            {
                orbitAngle += orbitAngularSpeed * Time.deltaTime;
                Vector2 offset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
                Vector2 desiredPosition = (Vector2)target.transform.position + offset;
                Vector2 rawOrbitDirection = (desiredPosition - (Vector2)transform.position).normalized;
                smoothedOrbitDirection = Vector2.Lerp(smoothedOrbitDirection, rawOrbitDirection, 0.1f);
                courseCorrectionTimer = courseCorrectionInterval;
            }
            Direction = smoothedOrbitDirection;
            ShootAtTarget();
        }
    }

    /// <summary>
    /// Returns enemy to pool on death (with explosion effect).
    /// NEVER calls Destroy - uses pooling!
    /// </summary>
    protected override void OnDeath()
    {
        var explosion = ExplosionFXPoolManager.Instance.Get();
        explosion.transform.position = transform.position;
        explosion.gameObject.SetActive(true);
        EnemyPoolManager.Instance.Return(this);
    }

    /// <summary>
    /// Finds the closest Carrier as attack target.
    /// </summary>
    /// <returns>Nearest alive Carrier or null.</returns>
    private Ship FindClosestShip()
    {
        Ship[] ships = FindObjectsByType<Carrier>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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

    /// <summary>
    /// Returns current target (simple implementation).
    /// </summary>
    /// <returns>Current target SpaceObject.</returns>
    protected override SpaceObject SelectTarget()
    {
        return target;
    }

    /// <summary>
    /// Sets ship state with potential for additional Enemy-specific logic.
    /// </summary>
    /// <param name="newState">New ship state.</param>
    public override void SetState(ShipState newState)
    {
        base.SetState(newState);
    }

    /// <summary>
    /// Spawns enemy at random screen edge and resets state.
    /// Used by EnemyPoolManager when getting enemy from pool.
    /// </summary>
    /// <param name="edge">Edge index (0-3: left, right, top, bottom). -1 for random.</param>
    public void SpawnAtEdge(int edge = -1)
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        if (edge == -1) edge = Random.Range(0, 4);
        Vector2 spawnPos = Vector2.zero;
        switch (edge)
        {
            case 0: spawnPos = new Vector2(-screenBounds.x - spawnEdgeOffset, Random.Range(-screenBounds.y, screenBounds.y)); break;
            case 1: spawnPos = new Vector2(screenBounds.x + spawnEdgeOffset, Random.Range(-screenBounds.y, screenBounds.y)); break;
            case 2: spawnPos = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), screenBounds.y + spawnEdgeOffset); break;
            case 3: spawnPos = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), -screenBounds.y - spawnEdgeOffset); break;
        }
        transform.position = spawnPos;
        SetAlive(true);
        Health = maxHealth;
        shields = maxShields;
        SetState(ShipState.PATROL);
        orbitAngle = Random.Range(0f, 2f * Mathf.PI);
        courseCorrectionTimer = 0f;
    }
}