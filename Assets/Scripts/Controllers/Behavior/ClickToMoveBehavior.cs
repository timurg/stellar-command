using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(SpaceObject))]
public class ClickToMoveBehavior : MonoBehaviour
{
    private SpaceObject spaceObject;
    private InputAction clickAction;
    private InputAction clickPositionAction;

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private Camera mainCamera;

    [Header("Click Settings")]
    [Tooltip("Минимальное расстояние до цели, чтобы остановиться")]
    public float stoppingDistance = 0.5f;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Tag your camera as 'MainCamera'.");
        }

        spaceObject = GetComponent<SpaceObject>();
        if (spaceObject == null)
        {
            Debug.LogError("SpaceObject component is missing!");
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable(); // Для тача

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found! Add it to the GameObject.");
            return;
        }

        clickAction = playerInput.actions["Click"];
        clickPositionAction = playerInput.actions["ClickPosition"];

        clickAction.performed += OnClickPerformed;
    }

    private void OnDisable()
    {
        clickAction.performed -= OnClickPerformed;
        EnhancedTouchSupport.Disable();
    }

private void OnClickPerformed(InputAction.CallbackContext context)
{
    if (mainCamera == null) return;

    Vector2 screenPos = clickPositionAction.ReadValue<Vector2>();
    
    // 2D Orthographic — самый надёжный способ
    Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane - mainCamera.transform.position.z));
    targetPosition = new Vector3(worldPos.x, worldPos.y, 0);

    // ДЕБАГ: выведем координаты
    Debug.Log($"[CLICK] Screen: {screenPos} → World: {targetPosition} | Ship: {transform.position}");

    hasTarget = true;
}

    private void FixedUpdate()
    {
        if (!hasTarget || spaceObject == null || !spaceObject.IsAlive()) return;

        Vector2 currentPos = transform.position;
        Vector2 toTarget = targetPosition - (Vector3)currentPos;
        float distance = toTarget.magnitude;

        if (distance <= stoppingDistance)
        {
            hasTarget = false;
            spaceObject.Move(Vector2.zero); // Остановка
            return;
        }

        // Только направление — нормализованное
        Vector2 direction = toTarget.normalized;
        spaceObject.Move(direction);
    }

    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        if (hasTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetPosition, stoppingDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}