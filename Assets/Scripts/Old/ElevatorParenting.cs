
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ElevatorParenting : MonoBehaviour
{
    public string triggerTag = "Player";


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {

        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {

        }
    }

}
