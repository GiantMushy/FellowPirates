using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    public ParticleSystem moveParticles;
    public float movementThreshold = 0.1f;

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;

        if (moveParticles != null)
            moveParticles.Stop();
    }

    void Update()
    {
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;

        if (speed > movementThreshold)
        {
            if (!moveParticles.isPlaying)
                moveParticles.Play();
        }
        else
        {
            if (moveParticles.isPlaying)
                moveParticles.Stop();
        }

        lastPosition = transform.position;
    }
    
}
