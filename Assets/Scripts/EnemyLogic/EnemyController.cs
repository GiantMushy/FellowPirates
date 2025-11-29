using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using System.Collections;


public class Heuristic
{

    public static int h(State a, State b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

}

public class EnemyController : MonoBehaviour
{
    private ShipController shipController;
    public GridManager grid;
    private int path_index;
    private List<State> path;
    private bool chasing;
    public Transform target;
    public float replan_interval = 0.3f;
    private float replan_timer;

    public float chase_delay = 3f;
    private bool waiting_to_chase = false;


    private float last_turn_dir = 0f;
    public float turn_deadzone = 5f; 
    public float turn_release_zone = 2f;

    void Start()
    {
        shipController = GetComponent<ShipController>();
        if (shipController == null)
            Debug.LogError("EnemyController requires a ShipController component!");
    }

    void Update()
    {
        if (chasing)
        {
            FollowPath();
            ReplanIfNeeded();
        }
    }


    public void StartChasing(Transform target)
    {
        if (chasing || waiting_to_chase)
        {
            return;
        }

        this.target = target;
        waiting_to_chase = true;
        StartCoroutine(Chase());


    }

    public IEnumerator Chase()
    {
        yield return new WaitForSeconds(chase_delay);

        waiting_to_chase = false;

        Debug.Log("Chasing started");

        State start = grid.GetStateFromWorldPos(transform.position);
        State goal = grid.GetStateFromWorldPos(target.position);

        if (start == null || goal == null)
        {
            Debug.LogWarning("start or end outside of bounds");
            chasing = false;
            yield break;
        }


        Debug.Log($"start: {start.x},{start.y} (neigh: {start.neighbours?.Count})");
        Debug.Log($"goal:  {goal.x},{goal.y} (neigh: {goal.neighbours?.Count})");

        path = A_star.Search(start, goal);
        path_index = 0;


        Debug.Log($"A* path count: {path.Count}");


        if (path != null && path.Count > 0)
        {
            chasing = true;
            shipController.SetAccelerate(true);
            shipController.SetDecelerate(false);
        }
        else
        {
            chasing = false;
            Debug.Log("did not find a path");
        }
    }

    private void StopChase()
    {
        Debug.Log("stopped chasing");
        chasing = false;
        shipController.SetAccelerate(false);
        shipController.SetDecelerate(true);
        shipController.SetTurnPort(false);
        shipController.SetTurnStarboard(false);
        return;
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

        State curr_goal = grid.GetStateFromWorldPos(target.position);
        State prev_goal = path[path.Count - 1];

        if (prev_goal != curr_goal)
        {
            Debug.Log("replanning");

            int clampedIndex = Mathf.Clamp(path_index, 0, path.Count - 1);
            State start = path[clampedIndex];

            List<State> newPath = A_star.Search(start, curr_goal);

            if (newPath != null && newPath.Count > 0)
            {
                List<State> prefix = path.GetRange(0, clampedIndex);
                prefix.AddRange(newPath);
                path = prefix;

                path_index = clampedIndex;
            }
        }
    }



}

