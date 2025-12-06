using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    //  global player data 
    public int maxHealth = 3;
    public int health = 3;
    public int healthInventory = 0;
    public int goldCoins = 0;

    //  battle state 
    public string returnSceneName;
    public Vector3 preBattlePosition;
    public Vector3 lastEnemyPosition;
    public EnemyController currentEnemy;
    public int enemyBribeCost;

    public Vector3 savedCameraOffset;
    public bool hasSavedCameraOffset;
    public float fleeCooldownUntil;

    private bool pendingBattleReturn = false;
    private bool pendingChaseReturn = false;
    private bool pendingDeathReturn = false;

    public Vector3 spawnPoint;
    public bool hasSpawnPoint = false;


    public string currentLevelName;

    public EnemyController chasingEnemy;
    public int enemyMaxHealth = 6;   // only 6 because i dont work with floats when losing half
    public int enemyHealth = 6;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentLevelName = SceneManager.GetActiveScene().name;

    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        pendingBattleReturn = false;
        pendingChaseReturn = false;

        var sceneName = SceneManager.GetActiveScene().name;
        currentLevelName = sceneName;
        SceneManager.LoadScene(sceneName);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        pendingBattleReturn = false;
        pendingChaseReturn = false;

        currentLevelName = "MainMenu";
        SceneManager.LoadScene("MainMenu");
    }


    public void StartBattle(PlayerController player, EnemyController enemy)
    {
        if (Camera.main != null)
        {
            savedCameraOffset = Camera.main.transform.position - player.transform.position;
            hasSavedCameraOffset = true;
        }

        currentEnemy = enemy;
        lastEnemyPosition = enemy.transform.position;

        returnSceneName = SceneManager.GetActiveScene().name;
        preBattlePosition = player.transform.position;

        enemyBribeCost = enemy.bribeCost;

        if (enemyHealth <= 0)
        {
            enemyHealth = enemyMaxHealth;
        }

        player.PrepareForBattle();

        SceneManager.LoadScene("FightDemo");
    }

    public void EndBattleWon()
    {
        pendingBattleReturn = true;
        pendingChaseReturn = false;
        pendingDeathReturn = false;

        fleeCooldownUntil = Time.time + 2f;
        SceneManager.LoadScene(returnSceneName);
    }

    public void EndBattleBribed()
    {
        pendingBattleReturn = true;
        pendingChaseReturn = false;
        pendingDeathReturn = false;

        fleeCooldownUntil = Time.time + 2f;
        SceneManager.LoadScene(returnSceneName);
    }

    public void EndBattlePlayerDied()
    {
        pendingBattleReturn = true;
        pendingChaseReturn = false;
        pendingDeathReturn = true;

        SceneManager.LoadScene(returnSceneName);
    }

    public void UseHealthItem()
    {
        if (healthInventory > 0 && health < maxHealth)
        {
            healthInventory--;
            health++;
        }
    }

    public void StartChase()
    {
        pendingBattleReturn = true;
        pendingChaseReturn = true;
        pendingDeathReturn = false;

        fleeCooldownUntil = Time.time + 2f;
        SceneManager.LoadScene(returnSceneName);
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!pendingBattleReturn) return;
        if (scene.name != returnSceneName) return;

        pendingBattleReturn = false;

        var player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        if (pendingDeathReturn)
        {
            pendingDeathReturn = false;

            var respawn = player.GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                respawn.Respawn();
            }

            health = maxHealth;
        }
        else
        {
            player.transform.position = preBattlePosition;
        }

        if (hasSavedCameraOffset && Camera.main != null)
        {
            Camera.main.transform.position = player.transform.position + savedCameraOffset;
        }

        if (pendingChaseReturn)
        {
            pendingChaseReturn = false;
            player.StartCoroutine(StartChaseAfterReturn(player.transform));
        }
    }


    private IEnumerator StartChaseAfterReturn(Transform playerTransform)
    {
        yield return null;

        var enemies = FindObjectsOfType<EnemyController>();
        if (enemies.Length == 0) yield break;

        EnemyController best = null;
        float bestDist = float.MaxValue;

        foreach (var e in enemies)
        {
            float d = (e.transform.position - lastEnemyPosition).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = e;
            }
        }

        if (best != null)
        {
            chasingEnemy = best;
            best.StartChasing(playerTransform);
        }
    }

    public void CancelChase()
    {
        if (chasingEnemy != null)
        {
            chasingEnemy.StopChase();
            chasingEnemy = null;
        }


        ChaseTime chaseUI = FindObjectOfType<ChaseTime>();
        if (chaseUI != null)
        {
            chaseUI.ForceStopChaseUI();
        }
    }

}
