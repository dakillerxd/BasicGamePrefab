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
    [Header("Spawn Settings")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private Vector2 spawnPoint;
    [SerializeField] private Vector2 lastCheckpoint;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float airMoveSpeed = 3f;
    [SerializeField] private bool canRun = true;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float airRunSpeed = 6f;
    

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] [Range(0, 5f)] private int maxAirJumps = 1;
    [SerializeField] [Range(0.1f, 1f)] private float holdJumpRequestTime = 0.2f; // For how long the jump buffer will hold

    [Header("Gravity Settings")]
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] [Range(0f, 3f)] private float fallMultiplier = 2.5f; // Gravity multiplayer when the payer is not jumping
    [SerializeField] private float maxFallSpeed = 10f;


    [Header("Debug")]
    [SerializeField] private bool showDebugText = false;
    [ReadOnly] [SerializeField] private float horizontalInput;
    [ReadOnly] [SerializeField] private bool runInput;
    [ReadOnly] [SerializeField] private bool wasRunning;
    [ReadOnly] [SerializeField] private bool jumpRequested;
    [ReadOnly] [SerializeField] private bool isGrounded;
    [ReadOnly] [SerializeField] private int remainingAirJumps;
    [ReadOnly] [SerializeField] private float jumpBufferTimer = 0;
    [ReadOnly] [SerializeField] private int currentHealth;
    [ReadOnly] [SerializeField] private int deaths;

    [Tab("References")]
    [Header("References")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D collBody;
    [SerializeField] private Collider2D collFeet;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private ParticleSystem airJumpEffect;
    [SerializeField] private ParticleSystem runEffect;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private ParticleSystem spawnEffect;
    [SerializeField] private TextMeshProUGUI debugText;

 

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
        HandleJump();
        

        if (showDebugText) { debugText.enabled = showDebugText; UpdateDebugText(); } else { debugText.enabled = false; }
    }

    private void UpdateDebugText() {

        debugText.text = 
        $"Health: {currentHealth} / {maxHealth} \n" +
        $"Deaths: {deaths}\n" +
        $"Velocity: {rigidBody.velocity}\n" +
        $"Grounded: {isGrounded}\n" +
        $"Air Jumps: {remainingAirJumps} / {maxAirJumps} \n";

        // $"Jump Buffer Timer: {jumpBufferTimer}\n"
        // $"Horizontal Input: {horizontalInput}\n" +
        // $"Run Input: {runInput}\n" +
        // $"Was Running: {wasRunning}\n" +
        // $"Jump Requested: {jumpRequested}\n" +

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





    //------------------------------------
    #region Movement/Gravity functions
    private void HandleMovement() {

        float movementSpeed = horizontalInput;

        if (isGrounded) { // On Ground
            
            if (canRun && runInput) { // Run
                movementSpeed *= runSpeed;
                wasRunning = true;

            } else { // Walk
                movementSpeed *= moveSpeed;
                wasRunning = false;
            }

        } else { // In air

            if (canRun && wasRunning) { // Run
                movementSpeed *= airRunSpeed;
            } else { // Walk
                movementSpeed *= airMoveSpeed;
                wasRunning = false;
            }
        }

        rigidBody.velocity = new Vector2(movementSpeed, rigidBody.velocity.y);
        // Debug.Log($"Movement speed: {movementSpeed}");

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
            }
            else { // Air jump
                if (remainingAirJumps > 0) {
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
                    remainingAirJumps--;
                    jumpRequested = false;
                    if (airJumpEffect) airJumpEffect.Play();
                }
            }
        }

        if (isGrounded) { // Reset air jumps when on ground
            remainingAirJumps = maxAirJumps;
        }
        
    }


    private void HandleGravity() {

        // the player is not on the ground
        if (!isGrounded) { 

            if (rigidBody.velocity.y > 0) { // Apply gravity while jumping
                rigidBody.velocity += gravityForce * Time.fixedDeltaTime * Vector2.down;;
            }
            else { // Apply gravity with fall multiplier
                rigidBody.velocity += fallMultiplier * gravityForce * Time.fixedDeltaTime * Vector2.down;
            }
        } else { // Apply gravity when on ground
            rigidBody.velocity += 0.1f * Time.fixedDeltaTime * Vector2.down;
        }


        // Cap fall speed
        if (rigidBody.velocity.y < -maxFallSpeed) {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, -maxFallSpeed);
        }
    }


    #endregion Movement/Gravity functions


    //------------------------------------
    #region Collision functions

    private void CollisionChecks() {

        // Check if the player is grounded
        isGrounded = collFeet.IsTouchingLayers(groundLayer);
    }

    private void OnCollisionEnter2D(Collision2D other) {

        if (other.gameObject.CompareTag("Enemy")) {

            DamageHealth(maxHealth);
        }
        else if(other.gameObject.CompareTag("Spike")) {

            // rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
            DamageHealth(maxHealth);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {

        if (other.gameObject.CompareTag("RespawnTrigger")) {

            Respawn(lastCheckpoint);
        }
        else if (other.gameObject.CompareTag("Checkpoint")) {

            if (other.gameObject.GetComponent<Checkpoint2D>().active == false) {
                other.gameObject.GetComponent<Checkpoint2D>().SetActive(true);
                SetCheckpoint(other.transform.position);
                
            }
            
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

        transform.position = position;
        rigidBody.velocity = Vector2.zero;
        currentHealth = maxHealth;
        deaths += 1;
        if (spawnEffect) spawnEffect.Play();
        Debug.Log("Respawned, Deaths: " + deaths);
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

    private void DamageHealth(int amount) {

        currentHealth -= amount;
        Debug.Log("Damaged, Health: " + currentHealth);
        
        if (currentHealth <= 0) {
            if (deathEffect) deathEffect.Play();
            Respawn(lastCheckpoint);
        }
        
    }

    private void ControlSprite() {

        if (horizontalInput > 0) {
            spriteRenderer.flipX = false;
        } else if (horizontalInput < 0) {
            spriteRenderer.flipX = true;
        }
    }
    

    private void CountTimers() {

        if (jumpBufferTimer <= holdJumpRequestTime) {

            jumpBufferTimer += Time.deltaTime;
        }
        
    }

    
    #endregion Other functions
}
