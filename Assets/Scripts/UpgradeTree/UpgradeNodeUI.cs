using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;

/// <summary>
/// UI component for a single upgrade node.
/// Handles visuals, interactions, and connections to other nodes.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>UpgradeNodeUI is the visual representation of UpgradeNodeModel.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>VISUALS: Background, title, level text, locked/maxed overlays.</item>
///   <item>INTERACTION: Click to upgrade, hover for tooltip.</item>
///   <item>ANIMATION: Pulse effect when unlocked, scale on click.</item>
///   <item>CONNECTIONS: ConnectTo() draws lines to child nodes.</item>
/// </list>
/// <para>Created by UpgradeTreeController.BuildTree().</para>
/// </remarks>
public class UpgradeNodeUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    /// <summary>Background image.</summary>
    [SerializeField] private Image background;
    /// <summary>Title text display.</summary>
    [SerializeField] private TextMeshProUGUI title;
    /// <summary>Level text display (current/max).</summary>
    [SerializeField] private TextMeshProUGUI levelText;
    /// <summary>Overlay shown when node is locked.</summary>
    [SerializeField] private GameObject lockedOverlay;
    /// <summary>Overlay shown when node is at max level.</summary>
    [SerializeField] private GameObject maxedOverlay;
    /// <summary>Prefab for connection lines.</summary>
    [SerializeField] private UILineRenderer uiLinePrefab;

    private UpgradeNodeModel model;
    private UpgradeTreeController controller;
    private Vector3 originalScale;
    private Coroutine pulseRoutine;
    private Coroutine scaleRoutine;
    private UILineRenderer connectedLine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Initializes node with model data and material.
    /// </summary>
    /// <param name="nodeModel">Model data for this node.</param>
    /// <param name="mat">Material for background.</param>
    /// <param name="treeController">Parent controller reference.</param>
    public void Initialize(UpgradeNodeModel nodeModel, Material mat, UpgradeTreeController treeController)
    {
        model = nodeModel;
        controller = treeController;
        if (background) background.material = mat;
        title.color = Color.black;
        RefreshVisuals();
    }

    /// <summary>
    /// Updates visuals based on current model state.
    /// </summary>
    public void RefreshVisuals()
    {
        if (title) title.text = model.title;
        if (levelText) levelText.text = $"{model.currentLevel}/{model.maxLevel}";
        if (lockedOverlay) lockedOverlay.SetActive(!model.isUnlocked);
        if (maxedOverlay) maxedOverlay.SetActive(model.currentLevel >= model.maxLevel);
        if (model.isUnlocked && pulseRoutine == null && background)
            pulseRoutine = StartCoroutine(PulseNeon());
    }

    /// <summary>
    /// Coroutine for neon pulse animation.
    /// </summary>
    private IEnumerator PulseNeon()
    {
        Color neonBright = new Color(0f, 1.2f, 2.5f, 0.9f);
        Color neonDim = new Color(0f, 0.8f, 1.8f, 0.7f);
        while (true)
        {
            yield return background.ColorTo(neonBright, 0.8f);
            yield return background.ColorTo(neonDim, 0.8f);
        }
    }

    /// <summary>
    /// Handles click to upgrade node.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (model.isUnlocked && model.currentLevel < model.maxLevel)
        {
            model.currentLevel++;
            controller.ApplyUpgrade(model);
            RefreshVisuals();
            if (scaleRoutine != null) StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(FlashEffect());
        }
    }

    /// <summary>
    /// Coroutine for upgrade flash animation.
    /// </summary>
    private IEnumerator FlashEffect()
    {
        yield return transform.ScaleTo(Vector3.one * 1.4f, 0.15f);
        yield return transform.ScaleTo(originalScale, 0.15f);
    }

    /// <summary>
    /// Handles pointer down - scales node.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(transform.ScaleTo(originalScale * 1.15f, 0.1f));
    }

    /// <summary>
    /// Handles pointer up - restores scale.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(transform.ScaleTo(originalScale, 0.2f, EaseType.OutBack));
    }

    /// <summary>
    /// Shows tooltip on hover.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        UpgradeTooltip.Instance?.Show(model);
    }

    /// <summary>
    /// Hides tooltip on exit.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        UpgradeTooltip.Instance?.Hide();
    }

    /// <summary>
    /// Connects this node to another with a line.
    /// </summary>
    /// <param name="next">Target node to connect to.</param>
    public void ConnectTo(UpgradeNodeUI next)
    {
        if (uiLinePrefab == null || next == null) return;

        var lineObj = Instantiate(uiLinePrefab, controller.lineRenderContentTransform);
        var uiLine = lineObj.GetComponent<UILineRenderer>();
        uiLine.Points = new Vector2[] { Vector2.zero, next.transform.localPosition - transform.localPosition };
        uiLine.color = new Color(0f, 2f, 3f, 1f);
        uiLine.material = background.material;
        connectedLine = uiLine;

        UpdateLinePosition(next);
    }

    private void LateUpdate()
    {
        if (connectedLine != null)
        {
            UpdateLinePosition(connectedLine.GetComponent<UpgradeNodeUI>());
        }
    }

    /// <summary>
    /// Updates line endpoint positions.
    /// </summary>
    private void UpdateLinePosition(UpgradeNodeUI next)
    {
        if (next == null) return;
        connectedLine.Points = new Vector2[] { transform.localPosition, next.transform.localPosition };
    }
}