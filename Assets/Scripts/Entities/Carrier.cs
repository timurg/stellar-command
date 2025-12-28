using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// Player's main ship - an aircraft carrier that commands interceptor drones.
/// Manages hangar, interceptor deployment, and serves as the primary protected object.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>Carrier is the player's main ship and the core of the defense system.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item>HANGAR MANAGEMENT: Stores interceptors, regenerates their fuel and shields.</item>
///   <item>DEPLOYMENT: Deploys interceptors on cooldown when enemies are present.</item>
///   <item>TARGETING: Uses AdmiralProtection.getTargetForProtectable() for own weapons.</item>
///   <item>PROTECTION: Registers itself with AdmiralProtection on Awake.</item>
///   <item>UPGRADE SYSTEM: Contains upgrade tree model for progression.</item>
/// </list>
/// <para>Movement: Controlled by player input (ClickToMoveBehavior or UserInputBehavior).</para>
/// <para>Lifecycle: Awake (register protection) → Start (init interceptors) → FixedUpdate (hangar/deploy)</para>
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Carrier : Ship
{
    /// <summary>Maximum number of interceptors in hangar.</summary>
    [SerializeField] private int maxInterceptors = 5;
    
    /// <summary>Fuel/shield regeneration rate per second in hangar.</summary>
    [SerializeField] private float fuelRegenRate = 2f;
    
    /// <summary>Cooldown between interceptor deployments.</summary>
    [SerializeField] private float deployCooldown = 1f;
    
    /// <summary>Transform marking hangar position for interceptor spawning.</summary>
    [SerializeField] public GameObject hangarObject;
    
    /// <summary>UI slider for health display.</summary>
    [SerializeField] private Slider HpBar;
    
    /// <summary>UI slider for shield display.</summary>
    [SerializeField] private Slider ShieldsBar;
    
    /// <summary>UI text for interceptor count display.</summary>
    [SerializeField] private Text InterceptorCountText;

    private UpgradeTreeModel UpgradeTreeModel;

    /// <summary>
    /// Creates the upgrade tree model for Carrier progression.
    /// </summary>
    /// <returns>Configured upgrade tree model.</returns>
    public UpgradeTreeModel CreateUpgradeTreeModel(){
        var model = new UpgradeTreeModel { treeName = "Carrier" };

        var hp = new UpgradeNodeModel { id = "hp", title = "ХП", maxLevel = 5, isUnlocked = true, description = "+100 HP за уровень" };
        var shields = new UpgradeNodeModel { id = "shields", title = "Щиты", maxLevel = 3, requiredNodeIds = { "hp" } };
        var guns = new UpgradeNodeModel { id = "guns", title = "Турели", maxLevel = 1 };
        var intGun = new UpgradeNodeModel { id = "int_gun", title = "Interceptor Gun", maxLevel = 4, requiredNodeIds = { "guns" } };
        var fleet = new UpgradeNodeModel { id = "fleet", title = "Флот", maxLevel = 3 };
        var speed = new UpgradeNodeModel { id = "speed", title = "Скорость", maxLevel = 5, requiredNodeIds = { "fleet" } };

        model.allNodes.AddRange(new[] { hp, shields, guns, intGun, fleet, speed });

        model.rootPaths.Add(new UpgradePath
        {
            pathName = "Прочность",
            nodeIds = new List<string> { "hp", "shields" }
        });

        model.rootPaths.Add(new UpgradePath
        {
            pathName = "Бой",
            nodeIds = new List<string> { "guns", "int_gun" }
        });

        model.rootPaths.Add(new UpgradePath
        {
            pathName = "Флот",
            nodeIds = new List<string> { "fleet", "speed" }
        });
        return model;
    }

    /// <summary>List of interceptors in hangar (not instantiated, from pool).</summary>
    private readonly List<Interceptor> hangar = new();
    private float deployTimer = 0f;

    private AdmiralProtection admiralProtection;
    
    /// <summary>
    /// Initializes interceptors from pool and sets up protection.
    /// </summary>
    new protected void Start()
    {
        base.Start();
        // Pooling: fill hangar from pool
        InitxInterceptors();
        
        //ShieldsBar.maxValue = maxShields;
        //HpBar.maxValue = maxHealth;
    }

    /// <summary>
    /// Fills hangar with interceptors from pool.
    /// </summary>
    protected void InitxInterceptors()
    {
        hangar.Clear();
        for (int i = 0; i < maxInterceptors; i++)
        {
            var interceptor = InterceptorPoolManager.Instance.Get();
            interceptor.transform.position = hangarObject.transform.position;
            interceptor.SetState(ShipState.HANGAR);
            interceptor.Carrier = this;
            interceptor.gameObject.SetActive(false);
            hangar.Add(interceptor);
        }
    }

    /// <summary>
    /// Registers with AdmiralProtection on awake.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        admiralProtection = FindFirstObjectByType<AdmiralProtection>();
        if (admiralProtection == null)
        {
            Debug.LogError("Carrier: No AdmiralProtection found in scene!");
            enabled = false;
            return;
        }
        admiralProtection.Protect(this);
    }

    /// <summary>
    /// Physics update: handles hangar, deployment, HUD, and shooting.
    /// </summary>
    new private void FixedUpdate()
    {
        base.FixedUpdate();
        HandleHangar(Time.fixedDeltaTime);
        HandleDeploy(Time.fixedDeltaTime);
        UpdateHud();
        ShootAtTarget();
    }

    /// <summary>
    /// Regenerates fuel and shields for interceptors in hangar.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    private void HandleHangar(float deltaTime)
    {
        foreach (var interceptor in hangar)
        {
            if (interceptor.GetState() == ShipState.HANGAR || interceptor.GetState() == ShipState.DAMAGED)
            {
                // Cap fuel and shield regeneration
                float fuelRegen = Mathf.Min(fuelRegenRate * deltaTime / 2, interceptor.MaxFuel - interceptor.Fuel);
                float shieldRegen = Mathf.Min(fuelRegenRate * deltaTime / 2, interceptor.MaxShields - interceptor.Shields);

                interceptor.Fuel += fuelRegen;
                interceptor.Shields += shieldRegen;

                if (interceptor.GetState() == ShipState.DAMAGED && interceptor.Shields >= 100)
                {
                    interceptor.SetState(ShipState.HANGAR);
                }
            }
        }
    }

    /// <summary>
    /// Deploys interceptors when cooldown allows and enemies exist.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    private void HandleDeploy(float deltaTime)
    {
        deployTimer -= deltaTime;
        if (deployTimer <= 0 && hangar.Count(s => s.GetState() != ShipState.HANGAR && s.GetState() != ShipState.DAMAGED) < maxInterceptors)
        {
            var idleInterceptor = hangar.FirstOrDefault(s => s.GetState() == ShipState.HANGAR);
            if (idleInterceptor != null)
            {
                Vector3 worldCenter = transform.position;
                var shieldEmitter = GetComponentInChildren<ShieldRippleEmitter>(true);
                if (shieldEmitter != null) {
                    shieldEmitter.AddHitWorld(worldCenter, 3.0f);
                }
                else
                {
                    Debug.Log("ShieldRippleEmitter not found");
                }
                ;
                idleInterceptor.Deploy(hangarObject.transform.position, this);
                deployTimer = deployCooldown;
                Debug.Log("Deployed an interceptor. Remaining in hangar: " + hangar.Count(s => s.GetState() == ShipState.HANGAR) + "/" + maxInterceptors);
            }
        }
    }

    /// <summary>
    /// Selects target using AdmiralProtection system.
    /// </summary>
    /// <returns>Current target or closest enemy from protection system.</returns>
    protected override SpaceObject SelectTarget()
    {
        if (weapons.Any())
        {
            if (target != null && target.IsAlive()) return target;
            // Select closest enemy
            var enemy = admiralProtection.getTargetForProtectable(this);
            if (enemy != null)
            {
                return enemy;
            }
        }
        return null;
    }

    /// <summary>
    /// Adds interceptor to hangar if space available.
    /// </summary>
    /// <param name="interceptor">Interceptor to add.</param>
    public void AddInterceptor(Interceptor interceptor)
    {
        if (hangar.Count < maxInterceptors) hangar.Add(interceptor);
    }

    /// <summary>
    /// Returns interceptor to pool.
    /// </summary>
    /// <param name="interceptor">Interceptor to return.</param>
    public void ReturnInterceptorToPool(Interceptor interceptor)
    {
        InterceptorPoolManager.Instance.Return(interceptor);
    }

    /// <summary>
    /// Updates HUD elements with current stats.
    /// </summary>
    public void UpdateHud()
    {
        //HpBar.value = Health;
        //ShieldsBar.value = Shields;
        //InterceptorCountText.text = $"Interceptors: {hangar.Count(s => s.GetState() != ShipState.HANGAR && s.GetState() != ShipState.DAMAGED)}/{maxInterceptors}";
    }
}
