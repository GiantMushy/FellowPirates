using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerRespawn : MonoBehaviour
{
    public Vector2 respawnPoint;
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private DamageTypeController damageTypeController;
    GameManager gameManager;
    public TrailRenderer trail;

    void Awake()
    {
        // Cache components on the same GameObject
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageTypeController = GetComponent<DamageTypeController>();
    }

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
        // // Move player back to checkpoint
        // transform.position = respawnPoint;

        // // Re-enable sprite
        // if (spriteRenderer != null)
        // {
        //     spriteRenderer.enabled = true;
        // }

        // // Re-enable damage controller if you disabled it on death
        // if (damageTypeController != null)
        // {
        //     damageTypeController.enabled = true;
        // }

        // if (gameManager != null)
        // {
        //     gameManager.CancelChase();
        //     gameManager.health = gameManager.maxHealth;
        // }

        // if (playerController != null)
        // {
        //     playerController.UpdateSprite();
        //     playerController.UpdateHeartsUI();
        // }

        // trail.Clear();


        if (gameManager != null)
        {
            gameManager.CancelChase();
            gameManager.health = gameManager.maxHealth;
            gameManager.collectedItemPositions.Clear();
           
            gameManager.respawningFromCheckpoint = true;
        }


        SceneManager.LoadScene("Alpha_Test_Level");
    }
}
