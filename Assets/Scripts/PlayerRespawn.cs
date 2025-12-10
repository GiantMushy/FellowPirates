using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


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

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = false;
            StartCoroutine(EnableTrailNextFrame());
        }
    }

    private IEnumerator EnableTrailNextFrame()
    {
        yield return null;
        if (trail != null)
            trail.emitting = true;
    }

    public void SetCheckpoint(Vector2 newPoint)
    {
        respawnPoint = newPoint;

        if (gameManager != null)
        {
            gameManager.spawnPoint = newPoint;
            gameManager.hasSpawnPoint = true;
            gameManager.SaveCheckpointState();
        }

        Debug.Log("Checkpoint updated: " + respawnPoint);
    }

    public void Respawn()
    {
        if (gameManager != null)
        {
            gameManager.enemiesWithIntroDialogue.Clear();
            gameManager.CancelChase();
            gameManager.health = gameManager.maxHealth;
            gameManager.collectedItemPositions.Clear();
            gameManager.respawningFromCheckpoint = true;
        }


        SceneManager.LoadScene("Alpha_Test_Level");
    }
}
