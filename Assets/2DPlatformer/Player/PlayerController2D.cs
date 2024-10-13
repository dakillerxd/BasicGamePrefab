using System.Collections;
using System.Collections.Generic;
using VInspector;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class PlayerController2D : MonoBehaviour
{
    [Tab("Player Settings")]
    [Header("Settings")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] [Range(0, 1f)] private float invincibilityTime = 1f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float airMoveSpeed = 3f;
    [SerializeField] private LayerMask groundLayer;

    [Foldout("Movement Settings")]
    [SerializeField] private bool canRun = true;
        [ShowIf("canRun")] 
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float airRunSpeed = 6f; 
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
        [EndIf]
    [EndFoldout]

    [Foldout("Jump Settings")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] [Range(0, 5f)] private int maxAirJumps = 1;
    [SerializeField] [Range(0.1f, 1f)] private float holdJumpRequestTime = 0.2f; // For how long the jump buffer will hold
    [EndFoldout]

    [Foldout("Gravity Settings")]
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] [Range(0f, 3f)] private float fallMultiplier = 2.5f; // Gravity multiplayer when the payer is not jumping
    [SerializeField] private float maxFallSpeed = 15f;
    [SerializeField] private bool canTakeFallDamage = true;
    [EndFoldout]


    [Header("Debug")]
    [SerializeField] private bool showDebugText = false;
    [SerializeField] private Vector2 spawnPoint;
    [SerializeField] private Vector2 lastCheckpoint;
    [ReadOnly] [SerializeField] private float horizontalInput;
    [ReadOnly] [SerializeField] private bool runInput;
    [ReadOnly] [SerializeField] private bool wasRunning;
    [ReadOnly] [SerializeField] private bool jumpRequested;
    [ReadOnly] [SerializeField] private bool isGrounded;
    [ReadOnly] [SerializeField] private bool isTouchingWall;
    [ReadOnly] [SerializeField] private int remainingAirJumps;
    [ReadOnly] [SerializeField] private float jumpBufferTimer = 0;
    [ReadOnly] [SerializeField] private int currentHealth;
    [ReadOnly] [SerializeField] private int deaths;
    [ReadOnly] [SerializeField] private bool isInvincible;
    [ReadOnly] [SerializeField] private float invincibilityTimer;
    [ReadOnly] [SerializeField] private bool isFreeFalling;
    [EndTab]

    [Tab("References")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D collBody;
    [SerializeField] private Collider2D collFeet;
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Vfx")]
    [SerializeField] private ParticleSystem airJumpEffect;
    [SerializeField] private ParticleSystem runEffect;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private ParticleSystem spawnEffect;

    [Header("Sfx")]
    [SerializeField] private AudioSource jumpSfx;
    [SerializeField] private AudioSource spawnSfx;
    [SerializeField] private AudioSource deathSfx;
    [EndTab]




 

    private void Start() {

        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 60;
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
        HandleStepClimbing();
        HandleJump();
        

        if (showDebugText && debugText) { debugText.enabled = showDebugText; UpdateDebugText(); } else { debugText.enabled = false; }
    }



    //------------------------------------
    #region Movement/Gravity functions
    private void HandleMovement() {

        float movementSpeed = horizontalInput;

        if (isGrounded) { // On Ground
            
            if (canRun && runInput) { // Run

                movementSpeed *= runSpeed;
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
                if (runEffect && runEffect.isPlaying) {runEffect.Stop();}
                

            } else { // Walk

                movementSpeed *= airMoveSpeed;
                wasRunning = false;
                if (runEffect && runEffect.isPlaying) {runEffect.Stop();}
            }
        }

        rigidBody.velocity = new Vector2(movementSpeed, rigidBody.velocity.y);
        // Debug.Log($"Movement speed: {movementSpeed}");

    }

    private void HandleStepClimbing()
    {
        if (autoClimbSteps && !isGrounded) return; // Only check for steps when grounded and can climb steps

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
                    isFreeFalling = false;
                }
            }
            else // When free falling
            {
                if (rigidBody.velocity.y < -maxFallSpeed) {

                    isFreeFalling = true;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, -maxFallSpeed);

                } else {

                    isFreeFalling = false;
                }
            }

        } else { // Apply gravity when grounded

            rigidBody.velocity += 0.1f * Time.fixedDeltaTime * Vector2.down;

            // Check for fall damage when landing
            if (canTakeFallDamage && isFreeFalling) {

                DamageHealth(1, "Took fall damage", false);
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce/2);
                isFreeFalling = false;
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

                DamageHealth(maxHealth, "Damaged by: " + collision.gameObject.name);
                break;

            case "Spike":

                if (currentHealth > 0) { rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce); }
                DamageHealth( 1, "Damaged by: " + collision.gameObject.name);
                
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
    private void Respawn(Vector2 position) {

        isInvincible = false;
        transform.position = position;
        rigidBody.velocity = Vector2.zero;
        currentHealth = maxHealth;
        deaths += 1;
        if (spawnEffect) {spawnEffect.Play();}
        if (spawnSfx) {spawnSfx.Play();}

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


    private void UpdateDebugText() {

        debugText.text = 
        $"Health: {currentHealth} / {maxHealth} \n" +
        $"Deaths: {deaths}\n\n" +
        
        $"Velocity: {rigidBody.velocity}\n" +
        $"Invincible: {isInvincible}\n" +
        $"Grounded: {isGrounded}\n" +
        $"Wall Sliding: {canWallSlide & isTouchingWall & !isGrounded}\n" +
        $"Free Falling: {isFreeFalling}\n" +
        $"Air Jumps: {remainingAirJumps} / {maxAirJumps} \n";

        // $"Jump Buffer Timer: {jumpBufferTimer}\n"
        // $"Horizontal Input: {horizontalInput}\n" +
        // $"Run Input: {runInput}\n" +
        // $"Was Running: {wasRunning}\n" +
        // $"Jump Requested: {jumpRequested}\n" +
        // $"Invincibility Timer: {invincibilityTimer}\n"

    }

    private void CheckForInput() {

        // Check for horizontal movement
        horizontalInput = Input.GetAxis("Horizontal");

        // Check for run input
        if (canRun) {runInput = Input.GetButton("Run");}

        // Set jumpRequested if Jump button is pressed
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
            jumpBufferTimer = 0f;
        }
    }


    private void DamageHealth(int damage, string cause = "", bool setInvincible = true) {


        if (currentHealth > 0 && !isInvincible) {
            
            currentHealth -= damage;
            isInvincible = setInvincible;
            Debug.Log(cause);

        } 

        if (currentHealth <= 0) {

            isInvincible = true;
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


        if (isInvincible) {

            invincibilityTimer += Time.deltaTime;

            if (invincibilityTimer >= invincibilityTime) {

                isInvincible = false;
                invincibilityTimer = 0f;
            }
        }
        
    }

    
    #endregion Other functions
}
