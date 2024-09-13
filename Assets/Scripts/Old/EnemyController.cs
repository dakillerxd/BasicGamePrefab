using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody),typeof(SphereCollider), typeof(AudioSource))]
public class EnemyController : MonoBehaviour
{

    [Header("Enemy Settings")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] [Range (0, 100)] private float _xBoundry;
    [SerializeField] [Range (0, 100)] private float _zBoundry;
    [SerializeField] [Range (0, 100)] private float _yBoundry;

    [Header("References")]
    private Rigidbody enemyRb;
    private AudioSource enemyAs;
    private GameObject playerOb;


    
    void Start()
    {
        playerOb = GameObject.Find("Player");
        enemyRb = GetComponent<Rigidbody>();
        enemyAs = GetComponent<AudioSource>();
    }

    
    void Update()
    {

        AttackPlayer();
        CheckMapBoundry();
    }



    // -----------------------------------------------------Functions

    private void AttackPlayer() {

        Vector3 lookDirection = (playerOb.transform.position - transform.position).normalized;
        enemyRb.AddForce(_moveSpeed * Time.deltaTime * lookDirection);
    }


    private void CheckMapBoundry() {


        if (transform.position.x > _xBoundry) {
            Destroy(gameObject);
        }
        else if (transform.position.x < -_xBoundry) {
            Destroy(gameObject);
        }

        else if (transform.position.y < -_yBoundry) {
            Destroy(gameObject);
        }
        else if (transform.position.y < -_yBoundry) {
            Destroy(gameObject);
        }

        else if (transform.position.z < -_zBoundry) {
            Destroy(gameObject);
        }
        else if (transform.position.z < -_zBoundry) {
            Destroy(gameObject);
        }    
    } 
}

