using UnityEngine;
using UnityEngine.InputSystem;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    private void FixedUpdate()
    {
        HandleMovement(Time.fixedDeltaTime);
    }
    private void HandleMovement(float deltaTime)
    {

        float rawX = moveHorizontal.ReadValue<float>();
        float rawY = moveVertical.ReadValue<float>();
        smoothInput = Vector2.Lerp(smoothInput, new Vector2(rawX, rawY), 0.1f);
        Vector2 direction = smoothInput.normalized; // Преобразуем в направление
        spa.Move(direction); // Вызываем Move с направлением
    }

    // Update is called once per frame
    void Update()
    {

    }
}
