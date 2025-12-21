using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class UpgradeTreeController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Animation")]
    [SerializeField] private RectTransform treeContainer;
    [SerializeField] private Image fullscreenOverlay;
    [SerializeField] private Button closeButton;

    [Header("Layout")]
    [SerializeField] private RectTransform content; // ← сюда всё спавнится
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private UpgradeNodeUI nodePrefab;
    [SerializeField] private Material defaultMaterial;

    [SerializeField] public RectTransform lineRenderContentTransform;

    public RectTransform ContentTransform => content;

    public RectTransform CanvasTransform => GetComponentInParent<Canvas>().GetComponent<RectTransform>();

    private UpgradeTreeModel currentModel;
    private readonly Dictionary<string, UpgradeNodeUI> nodeById = new();
    private Coroutine animRoutine;
    private bool isOpen;

    private void Awake()
    {
        closeButton.onClick.AddListener(CloseWithAnimation);
        treeContainer.anchoredPosition = Vector2.up * 3000f;
        fullscreenOverlay.color = Color.clear;
        gameObject.SetActive(false);

        // Важно: делаем Canvas overlay и высоким
        var canvas = GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 500;
    }

    public void OpenFullscreen(UpgradeTreeModel model, Material mat = null)
    {
        gameObject.SetActive(true);
        BuildTree(model, mat ?? defaultMaterial);
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(OpenAnimation());
    }

    private IEnumerator OpenAnimation()
    {
        isOpen = true;
        yield return fullscreenOverlay.Fade(0.88f, 0.6f, EaseType.OutCubic);
        yield return treeContainer.MoveAnchored(Vector2.zero, 0.9f, EaseType.OutBack);
        yield return treeContainer.ScaleTo(Vector3.one * 1.05f, 0.4f);
        yield return treeContainer.ScaleTo(Vector3.one, 0.2f);
    }

    public void CloseWithAnimation()
    {
        if (!isOpen) return;
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(CloseAnimation());
    }

    private IEnumerator CloseAnimation()
    {
        yield return treeContainer.ScaleTo(Vector3.one * 1.1f, 0.2f);
        yield return treeContainer.ScaleTo(Vector3.zero, 0.4f, EaseType.InBack);
        yield return treeContainer.MoveAnchored(Vector2.up * 3000f, 0.6f);
        yield return fullscreenOverlay.Fade(0f, 0.7f);
        isOpen = false;
        gameObject.SetActive(false);
    }

    public void BuildTree(UpgradeTreeModel model, Material overrideMaterial = null)
    {
        currentModel = model;
        nodeById.Clear();

        // Очищаем Content
        foreach (Transform child in content)
            Destroy(child.gameObject);

        Material mat = overrideMaterial ?? defaultMaterial;

        // Адаптивные параметры (учитываем размер Viewport)
        float viewportWidth = content.parent.GetComponent<RectTransform>().rect.width;
        float viewportHeight = content.parent.GetComponent<RectTransform>().rect.height;

        float columnSpacing = viewportWidth / 3f;  // для 3 колонок
        float verticalSpacing = 220f;
        float startX = -columnSpacing;  // центр первой колонки
        float startY = 0f;  // центр Y, с авто-центрированием ниже

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
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);  // центр
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localPosition = new Vector3(currentX, currentY, 0f);
                rt.sizeDelta = new Vector2(180f, 180f);

                nodeUI.Initialize(nodeModel, mat, this);
                Debug.Log($"Created node {nodeModel.id} at {rt.localPosition}, visible: {nodeUI.gameObject.activeSelf}");
                nodeById[nodeModel.id] = nodeUI;
                nodesInPath.Add(nodeUI);

                // Дочерние узлы (вправо, выравнивание по центру родителя)
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

                        nodeUI.ConnectTo(childUI);  // линия
                    }
                }

                currentY -= verticalSpacing;
            }

            ConnectVertical(nodesInPath);  // вертикальные линии

            currentX += columnSpacing;
        }

        // Авто-центрирование и подгонка размера
        StartCoroutine(CenterAndFitContentNextFrame(viewportWidth, viewportHeight));
    }

    private IEnumerator CenterAndFitContentNextFrame(float screenW, float screenH)
    {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        // Границы дерева (на основе localPosition узлов)
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var node in nodeById.Values)
        {
            if (node) bounds.Encapsulate(node.transform.localPosition);
        }

        // Подгоняем размер content под границы + отступы
        content.sizeDelta = new Vector2(
            bounds.size.x + 400f,
            bounds.size.y + 600f
        );

        // Центрируем TreeContainer по твоей формуле
        float containerWidth = treeContainer.rect.width;
        float containerHeight = treeContainer.rect.height;

        gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        /*
        treeContainer.right = new Vector3(
            (screenW / 2f) - (containerWidth / 2f),   // X: половина экрана минус половина контейнера
            (screenH / 2f) - (containerHeight / 2f),  // Y: половина экрана минус половина контейнера
            0f
        );
*/
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();

        scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);  // скролл в центр дерева
    }
    private void ConnectVertical(List<UpgradeNodeUI> nodes)
    {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            nodes[i].ConnectTo(nodes[i + 1]);
        }
    }

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

    public void OnBeginDrag(PointerEventData e) => scrollRect?.OnBeginDrag(e);
    public void OnDrag(PointerEventData e) => scrollRect?.OnDrag(e);
    public void OnEndDrag(PointerEventData e) => scrollRect?.OnEndDrag(e);
}