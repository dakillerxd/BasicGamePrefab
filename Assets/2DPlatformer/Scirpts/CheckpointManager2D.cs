using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckpointManager2D : MonoBehaviour
{
    public static CheckpointManager2D Instance { get; private set; }

    [Header("Settings")]
    
    [SerializeField] private Color disabledCheckpointColor = Color.white;
    [SerializeField] private Color activeCheckpointColor = Color.green;

    [SerializeField] [ReadOnly] public Vector2 playerSpawnPoint;
    [SerializeField] [ReadOnly] public GameObject activeCheckpoint;
    [SerializeField] [ReadOnly] private GameObject[] checkpointList;
    



    
    private void Awake() {

       if (Instance != null && Instance != this) {

            Destroy(gameObject);

        } else {

            Instance = this;
        }

    }

    private void Start() {

        FindAllCheckpoints();
    }

    private void FindAllCheckpoints() {

        checkpointList = GameObject.FindGameObjectsWithTag("Checkpoint");

        checkpointList = checkpointList.ToArray();
        Debug.Log($"Found {checkpointList.Length} checkpoints in the scene.");
    }


    public void SetSpawnPoint(Vector2 newSpawnPoint) {

        playerSpawnPoint = newSpawnPoint;
        Debug.Log("Set spawn point to: " + playerSpawnPoint);
    }



    public void ActivateCheckpoint(GameObject checkpoint) {

        DeactivateLastCheckpoint();
        activeCheckpoint = checkpoint;
        activeCheckpoint.GetComponent<SpriteRenderer>().color = activeCheckpointColor;
    }


    private void DeactivateLastCheckpoint() {

        if (!activeCheckpoint) return;
        activeCheckpoint.GetComponent<SpriteRenderer>().color = disabledCheckpointColor;
        activeCheckpoint = null;
        
    }

}
