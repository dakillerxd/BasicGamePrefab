using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{

    [Header("Settings")]
    public bool destroyTriggerItem = false;
    public string triggerTag = "Player";
    public float initialDelay = 0f;
    public float delayedTriggerTime = 0f;
    public bool triggerOnce = false;
    private bool triggered = false;

    [Header("Events")]
    public UnityEvent[] triggeredEvents;


    private void OnTriggerEnter(Collider other)
    {


        if(other.CompareTag(triggerTag) && !triggered)
        {
            triggered = true;
            if (destroyTriggerItem) {Destroy(other.gameObject);}
            StartCoroutine("CallEvent");
        }
        
        
    }

    private IEnumerator CallEvent()
    {
        yield return new WaitForSeconds(initialDelay); 

        for (int i = 0; i < triggeredEvents.Length; i++)
        {
            triggeredEvents[i].Invoke();
        
            yield return new WaitForSeconds(delayedTriggerTime); 

        }

        if(triggerOnce)
        {
            Destroy(gameObject);
        }
        else
        {
            triggered = false;
        }
    }
}