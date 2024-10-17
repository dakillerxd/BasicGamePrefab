
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using VInspector;

public class CameraController2D : MonoBehaviour
{
    public static CameraController2D Instance { get; private set; }


    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] [Range(0f, 2f)] private float smoothFollowSpeed = 0.5f;
    [SerializeField] [ReadOnly] private Vector3 targetPosition;


    [Header("Target Offset")]
    [SerializeField] private bool verticalOffset = true;
    [SerializeField] private bool horizontalOffset = true;
    [SerializeField] [Range(0f, 2f)] private float verticalOffsetStrength = 0.5f;
    [SerializeField] [Range(0f, 2f)] private float horizontalOffsetStrength = 0.5f;
    [SerializeField] [ReadOnly] private Vector3 targetOffset;

    [Header("Shake Settings")]
    [SerializeField] [ReadOnly] public bool isShaking;
    [SerializeField] [ReadOnly] private Vector3 shakeOffset;

    [Header("Camera Boundaries")]
    [SerializeField] private bool useBoundaries = true;
    [SerializeField] private float minXBoundary = -10f;
    [SerializeField] private float maxXBoundary = 10f;
    [SerializeField] private float minYBoundary = -5f;
    [SerializeField] private float maxYBoundary = 5f;

    [Header("Zoom Settings")]
    [SerializeField] private bool allowZoom = true;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;
    [ReadOnly] [SerializeField] private float targetZoom = 5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugText = false;
    [SerializeField] private TextMeshProUGUI debugText;
    [EndTab]


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

            if (debugText) {
                debugText.enabled = showDebugText;
                if (showDebugText) { UpdateDebugText(); }
            }
    }


    private void LateUpdate()
    {
        FollowTarget();
        HandleZoom();
        ApplyShake();
    }


    #region Target
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

    #endregion Target




    #region Zoom
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

    #endregion Zoom



    #region Boundaries
    private Vector3 HandleBoundaries(Vector3 position)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float minXBoundaryBoundary = minXBoundary + camWidth;
        float maxXBoundaryBoundary = maxXBoundary - camWidth;
        float minYBoundaryBoundary = minYBoundary + camHeight;
        float maxYBoundaryBoundary = maxYBoundary - camHeight;

        float newX = Mathf.Clamp(position.x, minXBoundaryBoundary, maxXBoundaryBoundary);
        float newY = Mathf.Clamp(position.y, minYBoundaryBoundary, maxYBoundaryBoundary);

        //Draw the box boundaries 
        Debug.DrawLine(new Vector3(minXBoundary, minYBoundary, 0), new Vector3(minXBoundary, maxYBoundary, 0), Color.blue); // Left line
        Debug.DrawLine(new Vector3(maxXBoundary, minYBoundary, 0), new Vector3(maxXBoundary, maxYBoundary, 0), Color.blue); // Right line
        Debug.DrawLine(new Vector3(minXBoundary, minYBoundary, 0), new Vector3(maxXBoundary, minYBoundary, 0), Color.blue); // Bottom line
        Debug.DrawLine(new Vector3(minXBoundary, maxYBoundary, 0), new Vector3(maxXBoundary, maxYBoundary, 0), Color.blue); // Top line


        return new Vector3(newX, newY, position.z);
    }


    public void SetBoundaries(float minXBoundary, float maxXBoundary, float minYBoundary, float maxYBoundary)
    {
        this.minXBoundary = minXBoundary;
        this.maxXBoundary = maxXBoundary;
        this.minYBoundary = minYBoundary;
        this.maxYBoundary = maxYBoundary;
    }

    #endregion Boundaries



    #region Shake
    public void ShakeCamera(float duration, float magnitude, float xShakeRange = 1f, float yShakeRange = 1f)
    {
        if (!target) return;
        StartCoroutine(Shake(duration, magnitude, xShakeRange, yShakeRange));
        Debug.Log("Shaking camera for: " + duration + ", At: " + magnitude + ", yRange: " + yShakeRange + ", xRange: " + xShakeRange);
    }

    private IEnumerator Shake(float duration, float magnitude, float xShakeRange, float yShakeRange)
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


    public void StopCameraShake() {
        isShaking = false;
        shakeOffset = Vector3.zero;
    }

    #endregion Shake


    #region Calculations
    private Vector3 CalculateTargetOffset()
    {
        Vector3 offset = Vector3.zero;

        if (target.gameObject.tag == "Player" && !isShaking)
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
                if (player.isGrounded) { // player is on the ground
                    offset.y = 1f;
                } else {
                    if (player.isWallSliding) { // Player is wall sliding
                        offset.y = verticalOffsetStrength + player.rigidBody.velocity.y;
                        
                    }  else { // Player is in the air
                        if (!player.isWallSliding && player.rigidBody.velocity.y > 0) { // Player is jumping
                            offset.y = verticalOffsetStrength * player.rigidBody.velocity.y;
                            
                        } else if (!player.isWallSliding && player.rigidBody.velocity.y < 0 && player.rigidBody.velocity.y > -7) { // Player is falling
                            offset.y = -verticalOffsetStrength + Mathf.Clamp(player.rigidBody.velocity.y/2f,-1,0);

                        } else if (!player.isWallSliding && player.rigidBody.velocity.y < -7) { // Player is fast falling
                            offset.y = -verticalOffsetStrength + Mathf.Clamp(player.rigidBody.velocity.y/2f,-10,0);

                        }
                    }
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


    #endregion Calculations

    #region Debugging functions
    #if UNITY_EDITOR || DEVELOPMENT_BUILD

    private StringBuilder debugStringBuilder = new StringBuilder(256);
    private void UpdateDebugText() {

        debugStringBuilder.Clear();
        
        debugStringBuilder.AppendFormat("Camera:\n");
        debugStringBuilder.AppendFormat("Shake Offset: ({0:0.0},{1:0.0})\n", shakeOffset.x, shakeOffset.y);
        debugStringBuilder.AppendFormat("Zoom: {0} ({1}/{2})\n", targetZoom, minZoom, maxZoom);

        debugStringBuilder.AppendFormat("\nTarget: {0}\n", target.name);
        debugStringBuilder.AppendFormat("Position: ({0:0.0},{1:0.0})\n", targetPosition.x, targetPosition.y);
        debugStringBuilder.AppendFormat("Offset: ({0:0.0},{1:0.0})\n", targetOffset.x, targetOffset.y);


        debugStringBuilder.AppendFormat("\nBoundaries: {0}\n", useBoundaries);
        debugStringBuilder.AppendFormat("Horizontal: {0:0.} / {1:0.}\n", minXBoundary, maxXBoundary);
        debugStringBuilder.AppendFormat("Vertical: {0:0.} / {1:0.}", minYBoundary, maxYBoundary);


        


        debugText.text = debugStringBuilder.ToString();
    }


    #endif
    #endregion Debugging functions

}