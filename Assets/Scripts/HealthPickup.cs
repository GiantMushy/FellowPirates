using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(PulseEffect.sprite_pulse(spriteRenderer, num_pulses: -1, speed: 0.7f, intensity: 1.7f));
    }
}