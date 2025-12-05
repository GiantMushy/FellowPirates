using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Vector2 respawnPoint;
    private PlayerController Player;
    GameManager gameManager;

    void Start()
    {
        respawnPoint = transform.position;
    }

    public void SetCheckpoint(Vector2 newPoint)
    {
        respawnPoint = newPoint;
        Debug.Log("Checkpoint updated: " + respawnPoint);
    }

    public void Respawn()
    {
        transform.position = respawnPoint;

    }
}
