using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Animations;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SpriteRenderer))]
public class Carrier : Ship
{
    

    [SerializeField] private int maxInterceptors = 5; // Максимум истребителей
    [SerializeField] private GameObject interceptorPrefab; // Prefab Interceptor
    [SerializeField] private float fuelRegenRate = 20f; // Реген топлива/щитов в ангаре
    [SerializeField] private float deployCooldown = 1f; // Кулдаун деплой
    [SerializeField] private GameObject hangarObject; // Объект ангарной точки
    

    private List<Interceptor> hangar = new List<Interceptor>(); // Ангар для истребителей
    private float deployTimer = 0f;

    private InputAction moveHorizontal;
    private InputAction moveVertical;
    private InputAction clickAction;
    private InputAction clickPosition;
    private Vector2 smoothInput;

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

        // Инициализация ангара (предварительный спавн Interceptor из Prefab)
        for (int i = 0; i < maxInterceptors; i++)
        {
            GameObject interceptorObj = Instantiate(interceptorPrefab, transform.position, Quaternion.identity);
            Interceptor interceptor = interceptorObj.GetComponent<Interceptor>();
            if (interceptor != null)
            {
                interceptor.SetState(ShipState.HANGAR); // Начальное состояние в ангаре
                interceptor.gameObject.SetActive(false); // Неактивен до деплой
                hangar.Add(interceptor);
            }
            else
            {
                Debug.LogError("Interceptor component not found on Prefab!");
            }
        }
    }

    new private void FixedUpdate()
    {
        base.FixedUpdate();
        HandleMovement(Time.fixedDeltaTime);
        HandleHangar(Time.fixedDeltaTime);
        HandleDeploy(Time.fixedDeltaTime);
    }

    private void HandleMovement(float deltaTime)
    {
        float rawX = moveHorizontal.ReadValue<float>();
        float rawY = moveVertical.ReadValue<float>();
        smoothInput = Vector2.Lerp(smoothInput, new Vector2(rawX, rawY), 0.1f);
        Vector2 direction = smoothInput.normalized; // Преобразуем в направление
        Move(direction); // Вызываем Move с направлением
        if (direction.magnitude > 0)
        {
            Debug.Log($"Applying force: {direction * acceleration}, Velocity: {Rigidbody.linearVelocity}");
        }
    }

    private void HandleHangar(float deltaTime)
    {
        foreach (var interceptor in hangar)
        {
            if (interceptor.GetState() == ShipState.HANGAR || interceptor.GetState() == ShipState.DAMAGED)
            {
                interceptor.Fuel = Mathf.Min(100, interceptor.Fuel + fuelRegenRate * deltaTime);
                interceptor.Shields = Mathf.Min(100, interceptor.Shields + fuelRegenRate * deltaTime / 2);
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
                idleInterceptor.Deploy(hangarObject.transform.position, this); // Вызов деплой
                deployTimer = deployCooldown;
                Debug.Log("Deployed an interceptor. Remaining in hangar: " + hangar.Count(s => s.GetState() == ShipState.HANGAR) + "/" + maxInterceptors);
            }
        }
    }

    protected override Entity SelectTarget()
    {
        return null; // Carrier не выбирает цель
    }


    public void AddInterceptor(Interceptor interceptor)
    {
        if (hangar.Count < maxInterceptors) hangar.Add(interceptor);
    }
}