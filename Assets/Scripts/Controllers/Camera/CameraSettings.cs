using UnityEngine;

/// <summary>
/// Adjusts camera orthographic size based on screen resolution.
/// Works with CameraFollow for resolution-responsive camera.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>CameraSettings handles resolution-based camera scaling.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>RESOLUTION SCALING: Adjusts orthographicSize based on screen height.</item>
///   <item>REFERENCE HEIGHT: Scales relative to 1080p reference.</item>
///   <item>INTEGRATION: Waits for CameraFollow zoom to complete before adjusting.</item>
/// </list>
/// <para>Automatically updates when screen resolution changes.</para>
/// </remarks>
[RequireComponent(typeof(Camera), typeof(CameraFollow))]
public class CameraSettings : MonoBehaviour
{
    private Camera cam;
    private CameraFollow cameraFollow;
    private float lastHeight = 0f;
    
    /// <summary>Reference screen height for scaling (e.g., 1080p).</summary>
    [SerializeField] private float referenceHeight = 1080f;
    
    [SerializeField] private const float PPI_ADJUSTMENT = 100f;

    /// <summary>
    /// Initializes references to Camera and CameraFollow.
    /// </summary>
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
    }

    /// <summary>
    /// Checks for resolution changes and updates camera size.
    /// </summary>
    private void Update()
    {
        if (Mathf.Abs(Screen.height - lastHeight) > 10f)
        {
            UpdateOrthographicSize();
        }
    }

    /// <summary>
    /// Updates orthographic size based on current resolution.
    /// Only adjusts after CameraFollow zoom animation completes.
    /// </summary>
    private void UpdateOrthographicSize()
    {
        lastHeight = Screen.height;

        if (cameraFollow != null && !cameraFollow.IsZooming)
        {
            float scaleFactor = Screen.height / referenceHeight;
            float baseOrthographicSize = cameraFollow.TargetOrthographicSize;
            cam.orthographicSize = baseOrthographicSize * scaleFactor;

            Debug.Log($"Camera orthographicSize updated to: {cam.orthographicSize} for height: {lastHeight}");
        }
    }
}