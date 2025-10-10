using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SpriteRenderer))]
public class Carrier : Ship
{
    

    [SerializeField] private int maxInterceptors = 5; // Максимум истребителей
    [SerializeField] private float fuelRegenRate = 20f; // Реген топлива/щитов в ангаре
    [SerializeField] private float deployCooldown = 1f; // Кулдаун деплой
    [SerializeField] public GameObject hangarObject; // Объект ангарной точки
    [SerializeField] private Slider HpBar; // Полоса здоровья
    [SerializeField] private Slider ShieldsBar; // Полоса щитов
    [SerializeField] private Text InterceptorCountText; // Текстовое поле для отображения количества истребителей

    private readonly List<Interceptor> hangar = new(); // Список для логики ангара (не для Instantiate)
    private float deployTimer = 0f;

    private InputAction moveHorizontal;
    private InputAction moveVertical;
    private InputAction clickAction;
    private InputAction clickPosition;
    private Vector2 smoothInput;

    new protected void Start()
    {
        base.Start();
        // Пуллинг: заполняем ангар из пула
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
        ShieldsBar.maxValue = maxShields;
        HpBar.maxValue = maxHealth;
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
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found on Carrier! Please add it.");
            return;
        }
        moveHorizontal = playerInput.actions["MoveHorizontal"];
        moveVertical = playerInput.actions["MoveVertical"];
        clickAction = playerInput.actions["Click"];
        clickPosition = playerInput.actions["ClickPosition"];

        
    }

    new private void FixedUpdate()
    {
        base.FixedUpdate();
        HandleMovement(Time.fixedDeltaTime);
        HandleHangar(Time.fixedDeltaTime);
        HandleDeploy(Time.fixedDeltaTime);
        UpdateHud();
        ShootAtTarget();
    }

    private void HandleMovement(float deltaTime)
    {
        float rawX = moveHorizontal.ReadValue<float>();
        float rawY = moveVertical.ReadValue<float>();
        smoothInput = Vector2.Lerp(smoothInput, new Vector2(rawX, rawY), 0.1f);
        Vector2 direction = smoothInput.normalized; // Преобразуем в направление
        Move(direction); // Вызываем Move с направлением
    }

    private void HandleHangar(float deltaTime)
    {
        foreach (var interceptor in hangar)
        {
            if (interceptor.GetState() == ShipState.HANGAR || interceptor.GetState() == ShipState.DAMAGED)
            {
                interceptor.Fuel = Mathf.Min(interceptor.MaxFuel, interceptor.Fuel + fuelRegenRate * deltaTime);
                //Debug.Log($"Regenerating interceptor fuel. Current fuel: {interceptor.Fuel}/{interceptor.MaxFuel}");
                interceptor.Shields = Mathf.Min(interceptor.MaxShields, interceptor.Shields + fuelRegenRate * deltaTime / 2);
                if (interceptor.GetState() == ShipState.DAMAGED && interceptor.Shields >= 100) interceptor.SetState(ShipState.HANGAR);
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
                //Debug.Log("Deployed an interceptor. Remaining in hangar: " + hangar.Count(s => s.GetState() == ShipState.HANGAR) + "/" + maxInterceptors);
            }
        }
    }

    protected override SpaceObject SelectTarget()
    {
        if (weapons.Any())
        {
            if (target != null && target.IsAlive()) return target;
            // Выбираем ближайшего врага
            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            SpaceObject closestTarget = null;
            float closestDistance = Mathf.Infinity;
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive())
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = enemy;
                    }
                }
            }
            return closestTarget;
        }
        else
        {
            return null; // Carrier не выбирает цель
        }
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
        HpBar.value = Health;
        ShieldsBar.value = Shields;
        InterceptorCountText.text = $"Interceptors: {hangar.Count(s => s.GetState() != ShipState.HANGAR && s.GetState() != ShipState.DAMAGED)}/{maxInterceptors}";
    }
}
