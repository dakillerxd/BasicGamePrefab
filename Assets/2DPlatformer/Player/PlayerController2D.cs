using System.Collections;
using System.Collections.Generic;
using VInspector;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Text;

public class PlayerController2D : MonoBehaviour
{
    public static PlayerController2D Instance { get; private set; }


    [Tab("Player Settings")]
    [Header("Health")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] [Range(0, 1f)] private float invincibilityTime = 1f;
    [SerializeField] private bool canTakeFallDamage = true;
        [ShowIf("canTakeFallDamage")]
        [SerializeField] private int maxFallDamage = 1;
        [EndIf]

    private int currentHealth;
    private int deaths;
    private bool isInvincible;
    private float invincibilityTimer;
    [HideInInspector] public  float horizontalInput;
    [HideInInspector] public  float verticalInput;
    [HideInInspector] public bool isFacingRight = true;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float airMoveSpeed = 3f;
    [SerializeField] [Range(0.1f, 2f)] private float moveAcceleration = 1f;
    [SerializeField] [Range(0f, 2f)] private float moveDeceleration = 0.25f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] [Range(0, 5f)] private int maxJumps = 2;
    [SerializeField] [Range(0.1f, 1f)] private float holdJumpBuffer = 0.2f; // For how long the jump buffer will hold
    [SerializeField] [Range(0, 2f)] private float coyoteJumpBuffer = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [HideInInspector] public bool isGrounded;
    private bool jumpRequested;
    private bool isJumping;
    private int remainingJumps;
    private float holdJumpTimer = 0;
    private bool canCoyoteJump;
    private float coyoteJumpTime;
    private bool wasGroundedLastFrame = false;

    [Header("Gravity")]
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] [Range(0f, 10f)] private float fallMultiplier = 2.5f; // Gravity multiplayer when the payer is not jumping
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
        [ShowIf("runAbility")] 
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float airRunSpeed = 6f; 
        private bool runInput;
        private bool wasRunning;
        [EndIf]

    [Header("Climb Steps")]
    [SerializeField] private bool autoClimbStepsAbility = true;
        [ShowIf("autoClimbStepsAbility")]
        [SerializeField] [Range(0, 1f)] private float stepHeight = 0.12f;
        [SerializeField] [Range(0, 1f)] private float stepWidth = 0.2f;
        [SerializeField] [Range(0, 1f)] private float stepCheckDistance = 0.04f;
        [SerializeField] private LayerMask stepLayer;
        [EndIf]
    
    [Header("Wall Slide")]
    [SerializeField] private bool wallSlideAbility = true;
        [ShowIf("wallSlideAbility")]
        [SerializeField] private float maxWallSlideSpeed = 3f;
        [SerializeField] [Range(0, 1f)] private float wallSlideStickStrength = 0.3f;
        private bool isTouchingWall;
        [HideInInspector] public bool isWallSliding;
        [EndIf]

    [Header("Dash")]
    [SerializeField] private bool dashAbility = true;
        [ShowIf("dashAbility")]
        [SerializeField] private float dashForce = 20f;
        [SerializeField] private int maxDashes = 1;
        [SerializeField] [Range(0.1f, 1f)] private float holdDashRequestTime = 0.1f; // For how long the dash buffer will hold
        private int remainingDashes;
        private bool dashRequested;
        private bool isDashing;
        private float dashBufferTimer = 0;
        [EndIf]

    [Header("Fast Drop")]
    [SerializeField] private bool fastDropAbility = true;
        [ShowIf("fastDropAbility")]
        [SerializeField] [Range(0, 1f)] private float fastFallAcceleration = 0.1f;
        private bool isFastDropping;
        [EndIf]
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
        Application.targetFrameRate = 120;
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

        if (debugText) {
            debugText.enabled = showDebugText;
            if (showDebugText) { UpdateDebugText(); }
        }

        if (fpsText) {
            fpsText.enabled = showFpsText;
            if (showFpsText) { UpdateFpsText(); }
        }
    }


    private void FixedUpdate() {

        CollisionChecks();
        HandleGravity();
        HandleMovement();
        HandleJump();
        HandleWallSlide();
        HandleDashing();
        HandleStepClimbing();
        HandleFastDrop();
    }


    //------------------------------------
    #region Movement/Gravity functions
    private void HandleMovement() {

        float movementSpeed = horizontalInput;
        float movementAcceleration = moveAcceleration;

        if (isWallSliding) { // Wall sliding

            if (horizontalInput < wallSlideStickStrength && horizontalInput > -wallSlideStickStrength) { 

                movementSpeed = 0; 
                movementAcceleration = 0;
            }
            
        } else {
            if (isGrounded) { // On Ground
                
                if (runAbility && runInput) { // Run

                    movementSpeed *= runSpeed;
                    movementAcceleration *= 1.5f;
                    wasRunning = true;
                    if (runEffect && runEffect.isStopped) {runEffect.Play();}

                } else { // Walk

                    movementSpeed *= moveSpeed;
                    wasRunning = false;
                    if (runEffect && runEffect.isPlaying) {runEffect.Stop();}
                }

            } else if (!isGrounded) { // In air

                if (runAbility && wasRunning) { // Run

                    movementSpeed *= airRunSpeed;
                    movementAcceleration /= 1.5f;

                } else { // Walk

                    movementSpeed *= airMoveSpeed;
                    movementAcceleration /= 1.5f;
                    wasRunning = false;
                    if (runEffect && runEffect.isPlaying) {runEffect.Stop();}
                }
            } 
        }



        if (horizontalInput == 0) { // If the player is not moving use decelerate value

            movementAcceleration = moveDeceleration;
        }

        // Lerp the player movement
        rigidBody.velocity = new Vector2 ( Mathf.Lerp(rigidBody.velocity.x, movementSpeed, movementAcceleration), rigidBody.velocity.y);

    }

    private void HandleFastDrop() {

        if (!fastDropAbility) return;
        if (isGrounded && !atMaxFallSpeed) return;

        if (verticalInput < 0) {isFastDropping = true;} else {isFastDropping = false;}

        if (isFastDropping) {

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y - fastFallAcceleration);
        }
    }

    private void HandleDashing() {


        if (!dashAbility) return; // Return if not allowed to dash
        if (isGrounded) { remainingDashes = maxDashes; } // Reset dashes when on ground

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

        if (wallSlideAbility && isTouchingWall && !isGrounded && rigidBody.velocity.y < 0) {
            isWallSliding = true;
        } else {
            isWallSliding = false;
        }

        if (isWallSliding) { // Cap slide speed

            if (isFastDropping) {

                rigidBody.velocity = new Vector2 ( rigidBody.velocity.x, -maxWallSlideSpeed*1.5f);
            } else {
                rigidBody.velocity = new Vector2 ( rigidBody.velocity.x, -maxWallSlideSpeed);
            }
            
            // rigidBody.velocity = new Vector2 ( rigidBody.velocity.x, Mathf.Lerp(rigidBody.velocity.y, -maxWallSlideSpeed, 2f * Time.fixedDeltaTime));
        }   
    }

    #endregion Movement functions


    //------------------------------------
    #region Jump functions
    private void HandleJump() {

        if (jumpRequested) {
            if (holdJumpTimer > holdJumpBuffer) {
                jumpRequested = false;
                return;
            }
            
            if (isGrounded || canCoyoteJump) { // Ground / Coyote jump
                ExecuteJump(1);
            } else if (!(isGrounded && canCoyoteJump) && remainingJumps > 1) { // Extra jump after coyote time passed
                ExecuteJump(2);
            } else if (!(isGrounded && canCoyoteJump) && remainingJumps > 0) { // Extra jumps
                ExecuteJump(1);
            }
        }

        if (isGrounded && !isJumping) { // Reset jumps
            remainingJumps = maxJumps;
        }

        
        if (!isGrounded && rigidBody.velocity.y <= 0) { // Reset jump state
            isJumping = false;
        }

        // Debug.Log("Jump: " + isJumping);
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

        // Reset coyote state
        coyoteJumpTime = 0;
        canCoyoteJump = false;

    }

    #endregion Jump functions

    //------------------------------------
    #region Gravity function
    private void HandleGravity()
    {
        if (!isGrounded) // Apply gravity when not grounded
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

            rigidBody.velocity += 0.1f * Time.fixedDeltaTime * Vector2.down;

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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Enemy":

                Vector2 enemyNormal = collision.GetContact(0).normal;
                DamageHealthAndPush(1, 1, enemyNormal, true, collision.gameObject.name);

            break;

            case "Spike":

                Vector2 spikeNormal = collision.GetContact(0).normal;
                
                DamageHealthAndPush(1, 2, spikeNormal, true, collision.gameObject.name);



            break;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "RespawnTrigger":

                DamageHealth(maxFallDamage, false, "World");

                break;
            case "Checkpoint":


                CheckpointManager2D.Instance.ActivateCheckpoint(collision.gameObject);
                break;
        }
    }


    #endregion Collision functions





    //------------------------------------
    #region  Checkpoint functions
    

    [Button] private void RespawnFromCheckpoint() {

            if (CheckpointManager2D.Instance.activeCheckpoint) {
                Respawn(CheckpointManager2D.Instance.activeCheckpoint.transform.position);
            } else { RespawnFromSpawnPoint();}

        
    }

    [Button] private void RespawnFromSpawnPoint() {

        Respawn(CheckpointManager2D.Instance.playerSpawnPoint);
    }
    
    #endregion Checkpoint functions


    //------------------------------------
    #region Other functions

    private void CheckForInput() {

        // Check for horizontal input
        horizontalInput = Input.GetAxis("Horizontal");

        // Check for vertical input
        verticalInput = Input.GetAxis("Vertical");

        // Check for run input
        if (runAbility) {runInput = Input.GetButton("Run");}

        // Set jumpRequested if Jump button is pressed
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
            holdJumpTimer = 0f;
        }

        // Check for dash input
        if (dashAbility && Input.GetButtonDown("Dash")) {

            dashRequested = true;
            isDashing = true;
            dashBufferTimer = 0f;
        }


    }

    private void Respawn(Vector2 position) {

        
        // Reset stats/states
        TurnInvincible();
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
            currentHealth -= damage;
            Debug.Log("Damaged by: " + cause);
        } 

        if (currentHealth <= 0) {

            if (deathEffect) {deathEffect.Play();}
            if (deathSfx) {deathSfx.Play();}

            Debug.Log("Death by: " + cause);

            RespawnFromCheckpoint();
        }
    }

    private void DamageHealthAndPush(int damage ,float pushStrengthMultiplier , Vector2 pushDir , bool setInvincible, string cause = "" )
    {

        if (currentHealth > 0 && !isInvincible) {
            
            if (setInvincible) {TurnInvincible();}
            rigidBody.velocity = new Vector2(rigidBody.velocity.x + (pushDir.x * 15 * pushStrengthMultiplier), rigidBody.velocity.y + (pushDir.y * 2 * pushStrengthMultiplier));
            currentHealth -= damage;
            Debug.Log("Damaged by: " + cause);
        } 

        if (currentHealth <= 0) {

            if (deathEffect) {deathEffect.Play();}
            if (deathSfx) {deathSfx.Play();}

            Debug.Log("Death by: " + cause);

            RespawnFromCheckpoint();
        }
    }

    private void TurnInvincible() {

        isInvincible = true;
        invincibilityTimer = 0f;
    }
    
    private void TurnVulnerable() {

        isInvincible = false;
        invincibilityTimer = 0f;
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

        if (horizontalInput > 0) {
            isFacingRight = true;
        } else if (horizontalInput < 0) {
            isFacingRight = false;
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

        // Invincibility timer
        if (isInvincible) {

            invincibilityTimer += Time.deltaTime;

            if (invincibilityTimer >= invincibilityTime) {
                TurnVulnerable();
            }
        }



    }
    #endregion Other functions
    
    #region Debugging functions
    #if UNITY_EDITOR || DEVELOPMENT_BUILD

    private StringBuilder debugStringBuilder = new StringBuilder(256);
    private void UpdateDebugText() {

        debugStringBuilder.Clear();
        
        debugStringBuilder.AppendFormat("Player:\n");
        debugStringBuilder.AppendFormat("Health: {0} / {1}\n", currentHealth, maxHealth);
        debugStringBuilder.AppendFormat("Deaths: {0}\n\n", deaths);
        debugStringBuilder.AppendFormat("Jumps: {0} / {1}\n", remainingJumps, maxJumps);
        debugStringBuilder.AppendFormat("Dashes: {0} / {1}\n", remainingDashes, maxDashes);
        debugStringBuilder.AppendFormat("Velocity: {0}\n", rigidBody.velocity);

        debugStringBuilder.AppendFormat("\nStates:\n");
        debugStringBuilder.AppendFormat("Facing Right: {0}\n", isFacingRight);
        debugStringBuilder.AppendFormat("Invincible: {0}\n", isInvincible);
        debugStringBuilder.AppendFormat("Grounded: {0}\n", isGrounded);
        debugStringBuilder.AppendFormat("Running: {0}\n", wasRunning);
        debugStringBuilder.AppendFormat("Dashing: {0}\n", isDashing);
        debugStringBuilder.AppendFormat("Wall Sliding: {0}\n", isWallSliding);
        debugStringBuilder.AppendFormat("Fast Dropping: {0}\n", isFastDropping);
        debugStringBuilder.AppendFormat("Coyote Jumping: {0} ({1:0.0} / {2:0.0})\n",canCoyoteJump, coyoteJumpTime,coyoteJumpBuffer);
        debugStringBuilder.AppendFormat("Jumping: {0}\n", isJumping);
        debugStringBuilder.AppendFormat("Fast Falling: {0}\n", isFastFalling);
        debugStringBuilder.AppendFormat("At Max Fall Speed: {0}\n", atMaxFallSpeed);

        debugStringBuilder.AppendFormat("\nInputs:\n");
        debugStringBuilder.AppendFormat($"H/V: {horizontalInput:F2} / {verticalInput:F2}\n");
        debugStringBuilder.AppendFormat("Run: {0}\n", runInput);
        debugStringBuilder.AppendFormat("Jump: {0}\n", Input.GetButtonDown("Jump"));
        debugStringBuilder.AppendFormat("Dash: {0}\n", Input.GetButtonDown("Dash"));

        debugText.text = debugStringBuilder.ToString();
    }

    private StringBuilder fpsStringBuilder = new StringBuilder(256);
    private void UpdateFpsText() {

        fpsStringBuilder.Clear();

        float deltaTime = 0.0f;
        deltaTime += Time.unscaledDeltaTime - deltaTime;
        float fps = 1.0f / deltaTime;
        fpsText.text = string.Format("{0:0.} FPS", fps);
        fpsStringBuilder.AppendFormat("{0}\n", (int)fps);

        fpsText.text = fpsStringBuilder.ToString();

        
    }

    #endif
    #endregion Debugging functions
    
}
