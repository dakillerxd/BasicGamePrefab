using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class Checkpoint2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color disabledColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;

    [Header("References")]
    [SerializeField] private ParticleSystem activateVfx;
    [SerializeField] private ParticleSystem deactivateVfx;
    [SerializeField] private AudioSource activeSfx;
    [SerializeField] private AudioSource deactivateSfx;


    [Header("Debug")]
    [ReadOnly] public bool active = false;


    private void Start() {
        GetComponent<SpriteRenderer>().color = active ? activeColor : disabledColor;
    }


    public void SetActive(bool state) {
        
        active = state;
        GetComponent<SpriteRenderer>().color = active ? activeColor : disabledColor;

        if (active) {
            SpawnParticleEffect(activateVfx);
        } else {
            SpawnParticleEffect(deactivateVfx);
        }
    }

    private void SpawnParticleEffect(ParticleSystem effect) {
        if (effect == null) return;
        ParticleSystem particleEffectInstance = Instantiate(effect, transform.position, Quaternion.identity);
        
    }
}
