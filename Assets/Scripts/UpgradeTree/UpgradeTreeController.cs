using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controller for the upgrade tree UI panel.
/// Handles opening/closing animation, tree building, and drag scrolling.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>UpgradeTreeController manages the fullscreen upgrade UI.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>ANIMATION: Smooth open/close with scaling and fading.</item>
///   <item>TREE BUILDING: Creates UpgradeNodeUI instances from model.</item>
///   <item>LAYOUT: Organizes nodes in columns by UpgradePath.</item>
///   <item>DRAG: Implements IBeginDragHandler for scroll navigation.</item>
///   <item>UPGRADE: ApplyUpgrade() unlocks dependent nodes.</item>
/// </list>
/// <para>Open via OpenFullscreen(model, material).</para>
/// </remarks>
[RequireComponent(typeof(Canvas))]
public class UpgradeTreeController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Animation")]
    /// <summary>Container for tree content (animated).</summary>
    [SerializeField] private RectTransform treeContainer;
    /// <summary>Background overlay image.</summary>
    [SerializeField] private Image fullscreenOverlay;
    /// <summary>Close button reference.</summary>
    [SerializeField] private Button closeButton;

    [Header("Layout")]
    /// <summary>Content container for spawning nodes.</summary>
    [SerializeField] private RectTransform content;
    /// <summary>ScrollRect for drag navigation.</summary>
    [SerializeField] private ScrollRect scrollRect;
    /// <summary>Prefab for upgrade node UI.</summary>
    [SerializeField] private UpgradeNodeUI nodePrefab;
    /// <summary>Default material for nodes.</summary>
    [SerializeField] private Material defaultMaterial;

    /// <summary>Container for line rendering.</summary>
    [SerializeField] public RectTransform lineRenderContentTransform;

    /// <summary>Content transform for node positioning.</summary>
    public RectTransform ContentTransform => content;

    /// <summary>Canvas transform for coordinate conversion.</summary>
    public RectTransform CanvasTransform => GetComponentInParent<Canvas>().GetComponent<RectTransform>();

    private UpgradeTreeModel currentModel;
    private readonly Dictionary<string, UpgradeNodeUI> nodeById = new();
    private Coroutine animRoutine;
    private bool isOpen;

    /// <summary>
    /// Initializes close button and hides panel.
    /// </summary>
    private void Awake()
    {
        closeButton.onClick.AddListener(CloseWithAnimation);
        treeContainer.anchoredPosition = Vector2.up * 3000f;
        fullscreenOverlay.color = Color.clear;
        gameObject.SetActive(false);

        var canvas = GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 500;
    }

    /// <summary>
    /// Opens upgrade tree panel with animation.
    /// </summary>
    /// <param name="model">Upgrade tree model to display.</param>
    /// <param name="mat">Optional material override.</param>
    public void OpenFullscreen(UpgradeTreeModel model, Material mat = null)
    {
        gameObject.SetActive(true);
        BuildTree(model, mat ?? defaultMaterial);
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(OpenAnimation());
    }

    /// <summary>
    /// Coroutine for opening animation.
    /// </summary>
    private IEnumerator OpenAnimation()
    {
        isOpen = true;
        yield return fullscreenOverlay.Fade(0.88f, 0.6f, EaseType.OutCubic);
        yield return treeContainer.MoveAnchored(Vector2.zero, 0.9f, EaseType.OutBack);
        yield return treeContainer.ScaleTo(Vector3.one * 1.05f, 0.4f);
        yield return treeContainer.ScaleTo(Vector3.one, 0.2f);
    }

    /// <summary>
    /// Starts closing animation.
    /// </summary>
    public void CloseWithAnimation()
    {
        if (!isOpen) return;
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(CloseAnimation());
    }

    /// <summary>
    /// Coroutine for closing animation.
    /// </summary>
    private IEnumerator CloseAnimation()
    {
        yield return treeContainer.ScaleTo(Vector3.one * 1.1f, 0.2f);
        yield return treeContainer.ScaleTo(Vector3.zero, 0.4f, EaseType.InBack);
        yield return treeContainer.MoveAnchored(Vector2.up * 3000f, 0.6f);
        yield return fullscreenOverlay.Fade(0f, 0.7f);
        isOpen = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Builds the upgrade tree UI from model data.
    /// </summary>
    /// <param name="model">Tree model to build.</param>
    /// <param name="overrideMaterial">Optional material override.</param>
    public void BuildTree(UpgradeTreeModel model, Material overrideMaterial = null)
    {
        currentModel = model;
        nodeById.Clear();

        // Clear Content
        foreach (Transform child in content)
            Destroy(child.gameObject);

        Material mat = overrideMaterial ?? defaultMaterial;

        // Adaptive parameters (account for Viewport size)
        float viewportWidth = content.parent.GetComponent<RectTransform>().rect.width;
        float viewportHeight = content.parent.GetComponent<RectTransform>().rect.height;

        float columnSpacing = viewportWidth / 3f;  // for 3 columns
        float verticalSpacing = 220f;
        float startX = -columnSpacing;  // center of first column
        float startY = 0f;  // center Y, with auto-centering below

        float currentX = startX;

        foreach (var path in model.rootPaths)
        {
            float currentY = startY;
            var nodesInPath = new List<UpgradeNodeUI>();

            foreach (var nodeId in path.nodeIds)
            {
                var nodeModel = model.allNodes.Find(n => n.id == nodeId);
                if (nodeModel == null) continue;

                var nodeUI = Instantiate(nodePrefab, content);
                var rt = nodeUI.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);  // center
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localPosition = new Vector3(currentX, currentY, 0f);
                rt.sizeDelta = new Vector2(180f, 180f);

                nodeUI.Initialize(nodeModel, mat, this);
                Debug.Log($"Created node {nodeModel.id} at {rt.localPosition}, visible: {nodeUI.gameObject.activeSelf}");
                nodeById[nodeModel.id] = nodeUI;
                nodesInPath.Add(nodeUI);

                // Child nodes (to the right, centered on parent)
                if (nodeModel.childNodeIds.Count > 0)
                {
                    float childX = currentX + 320f;
                    float childYStep = 180f;
                    float childCenterY = currentY - (nodeModel.childNodeIds.Count - 1) * childYStep / 2f;

                    for (int c = 0; c < nodeModel.childNodeIds.Count; c++)
                    {
                        var childId = nodeModel.childNodeIds[c];
                        var childModel = model.allNodes.Find(n => n.id == childId);
                        if (childModel == null) continue;

                        var childUI = Instantiate(nodePrefab, content);
                        var childRt = childUI.GetComponent<RectTransform>();
                        childRt.anchorMin = childRt.anchorMax = new Vector2(0.5f, 0.5f);
                        childRt.pivot = new Vector2(0.5f, 0.5f);
                        childRt.localPosition = new Vector3(childX, childCenterY + c * childYStep, 0f);
                        childRt.sizeDelta = new Vector2(160f, 160f);

                        childUI.Initialize(childModel, mat, this);
                        nodeById[childModel.id] = childUI;

                        nodeUI.ConnectTo(childUI);  // line
                    }
                }

                currentY -= verticalSpacing;
            }

            ConnectVertical(nodesInPath);  // vertical lines

            currentX += columnSpacing;
        }

        // Auto-center and fit size
        StartCoroutine(CenterAndFitContentNextFrame(viewportWidth, viewportHeight));
    }

    /// <summary>
    /// Centers and fits content after layout update.
    /// </summary>
    private IEnumerator CenterAndFitContentNextFrame(float screenW, float screenH)
    {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        // Tree bounds (based on localPosition of nodes)
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var node in nodeById.Values)
        {
            if (node) bounds.Encapsulate(node.transform.localPosition);
        }

        // Fit content size to bounds + padding
        content.sizeDelta = new Vector2(
            bounds.size.x + 400f,
            bounds.size.y + 600f
        );

        // Center TreeContainer
        float containerWidth = treeContainer.rect.width;
        float containerHeight = treeContainer.rect.height;

        gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();

        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);  // scroll to tree center
    }

    /// <summary>
    /// Connects nodes in path with vertical lines.
    /// </summary>
    private void ConnectVertical(List<UpgradeNodeUI> nodes)
    {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            nodes[i].ConnectTo(nodes[i + 1]);
        }
    }

    /// <summary>
    /// Applies upgrade and unlocks dependent nodes.
    /// </summary>
    /// <param name="node">Node that was upgraded.</param>
    public void ApplyUpgrade(UpgradeNodeModel node)
    {
        foreach (var n in currentModel.allNodes)
        {
            if (n.requiredNodeIds.Contains(node.id))
            {
                n.isUnlocked = true;
                if (nodeById.TryGetValue(n.id, out var ui)) ui.RefreshVisuals();
            }
        }
        node.onUpgrade?.Invoke(node);
    }

    /// <summary>Forwards begin drag to ScrollRect.</summary>
    public void OnBeginDrag(PointerEventData e) => scrollRect?.OnBeginDrag(e);
    /// <summary>Forwards drag to ScrollRect.</summary>
    public void OnDrag(PointerEventData e) => scrollRect?.OnDrag(e);
    /// <summary>Forwards end drag to ScrollRect.</summary>
    public void OnEndDrag(PointerEventData e) => scrollRect?.OnEndDrag(e);
}