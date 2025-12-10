using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public GameObject bridge;

    void Start()
    {
        if (bridge != null) bridge.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (bridge != null) bridge.SetActive(true);
            
            PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                respawn.SetCheckpoint(transform.position);
            }

        }
    }
}
