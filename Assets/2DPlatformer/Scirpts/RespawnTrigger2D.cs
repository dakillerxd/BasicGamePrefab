using UnityEngine;

public class RespawnTrigger2D : MonoBehaviour
{
    
    public Transform respawnTarget;
    public Vector3 offset;
    public string tagToCheck = "Player";
    public bool resetSpeed = true;


   void OnTriggerEnter(Collider other)
   {
        if(other.CompareTag(tagToCheck))
        {
                other.transform.position = respawnTarget.position + offset;

                if(resetSpeed)
                {
                    other.GetComponent<Rigidbody>().velocity = Vector3.zero;
                }
        }
   }
}
