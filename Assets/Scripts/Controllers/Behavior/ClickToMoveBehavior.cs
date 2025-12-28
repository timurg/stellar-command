using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

/// <summary>
/// Mouse/touch click-to-move controller for SpaceObject movement.
/// Moves ship toward clicked position until reaching stopping distance.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>ClickToMoveBehavior provides point-and-click movement.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>INPUT SYSTEM: Uses Unity's new Input System with touch support.</item>
///   <item>TARGET BASED: Stores target position, moves toward it.</item>
///   <item>STOPPING: Stops when within stoppingDistance of target.</item>
///   <item>DIRECTION BASED: Calls spaceObject.Move(direction).</item>
///   <item>GIZMOS: Visualizes target in editor.</item>
/// </list>
/// <para>Alternative: UserInputBehavior for keyboard/gamepad control.</para>
/// </remarks>
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
    /// <summary>Distance at which ship stops moving toward target.</summary>
    [Tooltip("Minimum distance to target to stop")]
    public float stoppingDistance = 0.5f;

    /// <summary>
    /// Initializes camera and component references.
    /// </summary>
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

    /// <summary>
    /// Enables touch support and subscribes to click events.
    /// </summary>
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();

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

    /// <summary>
    /// Unsubscribes from click events and disables touch support.
    /// </summary>
    private void OnDisable()
    {
        clickAction.performed -= OnClickPerformed;
        EnhancedTouchSupport.Disable();
    }

    /// <summary>
    /// Handles click input - converts screen position to world target.
    /// </summary>
    /// <param name="context">Input callback context.</param>
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (mainCamera == null) return;

        Vector2 screenPos = clickPositionAction.ReadValue<Vector2>();
        
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane - mainCamera.transform.position.z));
        targetPosition = new Vector3(worldPos.x, worldPos.y, 0);

        Debug.Log($"[CLICK] Screen: {screenPos} → World: {targetPosition} | Ship: {transform.position}");

        hasTarget = true;
    }

    /// <summary>
    /// Moves toward target in physics update.
    /// </summary>
    private void FixedUpdate()
    {
        if (!hasTarget || spaceObject == null || !spaceObject.IsAlive()) return;

        Vector2 currentPos = transform.position;
        Vector2 toTarget = targetPosition - (Vector3)currentPos;
        float distance = toTarget.magnitude;

        if (distance <= stoppingDistance)
        {
            hasTarget = false;
            spaceObject.Move(Vector2.zero);
            return;
        }

        Vector2 direction = toTarget.normalized;
        spaceObject.Move(direction);
    }

    /// <summary>
    /// Draws target visualization in editor.
    /// </summary>
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