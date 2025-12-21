using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Carrier : Ship
{
    [SerializeField] private int maxInterceptors = 5; // Максимум истребителей
    [SerializeField] private float fuelRegenRate = 2f; // Реген топлива/щитов в ангаре
    [SerializeField] private float deployCooldown = 1f; // Кулдаун деплой
    [SerializeField] public GameObject hangarObject; // Объект ангарной точки
    [SerializeField] private Slider HpBar; // Полоса здоровья
    [SerializeField] private Slider ShieldsBar; // Полоса щитов
    [SerializeField] private Text InterceptorCountText; // Текстовое поле для отображения количества истребителей


    private UpgradeTreeModel UpgradeTreeModel;

    public UpgradeTreeModel CreateUpgradeTreeModel(){
        var model = new UpgradeTreeModel { treeName = "Carrier" };

        // === Все узлы ===
        var hp = new UpgradeNodeModel { id = "hp", title = "ХП", maxLevel = 5, isUnlocked = true, description = "+100 HP за уровень" };
        var shields = new UpgradeNodeModel { id = "shields", title = "Щиты", maxLevel = 3, requiredNodeIds = { "hp" } };
        var guns = new UpgradeNodeModel { id = "guns", title = "Турели", maxLevel = 1 };
        var intGun = new UpgradeNodeModel { id = "int_gun", title = "Interceptor Gun", maxLevel = 4, requiredNodeIds = { "guns" } };
        var fleet = new UpgradeNodeModel { id = "fleet", title = "Флот", maxLevel = 3 };
        var speed = new UpgradeNodeModel { id = "speed", title = "Скорость", maxLevel = 5, requiredNodeIds = { "fleet" } };

        model.allNodes.AddRange(new[] { hp, shields, guns, intGun, fleet, speed });

        // === Пути ===
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

    private readonly List<Interceptor> hangar = new(); // Список для логики ангара (не для Instantiate)
    private float deployTimer = 0f;

    private AdmiralProtection admiralProtection;
    

    new protected void Start()
    {
        base.Start();
        // Пуллинг: заполняем ангар из пула
        InitxInterceptors();
        
        //ShieldsBar.maxValue = maxShields;
        //HpBar.maxValue = maxHealth;
    }

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
    /*
    protected override void UpdateRotation()
    {
        if (rotateToDirection)
        {
            if (Rigidbody.linearVelocity.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(Rigidbody.linearVelocity.y, Rigidbody.linearVelocity.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), 0.2f);
            }
            else
            {
                // Если почти не движется — нос вверх
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, 0f), 0.2f);
            }
        }
    }
*/
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

    new private void FixedUpdate()
    {
        base.FixedUpdate();
        //HandleMovement(Time.fixedDeltaTime);
        HandleHangar(Time.fixedDeltaTime);
        HandleDeploy(Time.fixedDeltaTime);
        UpdateHud();
        ShootAtTarget();
    }

    private void HandleHangar(float deltaTime)
    {
        foreach (var interceptor in hangar)
        {
            if (interceptor.GetState() == ShipState.HANGAR || interceptor.GetState() == ShipState.DAMAGED)
            {
                // Ограничиваем регенерацию топлива и щитов
                float fuelRegen = Mathf.Min(fuelRegenRate * deltaTime / 2, interceptor.MaxFuel - interceptor.Fuel);
                float shieldRegen = Mathf.Min(fuelRegenRate * deltaTime / 2, interceptor.MaxShields - interceptor.Shields);

                interceptor.Fuel += fuelRegen;
                interceptor.Shields += shieldRegen;

                // Логируем для отладки
                //Debug.Log($"Interceptor {interceptor.name}: Fuel {interceptor.Fuel}/{interceptor.MaxFuel}, Shields {interceptor.Shields}/{interceptor.MaxShields}");

                if (interceptor.GetState() == ShipState.DAMAGED && interceptor.Shields >= 100)
                {
                    interceptor.SetState(ShipState.HANGAR);
                }
            }
        }
    }

    private void HandleDeploy(float deltaTime)
    {
        deployTimer -= deltaTime;
        if (deployTimer <= 0 && hangar.Count(s => s.GetState() != ShipState.HANGAR && s.GetState() != ShipState.DAMAGED) < maxInterceptors)
        {
            var idleInterceptor = hangar.FirstOrDefault(s => s.GetState() == ShipState.HANGAR);
            if (idleInterceptor != null)
            {
                idleInterceptor.Deploy(hangarObject.transform.position, this);
                deployTimer = deployCooldown;
                Debug.Log("Deployed an interceptor. Remaining in hangar: " + hangar.Count(s => s.GetState() == ShipState.HANGAR) + "/" + maxInterceptors);
            }
        }
    }

    protected override SpaceObject SelectTarget()
    {
        if (weapons.Any())
        {
            if (target != null && target.IsAlive()) return target;
            // Выбираем ближайшего врага
            var enemy = admiralProtection.getTargetForProtectable(this);
            if (enemy != null)
            {
                return enemy;
            }

        }
        return null;
    }


    public void AddInterceptor(Interceptor interceptor)
    {
        if (hangar.Count < maxInterceptors) hangar.Add(interceptor);
    }

    public void ReturnInterceptorToPool(Interceptor interceptor)
    {
        InterceptorPoolManager.Instance.Return(interceptor);
    }

    public void UpdateHud()
    {
        //HpBar.value = Health;
        //ShieldsBar.value = Shields;
        //InterceptorCountText.text = $"Interceptors: {hangar.Count(s => s.GetState() != ShipState.HANGAR && s.GetState() != ShipState.DAMAGED)}/{maxInterceptors}";
    }
}
