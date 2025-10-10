using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSettings : MonoBehaviour
{
    private Camera cam;
    private CameraFollow cameraFollow;
    private float lastHeight = 0f;
    private float referenceHeight = 1080f; // Reference resolution height (e.g., 1080p)
    private const float PPI_ADJUSTMENT = 100f; // Keep for compatibility, but adjust logic

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cameraFollow = GetComponent<CameraFollow>();
        if (cam == null)
        {
            Debug.LogError("CameraSettings: No Camera component found! Disabling script.");
            enabled = false;
            return;
        }
        if (cameraFollow == null)
        {
            Debug.LogError("CameraSettings: No CameraFollow component found! Disabling script.");
            enabled = false;
            return;
        }
        lastHeight = Screen.height;
        // Initial size is handled by CameraFollow, so no need to set orthographicSize here
    }

    private void Update()
    {
        if (Mathf.Abs(Screen.height - lastHeight) > 10f) // Check for significant resolution change
        {
            UpdateOrthographicSize();
        }
    }

    private void UpdateOrthographicSize()
    {
        lastHeight = Screen.height;

        // Only adjust orthographicSize if CameraFollow is not zooming
        if (cameraFollow != null && !cameraFollow.IsZooming) // Assumes IsZooming getter in CameraFollow
        {
            // Scale targetOrthographicSize based on resolution relative to reference height
            float scaleFactor = Screen.height / referenceHeight;
            float baseOrthographicSize = cameraFollow.TargetOrthographicSize; // Assumes getter in CameraFollow
            cam.orthographicSize = baseOrthographicSize * scaleFactor;

            Debug.Log($"Camera orthographicSize updated to: {cam.orthographicSize} for height: {lastHeight}");
        }
    }
}