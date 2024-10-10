using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class Checkpoint2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color disabledColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;


    [Header("Debug")]
    [ReadOnly] public bool active = false;


    private void Start() {
        GetComponent<SpriteRenderer>().color = active ? activeColor : disabledColor;
    }


    public void SetActive(bool state) {
        
        active = state;
        GetComponent<SpriteRenderer>().color = active ? activeColor : disabledColor;
    
    }

    // private void OnCollisionEnter2D(Collision2D other) {
    //     if (other.gameObject.CompareTag("Player")) {
    //         SetActive(true);
    //     }
    // }

}
