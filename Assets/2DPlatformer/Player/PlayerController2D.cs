using System.Collections;
using System.Collections.Generic;
using VInspector;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private Vector2 spawnPoint;
    [SerializeField] private Vector2 lastCheckpoint;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float airMoveSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float airRunSpeed = 4f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] [Range(0, 5f)] private int maxAirJumps = 1;
    [SerializeField] [Range(0.1f, 1f)] private float holdJumpRequestTime = 0.2f; // For how long the jump buffer will hold

    [Header("Gravity Settings")]
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] [Range(0f, 3f)] private float fallMultiplier = 2.5f; // Gravity multiplayer when the payer is not jumping


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

    [Header("Debug")]
    [SerializeField] private float horizontalInput;
    [SerializeField] private bool runInput;
    [SerializeField] private bool jumpRequested;
    [SerializeField] private bool isGrounded;
    [SerializeField] private int remainingAirJumps;
    [SerializeField] private float jumpBufferTimer = 0;
    [SerializeField] private int currentHealth;
    [SerializeField] private int deaths;




    private void Awake() {

        // if (!rigidBody) {rigidBody = GetComponent<Rigidbody2D>();}
        // if (!spriteRenderer) {spriteRenderer = GetComponent<SpriteRenderer>();}
        // if (!collBody) {collBody = GetComponent<Collider2D>();}
        // if (!collFeet) {collFeet = GetComponent<Collider2D>();}
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
        HandleJump();
        
    }
    

    private void CheckForInput() {

        // Check for horizontal movement
        horizontalInput = Input.GetAxis("Horizontal");

        // Check for run input
        runInput = Input.GetButton("Run");

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



        if (runInput) { // Run

            if (isGrounded) { // On ground
                rigidBody.velocity = new Vector2(horizontalInput * runSpeed, rigidBody.velocity.y);
            } else { // In air
                rigidBody.velocity = new Vector2(horizontalInput * airRunSpeed, rigidBody.velocity.y);
            }
            
            if (runEffect) runEffect.Play();
        }
        else { // Walk

            if (isGrounded) { // On ground
                rigidBody.velocity = new Vector2(horizontalInput * moveSpeed, rigidBody.velocity.y);
            } else { // In air
                rigidBody.velocity = new Vector2(horizontalInput * airMoveSpeed, rigidBody.velocity.y);
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
                rigidBody.velocity += Vector2.down * gravityForce * Time.fixedDeltaTime;;
            }
            else { // Apply gravity with fall multiplier
                rigidBody.velocity += Vector2.down * gravityForce * fallMultiplier * Time.fixedDeltaTime;
            }
            
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

            DamageHealth(1);
        }
        else if(other.gameObject.CompareTag("Spike")) {
    
            
            Vector2 playerPos = transform.position;
            Vector2 spikePos = other.transform.position;
            Vector2 awayDirection = (playerPos - spikePos).normalized;
            
            Debug.Log($"Player position: {playerPos}");
            Debug.Log($"Spike position: {spikePos}" );
            Debug.Log($"Away direction (before normalization): {playerPos - spikePos}");
            Debug.Log($"Away direction (after normalization): {awayDirection}");
            
            Vector2 force = awayDirection;
            force *= 3f;
            Debug.Log($"Applied force: {force}");
            
            // rigidBody.AddForce(force, ForceMode2D.Impulse);
            rigidBody.velocity = force;
            Debug.DrawRay(transform.position, awayDirection, Color.red, 3f);



            // rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
            // DamageHealth(1);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {

        if (other.gameObject.CompareTag("RespawnTrigger")) {

            Respawn(lastCheckpoint);
        }
        else if (other.gameObject.CompareTag("Checkpoint")) {

            SetCheckpoint(other.transform.position);
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

        rigidBody.velocity = Vector2.zero;
        transform.position = position;
        currentHealth = maxHealth;
        deaths += 1;
        if (spawnEffect) spawnEffect.Play();
        Debug.Log("Respawned");
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
        if (currentHealth <= 0) {
            if (deathEffect) deathEffect.Play();
            Respawn(lastCheckpoint);
        }
        Debug.Log("Current health: " + currentHealth);
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
