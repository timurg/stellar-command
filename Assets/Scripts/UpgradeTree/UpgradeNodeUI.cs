using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;

/// <summary>
/// UI-компонент для одного узла прокачки.
/// </summary>
public class UpgradeNodeUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject maxedOverlay;
    [SerializeField] private UILineRenderer uiLinePrefab; // Prefab для линий

    private UpgradeNodeModel model;
    private UpgradeTreeController controller;
    private Vector3 originalScale;
    private Coroutine pulseRoutine;
    private Coroutine scaleRoutine;
    private UILineRenderer connectedLine; // Для обновления позиции

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Initialize(UpgradeNodeModel nodeModel, Material mat, UpgradeTreeController treeController)
    {
        model = nodeModel;
        controller = treeController;
        if (background) background.material = mat;
        title.color = Color.black; // Для видимости
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        if (title) title.text = model.title;
        if (levelText) levelText.text = $"{model.currentLevel}/{model.maxLevel}";
        if (lockedOverlay) lockedOverlay.SetActive(!model.isUnlocked);
        if (maxedOverlay) maxedOverlay.SetActive(model.currentLevel >= model.maxLevel);
        if (model.isUnlocked && pulseRoutine == null && background)
            pulseRoutine = StartCoroutine(PulseNeon());
    }

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

    private IEnumerator FlashEffect()
    {
        yield return transform.ScaleTo(Vector3.one * 1.4f, 0.15f);
        yield return transform.ScaleTo(originalScale, 0.15f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(transform.ScaleTo(originalScale * 1.15f, 0.1f));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(transform.ScaleTo(originalScale, 0.2f, EaseType.OutBack));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpgradeTooltip.Instance?.Show(model);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpgradeTooltip.Instance?.Hide();
    }

    /// <summary>
    /// Соединяет с следующим узлом (создаёт линию под content для адаптивности).
    /// </summary>
    public void ConnectTo(UpgradeNodeUI next)
    {
        if (uiLinePrefab == null || next == null) return;

        // Создаём линию под content (родителем дерева), чтобы избежать scale искажений
        var lineObj = Instantiate(uiLinePrefab, controller.lineRenderContentTransform); // под content
        var uiLine = lineObj.GetComponent<UILineRenderer>();
        uiLine.Points = new Vector2[] { Vector2.zero, next.transform.localPosition - transform.localPosition }; // относительные позиции
        uiLine.color = new Color(0f, 2f, 3f, 1f); // или градиент через List<Color>
        uiLine.material = background.material; // для glow
        connectedLine = uiLine;
        //uiLine.thickness = 4f;

        UpdateLinePosition(next); // Инициализация позиции
    }

    private void LateUpdate()
    {
        if (connectedLine != null)
        {
            // Обновляем позицию линии при перемещении/resize (адаптивно)
            UpdateLinePosition(connectedLine.GetComponent<UpgradeNodeUI>()); // Замени на next узел
        }
    }

    private void UpdateLinePosition(UpgradeNodeUI next)
    {
        if (next == null) return;
        connectedLine.Points = new Vector2[] { transform.localPosition,next.transform.localPosition };
    }
}