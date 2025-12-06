using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Vector2 respawnPoint;
    private PlayerController Player;
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            if (!gameManager.hasSpawnPoint)
            {
                gameManager.spawnPoint = transform.position;
                gameManager.hasSpawnPoint = true;
            }

            respawnPoint = gameManager.spawnPoint;
        }
        else
        {
            respawnPoint = transform.position;
        }
    }

    public void SetCheckpoint(Vector2 newPoint)
    {
        respawnPoint = newPoint;

        if (gameManager != null)
        {
            gameManager.spawnPoint = newPoint;
            gameManager.hasSpawnPoint = true;
        }

        Debug.Log("Checkpoint updated: " + respawnPoint);
    }

    public void Respawn()
    {
        transform.position = respawnPoint;

        if (gameManager != null)
        {
            gameManager.CancelChase();
        }

    }
}
