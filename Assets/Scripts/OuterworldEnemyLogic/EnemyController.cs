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

    // for bribe
    public int bribeCost = 1;
    private Collider2D enemyCollider;
    private bool bribeFleeing = false;
    public Tilemap oceanTilemap;


    void Start()
    {
        var gm = GameManager.Instance;
        if (gm != null && gm.defeatedEnemies.Contains(enemyId))
        {
            Destroy(gameObject);
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

    public void StartChasing(Transform target)
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
            chasing = false;
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
            yield return true;
        }
        StopChase();
    }

    public void StopChase()
    {
        Debug.Log("Stopped chasing");
        chasing = false;
        shipController.SetAccelerate(false);
        shipController.SetDecelerate(true);
        shipController.SetTurnPort(false);
        shipController.SetTurnStarboard(false);
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
                Debug.Log("would have stopped");
                ReplanIfNeeded();
                // StopChase();
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

    }


    private void FollowBribeFleePath()
    {

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
        if (oceanTilemap == null)
        {
            return false;
        }
        Vector3Int cell = oceanTilemap.WorldToCell(transform.position);

        return !oceanTilemap.HasTile(cell);
    }

}

