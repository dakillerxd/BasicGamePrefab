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
    [SerializeField] private bool active = false;


    private void Start() {
        GetComponent<SpriteRenderer>().color = active ? activeColor : disabledColor;
    }


    public void SetActive(bool state) {
        
        active = state;
        GetComponent<SpriteRenderer>().color = active ? activeColor : disabledColor;
    
    }

    [Button] private void ToggleActive() => SetActive(!active);

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Player")) {
            other.gameObject.GetComponent<PlayerController2D>().SetCheckpoint(transform.position);
            SetActive(true);
        }
    }

}
