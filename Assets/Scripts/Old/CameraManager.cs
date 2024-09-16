using System;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class CameraManager : MonoBehaviour
{

    [Header("Camera Settings")]
    public CameraState _currentCameraState;
    public enum CameraState
    {
        Static,
        Free,
        FollowTarget,
    }
    private GameObject cameraFocalPoint;
    private GameObject mainCamera;
    [SerializeField] [Range (0, 1)] private float _moveSpeed;
    [SerializeField] [Range (0, 1)] private float _rotateSpeed;
    [SerializeField] [Range (0, 1)] private float _zoomSpeed;

    [Header("Target Settings")]
    [SerializeField] private Vector3 _targetOffset;
    [SerializeField] private GameObject targetObject;
    

    [Header("Just For The Inspector")]
    [SerializeField] private float _mouseHorizontalInput;
    [SerializeField] private float _mouseVerticalInput;
    [SerializeField] private float _mouseWheelInput;
    [SerializeField] private bool _mouseRotateInput;

    
    void Start()
    {
        cameraFocalPoint = GameObject.Find("CameraFocalPoint");
        mainCamera = GameObject.Find("MainCamera");
        targetObject = GameObject.Find("Player");
        SetCameraState(CameraState.Free);
    }

    void Update() {

        // Get the input in Update so there is no input lag
        _mouseVerticalInput = Input.GetAxis("Mouse Y"); 
        _mouseHorizontalInput = Input.GetAxis("Mouse X");   
        _mouseWheelInput = Input.GetAxis("Mouse ScrollWheel");
        _mouseRotateInput = Input.GetMouseButton(1);
        CycleCameraStates(KeyCode.E, KeyCode.Q);
    }

    void LateUpdate()
    {
        // Control the Camera movement in LateUpdate so it will move smoothly after the target

        ControlCamera();
    }

    void ControlCamera() {

        
        switch (_currentCameraState)
        {
            case CameraState.Static:

                break;
            case CameraState.Free:
                MoveCamera();
                ZoomCamera();
                RotateCamera();
                break;
            case CameraState.FollowTarget:
                FollowTarget();
                ZoomCamera();
                RotateCamera();
                break;
            default:
                Debug.LogError("Unexpected camera state: " + _currentCameraState);
                break;
        }

    }


    void CycleCameraStates(KeyCode CycleDown,KeyCode CycleUp) {

        int numCameraStates = Enum.GetValues(typeof(CameraState)).Length;
        // int numCurrentCameraState = (int)_currentCameraState;


        if (Input.GetKeyUp(CycleUp)) {
            _currentCameraState ++;
            
            if (_currentCameraState > (CameraState)numCameraStates-1) { _currentCameraState = 0;}
            Debug.Log("Current Camera State: " + _currentCameraState);

        }
        else if (Input.GetKeyUp(CycleDown)) {
            _currentCameraState --;
            
            if (_currentCameraState < 0) { _currentCameraState = (CameraState)numCameraStates-1; }
            Debug.Log("Current Camera State: " + _currentCameraState);
        }

    }


    void SetCameraState(CameraState cameraState) { _currentCameraState = cameraState; }

    void MoveCamera() {
        if (!_mouseRotateInput) { // Check not rotating 

        // Using translate so i can change the effect the local position
        cameraFocalPoint.transform.Translate(_mouseHorizontalInput * _moveSpeed, 0, _mouseVerticalInput * _moveSpeed, Space.Self);

        }
    }

    void RotateCamera() {
        if (_mouseRotateInput) { // Check rotating

            Vector3 inputDirection = new(_mouseVerticalInput, _mouseHorizontalInput, 0.0f);
            cameraFocalPoint.transform.Rotate(inputDirection * _rotateSpeed, Space.Self);

            Vector3 currentRotation = cameraFocalPoint.transform.eulerAngles;
            currentRotation.z = 0.0f;
            cameraFocalPoint.transform.eulerAngles = currentRotation;
        }
    }

    void ZoomCamera() {
        float distance = Vector3.Distance(mainCamera.transform.position, cameraFocalPoint.transform.position);
        mainCamera.transform.Translate(0, _mouseWheelInput * _zoomSpeed, 0, Space.Self);
    }

    void FollowTarget() {
        if (targetObject) {
            cameraFocalPoint.transform.position = targetObject.transform.position;
        } 
    }
}
