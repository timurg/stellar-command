using UnityEngine;
using System.Collections;

/// <summary>
/// Camera controller that follows a target with smooth movement and zoom animation.
/// Typically follows the Carrier (player's main ship).
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>CameraFollow handles camera tracking and initial zoom animation.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>FOLLOW: Smoothly follows target transform in LateUpdate.</item>
///   <item>ZOOM: Animates from startOrthographicSize to targetOrthographicSize on start.</item>
///   <item>OFFSET: Maintains Z offset for 2D camera positioning.</item>
///   <item>INTEGRATION: Exposes IsZooming/TargetOrthographicSize for CameraSettings.</item>
/// </list>
/// <para>Assign Carrier transform as target in Inspector.</para>
/// </remarks>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    /// <summary>Target to follow (assign Carrier).</summary>
    [SerializeField] private Transform target;
    
    /// <summary>Smoothness of camera following.</summary>
    [SerializeField] private float smoothSpeed = 0.125f;
    
    /// <summary>Offset from target (Z should be negative for 2D).</summary>
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    
    /// <summary>Initial zoom level (close).</summary>
    [SerializeField] private float startOrthographicSize = 5f;
    
    /// <summary>Final zoom level (far).</summary>
    [SerializeField] private float targetOrthographicSize = 10f;
    
    /// <summary>Duration of zoom animation in seconds.</summary>
    [SerializeField] private float zoomDuration = 2f;

    private Camera cam;
    private bool isZooming = true;

    /// <summary>Whether zoom animation is in progress.</summary>
    public bool IsZooming => isZooming;
    
    /// <summary>Target orthographic size after zoom.</summary>
    public float TargetOrthographicSize => targetOrthographicSize;

    /// <summary>
    /// Initializes camera reference and sets initial zoom.
    /// </summary>
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFollow: No Camera component found!");
            return;
        }
        cam.orthographicSize = startOrthographicSize;
    }

    /// <summary>
    /// Starts zoom animation coroutine.
    /// </summary>
    private void Start()
    {
        StartCoroutine(ZoomOutCoroutine());
    }

    /// <summary>
    /// Follows target after zoom completes.
    /// </summary>
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

    /// <summary>
    /// Coroutine for smooth zoom animation from start to target size.
    /// </summary>
    private IEnumerator ZoomOutCoroutine()
    {
        float elapsedTime = 0f;
        float initialSize = cam.orthographicSize;
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            cam.orthographicSize = Mathf.Lerp(initialSize, targetOrthographicSize, t);
            yield return null;
        }
        cam.orthographicSize = targetOrthographicSize;
        isZooming = false;
    }
}