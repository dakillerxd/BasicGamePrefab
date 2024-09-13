using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class ElevatorMovement : MonoBehaviour
{
    public bool movePlatform = true;
    public bool rotatePlatform = false;


    [Header("Moving Platform")]
    public Vector3 movementAxis = Vector3.right;
    public float movementDistance = 5.0f;
    public float movementTime = 5.0f;
    public float stopTime = 2.5f;

    public enum RotationDirections
    {
        XAxis, YAxis, ZAxis
    };
    
    [Header("Rotating Platform")]
    public Transform rotatingObject;
    public RotationDirections rotationDirection = RotationDirections.YAxis;
    public float rotationSpeed = 15f;

    private Vector3 pointA;
    private Vector3 pointB;
    private bool movingToB = true;
    private float timer = 0.0f;

    void Start()
    {
        pointA = transform.position;
        pointB = transform.position + (movementAxis.normalized * movementDistance);
    }

    void Update()
    {
        if(rotatePlatform && rotatingObject)
        {
            RotatingPlatform();
        }

        if(movePlatform)
        {
            MovingPlatform();
        }
    }

    void RotatingPlatform()
    {
        
        switch (rotationDirection)
        {
            default:
                rotatingObject.Rotate(Vector3.up*rotationSpeed*Time.deltaTime);
            break; 

            case RotationDirections.XAxis:
                rotatingObject.Rotate(Vector3.right*rotationSpeed*Time.deltaTime);
            break;

            case RotationDirections.YAxis:
                rotatingObject.Rotate(Vector3.up*rotationSpeed*Time.deltaTime);
            break;

            case RotationDirections.ZAxis:
                rotatingObject.Rotate(Vector3.forward*rotationSpeed*Time.deltaTime);
            break;
        }
    }

    void MovingPlatform()
    {
        timer += Time.deltaTime;

        if (timer < stopTime)
        {
            return;
        }

        float t = Mathf.Clamp01((timer - stopTime) / movementTime);
        transform.position = Vector3.Lerp(pointA, pointB, t);

        if (t >= 1.0f)
        {
            timer = 0.0f;
            movingToB = !movingToB;
            pointA = transform.position;
            pointB = transform.position + (movementAxis.normalized * movementDistance * (movingToB ? 1 : -1));
        }

    }
}

