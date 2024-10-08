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
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private int maxJumps = 2;
    private bool isGrounded;
    private int remainingJumps;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] private float fallMultiplier = 2.5f;


    // Input vars
    private float horizontalInput;
    private bool jumpInput;


    


    



    private void Awake() {

        // if (!rigidBody) {rigidBody = GetComponent<Rigidbody2D>();}
        // if (!spriteRenderer) {spriteRenderer = GetComponent<SpriteRenderer>();}
        // if (!collBody) {collBody = GetComponent<Collider2D>();}
        // if (!collFeet) {collFeet = GetComponent<Collider2D>();}
    }
 

    private void Start()
    {
        
    }


    private void Update()
    {
        CheckForInput();
    }

    private void FixedUpdate() {

        CollisionChecks();
        HandleGravity();
        HandleMovement();
        
    }
    

    private void CheckForInput() {

        // Check for horizontal movement
        horizontalInput = Input.GetAxis("Horizontal");

        // Check for jump input
        jumpInput = Input.GetButtonDown("Jump");
    }

    private void HandleMovement() {

        rigidBody.velocity = new Vector2(horizontalInput * moveSpeed, rigidBody.velocity.y);
    }

    private void HandleGravity() {

        // the player is not on the ground and not moving upwards
        if ( !isGrounded && rigidBody.velocity.y < 0) { 
            rigidBody.velocity += Vector2.up * gravityForce * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    private void CollisionChecks() {

        // Check if the player is grounded
        isGrounded = collFeet.IsTouchingLayers(groundLayer);
    }
    
}
