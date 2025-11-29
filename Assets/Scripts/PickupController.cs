using UnityEngine;

public class PickupController : MonoBehaviour
{
    public float respawnTime = -1f;

    void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        if (respawnTime > 0f)
        {
            Invoke(nameof(Respawn), respawnTime);
        }

        // if (other.tag == "HealthIt")
    }

    void Respawn()
    {
        gameObject.SetActive(true);
    }
}