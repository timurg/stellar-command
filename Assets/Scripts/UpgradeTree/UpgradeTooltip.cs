using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Singleton tooltip manager for upgrade node descriptions.
/// Displays holographic panel with neon text and smooth animations.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>UpgradeTooltip is a singleton that manages hover tooltips for upgrade nodes.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>SINGLETON: Instance property, DontDestroyOnLoad</item>
///   <item>INPUT: Uses PlayerInput "ClickPosition" action for cursor tracking</item>
///   <item>ANIMATION: Show/Hide coroutines with scale and fade effects</item>
///   <item>POSITIONING: Follows cursor with offset, stays within canvas</item>
/// </list>
/// <para>Called by UpgradeNodeUI.OnPointerEnter/Exit.</para>
/// </remarks>
public class UpgradeTooltip : MonoBehaviour
{
    /// <summary>Singleton instance.</summary>
    public static UpgradeTooltip Instance { get; private set; }

    /// <summary>Tooltip panel RectTransform.</summary>
    [SerializeField] private RectTransform panel;
    /// <summary>Description text component.</summary>
    [SerializeField] private TextMeshProUGUI descriptionText;
    /// <summary>Background image for hologram effect.</summary>
    [SerializeField] private Image backgroundImage;
    /// <summary>Material for hologram visual effect.</summary>
    [SerializeField] private Material hologramMaterial;

    private Coroutine showRoutine;
    private Coroutine hideRoutine;
    private bool isVisible = false;

    // Cached input action from existing PlayerInput
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

        // Find PlayerInput in scene (once)
        playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("[UpgradeTooltip] PlayerInput component not found in scene! Tooltip positioning will not work.");
            return;
        }

        // Get "ClickPosition" action by name
        pointerPositionAction = playerInput.actions["ClickPosition"];
        if (pointerPositionAction == null)
        {
            Debug.LogError("[UpgradeTooltip] Input Action 'ClickPosition' not found! Check your Input Action Asset.");
            return;
        }

        // Enable action and subscribe to changes
        pointerPositionAction.Enable();
        pointerPositionAction.performed += _ => UpdatePositionIfVisible();

        // Initially hidden
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
    /// Shows tooltip with model description.
    /// </summary>
    /// <param name="model">Upgrade node model to display.</param>
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

    /// <summary>
    /// Coroutine for show animation with fade and scale.
    /// </summary>
    private IEnumerator ShowAnimation()
    {
        if (backgroundImage) yield return backgroundImage.Fade(0.95f, 0.3f, EaseType.OutCubic);
        if (panel) yield return panel.ScaleTo(Vector3.one, 0.25f, EaseType.OutBack);
        if (descriptionText) yield return descriptionText.ColorTo(new Color(0f, 1.2f, 2.5f, 1f), 0.4f);
    }

    /// <summary>
    /// Hides tooltip with animation.
    /// </summary>
    public void Hide()
    {
        if (showRoutine != null) StopCoroutine(showRoutine);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAnimation());
    }

    /// <summary>
    /// Coroutine for hide animation with scale and fade.
    /// </summary>
    private IEnumerator HideAnimation()
    {
        if (panel) yield return panel.ScaleTo(Vector3.zero, 0.2f, EaseType.InBack);
        if (backgroundImage) yield return backgroundImage.Fade(0f, 0.3f);
        if (panel) panel.gameObject.SetActive(false);

        isVisible = false;
    }

    /// <summary>
    /// Updates panel position to follow cursor.
    /// Called from Input System event and Show().
    /// </summary>
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

        // Offset from cursor + screen bounds protection
        Vector2 offset = new Vector2(20f, -20f);
        panel.anchoredPosition = localPoint + offset;
    }

    /// <summary>
    /// Backup position update in LateUpdate.
    /// </summary>
    private void LateUpdate()
    {
        if (isVisible)
            UpdatePositionIfVisible();
    }
}