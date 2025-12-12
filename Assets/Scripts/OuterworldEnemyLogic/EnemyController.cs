using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
// using static State;

public class EnemyController : MonoBehaviour
{
    private ShipController shipController;
    public string enemyId;

    // CHASSSINNNNG LOGIC VARIABLES
    public GridManager grid;
    private int path_index;
    private List<State> path;
    public Transform player;
    public float replan_interval = 0.3f;
    private float replan_timer = 0;

    private bool chasing = false;
    public float chase_delay = 3f;
    private bool waiting_to_chase = false;

    // next 3 are to get rid of jitter
    // where turning last
    // 1 = turning port
    // -1 turning starboard 
    // 0 = not turning
    private float last_turn_dir = 0f;
    public float turn_deadzone = 5f; // minimum angle off needed before changing angle
    public float turn_release_zone = 2f; // stop turning 

    public ChaseTime chaseTimeController;

    // reward money
    public int rewardMoney = 10;

    // for bribe
    public int bribeCost = 1;
    private Collider2D enemyCollider;
    private bool bribeFleeing = false;
    public Tilemap oceanTilemap;

    [Header("Explosion")]
    public GameObject deathExplosionPrefab;
    public Vector3 deathExplosionOffset = Vector3.zero;

    [Header("Battle")]
    public string battleSceneName = "FightDemo";



