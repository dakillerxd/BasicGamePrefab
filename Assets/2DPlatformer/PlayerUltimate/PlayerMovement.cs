
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D collBody;
    [SerializeField] private Collider2D collFeet;

    // Movement vars
    private Vector2 moveVelocity;
    private bool isFacingRight;


    // Collision check var
    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private bool isGrounded;
    private bool bumpedHead;

    // Apex vars
    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    // Jump buffer vars
    private float jumpBufferTimer;
    private float jumpReleasedDuringBuffer;

    // Coyote time vars
    private float coyoteTimer;


    void Awake() {

        isFacingRight = true;
        if (!rigidBody) {rigidBody = GetComponent<Rigidbody2D>();}
    }


    void Update()
    {
        CountTimers();
        JumpChecks();
    }

    void FixedUpdate() {

        CollisionChecks();

        if (isGrounded) {

            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManagerUltimate.Movement);
        }
        else {

            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManagerUltimate.Movement);
        }

    }



    #region  Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput) {

        if (moveInput != Vector2.zero) {
            
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManagerUltimate.RunIsHeld) {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else { targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed; }

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.deltaTime);
            rigidBody.velocity = new Vector2(moveVelocity.x, rigidBody.velocity.y);
        }
        else if (moveInput == Vector2.zero) {

            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.deltaTime);
            rigidBody.velocity = new Vector2(moveVelocity.x, rigidBody.velocity.y);
        }
    }

    private void TurnCheck(Vector2 moveInput) {

        if (isFacingRight && moveInput.x < 0f) { Turn(false); }
        else if (!isFacingRight && moveInput.x > 0f) { Turn(true); }

    }

    private void Turn(bool turnRight) {

        if (turnRight) {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion Movement


    #region Jumping

    private void JumpChecks() {




    }

    private void Jump() {

    }


    #endregion Jumping



    #region Collision Checks

    private void IsGrounded() {

        Vector2 boxCastOrigin = new Vector2(collFeet.bounds.center.x, collFeet.bounds.center.y);
        Vector2 boxCastSize = new Vector2(collFeet.bounds.size.x, collFeet.bounds.size.y);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectRayLength, MoveStats.GroundLayerMask);

        if (groundHit.collider != null) { isGrounded = true; }
        else { isGrounded = false; }

        if (MoveStats.DebugShowIsGroundedBox) {

            Color rayColor;
            if (isGrounded) { rayColor = Color.green; }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MoveStats.GroundDetectRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
    }

    private void CollisionChecks() {

        IsGrounded();
    }


    #endregion Collision Checks


    #region  Timers

    private void CountTimers() {
        jumpBufferTimer -= Time.deltaTime;

        if (!isGrounded) {
            coyoteTimer -= Time.deltaTime;
        }
        else { coyoteTimer = MoveStats.JumpCoyoteTime; }
    }


    #endregion Timers


    
}
