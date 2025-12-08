using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //  global player data 
    public PlayerController player;
    public int healthInventory = 0;
    public int goldCoins = 0;

    // global enemy data
    public List<EnemyController> Enemies;
    public EnemyController activeEnemy;

    //  battle state 
    public GameObject fightPrefab; // Assign in inspector
    public float fleeCooldownUntil;

    private Vector3 savedCameraPosition;
    public bool hasSavedCameraOffset;
    private bool pendingChaseReturn = false;
    private bool pendingDeathReturn = false;

    public Vector3 spawnPoint;
    public bool hasSpawnPoint = false;
    public string currentLevelName;

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
        player = GetComponent<PlayerController>();
        // If player is a separate GameObject, make sure it persists
        if (player != null && player.gameObject != this.gameObject)
        {
            DontDestroyOnLoad(player.gameObject);
        }
    }



    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        pendingChaseReturn = false;

        var sceneName = SceneManager.GetActiveScene().name;
        currentLevelName = sceneName;
        SceneManager.LoadScene(sceneName);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        pendingChaseReturn = false;

        currentLevelName = "MainMenu";
        SceneManager.LoadScene("MainMenu");
    }

    public void StartBattle(PlayerController player, EnemyController enemy)
    {
        activeEnemy = enemy;
        player.PrepareForBattle();

        // Save camera position
        if (Camera.main != null)
        {
            savedCameraPosition = Camera.main.transform.position;
        }

        // Enable fight prefab and move camera
        if (fightPrefab != null)
        {
            fightPrefab.SetActive(true);
            
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3(
                    fightPrefab.transform.position.x,
                    fightPrefab.transform.position.y,
                    Camera.main.transform.position.z
                );
            }
        }
    }
    
    public void EndBattleWon()
    {
        activeEnemy.isDefeated = true;
        EndBattle();
    }

    public void EndBattleBribed()
    {
        activeEnemy.isDefeated = true;
        EndBattle();
    }

    public void EndBattlePlayerDied()
    {
        pendingDeathReturn = true;
        EndBattle();
    }

    public void UseHealthItem()
    {
        player.UseHealthItem();
    }

    public void StartChase()
    {
        pendingChaseReturn = true;
        fleeCooldownUntil = Time.time + 2f;
        EndBattle();
    }

    private void EndBattle()
    {
        // Disable fight prefab
        if (fightPrefab != null)
        {
            fightPrefab.SetActive(false);
        }

        // Return camera to saved position
        if (Camera.main != null)
        {
            Camera.main.transform.position = savedCameraPosition;
        }

        // Handle post-battle logic
        if (player == null) return;
        
        if (pendingDeathReturn)
        {
            pendingDeathReturn = false;
            player.Respawn();
        }

        if (pendingChaseReturn)
        {
            pendingChaseReturn = false;
            StopAllCoroutines();
            player.StartCoroutine(StartChaseAfterReturn(player.transform));
        }
        else
        {
            fleeCooldownUntil = Time.time + 5f;
        }
        
        player.EnableControl();
    }


    private IEnumerator StartChaseAfterReturn(Transform playerTransform)
    {
        yield return null;

        if (Enemies.Count == 0) yield break;
 
        EnemyController curr = null;
        float bestDist = float.MaxValue; 
 
        foreach (var enemy in Enemies)
        {// Fund the closest enemy and initiate chase
            if (enemy.isDefeated) continue;
            if (enemy.isChasing) curr = enemy;
            float d = (enemy.transform.position - player.transform.position).sqrMagnitude;
            if (d < bestDist) 
            { 
                bestDist = d; 
                curr = enemy;
            } 
        } 

        if (curr != null)
        {
            activeEnemy = curr;
            curr.StartChasing(playerTransform);
        }
    }

    public void CancelChase()
    {
        if (activeEnemy != null)
        {
            activeEnemy.StopChase();
            activeEnemy = null;
        }

        ChaseTime chaseUI = FindFirstObjectByType<ChaseTime>();
        if (chaseUI != null)
        {
            chaseUI.ForceStopChaseUI();
        }

        StopAllCoroutines();
    }

    public int GetPlayerHealth()
    {
        return player.GetHealth();
    }
}