    void Start()
    {
        var gm = GameManager.Instance;
        if (gm != null && gm.defeatedEnemies.Contains(enemyId))
        {
            if (gm.currentEnemyId == enemyId)
            {
                StartCoroutine(PlayDeathExplosionAndDestroy(gm));
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            shipController = GetComponent<ShipController>();
            enemyCollider = GetComponent<Collider2D>();
            if (shipController == null)
            {
                Debug.LogError("EnemyController requires a ShipController component!");
            }

            chaseTimeController.timeWait = chase_delay;
        }
    }


    private IEnumerator PlayDeathExplosionAndDestroy(GameManager gm)
    {
        yield return null;

        if (deathExplosionPrefab != null)
        {
            Vector3 spawnPos = transform.position + deathExplosionOffset;
            Instantiate(deathExplosionPrefab, spawnPos, Quaternion.identity);
        }

        gm.currentEnemyId = null;

        Destroy(gameObject);
    }

    void Update()
    {
        if (chasing)
        {
            FollowPath();
            ReplanIfNeeded();
        }
        else if (bribeFleeing)
        {
            FollowBribeFleePath();
        }
    }

    public void StartChasing(Transform target, bool fromBattle = true)
    {
        if (chasing || waiting_to_chase)
        {
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }


        player = target;
        waiting_to_chase = true;
        StartCoroutine(Chase(fromBattle));
    }

    private IEnumerator Chase(bool fromBattle = true)
    {
        if (fromBattle)
        {
            chaseTimeController.timeWait = chase_delay;
            chaseTimeController.startChaseCountodwn();

            yield return new WaitForSeconds(chase_delay);

            chaseTimeController.StartChase();
        }


        waiting_to_chase = false;

        Debug.Log("Enemy chasing started");

        State start = grid.GetStateFromWorldPos(transform.position);
        State goal = grid.GetStateFromWorldPos(player.position);

        if (start == null || goal == null)
        {
            Debug.LogWarning("Start or end outside of bounds");
            chasing = false;
            chaseTimeController.ForceStopChaseUI();
            yield break;
        }

        path = AStar.Search(start, goal);
        path_index = 0;

        if (path != null && path.Count > 0)
        {
            chasing = true;
            shipController.SetAccelerate(true);
            shipController.SetDecelerate(false);
        }
        else
        {
            chasing = false;
            Debug.Log("Enemy did not find a path to player");
        }

        float timer = 0f;
        float chaseTime = 15f;
        while (timer < chaseTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        StopChase();
    }

    public void StopChase()
    {
        var gameManager = GameManager.Instance;

        Debug.Log("Stopped chasing");
        chasing = false;
        shipController.SetAccelerate(false);
        shipController.SetDecelerate(true);
        shipController.SetTurnPort(false);
        shipController.SetTurnStarboard(false);


        // testing
        float dist = Vector3.Distance(transform.position, player.position);
        Debug.Log("dist: " + dist);
        if (dist <= 1.5f)
        {
            Debug.Log("started battle from stop chase");
            var playerController = player.GetComponent<PlayerController>();
            gameManager.StartBattle(playerController, this);
        }
    }

    private void FollowPath()
    {
        if (path == null || path_index >= path.Count)
        {
            // stop following path
            StopChase();
            return;
        }

        State current_state = path[path_index];

        UnityEngine.Vector3 dist_to_player = grid.GetWorldPosFromState(current_state) - transform.position;
        dist_to_player.z = 0;

        float dist = dist_to_player.magnitude;

        if (dist < 1f)
        {
            path_index++;
            if (path_index >= path.Count)
            {
                ReplanIfNeeded();
            }
            return;
        }

        UnityEngine.Vector3 forward = transform.up;
        UnityEngine.Vector3 dir = dist_to_player.normalized;

        float angle = UnityEngine.Vector3.SignedAngle(forward, dir, UnityEngine.Vector3.forward);
        float angle_abs = Mathf.Abs(angle);

        shipController.SetTurnPort(false);
        shipController.SetTurnStarboard(false);

        if (angle_abs > turn_deadzone)
        {
            if (angle > 0f)
            {
                shipController.SetTurnPort(true);
                last_turn_dir = 1f;
            }
            else
            {
                shipController.SetTurnStarboard(true);
                last_turn_dir = -1f;
            }
        }
        else if (angle_abs > turn_release_zone)
        {
            if (last_turn_dir > 0f)
            {
                shipController.SetTurnPort(true);
            }
            else if (last_turn_dir < 0f)
            {
                shipController.SetTurnStarboard(true);
            }
        }
        else
        {
            last_turn_dir = 0f;
        }

        shipController.SetAccelerate(true);
        shipController.SetDecelerate(false);
    }

    private void ReplanIfNeeded()
    {
        replan_timer += Time.deltaTime;

        if (replan_timer < replan_interval)
        {
            return;
        }

        if (path == null || path.Count == 0)
        {
            return;
        }

        replan_timer = 0;

        State curr_goal = grid.GetStateFromWorldPos(player.position);
        State prev_goal = path[path.Count - 1];

        if (prev_goal != curr_goal)
        {
            if (path_index < 0 || path_index >= path.Count)
            {
                return;
            }

            State start = path[path_index];

            List<State> newPath = AStar.Search(start, curr_goal);

            // To smoothly move to the new path
            if (newPath != null && newPath.Count > 0)
            {
                List<State> prefix = path.GetRange(0, path_index);
                prefix.AddRange(newPath);
                path = prefix;
            }
        }
    }
    public void StartBribeFlee(Transform playerTransform)
    {
        if (bribeFleeing)
            return;

        chasing = false;
        waiting_to_chase = false;
        StopAllCoroutines();
        if (chaseTimeController != null)
        {
            chaseTimeController.ForceStopChaseUI();
        }

        player = playerTransform;

        State start = grid.GetStateFromWorldPos(transform.position);
        State playerState = grid.GetStateFromWorldPos(player.position);

        if (start == null || playerState == null)
        {
            Debug.LogWarning("Bribe flee: start or player state invalid");
            return;
        }

        State fleeGoal = FindFleeGoal(start, playerState);
        if (fleeGoal == null)
        {
            Debug.LogWarning("Bribe flee: no flee goal found");
            return;
        }

        path = AStar.Search(start, fleeGoal);
        path_index = 0;

        if (path == null || path.Count == 0)
        {
            Debug.Log("Bribe flee: A* found no path");
            return;
        }

        bribeFleeing = true;
        SetIgnoreWorldBorders(true);

        shipController.SetAccelerate(true);
        shipController.SetDecelerate(false);
        shipController.SetTurnPort(false);
        shipController.SetTurnStarboard(false);
    }



    private State FindFleeGoal(State start, State playerState)
    {
        if (grid == null || grid.grid == null)
            return null;

        int width = grid.grid.GetLength(0);
        int height = grid.grid.GetLength(1);

        int fleeX;
        int dirX;
        if (playerState.grid_x <= start.grid_x)
        {
            fleeX = width - 1;
            dirX = 1;
        }
        else
        {
            fleeX = 0;
            dirX = -1;
        }

        State best = null;
        int bestScore = int.MinValue;

        for (int y = 0; y < height; y++)
        {
            if (!IsGoodFleeCandidate(fleeX, y, dirX))
                continue;

            State s = grid.grid[fleeX, y];

            Vector3 worldPos = grid.GetWorldPosFromState(s);

            if (grid.land_tilemap != null)
            {
                Vector3 offset = new Vector3(dirX * grid.land_tilemap.cellSize.x, 0f, 0f);
                Vector3Int outsideCell = grid.land_tilemap.WorldToCell(worldPos + offset);

                if (grid.land_tilemap.HasTile(outsideCell))
                {
                    continue;
                }
            }

            int dx = s.grid_x - start.grid_x;
            int dy = s.grid_y - start.grid_y;
            int dist = dx * dx + dy * dy;

            int score = -dist;

            if (score > bestScore)
            {
                bestScore = score;
                best = s;
            }
        }

        return best;
    }



    private void FollowBribeFleePath()
    {
        if (IsOutsideOcean())
        {
            SetIgnoreWorldBorders(false);
            Destroy(gameObject);
            return;
        }

        if (path == null || path_index >= path.Count)
        {
            shipController.SetTurnPort(false);
            shipController.SetTurnStarboard(false);
            shipController.SetAccelerate(true);
            shipController.SetDecelerate(false);
            return;
        }

        State current_state = path[path_index];
        Vector3 targetPos = grid.GetWorldPosFromState(current_state);

        Vector3 distVec = targetPos - transform.position;
        distVec.z = 0f;

        float dist = distVec.magnitude;

        if (dist < 1f)
        {
            path_index++;
            return;
        }

        Vector3 forward = transform.up;
        Vector3 dir = distVec.normalized;

        float angle = Vector3.SignedAngle(forward, dir, Vector3.forward);
        float angle_abs = Mathf.Abs(angle);

        shipController.SetTurnPort(false);
        shipController.SetTurnStarboard(false);

        if (angle_abs > turn_deadzone)
        {
            if (angle > 0f)
            {
                shipController.SetTurnPort(true);
                last_turn_dir = 1f;
            }
            else
            {
                shipController.SetTurnStarboard(true);
                last_turn_dir = -1f;
            }
        }
        else if (angle_abs > turn_release_zone)
        {
            if (last_turn_dir > 0f)
            {
                shipController.SetTurnPort(true);
            }
            else if (last_turn_dir < 0f)
            {
                shipController.SetTurnStarboard(true);
            }
        }
        else
        {
            last_turn_dir = 0f;
        }

        shipController.SetAccelerate(true);
        shipController.SetDecelerate(false);
    }





    private void SetIgnoreWorldBorders(bool ignore)
    {
        if (enemyCollider == null) return;

        GameObject[] borders = GameObject.FindGameObjectsWithTag("WorldBorders");
        foreach (var go in borders)
        {
            var borderColliders = go.GetComponentsInChildren<Collider2D>();
            foreach (var borderCol in borderColliders)
            {
                if (borderCol != null && borderCol != enemyCollider)
                {
                    Physics2D.IgnoreCollision(enemyCollider, borderCol, ignore);
                }
            }
        }
    }


    private bool IsOutsideOcean()
    {
        if (oceanTilemap == null)
        {
            return false;
        }
        Vector3Int cell = oceanTilemap.WorldToCell(transform.position);

        return !oceanTilemap.HasTile(cell);
    }

    private bool IsOceanState(int x, int y)
    {
        if (grid == null || grid.grid == null) return false;

        int width = grid.grid.GetLength(0);
        int height = grid.grid.GetLength(1);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return false;

        return grid.grid[x, y] != null;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (bribeFleeing) return;
        if (!other.CompareTag("Player"))
        {
            return;
        }

        GameManager gameManager = GameManager.Instance;

        if (gameManager != null && Time.time < gameManager.fleeCooldownUntil)
        {
            return;
        }

        StartChasing(other.transform, false);
    }

    private bool IsGoodFleeCandidate(int x, int y, int dirX)
    {
        if (!IsOceanState(x, y))
            return false;

        if (!IsOceanState(x, y - 1) || !IsOceanState(x, y + 1))
            return false;

        int innerX = x - dirX;

        if (!IsOceanState(innerX, y)) return false;
        if (!IsOceanState(innerX, y - 1)) return false;
        if (!IsOceanState(innerX, y + 1)) return false;

        return true;
    }

}

