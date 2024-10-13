using System.Collections;
using System.Collections.Generic;
using VInspector;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Text;
using Unity.VisualScripting;

public class PlayerController2D : MonoBehaviour
{
    [Tab("Player Settings")]
    [Header("Settings")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] [Range(0, 1f)] private float invincibilityTime = 1f;
    [SerializeField] private bool canTakeFallDamage = true;
        [ShowIf("canTakeFallDamage")]
        [SerializeField] private int maxFallDamage = 1;
        [EndIf]
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    private int currentHealth;
    private int deaths;
    private bool isInvincible;
    private float invincibilityTimer;
    private float horizontalInput;
    private float verticalInput;

    [Foldout("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float airMoveSpeed = 3f;
    [SerializeField] [Range(0.1f, 2f)] private float moveAcceleration = 1f;
    [SerializeField] [Range(0.05f, 0.5f)] private float moveDeceleration = 0.25f;
    [SerializeField] private bool canRun = true;
        [ShowIf("canRun")] 
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float airRunSpeed = 6f; 
        private bool runInput;
        private bool wasRunning;
        [EndIf]
    [SerializeField] private bool autoClimbSteps = true;
        [ShowIf("autoClimbSteps")]
        [SerializeField] [Range(0, 1f)] private float stepHeight = 0.12f;
        [SerializeField] [Range(0, 1f)] private float stepWidth = 0.2f;
        [SerializeField] [Range(0, 1f)] private float stepCheckDistance = 0.04f;
        [SerializeField] private LayerMask stepLayer;
        [EndIf]
    [SerializeField] private bool canWallSlide = true;
        [ShowIf("canWallSlide")]
        [SerializeField] private float wallSlideSpeed = 3f;
        private bool isTouchingWall;
        [EndIf]
    [SerializeField] private bool canDash = true;
        [ShowIf("canDash")]
        [SerializeField] private float dashForce = 5f;
        [SerializeField] private int maxDashes = 1;
        [SerializeField] [Range(0.1f, 1f)] private float holdDashRequestTime = 0.1f; // For how long the dash buffer will hold
        private int remainingDashes;
        private bool dashRequested;
        private float dashBufferTimer = 0;
        [EndIf]
    [SerializeField] private bool canFastFall = true;
        [ShowIf("canFastFall")]
        [SerializeField] [Range(0, 1f)] private float fastFallAcceleration = 0.1f;
        [EndIf]
    [EndFoldout]

    [Foldout("Jump Settings")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] [Range(0, 5f)] private int maxAirJumps = 1;
    [SerializeField] [Range(0.1f, 1f)] private float holdJumpRequestTime = 0.2f; // For how long the jump buffer will hold
    private bool jumpRequested;
    private int remainingAirJumps;
    private float jumpBufferTimer = 0;
    [EndFoldout]

    [Foldout("Gravity Settings")]
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] [Range(0f, 3f)] private float fallMultiplier = 2.5f; // Gravity multiplayer when the payer is not jumping
    [SerializeField] private float maxFallSpeed = 15f;
    private bool atMaxFallSpeed;
    [EndFoldout]
    

    [Header("Debug")]
    [SerializeField] private bool showDebugText = false;
    [SerializeField] private Vector2 spawnPoint;
    [SerializeField] private Vector2 lastCheckpoint;
    [EndTab]


    [Tab("References")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D collBody;
    [SerializeField] private Collider2D collFeet;

    [Header("VFX")]
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
    [EndTab]



    private void Awake() {

        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 60;
    }
 

    private void Start() {

        currentHealth = maxHealth;
        deaths = 0;
        SetSpawnPoint(transform.position);

    }


    private void Update() {

        CheckForInput();
        CountTimers();
        ControlSprite();
    }

    private void FixedUpdate() {

        CollisionChecks();
        HandleGravity();
        HandleMovement();
        HandleDash();
        HandleStepClimbing();
        HandleFastFall();
        HandleJump();
        

        if (showDebugText && debugText) { debugText.enabled = showDebugText; UpdateDebugText(); } else { debugText.enabled = false; }
    }



    //------------------------------------
    #region Movement/Gravity functions
    private void HandleMovement() {

        float movementSpeed = horizontalInput;
        float movementAcceleration = moveAcceleration;

        if (isGrounded) { // On Ground
            
            if (canRun && runInput) { // Run

                movementSpeed *= runSpeed;
                movementAcceleration *= 1.5f;
                wasRunning = true;
                if (runEffect && runEffect.isStopped) {runEffect.Play();}

            } else { // Walk

                movementSpeed *= moveSpeed;
                wasRunning = false;
                if (runEffect && runEffect.isPlaying) {runEffect.Stop();}
            }

        } else { // In air

            if (canRun && wasRunning) { // Run

                movementSpeed *= airRunSpeed;
                movementAcceleration /= 1.5f;
                if (runEffect && runEffect.isPlaying) {runEffect.Stop();}

            } else { // Walk

                movementSpeed *= airMoveSpeed;
                movementAcceleration /= 1.5f;
                wasRunning = false;
                if (runEffect && runEffect.isPlaying) {runEffect.Stop();}
            }
        }
        
        if (horizontalInput == 0) { // If the player is not moving use decelerate value

            movementAcceleration = moveDeceleration;
        }

        // Lerp the player movement
        float newVelocityX  = Mathf.Lerp(rigidBody.velocity.x, movementSpeed, movementAcceleration);
        rigidBody.velocity = new Vector2(newVelocityX , rigidBody.velocity.y);


    }

    private void HandleFastFall() {

        if (!canFastFall) return;
        if (isGrounded && !atMaxFallSpeed) return;

        if (verticalInput < 0) {

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y - fastFallAcceleration);
        }

    }

    private void HandleDash() {

        if (!canDash) return;
        if (dashRequested && remainingDashes > 0) {

            dashRequested = false;
            rigidBody.velocity = new Vector2(dashForce, rigidBody.velocity.y);
            if (!isGrounded) { remainingDashes -= 1;}

            if (dashEffect) {dashEffect.Play();}
            if (dashSfx) {dashSfx.Play();}
        }

        if (isGrounded) { remainingDashes = maxDashes; } // Reset dashes when on ground
    }


    private void HandleStepClimbing()
    {
        if (!autoClimbSteps) return; // Only check for steps can climb steps
        if (!isGrounded) return; // Only check for steps when grounded

        // Determine the direction based on horizontal input
        Vector2 moveDirection = new Vector2(horizontalInput, 0).normalized;
        if (moveDirection == Vector2.zero) return; // Not moving horizontally

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

    private void HandleJump() {

        if (jumpRequested) {

            if (jumpBufferTimer > holdJumpRequestTime) { // Jump buffer

                jumpRequested = false;
                return;
            }

            if (isGrounded) { // Jump on ground

                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
                jumpRequested = false;
                if (jumpSfx) {jumpSfx.Play();}
            }
            else { // Air jump
                if (remainingAirJumps > 0) {
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
                    remainingAirJumps--;
                    jumpRequested = false;
                    if (airJumpEffect) {airJumpEffect.Play();}
                    if (jumpSfx) {jumpSfx.Play();}
                }
            }
        }

        if (isGrounded) { // Reset air jumps when on ground
            remainingAirJumps = maxAirJumps;
        }
        
    }


    private void HandleGravity()
    {
        if (!isGrounded) // Apply gravity when not grounded
        {
            // Apply gravity and apply the fall multiplier if the player is not jumping
            float gravityMultiplier = rigidBody.velocity.y > 0 ? 1f : fallMultiplier;
            rigidBody.velocity += gravityForce * gravityMultiplier * Time.fixedDeltaTime * Vector2.down;


            // Cap fall speed
            if (canWallSlide && isTouchingWall) { // When wall sliding

                if (rigidBody.velocity.y < -wallSlideSpeed) {

                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, -wallSlideSpeed);
                    atMaxFallSpeed = false;
                }
            }
            else // When at max fall speed
            {
                if (rigidBody.velocity.y < -maxFallSpeed) {

                    atMaxFallSpeed = true;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, -maxFallSpeed);

                } else {

                    atMaxFallSpeed = false;
                }
            }

        } else { // Apply gravity when grounded

            rigidBody.velocity += 0.1f * Time.fixedDeltaTime * Vector2.down;

            // Check for fall damage when landing
            if (canTakeFallDamage && atMaxFallSpeed) {

                DamageHealth(maxFallDamage, "Took fall damage", false);
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce/2);
                atMaxFallSpeed = false;
            }
        }
    }


    #endregion Movement/Gravity functions


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

                DamageHealth( 1, "Damaged by: " + collision.gameObject.name);
                break;

            case "Spike":

                DamageHealth( 1, "Damaged by: " + collision.gameObject.name);
                if (currentHealth > 0) { rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce); }
                
                break;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "RespawnTrigger":

                DamageHealth(maxHealth, "Fell off the map");

                break;
            case "Checkpoint":

                var checkpoint = collision.gameObject.GetComponent<Checkpoint2D>();

                if (!checkpoint.active) {

                    checkpoint.SetActive(true);
                    SetCheckpoint(collision.transform.position);
                }

                break;
        }
    }


    #endregion Collision functions





    //------------------------------------
    #region  Checkpoint functions
    
    private void SetSpawnPoint(Vector2 newSpawnPoint) {

        spawnPoint = newSpawnPoint;
        SetCheckpoint(spawnPoint);
        Debug.Log("Set spawn point to: " + spawnPoint);
    }

    private void SetCheckpoint(Vector2 newCheckpoint) {

        lastCheckpoint = newCheckpoint;
        Debug.Log("Set checkpoint to: " + lastCheckpoint);
    }

    [Button] private void RespawnFromCheckpoint() {

        Respawn(lastCheckpoint);
    }

    [Button] private void RespawnFromSpawnPoint() {

        Respawn(spawnPoint);
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
        if (canRun) {runInput = Input.GetButton("Run");}

        // Set jumpRequested if Jump button is pressed
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
            jumpBufferTimer = 0f;
        }

        // Check for dash input
        if (canDash && Input.GetButtonDown("Dash")) {

            dashRequested = true;
            dashBufferTimer = 0f;
        }


    }

    private void Respawn(Vector2 position) {

        isInvincible = true;
        invincibilityTimer = 0f;
        deaths += 1;
        transform.position = position;
        rigidBody.velocity = new Vector2(0, 0);
        currentHealth = maxHealth;
        if (spawnEffect) {spawnEffect.Play();}
        if (spawnSfx) {spawnSfx.Play();}
        Debug.Log("Respawned, Deaths: " + deaths);
    }

    private void DamageHealth(int damage, string cause = "", bool setInvincible = true) {

        if (currentHealth > 0 && !isInvincible) {
            
            isInvincible = setInvincible;
            invincibilityTimer = 0f;
            currentHealth -= damage;
            Debug.Log(cause);
        } 

        if (currentHealth <= 0) {

            if (deathEffect) {deathEffect.Play();}
            if (deathSfx) {deathSfx.Play();}
            Respawn(lastCheckpoint);
        }
    }

    private void ControlSprite() {

        if (horizontalInput > 0) {
            spriteRenderer.flipX = false;
        } else if (horizontalInput < 0) {
            spriteRenderer.flipX = true;
        }

        if (isInvincible) {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        } else {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }
    

    private void CountTimers() {

        if (jumpBufferTimer <= holdJumpRequestTime) {

            jumpBufferTimer += Time.deltaTime;
        }

        if (dashBufferTimer <= holdDashRequestTime) {

            dashBufferTimer += Time.deltaTime;
        }

        if (isInvincible) {

            invincibilityTimer += Time.deltaTime;

            if (invincibilityTimer >= invincibilityTime) {

                isInvincible = false;
                invincibilityTimer = 0f;
            }
        }
    }

private StringBuilder debugStringBuilder = new StringBuilder(256);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void UpdateDebugText() {

    debugStringBuilder.Clear();
    
    debugStringBuilder.AppendFormat("Stats:\n");
    debugStringBuilder.AppendFormat("Health: {0} / {1}\n", currentHealth, maxHealth);
    debugStringBuilder.AppendFormat("Deaths: {0}\n\n", deaths);
    debugStringBuilder.AppendFormat("Air Jumps: {0} / {1}\n", remainingAirJumps, maxAirJumps);
    debugStringBuilder.AppendFormat("Dashes: {0} / {1}\n", remainingDashes, maxDashes);
    debugStringBuilder.AppendFormat("Velocity: {0}\n", rigidBody.velocity);

    debugStringBuilder.AppendFormat("\nStates:\n");
    debugStringBuilder.AppendFormat("Invincible: {0}\n", isInvincible);
    debugStringBuilder.AppendFormat("Grounded: {0}\n", isGrounded);
    debugStringBuilder.AppendFormat("Running: {0}\n", wasRunning);
    bool isWallSliding = canWallSlide && isTouchingWall && !isGrounded;
    debugStringBuilder.AppendFormat("Wall Sliding: {0}\n", isWallSliding);
    debugStringBuilder.AppendFormat("At Max Fall Speed: {0}\n", atMaxFallSpeed);

    debugStringBuilder.AppendFormat("\nInputs:\n");
    debugStringBuilder.AppendFormat("H/V: {0} / {1}\n", horizontalInput,verticalInput);
    debugStringBuilder.AppendFormat("Run: {0}\n", runInput);
    debugStringBuilder.AppendFormat("Jump: {0}\n", jumpRequested);
    debugStringBuilder.AppendFormat("Dash: {0}\n", dashRequested);

    debugText.text = debugStringBuilder.ToString();
    }
#endif
    
    #endregion Other functions
}
