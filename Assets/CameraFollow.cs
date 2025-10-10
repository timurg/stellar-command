using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Назначь Carrier
    [SerializeField] private float smoothSpeed = 0.125f; // Плавность следования
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Базовое смещение
    [SerializeField] private float startOrthographicSize = 5f; // Начальный зум (близко)
    [SerializeField] private float targetOrthographicSize = 10f; // Конечный зум (отдаление)
    [SerializeField] private float zoomDuration = 2f; // Длительность анимации (секунд)

    private Camera cam;
    private bool isZooming = true; // Флаг для анимации

    // Public getters for CameraSettings
    public bool IsZooming => isZooming;
    public float TargetOrthographicSize => targetOrthographicSize;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFollow: No Camera component found!");
            return;
        }
        cam.orthographicSize = startOrthographicSize; // Устанавливаем начальный зум
    }

    private void Start()
    {
        StartCoroutine(ZoomOutCoroutine());
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Target not assigned!");
            return;
        }

        if (!isZooming)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

    private IEnumerator ZoomOutCoroutine()
    {
        float elapsedTime = 0f;
        float initialSize = cam.orthographicSize;
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            cam.orthographicSize = Mathf.Lerp(initialSize, targetOrthographicSize, t);
            yield return null; // Ждём следующий кадр
        }
        cam.orthographicSize = targetOrthographicSize; // Устанавливаем финальный зум
        isZooming = false; // Завершаем анимацию
    }
}