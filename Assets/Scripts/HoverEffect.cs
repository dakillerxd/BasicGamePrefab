using UnityEngine;

public class MainMenuCameraHover : MonoBehaviour
{
    [Header("Hover Settings")]
    public float hoverSpeed = 0.5f;
    public float hoverAmount = 0.1f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 0.2f;
    public float rotationAmount = 2f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float hoverTime;
    private float rotationTime;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        hoverTime = Random.value * Mathf.PI * 2; 
        rotationTime = Random.value * Mathf.PI * 2; 
    }

    private void Update()
    {
        // Update the time values
        hoverTime += hoverSpeed * Time.deltaTime;
        rotationTime += rotationSpeed * Time.deltaTime;

        // Calculate and apply the hover offset
        float hoverOffset = Mathf.Sin(hoverTime) * hoverAmount;
        Vector3 newPosition = initialPosition + Vector3.up * hoverOffset;
        transform.position = newPosition;

        // Calculate and apply the rotation offset
        float rotationOffset = Mathf.Sin(rotationTime) * rotationAmount;
        Quaternion newRotation = initialRotation * Quaternion.Euler(0, rotationOffset, 0);
        transform.rotation = newRotation;
    }
}