using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSettings : MonoBehaviour
{
    private Camera cam;
    private float lastHeight = 0f;
    private const float PPI_ADJUSTMENT = 100f; // Константа для PPI (пример для 1080p)

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraSettings: No Camera component found! Disabling script.");
            enabled = false;
            return;
        }
        UpdateOrthographicSize(); // Инициализация размера
    }

    private void Update()
    {
        if (Mathf.Abs(Screen.height - lastHeight) > 10f) // Минимальный порог 10 пикселей
        {
            UpdateOrthographicSize();
        }
    }

    private void UpdateOrthographicSize()
    {
        lastHeight = Screen.height;
        cam.orthographicSize = lastHeight / (2f * PPI_ADJUSTMENT); // Половина высоты в мировых единицах
        Debug.Log($"Camera orthographicSize updated to: {cam.orthographicSize} for height: {lastHeight}");
    }

    private void OnEnable()
    {
        //ScreenResolutionChanged += OnResolutionChanged; // Исправлено на ScreenResolutionChanged
    }

    private void OnDisable()
    {
       // ScreenResolutionChanged -= OnResolutionChanged; // Убедись, что отписываемся
    }

    private void OnResolutionChanged(Resolution resolution)
    {
        UpdateOrthographicSize();
    }
}