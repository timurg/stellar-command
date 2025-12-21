using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Singleton-менеджер тултипа для отображения описания узла прокачки.
/// Sci-Fi стиль: голографическая панель с неоновым текстом и лёгкой анимацией.
/// </summary>
public class UpgradeTooltip : MonoBehaviour
{
    public static UpgradeTooltip Instance { get; private set; }

    [SerializeField] private RectTransform panel;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Material hologramMaterial;

    private Coroutine showRoutine;
    private Coroutine hideRoutine;
    private bool isVisible = false;

    // Кэшируем действие из существующего PlayerInput
    private InputAction pointerPositionAction;
    private PlayerInput playerInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ищем PlayerInput в сцене (один раз)
        playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("[UpgradeTooltip] PlayerInput component not found in scene! Tooltip positioning will not work.");
            return;
        }

        // Берём действие "ClickPosition" по имени (как у тебя уже есть)
        pointerPositionAction = playerInput.actions["ClickPosition"];
        if (pointerPositionAction == null)
        {
            Debug.LogError("[UpgradeTooltip] Input Action 'ClickPosition' not found! Check your Input Action Asset.");
            return;
        }

        // Включаем действие и подписываемся на изменения
        pointerPositionAction.Enable();
        pointerPositionAction.performed += _ => UpdatePositionIfVisible();

        // Изначально скрыто
        if (panel) panel.gameObject.SetActive(false);
        if (backgroundImage && hologramMaterial) backgroundImage.material = hologramMaterial;
    }

    private void OnDestroy()
    {
        if (pointerPositionAction != null)
        {
            pointerPositionAction.performed -= _ => UpdatePositionIfVisible();
            pointerPositionAction.Disable();
        }
    }

    /// <summary>
    /// Показывает тултип с описанием модели.
    /// </summary>
    public void Show(UpgradeNodeModel model)
    {
        if (hideRoutine != null) StopCoroutine(hideRoutine);

        if (descriptionText)
            descriptionText.text = model.description ?? "No description available.";

        if (panel) panel.gameObject.SetActive(true);

        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowAnimation());

        isVisible = true;
        UpdatePositionIfVisible(); // Сразу позиционируем
    }

    private IEnumerator ShowAnimation()
    {
        if (backgroundImage) yield return backgroundImage.Fade(0.95f, 0.3f, EaseType.OutCubic);
        if (panel) yield return panel.ScaleTo(Vector3.one, 0.25f, EaseType.OutBack);
        if (descriptionText) yield return descriptionText.ColorTo(new Color(0f, 1.2f, 2.5f, 1f), 0.4f);
    }

    /// <summary>
    /// Скрывает тултип.
    /// </summary>
    public void Hide()
    {
        if (showRoutine != null) StopCoroutine(showRoutine);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAnimation());
    }

    private IEnumerator HideAnimation()
    {
        if (panel) yield return panel.ScaleTo(Vector3.zero, 0.2f, EaseType.InBack);
        if (backgroundImage) yield return backgroundImage.Fade(0f, 0.3f);
        if (panel) panel.gameObject.SetActive(false);

        isVisible = false;
    }

    // Вызывается из события Input System и из Show()
    private void UpdatePositionIfVisible()
    {
        if (!isVisible || panel == null || pointerPositionAction == null) return;

        Vector2 screenPos = pointerPositionAction.ReadValue<Vector2>();

        RectTransform canvasRect = panel.parent as RectTransform;
        Camera cam = null;
        if (canvasRect && canvasRect.GetComponentInParent<Canvas>()?.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvasRect.GetComponentInParent<Canvas>().worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            cam,
            out Vector2 localPoint
        );

        // Отступ от курсора + защита от выхода за экран (опционально)
        Vector2 offset = new Vector2(20f, -20f);
        panel.anchoredPosition = localPoint + offset;
    }

    // Резервный Update на случай, если действие не сработало (редко)
    private void LateUpdate()
    {
        if (isVisible)
            UpdatePositionIfVisible();
    }
}