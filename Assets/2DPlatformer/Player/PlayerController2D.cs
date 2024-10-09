using System.Collections;
using System.Collections.Generic;
using VInspector;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private Vector2 spawnPoint;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

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




    private void Awake() {

        // if (!rigidBody) {rigidBody = GetComponent<Rigidbody2D>();}
        // if (!spriteRenderer) {spriteRenderer = GetComponent<SpriteRenderer>();}
        // if (!collBody) {collBody = GetComponent<Collider2D>();}
        // if (!collFeet) {collFeet = GetComponent<Collider2D>();}
    }
 
    private void Start() {

        currentHealth = maxHealth;
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

    private void HandleMovement() {



        if (runInput) { // Run
            rigidBody.velocity = new Vector2(horizontalInput * runSpeed, rigidBody.velocity.y);
            if (runEffect) runEffect.Play();
        }
        else { // Walk

            if (isGrounded) {
                rigidBody.velocity = new Vector2(horizontalInput * moveSpeed, rigidBody.velocity.y);
            } else {
                rigidBody.velocity = new Vector2(horizontalInput * (moveSpeed/2), rigidBody.velocity.y);
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

    private void CollisionChecks() {

        // Check if the player is grounded
        isGrounded = collFeet.IsTouchingLayers(groundLayer);
    }

    private void OnTriggerEnter2D(Collider2D other) {

        // Debug.Log("Trigger entered: " + other.gameObject.name);
        
        if(other.CompareTag("Spike")) {
            
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
            DamageHealth(1);
        }
        else if (other.CompareTag("RespawnTrigger")) {

            Respawn();
        }
    }


    private void DamageHealth(int amount) {

        currentHealth -= amount;
        if (currentHealth <= 0) {
            if (deathEffect) deathEffect.Play();
            Respawn();
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

    
    [Button] public void SetSpawnPoint(Vector2 newSpawnPoint) {

        spawnPoint = newSpawnPoint;
        Debug.Log("Set spawn point to: " + spawnPoint);
    }

    
    [Button] public void Respawn() {

        transform.position = spawnPoint;
        currentHealth = maxHealth;
        if (spawnEffect) spawnEffect.Play();
        Debug.Log("Respawned");
    }

}
