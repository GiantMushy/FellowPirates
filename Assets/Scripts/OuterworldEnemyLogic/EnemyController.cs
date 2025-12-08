using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class EnemyController : MonoBehaviour
{
    private ShipController ship;
    public int enemyId;
    public int bribeCost = 1;
    public bool isDefeated = false;
    public bool isChasing = false;


    // CHASSSINNNNG LOGIC VARIABLES
    public GridManager grid;
    private int path_index;
    private List<State> path;
    public Transform player;
    public float replan_interval = 0.3f;
    private float replan_timer = 0;
    public float chase_delay = 2f;
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
    private Collider2D enemyCollider;
    private bool bribeFleeing = false;
    public Tilemap oceanTilemap;


    void Start()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Destroy(gameObject);
            Debug.Log("Missing GameManager instance - destroying enemy");
        }
        else if (isDefeated)
        {
            Destroy(gameObject);
            Debug.Log("EnemyController: Destroying enemy as it is already defeated.");
        }
        else
        {
            ship = GetComponent<ShipController>();
            enemyCollider = GetComponent<Collider2D>();
            if (ship == null)
            {
                Debug.LogError("EnemyController requires a ShipController component!");
            }

            // chaseTime.SetActive(false);
            chaseTimeController.timeWait = chase_delay;
        }
    }

    void Update()
    {
        if (isChasing)
        {
            FollowPath();
            ReplanIfNeeded();
        }

        else if (bribeFleeing)
        {
            FollowBribeFleePath();
        }
    }


    public void StartChasing(Transform target)
    {
        if (isChasing || waiting_to_chase)
        {
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }


        player = target;
        waiting_to_chase = true;
        StartCoroutine(Chase());
    }

    private IEnumerator Chase()
    {
        chaseTimeController.startChaseCountodwn();


        yield return new WaitForSeconds(chase_delay);

        chaseTimeController.StartChase();

        waiting_to_chase = false;

        Debug.Log("Enemy chasing started");

        State start = grid.GetStateFromWorldPos(transform.position);
        State goal = grid.GetStateFromWorldPos(player.position);

        if (start == null || goal == null)
        {
            Debug.LogWarning("Start or end outside of bounds");
            isChasing = false;
            yield break;
        }

        path = AStar.Search(start, goal);
        path_index = 0;

        if (path != null && path.Count > 0)
        {
            isChasing = true;
            ship.SetAccelerate(true);
            ship.SetDecelerate(false);
        }
        else
        {
            isChasing = false;
            Debug.Log("Enemy did not find a path to player");
        }

        float timer = 0f;
        float chaseTime = 15f;
        while (timer < chaseTime)
        {
            timer += Time.deltaTime;
            yield return true;
        }
        StopChase();
    }

    public void StopChase()
    {
        Debug.Log("Stopped chasing");
        isChasing = false;
        ship.SetAccelerate(false);
        ship.SetDecelerate(false);
        ship.SetTurnPort(false);
        ship.SetTurnStarboard(false);
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
                StopChase();
            }
            return;
        }

        UnityEngine.Vector3 forward = transform.up;
        UnityEngine.Vector3 dir = dist_to_player.normalized;

        float angle = UnityEngine.Vector3.SignedAngle(forward, dir, UnityEngine.Vector3.forward);
        float angle_abs = Mathf.Abs(angle);

        ship.SetTurnPort(false);
        ship.SetTurnStarboard(false);

        if (angle_abs > turn_deadzone)
        {
            if (angle > 0f)
            {
                ship.SetTurnPort(true);
                last_turn_dir = 1f;
            }
            else
            {
                ship.SetTurnStarboard(true);
                last_turn_dir = -1f;
            }
        }
        else if (angle_abs > turn_release_zone)
        {
            if (last_turn_dir > 0f)
            {
                ship.SetTurnPort(true);
            }
            else if (last_turn_dir < 0f)
            {
                ship.SetTurnStarboard(true);
            }
        }
        else
        {
            last_turn_dir = 0f;
        }

        ship.SetAccelerate(true);
        ship.SetDecelerate(false);


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
            Debug.Log("Enemy replanning path");

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
        StopChase();

        if (chaseTimeController != null)
        {
            chaseTimeController.ForceStopChaseUI();
        }

        player = playerTransform;

        bribeFleeing = true;
        SetIgnoreWorldBorders(true);

        ship.SetAccelerate(true);
        ship.SetDecelerate(false);
    }


    private void FollowBribeFleePath()
    {
        if (!bribeFleeing || player == null)
            return;

        if (IsOutsideOcean())
        {
            Debug.Log("[BribeFlee] Outside ocean – destroying enemy");
            Destroy(gameObject);
            return;
        }

        Vector3 away = transform.position - player.position;
        away.z = 0f;
        if (away.sqrMagnitude < 0.01f)
        {
            away = transform.up;
        }

        Vector3 dir = away.normalized;
        Vector3 forward = transform.up;

        float angle = Vector3.SignedAngle(forward, dir, Vector3.forward);
        float angle_abs = Mathf.Abs(angle);

        ship.SetTurnPort(false);
        ship.SetTurnStarboard(false);

        if (angle_abs > turn_deadzone)
        {
            if (angle > 0f)
            {
                ship.SetTurnPort(true);
                last_turn_dir = 1f;
            }
            else
            {
                ship.SetTurnStarboard(true);
                last_turn_dir = -1f;
            }
        }
        else if (angle_abs > turn_release_zone)
        {
            if (last_turn_dir > 0f)
            {
                ship.SetTurnPort(true);
            }
            else if (last_turn_dir < 0f)
            {
                ship.SetTurnStarboard(true);
            }
        }
        else
        {
            last_turn_dir = 0f;
        }

        ship.SetAccelerate(true);
        ship.SetDecelerate(false);
    }


    private void SetIgnoreWorldBorders(bool ignore)
    {
        if (enemyCollider == null) return;

        GameObject[] borders = GameObject.FindGameObjectsWithTag("WorldBorders");
        foreach (var go in borders)
        {
            Collider2D borderCol = go.GetComponent<Collider2D>();
            if (borderCol != null)
            {
                Physics2D.IgnoreCollision(enemyCollider, borderCol, ignore);
            }
        }
    }

    private bool IsOutsideOcean()
    {
        if (oceanTilemap == null) return false;
        Vector3Int cell = oceanTilemap.WorldToCell(transform.position);
        return !oceanTilemap.HasTile(cell);
    }

    public bool IsDead()
    {
        return isDefeated;
    }

    public void TakeDamage(int damage)
    {
        ship.health -= damage;
        if (ship.health <= 0)
        {
            isDefeated = true;
        }
    }

    public int GetHealth()
    {
        return ship.health;
    }

    public int GetMaxHealth()
    {
        return ship.maxHealth;
    }
}

