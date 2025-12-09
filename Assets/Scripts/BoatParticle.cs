using UnityEngine;

public class BoatParticle : MonoBehaviour
{
    public ParticleSystem wakeParticles;
    public Rigidbody2D rb;

    void Update()
    {
        if (wakeParticles == null || rb == null) return;

        var emission = wakeParticles.emission;

        float speed = rb.linearVelocity.magnitude;

        emission.rateOverTime = speed * 20f; 
    }
}
