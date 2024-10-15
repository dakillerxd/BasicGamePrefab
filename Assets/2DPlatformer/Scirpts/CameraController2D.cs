using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    public static CameraController2D Instance { get; private set; }

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] [Range(0f, 2f)] private float smoothFollowSpeed = 0.5f;
    [SerializeField] [ReadOnly] private Vector3 targetPosition;

    [Header("Shake Settings")]
    [SerializeField] private float xShakeRange = 1;
    [SerializeField] private float yShakeRange = 1;
    [SerializeField] [ReadOnly] public bool isShaking;
    private Vector3 shakeOffset;


    [Header("Target Offset")]
    [SerializeField] private bool verticalOffset = true;
    [SerializeField] private bool horizontalOffset = true;
    [SerializeField] [Range(0f, 2f)] private float verticalOffsetStrength = 0.5f;
    [SerializeField] [Range(0f, 2f)] private float horizontalOffsetStrength = 0.5f;
    [SerializeField] [ReadOnly] private Vector3 targetOffset;

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


    private void Awake() {

       if (Instance != null && Instance != this) {

            Destroy(gameObject);

        } else {
            
            Instance = this;
        }
    }


    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = targetZoom;
        isShaking = false;
    }

    private void Update()
    {
        HandleZoomInput();
        HandleTargetSelection();
    }


    private void LateUpdate()
    {
        FollowTarget();
        HandleZoom();
        ApplyShake();
    }


    private void HandleTargetSelection()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero); ;

            if (hit)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    SetTarget(hit.collider.transform.parent);
                    Debug.Log("Set camera target to: " + hit.collider.transform.parent.name);
                }
                else
                {
                    Debug.Log("Clicked on: " + hit.collider.gameObject.name);
                }
            }
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void FollowTarget()
    {
        if (!target) return;

        targetPosition = CalculateTargetPosition();
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothFollowSpeed, Mathf.Infinity, Time.deltaTime);

        if (useBoundaries) {

            smoothedPosition = HandleBoundaries(smoothedPosition);
        }

        transform.position = smoothedPosition;
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

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, smoothFollowSpeed, Mathf.Infinity, Time.fixedDeltaTime);
    }


    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }
    private Vector3 HandleBoundaries(Vector3 position)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float minXBoundary = minX + camWidth;
        float maxXBoundary = maxX - camWidth;
        float minYBoundary = minY + camHeight;
        float maxYBoundary = maxY - camHeight;

        float newX = Mathf.Clamp(position.x, minXBoundary, maxXBoundary);
        float newY = Mathf.Clamp(position.y, minYBoundary, maxYBoundary);

        //Draw the box boundaries 
        Debug.DrawLine(new Vector3(minX, minY, 0), new Vector3(minX, maxY, 0), Color.blue); // Left line
        Debug.DrawLine(new Vector3(maxX, minY, 0), new Vector3(maxX, maxY, 0), Color.blue); // Right line
        Debug.DrawLine(new Vector3(minX, minY, 0), new Vector3(maxX, minY, 0), Color.blue); // Bottom line
        Debug.DrawLine(new Vector3(minX, maxY, 0), new Vector3(maxX, maxY, 0), Color.blue); // Top line


        return new Vector3(newX, newY, position.z);
    }


    public void SetBoundaries(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }





    public void ShakeCamera(float duration = 0.25f, float magnitude = 0.25f)
    {
        if (!target) return;
        StartCoroutine(Shake(duration, magnitude));
        Debug.Log("Shaking camera for: " + duration + ", At: " + magnitude);
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (isShaking && elapsed < duration)
        {
            float x = Random.Range(-xShakeRange, xShakeRange) * magnitude;
            float y = Random.Range(-yShakeRange, yShakeRange) * magnitude;

            shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        isShaking = false;
    }
    private void ApplyShake()
    {
        if (isShaking)
        {
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothFollowSpeed, Mathf.Infinity, Time.deltaTime);

            if (useBoundaries)
            {
                smoothedPosition = HandleBoundaries(smoothedPosition);
            }

            transform.position = smoothedPosition;
        }
    }
    private Vector3 CalculateTargetOffset()
    {
        Vector3 offset = Vector3.zero;

        if (target.gameObject.tag == "Player")
        {
            PlayerController2D player = target.GetComponent<PlayerController2D>();
            if (horizontalOffset) {
                if (player.rigidBody.velocity.x != 0 ) {

                    if (player.isFacingRight) {
                        offset.x = horizontalOffsetStrength + (player.rigidBody.velocity.x/1.5f);
                    } else {
                        offset.x = -horizontalOffsetStrength + (player.rigidBody.velocity.x/1.5f);
                    }
                }
            }

            if (verticalOffset) {
                if (player.isGrounded) {
                    offset.y = 1f;
                } else if (!player.isGrounded && player.rigidBody.velocity.y > 0) { // Player is jumping
                    offset.y = verticalOffsetStrength;
                    
                } else if (!player.isGrounded && player.rigidBody.velocity.y < -2) { // Player is falling
                    offset.y = -verticalOffsetStrength + Mathf.Clamp(player.rigidBody.velocity.y,-4,0);

                } else if (!player.isGrounded && player.rigidBody.velocity.y < -6) { // Player is falling faster
                    offset.y = -verticalOffsetStrength + Mathf.Clamp(player.rigidBody.velocity.y,-5,0);
                    
                } else if (!player.isGrounded && player.rigidBody.velocity.y <= -player.maxFallSpeed) { // Player is falling at max speed
                    offset.y = -verticalOffsetStrength + Mathf.Clamp(player.rigidBody.velocity.y,-6,0);
                    
                }
            }
        }

        targetOffset = offset;
        return offset;
    }

    private Vector3 CalculateTargetPosition()
    {
        CalculateTargetOffset();
        Vector3 basePosition = new Vector3(target.position.x, target.position.y, transform.position.z) + targetOffset;
        return basePosition + shakeOffset;
    }


}