using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{


    [Header("References")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D collBody;
    [SerializeField] private Collider2D collFeet;
    [SerializeField] private LayerMask groundLayer;


    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float dashForce = 5f;
    [SerializeField] private int maxJumps = 1;
    [SerializeField] private int maxDashes = 1;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("Debug")]
    [SerializeField] private float horizontalInput;
    [SerializeField] private bool runInput;
    [SerializeField] private bool jumpRequested;
    [SerializeField] private bool dashRequested;
    [SerializeField] private bool isGrounded;
    [SerializeField] private int remainingJumps;
    [SerializeField] private int remainingDashes;


    // Too implement:
    // Walk
    // Jump
    // -Run
    // -Dash
    // -Double jump
    // -Jump buffer
    // -Coyote jump
    // -Respawn
    // -Wall slide



    private void Awake() {

        // if (!rigidBody) {rigidBody = GetComponent<Rigidbody2D>();}
        // if (!spriteRenderer) {spriteRenderer = GetComponent<SpriteRenderer>();}
        // if (!collBody) {collBody = GetComponent<Collider2D>();}
        // if (!collFeet) {collFeet = GetComponent<Collider2D>();}
    }
 


    private void Update() {

        CheckForInput();
        ControlSprite();
    }

    private void FixedUpdate() {

        CollisionChecks();
        HandleGravity();
        HandleMovement();
        HandleJump();
        HandleDash();
        
        
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
        }

        // Check for dash input
        if (Input.GetButtonDown("Dash"))
        {
            dashRequested = true;
        }
    }

    private void HandleMovement() {

        if (isGrounded && runInput) {
            rigidBody.velocity = new Vector2(horizontalInput * runSpeed, rigidBody.velocity.y);
        }
        else {
            rigidBody.velocity = new Vector2(horizontalInput * moveSpeed, rigidBody.velocity.y);
        }
        
    }

    private void HandleJump() {

        if (jumpRequested && isGrounded) {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
            jumpRequested = false;
        }
    }

    private void HandleDash() {
        
        if (dashRequested && remainingDashes > 0) {
            rigidBody.velocity = new Vector2(horizontalInput * dashForce, rigidBody.velocity.y);
            dashRequested = false;
            remainingDashes--;
        }
    }

    private void HandleGravity() {

        // the player is not on the ground and not moving upwards
        if (!isGrounded) { 
            rigidBody.velocity += Vector2.down * gravityForce * Time.fixedDeltaTime;;
        }
    }

    private void CollisionChecks() {

        // Check if the player is grounded
        isGrounded = collFeet.IsTouchingLayers(groundLayer);
    }


    private void ControlSprite() {

        if (horizontalInput > 0) {
            spriteRenderer.flipX = false;
        } else if (horizontalInput < 0) {
            spriteRenderer.flipX = true;
        }
    }
    
}
