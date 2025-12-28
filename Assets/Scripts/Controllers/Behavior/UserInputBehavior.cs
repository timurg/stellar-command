using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Keyboard/gamepad input controller for SpaceObject movement.
/// Reads horizontal/vertical input and sets Direction via Move().
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>UserInputBehavior translates player input to movement.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>INPUT SYSTEM: Uses Unity's new Input System.</item>
///   <item>SMOOTH INPUT: Lerps raw input for smooth acceleration.</item>
///   <item>DIRECTION BASED: Calls spaceObject.Move(direction).</item>
///   <item>FIXED UPDATE: Processes input in physics update.</item>
/// </list>
/// <para>Requires PlayerInput component on same GameObject.</para>
/// <para>Alternative: ClickToMoveBehavior for mouse/touch control.</para>
/// </remarks>
[RequireComponent(typeof(UserInputBehavior))]
[RequireComponent(typeof(SpaceObject))]
public class UserInputBehavior : MonoBehaviour
{
    private SpaceObject spa;
    private InputAction moveHorizontal;
    private InputAction moveVertical;
    private InputAction clickAction;
    private InputAction clickPosition;

    private Vector2 smoothInput;

    /// <summary>
    /// Initializes input actions from PlayerInput.
    /// </summary>
    void Start()
    {
        spa = GetComponent<SpaceObject>();
        if (spa == null)
        {
            Debug.LogError("UserInputBehavior requires a SpaceObject component.");
        }
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

    /// <summary>
    /// Processes input and applies movement in physics update.
    /// </summary>
    private void FixedUpdate()
    {
        HandleMovement(Time.fixedDeltaTime);
    }

    /// <summary>
    /// Reads input and calls Move with smoothed direction.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    private void HandleMovement(float deltaTime)
    {
        float rawX = moveHorizontal.ReadValue<float>();
        float rawY = moveVertical.ReadValue<float>();
        smoothInput = Vector2.Lerp(smoothInput, new Vector2(rawX, rawY), 0.1f);
        Vector2 direction = smoothInput.normalized;
        spa.Move(direction);
    }

    void Update()
    {
    }
}