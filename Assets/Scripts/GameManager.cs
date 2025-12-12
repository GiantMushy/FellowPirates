using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
    public int enemyRewardAmount;

    public Vector3 savedCameraOffset;
    public bool hasSavedCameraOffset;
    public float fleeCooldownUntil;

    private bool pendingBattleReturn = false;
    private bool pendingChaseReturn = false;
    private bool pendingDeathReturn = false;
    private bool pendingBribeReturn = false;
    private bool pendingGoldRewardPopup = false;

    // respawn values
    public bool respawningFromCheckpoint = false;
    public Vector3 spawnPoint;
    public bool hasSpawnPoint = false;
    public int checkpointGoldCoins = 0;
    public int checkpointHealthInventory = 0;


    public string currentLevelName;

    public EnemyController chasingEnemy;
    public int enemyMaxHealth = 6;   // only 6 because i dont work with floats when losing half
    public int enemyHealth = 6;



    public HashSet<string> fleeDisabledEnemies = new HashSet<string>();  // flee banned enemies
    public bool playerCaughtWhileFleeing = false;

    public HashSet<string> defeatedEnemies = new HashSet<string>(); // to not render the dead dudes
    public Dictionary<string, int> enemyHealthById = new Dictionary<string, int>(); // to keep lives persistent per enemy

    public string currentEnemyId;

    // for persistence 
    public List<Vector3> collectedItemPositions = new List<Vector3>();
    public Dictionary<string, Vector3> enemyPositionBeforeBattle = new Dictionary<string, Vector3>();

    public EnemyDialougeController enemyDialogueController;

    public HashSet<string> enemiesWithIntroDialogue = new HashSet<string>();

    public HashSet<string> activatedCheckpoints = new HashSet<string>();


    public GameObject itemsCanvas;


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
        StartCoroutine(StartBattleWithDialogue(player, enemy));
    }

    private IEnumerator StartBattleWithDialogue(PlayerController player, EnemyController enemy)
    {
        if (Camera.main != null)
        {
            savedCameraOffset = Camera.main.transform.position - player.transform.position;
            hasSavedCameraOffset = true;
        }

        currentEnemy = enemy;
        currentEnemyId = enemy.enemyId;
        lastEnemyPosition = enemy.transform.position;

        returnSceneName = SceneManager.GetActiveScene().name;
        preBattlePosition = player.transform.position;

        enemyBribeCost = enemy.bribeCost;
        enemyRewardAmount = enemy.rewardMoney;

        if (!string.IsNullOrEmpty(currentEnemyId) &&
            enemyHealthById.TryGetValue(currentEnemyId, out var savedHp))
        {
            enemyHealth = Mathf.Clamp(savedHp, 0, enemyMaxHealth);
        }
        else
        {
            enemyHealth = enemyMaxHealth;
        }

        player.PrepareForBattle();

        enemyPositionBeforeBattle.Clear();
        var enemies = FindObjectsOfType<EnemyController>();
        foreach (var e in enemies)
        {
            if (!string.IsNullOrEmpty(e.enemyId))
            {
                enemyPositionBeforeBattle[e.enemyId] = e.transform.position;
            }
        }


        bool isFirstEncounter = !string.IsNullOrEmpty(currentEnemyId) &&
                            !enemiesWithIntroDialogue.Contains(currentEnemyId);

        // var dialogue = FindObjectOfType<EnemyDialougeController>();
        if (isFirstEncounter)
        {
            enemiesWithIntroDialogue.Add(currentEnemyId);

            if (enemyDialogueController != null)
            {
                // freeze gameplay
                Time.timeScale = 0f;

                Debug.Log("enemy.enemyId " + enemy.enemyId);

                switch (enemy.enemyId)
                {
                    case "1":
                        Debug.Log("Starting dialouge 1");

                        enemyDialogueController.StartFirstEnemyDialouge();
                        break;
                    case "2":
                        Debug.Log("Starting dialouge 2");

                        enemyDialogueController.StartSecondEnemyDialouge();
                        break;
                    case "3":
                        Debug.Log("Starting dialouge 3");

                        enemyDialogueController.StartThirdEnemyDialouge();
                        break;
                    case "4":
                        Debug.Log("Starting dialouge 4");
                        enemyDialogueController.StartBlackbeardEnemyDialouge();
                        break;
                    default:
                        enemyDialogueController.DisableAllDialogue();
                        break;
                }

                yield return new WaitUntil(() => enemyDialogueController.IsDialogueFinished);

                Time.timeScale = 1f;
            }
        }

        SceneManager.LoadScene(enemy.battleSceneName);
    }


    public void EndBattleWon()
    {
        pendingBattleReturn = true;
        pendingChaseReturn = false;
        pendingDeathReturn = false;


        fleeCooldownUntil = Time.time + 2f;

        goldCoins += enemyRewardAmount;

        pendingGoldRewardPopup = true;

        if (!string.IsNullOrEmpty(currentEnemyId))
        {
            enemyHealthById.Remove(currentEnemyId);
            defeatedEnemies.Add(currentEnemyId);
            fleeDisabledEnemies.Remove(currentEnemyId);
        }

        SceneManager.LoadScene(returnSceneName);
    }

    public void EndBattleBribed()
    {
        pendingBattleReturn = true;
        pendingChaseReturn = false;
        pendingDeathReturn = false;
        pendingBribeReturn = true;

        fleeCooldownUntil = Time.time + 2f;
        SceneManager.LoadScene(returnSceneName);
    }

    public void EndBattlePlayerDied()
    {
        pendingBattleReturn = true;
        pendingChaseReturn = false;
        pendingDeathReturn = true;

        enemyHealthById.Remove(currentEnemyId);
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
        enemyDialogueController = FindObjectOfType<EnemyDialougeController>();

        var player = FindObjectOfType<PlayerController>();

        if (respawningFromCheckpoint)
        {
            respawningFromCheckpoint = false;
            RestoreCheckpointState();
            player.transform.position = spawnPoint;
            player.UpdateSprite();
            player.UpdateHeartsUI();
            return;
        }

        if (!pendingBattleReturn) return;
        if (scene.name != returnSceneName) return;

        pendingBattleReturn = false;

        if (player == null) return;

        if (pendingDeathReturn)
        {
            pendingDeathReturn = false;

            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.OnBattleDeathReturn();
            }

            return;
        }
        else
        {
            player.transform.position = preBattlePosition;
        }

        if (hasSavedCameraOffset && Camera.main != null)
        {
            Camera.main.transform.position = player.transform.position + savedCameraOffset;
        }

        if (pendingGoldRewardPopup)
        {
            pendingGoldRewardPopup = false;
            player.ShowBattleGoldReward();
        }

        if (health < maxHealth && healthInventory > 0)
        {
            player.TryAutoHealFromBattle();
        }


        if (pendingBribeReturn)
        {
            pendingBribeReturn = false;
            player.StartCoroutine(HandleBribedEnemyReturn(player.transform));
        }

        if (pendingChaseReturn)
        {
            pendingChaseReturn = false;
            StopAllCoroutines();
            player.StartCoroutine(StartChaseAfterReturn(player.transform));
        }

        RestoreEnemyPositionAfterBattle();

        if (itemsCanvas == null)
            itemsCanvas = GameObject.Find("ItemsCanvas");

        CleanupCollectedItems(scene);
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

        StopAllCoroutines();
    }

    private IEnumerator HandleBribedEnemyReturn(Transform playerTransform)
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
            best.StartBribeFlee(playerTransform);
        }
    }

    public void AddCollectedItemPosition(Vector3 pos)
    {
        collectedItemPositions.Add(pos);
    }

    public bool WasItemCollected(Vector3 pos, float tolerance = 0.01f)
    {
        foreach (var p in collectedItemPositions)
        {
            if (Vector3.Distance(p, pos) <= tolerance)
            {
                return true;
            }
        }
        return false;
    }

    private void CleanupCollectedItems(Scene scene)
    {
        if (scene.name != "Alpha_Test_Level")
        {
            return;
        }

        string[] tags = { "GoldPickup", "HealthPickup" };

        foreach (var tag in tags)
        {
            var items = GameObject.FindGameObjectsWithTag(tag);
            foreach (var item in items)
            {
                if (WasItemCollected(item.transform.position))
                {
                    Destroy(item);
                }
            }
        }
    }

    public void SaveCheckpointState()
    {
        checkpointGoldCoins = goldCoins;
        checkpointHealthInventory = healthInventory;
    }

    public void RestoreCheckpointState()
    {
        goldCoins = checkpointGoldCoins;
        healthInventory = checkpointHealthInventory;
    }

    public void RestoreEnemyPositionAfterBattle()
    {
        var enemies = FindObjectsOfType<EnemyController>();
        foreach (var e in enemies)
        {
            if (string.IsNullOrEmpty(e.enemyId))
                continue;


            if (enemyPositionBeforeBattle.TryGetValue(e.enemyId, out var pos))
            {
                e.transform.position = pos;
            }
        }
    }

    public void SetItemsCanvasActive(bool active)
    {
        if (itemsCanvas == null)
            itemsCanvas = GameObject.Find("ItemsCanvas");

        if (itemsCanvas != null)
            itemsCanvas.SetActive(active);
    }

}
