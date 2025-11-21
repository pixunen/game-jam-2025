using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // The player's transform

    [Header("Movement")]
    public float smoothSpeed = 0.125f;
    private Vector3 velocity = Vector3.zero;

    [Header("Zoom")]
    public float minOrthographicSize = 5f;
    public float maxOrthographicSize = 20f;
    public float zoomSpeed = 0.5f;
    private float targetOrthographicSize;

    private Camera cam;
    private float initialZ;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target == null)
        {
            // Try to find player automatically
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("CameraController: No target assigned and PlayerController not found!");
                this.enabled = false;
                return;
            }
        }

        initialZ = transform.position.z;
        UpdateTargetZoom();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Update zoom level smoothly
        UpdateTargetZoom();
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthographicSize, Time.deltaTime * zoomSpeed);

        // Follow target smoothly
        Vector3 desiredPosition = target.position;
        desiredPosition.z = initialZ;

        // Clamp position to grid bounds
        desiredPosition = ClampToGridBounds(desiredPosition);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
    }

    void UpdateTargetZoom()
    {
        if (GridManager.Instance == null) return;

        // Zoom out based on grid size
        float gridSize = Mathf.Max(GridManager.Instance.gridWidth, GridManager.Instance.gridHeight);
        // This formula can be tweaked for better feel.
        // A simple approach: map grid size to orthographic size.
        // Let's say an 8x8 grid is min size, and a 32x32 grid is max size.
        float t = Mathf.InverseLerp(8, 32, gridSize);
        targetOrthographicSize = Mathf.Lerp(minOrthographicSize, maxOrthographicSize, t);
    }

    Vector3 ClampToGridBounds(Vector3 desiredPosition)
    {
        if (GridManager.Instance == null) return desiredPosition;

        // Get grid dimensions
        float gridWorldWidth = GridManager.Instance.gridWidth * GridManager.Instance.cellSize;
        float gridWorldHeight = GridManager.Instance.gridHeight * GridManager.Instance.cellSize;

        // Camera dimensions in world units
        float camHeight = cam.orthographicSize * 2;
        float camWidth = cam.aspect * camHeight;

        // Grid boundaries in world coordinates (centered at 0,0)
        float minX = -gridWorldWidth / 2f;
        float maxX = gridWorldWidth / 2f;
        float minY = -gridWorldHeight / 2f;
        float maxY = gridWorldHeight / 2f;

        // Calculate camera position limits
        float clampedX = Mathf.Clamp(desiredPosition.x, minX + camWidth / 2, maxX - camWidth / 2);
        float clampedY = Mathf.Clamp(desiredPosition.y, minY + camHeight / 2, maxY - camHeight / 2);

        // If grid is smaller than camera view, center the camera
        if (gridWorldWidth < camWidth)
        {
            clampedX = 0;
        }
        if (gridWorldHeight < camHeight)
        {
            clampedY = 0;
        }

        return new Vector3(clampedX, clampedY, desiredPosition.z);
    }
}
