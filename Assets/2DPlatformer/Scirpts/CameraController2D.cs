using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset;

    [Header("Camera Boundaries")]
    [SerializeField] private bool useBoundaries = true;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 5f;

    [Header("Zoom Settings")]
    [SerializeField] private bool allowZoom = true;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;
    [ReadOnly] [SerializeField] private float targetZoom = 5f;

    private Camera cam;
    private Vector3 currentVelocity;
    private float zoomVelocity;

    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = targetZoom;
    }

    private void Update()
    {
        HandleZoomInput();
    }

    private void LateUpdate()
    {
        FollowTarget();
        HandleZoom();
    }

    private void FollowTarget()
    {
        if (target == null) return;

        Vector3 targetPosition = CalculateTargetPosition();
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothSpeed, Mathf.Infinity, Time.fixedDeltaTime);

        if (useBoundaries)
        {
            smoothedPosition = ApplyBoundaries(smoothedPosition);
        }

        transform.position = smoothedPosition;
    }

    private Vector3 CalculateTargetPosition()
    {
        return new Vector3(target.position.x, target.position.y, transform.position.z) + offset;
    }

    private void HandleZoomInput()
    {
        if (!allowZoom) return;

        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= zoomInput * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
    }

    private void HandleZoom()
    {
        if (!allowZoom) return;

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, smoothSpeed, Mathf.Infinity, Time.fixedDeltaTime);
    }

    private Vector3 ApplyBoundaries(Vector3 position)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float minXBoundary = minX + camWidth;
        float maxXBoundary = maxX - camWidth;
        float minYBoundary = minY + camHeight;
        float maxYBoundary = maxY - camHeight;

        float newX = Mathf.Clamp(position.x, minXBoundary, maxXBoundary);
        float newY = Mathf.Clamp(position.y, minYBoundary, maxYBoundary);

        return new Vector3(newX, newY, position.z);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetBoundaries(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }

    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }
}