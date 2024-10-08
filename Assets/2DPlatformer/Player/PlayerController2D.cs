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

    [Header("Gravity Settings")]
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] private float fallMultiplier = 2.5f;

    



    void Awake() {

        // if (!rigidBody) {rigidBody = GetComponent<Rigidbody2D>();}
        // if (!spriteRenderer) {spriteRenderer = GetComponent<SpriteRenderer>();}
        // if (!collBody) {collBody = GetComponent<Collider2D>();}
        // if (!collFeet) {collFeet = GetComponent<Collider2D>();}

    }
 

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
