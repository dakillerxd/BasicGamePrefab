using VInspector;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Text;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    public static PlayerController2D Instance { get; private set; }


    [Tab("Player Settings")]
    [Header("Health")]
    [SerializeField] private int maxHealth = 2;
        [SerializeField] private bool canTakeFallDamage = true;
        [ShowIf("canTakeFallDamage")]
        [SerializeField] private int maxFallDamage = 1;
        [EndIf]

    private int currentHealth;
    private int deaths;
    private bool isInvincible;
    private float invincibilityTime;
    private bool isStunLocked;
    private float stunLockTime;
    [HideInInspector] public  float horizontalInput;
    [HideInInspector] public  float verticalInput;
    [HideInInspector] public bool isFacingRight = true;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float airMoveSpeed = 3f;
    [SerializeField] private float moveAcceleration = 2f; // How fast the player gets to max speed
    [SerializeField] private float groundFriction = 5f; // The higher the friction there is less resistance
    [SerializeField] private float airFriction = 1f; // The higher the friction there is less resistance

    [Header("Jump")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private float variableJumpMaxHoldDuration = 0.3f; // How long the jump button can be held
    [SerializeField] [Range(0.1f, 1f)] private float variableJumpMultiplier = 0.5f; // Multiplier for jump cut height
    [SerializeField] [Range(0, 5f)] private int maxJumps = 2;
    [SerializeField] [Range(0.1f, 1f)] private float holdJumpBuffer = 0.2f; // For how long the jump buffer will hold
    [SerializeField] [Range(0, 2f)] private float coyoteJumpBuffer = 0.1f; // For how long the coyote buffer will hold
    [SerializeField] private LayerMask groundLayer;
    [HideInInspector] public bool isGrounded;
    private bool jumpInputUp;
    private bool jumpRequested;
    private bool isJumping;
    private bool isJumpInputHeld;
    private int remainingJumps;
    private float holdJumpTimer = 0;
    private bool canCoyoteJump;
    private float coyoteJumpTime;
    private float variableJumpHeldDuration;

    [Header("Gravity")]
    [SerializeField] private float gravityForce = 0.5f;
    [SerializeField] private float fallMultiplier = 4f; // Gravity multiplayer when the payer is falling
    [SerializeField] public float maxFallSpeed = 20f;
    [HideInInspector] public bool isFastFalling;
    [HideInInspector] public bool atMaxFallSpeed;
    

    [Header("Debug")]
    [SerializeField] private bool showDebugText = false;
    [SerializeField] private bool showFpsText = false;
    [EndTab]

    // ----------------------------------------------------------------------

    [Tab("Player Abilities")]
    [Header("Running")]
    [SerializeField] private bool runAbility = true;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float airRunSpeed = 6f; 
    private bool runInput;
    private bool wasRunning;

    [Header("Climb Steps")]
    [SerializeField] private bool autoClimbStepsAbility = true;
    [SerializeField] [Range(0, 1f)] private float stepHeight = 0.12f;
    [SerializeField] [Range(0, 1f)] private float stepWidth = 0.2f;
    [SerializeField] [Range(0, 1f)] private float stepCheckDistance = 0.04f;
    [SerializeField] private LayerMask stepLayer;
    
    [Header("Wall Slide")]
    [SerializeField] private bool wallSlideAbility = true;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float maxWallSlideSpeed = 3f;
    [SerializeField] [Range(0, 1f)] private float wallSlideStickStrength = 0.3f;
    [SerializeField] [Range(0, 3f)] private float wallCheckDistance = 1;
    private bool isTouchingWall;
    private bool isTouchingWallOnRight;
    private bool isTouchingWallOnLeft;
    [HideInInspector] public bool isWallSliding;

    [Header("Wall Jump")]
    [SerializeField] private bool wallJumpAbility = true;
    [SerializeField] private float wallJumpVerticalForce = 3f;
    [SerializeField] private float wallJumpHorizontalForce = 4f;

    [Header("Dash")]
    [SerializeField] private bool dashAbility = true;
    [SerializeField] private float dashForce = 12f;
    [SerializeField] private int maxDashes = 1;
    [SerializeField] [Range(0.1f, 1f)] private float holdDashRequestTime = 0.1f; // For how long the dash buffer will hold
    private int remainingDashes;
    private bool dashRequested;
    private bool isDashing;
    private float dashBufferTimer = 0;

    [Header("Fast Drop")]
    [SerializeField] private bool fastDropAbility = true;
    [SerializeField] [Range(0, 1f)] private float fastFallAcceleration = 0.1f;
    private bool isFastDropping;
    [EndTab]
    
    // ----------------------------------------------------------------------

    [Tab("References")]
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D collBody;
    [SerializeField] private Collider2D collFeet;

    [Header("VFX")]
    [SerializeField] private ParticleSystem jumpEffect;
    [SerializeField] private ParticleSystem airJumpEffect;
    [SerializeField] private ParticleSystem runEffect;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private ParticleSystem spawnEffect;
    [SerializeField] private ParticleSystem dashEffect;

    [Header("SFX")]
    [SerializeField] private AudioSource jumpSfx;
    [SerializeField] private AudioSource spawnSfx;
    [SerializeField] private AudioSource deathSfx;
    [SerializeField] private AudioSource dashSfx;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI fpsText;
    [EndTab]



    private void Awake() {

       if (Instance != null && Instance != this) {

            Destroy(gameObject);

        } else {

            Instance = this;
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }
 

    private void Start() {

        currentHealth = maxHealth;
        remainingDashes = maxDashes;
        deaths = 0;
        CheckpointManager2D.Instance.SetSpawnPoint(transform.position);
    }


    private void Update() {

        CheckForInput();
        CountTimers();
        CoyoteTimeCheck();
        CheckFaceDirection();
        ControlSprite();

        if (Input.GetKeyDown(KeyCode.R)) { RespawnFromCheckpoint();}
        if (Input.GetKeyDown(KeyCode.F)) { CameraController2D.Instance.ShakeCamera(4,0.5f);}
        UpdateDebugText(); 
        UpdateFpsText();
    }


    private void FixedUpdate() {

        CollisionChecks();
        HandleGravity();
        HandleMovement();
        HandleJump();
        HandleWallSlide();
        HandleWallJump();
        HandleDashing();
        HandleStepClimbing();
        HandleFastDrop();
    }


    //------------------------------------
    #region Movement/Gravity functions
    private void HandleMovement() {

        float speed = horizontalInput;
        float acceleration = moveAcceleration;

        if (isWallSliding) { // Wall sliding

            if (horizontalInput < wallSlideStickStrength && horizontalInput > -wallSlideStickStrength) { 

                speed = 0; 
                acceleration = 0;
            }
            
        } else {
            if (isGrounded) { // On Ground
                
                if (runAbility && runInput) { // Run

                    speed *= runSpeed;
                    wasRunning = true;

                } else { // Walk

                    speed *= moveSpeed;
                    wasRunning = false;
                }

            } else if (!isGrounded) { // In air

                if (isTouchingWall) { wasRunning = false;}

                if (runAbility && wasRunning) { // Run

                    speed *= airRunSpeed;

                } else { // Walk

                    speed *= airMoveSpeed;
                    wasRunning = false;
                }
            } 
        }


        // Play run effect
        if (runEffect) {
            if (runAbility && runInput && isGrounded)
            {
                if (runEffect.isStopped) runEffect.Play();
            }
            else
            {
                if (runEffect.isPlaying) runEffect.Stop();
            }
        }

        // Apply friction
        if (isGrounded) {acceleration *= groundFriction;} else {acceleration *= airFriction;}

        // Lerp the player movement
        float newXVelocity = Mathf.Lerp(rigidBody.velocity.x, speed, acceleration * Time.fixedDeltaTime);
        rigidBody.velocity = new Vector2(newXVelocity, rigidBody.velocity.y);


    }

    private void HandleFastDrop() {

        if (!fastDropAbility) return;
        if (isGrounded && !atMaxFallSpeed) return;

        if (verticalInput < 0) {isFastDropping = true;} else {isFastDropping = false;}

        if (isFastDropping) {

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y - fastFallAcceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleDashing() {


        if (!dashAbility) return; // Return if not allowed to dash

        if (dashRequested && remainingDashes > 0) {

            
            isDashing = true;
            TurnInvincible();

            int dashDirection = isFacingRight ? 1 : -1;
            rigidBody.velocity = new Vector2(dashForce * dashDirection, rigidBody.velocity.y);
            dashRequested = false;
            if (!isGrounded) { remainingDashes -= 1;}

            if (dashEffect) {dashEffect.Play();}
            if (dashSfx) {dashSfx.Play();}

            isDashing = false;
        }


        if (isGrounded) { remainingDashes = maxDashes; } // Reset dashes when on ground
    }


    private void HandleStepClimbing()
    {
        if (!autoClimbStepsAbility) return; // Only check for steps can climb steps
        if (!isGrounded) return; // Only check for steps when grounded

        Vector2 moveDirection = new Vector2(rigidBody.velocity.x, 0).normalized;
        if (moveDirection != Vector2.zero ) { // Moving horizontally

            // Check for step in front of the player
            RaycastHit2D hitLower = Physics2D.Raycast(collFeet.bounds.center, moveDirection, collFeet.bounds.extents.x + stepCheckDistance, stepLayer);

            // Draw the raycast for debugging
            Debug.DrawRay(collFeet.bounds.center, moveDirection * (collFeet.bounds.extents.x + stepCheckDistance), Color.red);

            if (hitLower.collider != null) {

                // Check if there's space above the step
                RaycastHit2D hitUpper = Physics2D.Raycast(collFeet.bounds.center + new Vector3(0, stepHeight, 0), moveDirection, collFeet.bounds.extents.x + stepCheckDistance, stepLayer) ;

                if (hitUpper.collider == null) {
                    // Move the player up
                    rigidBody.position += new Vector2(horizontalInput * stepWidth, stepHeight);
                }
            }
        } 
    }



    private void HandleWallSlide() {

        if (wallSlideAbility && isTouchingWall && !isGrounded && rigidBody.velocity.y < 0) { // Check if wall sliding
            isWallSliding = true;
        } else {
            isWallSliding = false;
        }

        if (isWallSliding) { 

            // Make the player face the opposite direction from the wall
            if (isTouchingWallOnLeft && !isFacingRight) { 
                FlipPlayer("Right");
            } else if (isTouchingWallOnRight && isFacingRight) {
                FlipPlayer("Left");
            }

            // Set slide speed
            float slideSpeed = wallSlideSpeed;
            float maxSlideSpeed = maxWallSlideSpeed;

            // Accelerate slide if fast dropping
            if (isFastDropping) {
                slideSpeed *= 1.5f;
                maxSlideSpeed *= 1.5f;
            }


            // Lerp the fall speed
            float newYVelocity = Mathf.Lerp(rigidBody.velocity.y, -maxSlideSpeed, slideSpeed  * Time.fixedDeltaTime);
            rigidBody.velocity = new Vector2 ( rigidBody.velocity.x, newYVelocity);

            if (rigidBody.velocity.y < -maxSlideSpeed) { // Clamp fall speed

                rigidBody.velocity = new Vector2(rigidBody.velocity.x, -maxSlideSpeed);

            }

        }   
    }


    #endregion Movement functions


    //------------------------------------
    #region Jump functions
    private void HandleJump() {

        // Handle jump input timing
        if (Input.GetButton("Jump") && isJumping) {
            variableJumpHeldDuration += Time.fixedDeltaTime;
            isJumpInputHeld = variableJumpHeldDuration < variableJumpMaxHoldDuration;
        }

        // Cut jump height if button is released early
        if (jumpInputUp) {
            if (isJumping && rigidBody.velocity.y > 0) {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * variableJumpMultiplier);
            }
            isJumpInputHeld = false;
            variableJumpHeldDuration = 0;
        }

        if (jumpRequested) { // Jump

            if (holdJumpTimer > holdJumpBuffer) {
                jumpRequested = false;
                return;
            }
            
            if (isGrounded || canCoyoteJump) { // Ground / Coyote jump
                ExecuteJump(1);
            } else if (!(isGrounded && canCoyoteJump )&& !isTouchingWall && remainingJumps > 1) { // Extra jump after coyote time passed
                ExecuteJump(2);
            } else if (!(isGrounded && canCoyoteJump  && isJumping)&& !isTouchingWall && remainingJumps > 0) { // Extra jumps
                ExecuteJump(1);
            }
        }

        if (isGrounded && !isJumping) { // Reset jumps
            remainingJumps = maxJumps;
        }

        
        if (!isGrounded && rigidBody.velocity.y <= 0) { // Reset jump state
            isJumping = false;
        }
    }

    private void ExecuteJump(int jumpCost) { 

        // Play effects
        if (isGrounded) {
            if (jumpEffect) jumpEffect.Play();
        } else {
            if (airJumpEffect) airJumpEffect.Play();
        } 

        // Jump
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
        jumpRequested = false;
        remainingJumps -= jumpCost;
        isJumping = true;
        isJumpInputHeld = true;
        variableJumpHeldDuration = 0;

        // Reset coyote state
        coyoteJumpTime = 0;
        canCoyoteJump = false;
    }


    private void HandleWallJump () {

        if (!wallJumpAbility) return;

        if (jumpRequested) {
            if (holdJumpTimer > holdJumpBuffer) {
                jumpRequested = false;
                return;
            }
            
            if (!isGrounded && isTouchingWallOnRight) { 
                ExecuteWallJump("Left");
                
            } else if (!isGrounded && isTouchingWallOnLeft) {
                ExecuteWallJump("Right");
            }
        }


        if (!isGrounded && isTouchingWall) { remainingDashes = maxDashes; } // Reset dashes

        if (!isGrounded && isTouchingWall) { // Reset jumps
            remainingJumps = maxJumps;
        }
    }

    private void ExecuteWallJump(string side) {

        // Play effects
        if (airJumpEffect) airJumpEffect.Play();

        // Jump
        if (side == "Right") {
            rigidBody.velocity = new Vector2(wallJumpHorizontalForce, wallJumpVerticalForce);
            FlipPlayer("Right");
        } else if (side == "Left") {
            rigidBody.velocity = new Vector2(-wallJumpHorizontalForce, wallJumpVerticalForce);
            FlipPlayer("Left");
        }
        jumpRequested = false;
        variableJumpHeldDuration = 0;
        isJumping = true;
        TurnStunLocked();
    }

    private void CoyoteTimeCheck() {

        // Reset coyote jump
        if (isGrounded) {
            canCoyoteJump = true;
            coyoteJumpTime = coyoteJumpBuffer;
        }

        // Update coyote time
        if (canCoyoteJump) {
            if (coyoteJumpTime > 0) {
                coyoteJumpTime -= Time.deltaTime;
            } else {
                canCoyoteJump = false;
            }
        }
    }



    #endregion Jump functions

    //------------------------------------
    #region Gravity function
    private void HandleGravity()
    {
        if (!isGrounded && !isWallSliding) // Apply gravity when not grounded
        {
            // Apply gravity and apply the fall multiplier if the player is falling
            float gravityMultiplier = rigidBody.velocity.y > 0 ? 1f : fallMultiplier; 
            rigidBody.velocity = new Vector2 ( rigidBody.velocity.x, Mathf.Lerp(rigidBody.velocity.y, -maxFallSpeed, gravityForce * gravityMultiplier * Time.fixedDeltaTime));


            // Cap fall speed
            if (!isWallSliding) { 

                if (rigidBody.velocity.y < -maxFallSpeed/2) {

                    isFastFalling = true;

                } else { isFastFalling = false; }


                if (rigidBody.velocity.y < -maxFallSpeed) {

                    atMaxFallSpeed = true;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, -maxFallSpeed);
                    if (CameraController2D.Instance && !CameraController2D.Instance.isShaking) { CameraController2D.Instance.ShakeCamera( 1f, 4f,2,2); } // shake camera when at max fall speed

                } else { atMaxFallSpeed = false; }
            }

        } else { // Apply gravity when grounded

            const float groundGravityForce = 0.1f;
            rigidBody.velocity += groundGravityForce * Time.fixedDeltaTime * Vector2.down;

            // Check for landing at fast falling
            if (isFastFalling) {

                if (CameraController2D.Instance && CameraController2D.Instance.isShaking) { // Stop camera shake
                    CameraController2D.Instance.StopCameraShake();
                }

                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce/2); // Make the player bop
                isFastFalling = false;
            }

            // Check for landing at max speed
            if (atMaxFallSpeed) {

                if (canTakeFallDamage) { DamageHealth(maxFallDamage, false, "Ground");} // Take damage 
                

                if (CameraController2D.Instance && CameraController2D.Instance.isShaking) { // Stop camera shake
                    CameraController2D.Instance.StopCameraShake();
                }

                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce); // Make the player bop
                atMaxFallSpeed = false;
                isFastFalling = false;
            }
        }
    }


    #endregion Gravity functions


    //------------------------------------
    #region Collision functions

    private void CollisionChecks() {

        // Check if the player is grounded
        isGrounded = collFeet.IsTouchingLayers(groundLayer);

        // Check if the player is touching a wall
        isTouchingWall = collBody.IsTouchingLayers(groundLayer);

        if (isTouchingWall) {

            // Check collision with walls on the right
            RaycastHit2D hitRight = Physics2D.Raycast(collBody.bounds.center, Vector2.right, collBody.bounds.extents.x + wallCheckDistance, groundLayer);
            Debug.DrawRay(collBody.bounds.center, Vector2.right * (collBody.bounds.extents.x + wallCheckDistance), Color.red);
            isTouchingWallOnRight = hitRight;

            // Check collision with walls on the left
            RaycastHit2D hitLeft = Physics2D.Raycast(collBody.bounds.center, Vector2.left, collBody.bounds.extents.x + wallCheckDistance, groundLayer);
            Debug.DrawRay(collBody.bounds.center, Vector2.left * (collBody.bounds.extents.x + wallCheckDistance), Color.red);
            isTouchingWallOnLeft = hitLeft;
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {

        switch (collision.gameObject.tag)
        {
            case "Enemy":

                Vector2 enemyNormal = collision.GetContact(0).normal;
                Vector2 enemyPushForce = enemyNormal * 5f;
                DamageHealthAndPush(1, enemyPushForce, true, collision.gameObject.name);

            break;

            case "Spike":

                Vector2 spikeNormal = collision.GetContact(0).normal;
                Vector2 spikePushForce = spikeNormal * 5f;
                
                DamageHealthAndPush(1, spikePushForce, true, collision.gameObject.name);



            break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        switch (collision.gameObject.tag)
        {
            case "RespawnTrigger":

                RespawnFromCheckpoint();

                break;
            case "Checkpoint":


                CheckpointManager2D.Instance.ActivateCheckpoint(collision.gameObject);

                break;
            case "Teleporter":

                Teleporter2D teleporter = collision.gameObject.GetComponent<Teleporter2D>();
                teleporter.GoToSelectedLevel();
                break;
        }
    }


    #endregion Collision functions





    //------------------------------------
    #region Health/Checkpoint functions
    

    [Button] private void RespawnFromCheckpoint() {

            if (CheckpointManager2D.Instance.activeCheckpoint) {
                Respawn(CheckpointManager2D.Instance.activeCheckpoint.transform.position);
            } else { RespawnFromSpawnPoint();}

        
    }

    [Button] private void RespawnFromSpawnPoint() {

        Respawn(CheckpointManager2D.Instance.playerSpawnPoint);
    }
    
    
    private void Respawn(Vector2 position) {

        // Reset stats/states
        TurnInvincible(2f);
        TurnStunLocked(0.5f);
        deaths += 1;
        transform.position = position;
        rigidBody.velocity = new Vector2(0, 0);
        currentHealth = maxHealth;
        remainingDashes = maxDashes;
        remainingJumps = maxJumps;

        // Play effects
        if (spawnEffect) {spawnEffect.Play();}
        if (spawnSfx) {spawnSfx.Play();}
        if (CameraController2D.Instance && CameraController2D.Instance.isShaking) { // Stop camera shake
            CameraController2D.Instance.StopCameraShake();
        }

        Debug.Log("Respawned, Deaths: " + deaths);
    }


    private void DamageHealth(int damage, bool setInvincible, string cause = "")
    {

        if (currentHealth > 0 && !isInvincible) {
            
            if (setInvincible) {TurnInvincible();}
            TurnStunLocked();
            currentHealth -= damage;
            Debug.Log("Damaged by: " + cause);
        } 

        CheckIfDead(cause);
    }

    private void DamageHealthAndPush(int damage ,Vector2 pushForce , bool setInvincible, string cause = "" ) {

        if (currentHealth > 0 && !isInvincible)
        {
            // Reset current velocity before applying push
            rigidBody.velocity = Vector2.zero;
            
            // Apply a consistent impulse force
            rigidBody.AddForce(pushForce, ForceMode2D.Impulse);
            
            // Clamp the resulting velocity to prevent excessive speed
            float maxPushSpeed = 4f; // Adjust this value to control maximum push speed
            rigidBody.velocity = Vector2.ClampMagnitude(rigidBody.velocity, maxPushSpeed);
            
            if (setInvincible) { TurnInvincible(); }
            TurnStunLocked();
            currentHealth -= damage;
            Debug.Log("Damaged by: " + cause);
        }

        CheckIfDead();

    }

    private void CheckIfDead(string cause = "") {

        if (currentHealth <= 0) {

            if (deathEffect) {deathEffect.Play();}
            if (deathSfx) {deathSfx.Play();}

            Debug.Log("Death by: " + cause);

            RespawnFromCheckpoint();
        }
    }
    private void TurnStunLocked(float stunLockDuration = 0.1f) {
        
        StartCoroutine(StuckLock(stunLockDuration));
    }

    private void UnStuckLock() {

        isStunLocked = false;
        stunLockTime = 0f;
    
    }

    private IEnumerator StuckLock(float stunLockDuration)
    {
        
        isStunLocked = true;
        stunLockTime = stunLockDuration;

        while (isStunLocked && stunLockTime > 0) {
            stunLockTime -= Time.deltaTime;
            yield return null;
        }

        UnStuckLock();
    }



    private void TurnInvincible(float invincibilityDuration = 0.5f) {

        StartCoroutine(Invisible(invincibilityDuration));
    }
    
    private void TurnVulnerable() {

        isInvincible = false;
    }

    private IEnumerator Invisible(float invincibilityDuration)
    {
        
        isInvincible = true;
        invincibilityTime = invincibilityDuration;

        while (isInvincible && invincibilityTime > 0) {
            invincibilityTime -= Time.deltaTime;
            yield return null;
        }

        TurnVulnerable();
    }

    #endregion Health/Checkpoint functions

    //------------------------------------

    #region Other functions

    private void CheckForInput() {

        if (CanMove()) { // Only check for input if the player can move

            // Check for horizontal input
            horizontalInput = Input.GetAxis("Horizontal");

            // Check for vertical input
            verticalInput = Input.GetAxis("Vertical");


            // Check for jump inputs
            if (Input.GetButtonDown("Jump")) {
                jumpRequested = true;
                holdJumpTimer = 0f;
            }
            if (Input.GetButtonUp("Jump")) {
                jumpInputUp = true;
            }
            
            // Check for run input
            if (runAbility) {
                runInput = Input.GetButton("Run");
            }

            // Check for dash input
            if (dashAbility && Input.GetButtonDown("Dash")) {
                dashRequested = true;
                isDashing = true;
                dashBufferTimer = 0f;
            }





        } else { // Set inputs to 0 if the player cannot move

            horizontalInput = 0;
            verticalInput = 0;
            jumpRequested = false;
            holdJumpTimer = 0f;
            variableJumpHeldDuration = 0;
            runInput = false;
            dashRequested = false;

        }


    }

    private void ControlSprite() {

        if (isFacingRight) {
            spriteRenderer.flipX = false;
        } else {
            spriteRenderer.flipX = true;
        }

        if (isInvincible) {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        } else {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    private void CheckFaceDirection() {

        if (isWallSliding) return; // Only flip the player based on input if he is not wall sliding

        if (horizontalInput > 0) {
            FlipPlayer("Right");
        } else if (horizontalInput < 0) {
            FlipPlayer("Left");
        }
    }
    
    private void FlipPlayer(string side) {
        
        if (side == "Left") {
            isFacingRight = false;
        } else if (side == "Right") {
            isFacingRight = true;
        }
    }


    private void CountTimers() {

        // Jump buffer timer
        if (holdJumpTimer <= holdJumpBuffer) {

            holdJumpTimer += Time.deltaTime;
        }

        // Dash buffer timer
        if (dashBufferTimer <= holdDashRequestTime) {

            dashBufferTimer += Time.deltaTime;
        }


    }


    private bool CanMove() {
        return !isStunLocked && !isDashing;
    }


    #endregion Other functions
    
    #region Debugging functions
    #if UNITY_EDITOR || DEVELOPMENT_BUILD

    private StringBuilder debugStringBuilder = new StringBuilder(256);
    private void UpdateDebugText() {

        if (debugText) {
            debugText.enabled = showDebugText;
            if (showDebugText) {  

                debugStringBuilder.Clear();
                
                debugStringBuilder.AppendFormat("Player:\n");
                debugStringBuilder.AppendFormat("Health: {0} / {1}\n", currentHealth, maxHealth);
                debugStringBuilder.AppendFormat("Deaths: {0}\n\n", deaths);
                debugStringBuilder.AppendFormat("Jumps: {0} / {1}\n", remainingJumps, maxJumps);
                debugStringBuilder.AppendFormat("Dashes: {0} / {1}\n", remainingDashes, maxDashes);
                debugStringBuilder.AppendFormat("Velocity: {0}\n", rigidBody.velocity);

                debugStringBuilder.AppendFormat("\nStates:\n");
                debugStringBuilder.AppendFormat("Facing Right: {0}\n", isFacingRight);
                debugStringBuilder.AppendFormat("Invincible: {0} ({1:0.0})\n", isInvincible, invincibilityTime);
                debugStringBuilder.AppendFormat("Stun Locked: {0} ({1:0.0})\n", isStunLocked, stunLockTime);
                debugStringBuilder.AppendFormat("Running: {0}\n", wasRunning);
                debugStringBuilder.AppendFormat("Dashing: {0}\n", isDashing);
                debugStringBuilder.AppendFormat("Wall Sliding: {0}\n", isWallSliding);
                debugStringBuilder.AppendFormat("Fast Dropping: {0}\n", isFastDropping);
                debugStringBuilder.AppendFormat("Coyote Jumping: {0} ({1:0.0} / {2:0.0})\n",canCoyoteJump, coyoteJumpTime,coyoteJumpBuffer);
                debugStringBuilder.AppendFormat("Jumping: {0}\n", isJumping);
                debugStringBuilder.AppendFormat("Fast Falling: {0}\n", isFastFalling);
                debugStringBuilder.AppendFormat("At Max Fall Speed: {0}\n", atMaxFallSpeed);

                debugStringBuilder.AppendFormat("\nCollisions:\n");
                debugStringBuilder.AppendFormat("Grounded: {0}\n", isGrounded);
                debugStringBuilder.AppendFormat("Touching Wall: {0}\n", isTouchingWall);
                debugStringBuilder.AppendFormat("Wall on Right: {0}\n", isTouchingWallOnRight);
                debugStringBuilder.AppendFormat("Wall on Left: {0}\n", isTouchingWallOnLeft);

                debugStringBuilder.AppendFormat("\nInputs:\n");
                debugStringBuilder.AppendFormat($"H/V: {horizontalInput:F2} / {verticalInput:F2}\n");
                debugStringBuilder.AppendFormat("Run: {0}\n", runInput);
                debugStringBuilder.AppendFormat("Jump: {0} ({1:0.0} / {2:0.0})\n", Input.GetButtonDown("Jump"), variableJumpHeldDuration, variableJumpMaxHoldDuration);
                debugStringBuilder.AppendFormat("Dash: {0}\n", Input.GetButtonDown("Dash"));

                debugText.text = debugStringBuilder.ToString();
            }
        }
    }

    private StringBuilder fpsStringBuilder = new StringBuilder(256);
    private void UpdateFpsText() {

        if (fpsText) {
            fpsText.enabled = showFpsText;
            if (showFpsText) {  

                fpsStringBuilder.Clear();

                float deltaTime = 0.0f;
                deltaTime += Time.unscaledDeltaTime - deltaTime;
                float fps = 1.0f / deltaTime;
                fpsText.text = string.Format("{0:0.} FPS", fps);
                fpsStringBuilder.AppendFormat("{0}\n", (int)fps);

                fpsText.text = fpsStringBuilder.ToString();
            }
        }
    }

    #endif
    #endregion Debugging functions
    
}
