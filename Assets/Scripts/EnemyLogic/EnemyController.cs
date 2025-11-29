using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public struct State
{

    public int x, y;
    public List<State> neighbours;
    public State(int in_x, int in_y)
    {
        x = in_x;
        y = in_y;
        neighbours = new List<State>();
    }

    public static bool operator ==(State a, State b)
    {
        return a.x == b.x && a.y == b.y;
    }
    public static bool operator !=(State a, State b)
    {
        return !(a == b);
    }
    public override bool Equals(object obj)
    {
        if (!(obj is State))
        {
            return false;
        }

        State other = (State)obj;
        return x == other.x && y == other.y;
    }

    public override int GetHashCode()
    {
        return (x, y).GetHashCode();
    }

}

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
    public float replan_interval = 0.5f;
    private float replan_timer;

    void Start()
    {
        shipController = GetComponent<ShipController>();
        if (shipController == null)
            Debug.LogError("EnemyController requires a ShipController component!");

        // shipController.SetSpeed(0.8f);
        // shipController.SetTurnStarboard(true);
    }

    void Update()
    {
        if (chasing)
        {
            FollowPath();
            ReplanIfNeeded();
        }
    }

    private List<State> ReconstructPath(Dictionary<State, State> cameFrom, State current)
    {
        List<State> total_path = new List<State>();
        total_path.Add(current);

        while (cameFrom.Keys.Contains(current))
        {
            current = cameFrom[current];
            total_path.Insert(0, current);
        }

        return total_path;
    }

    private List<State> A_Star(State start, State goal)
    {
        // https://en.wikipedia.org/wiki/A*_search_algorithm

        // HashSet<State> open_set = { start };
        // PriorityQueue open_set = new PriorityQueue<string, int>();
        // open_set.Enque(start);
        List<State> open_set = new List<State> { start };
        HashSet<State> closed_set = new HashSet<State>();


        Dictionary<State, State> came_from = new Dictionary<State, State>();
        Dictionary<State, int> g_score = new Dictionary<State, int>();
        Dictionary<State, int> f_score = new Dictionary<State, int>();

        g_score[start] = 0;
        f_score[start] = Heuristic.h(start, goal);

        while (open_set.Count > 0)
        {
            State current = open_set
            .OrderBy(n => f_score.ContainsKey(n) ? f_score[n] : int.MaxValue)
            .First();


            if (current == goal)
            {
                return ReconstructPath(came_from, current);
            }

            open_set.Remove(current);
            closed_set.Add(current);

            foreach (State neighbour in current.neighbours)
            {
                if (closed_set.Contains(neighbour))
                {
                    continue;
                }

                int g = g_score.ContainsKey(current) ? g_score[current] : int.MaxValue;

                int tentative_g_score = 0;
                if (g == int.MaxValue)
                {
                    tentative_g_score = int.MaxValue;
                }
                else
                {
                    tentative_g_score = g + 1;
                }

                int neighbour_g = g_score.ContainsKey(neighbour) ? g_score[neighbour] : int.MaxValue;

                if (tentative_g_score < neighbour_g)
                {
                    came_from[neighbour] = current;
                    g_score[neighbour] = tentative_g_score;
                    f_score[neighbour] = tentative_g_score + Heuristic.h(neighbour, goal);

                    if (!open_set.Contains(neighbour))
                    {
                        open_set.Add(neighbour);
                        // g_score[neighbour] = int.MaxValue;
                        // f_score[neighbour] = int.MaxValue;
                    }
                }
            }
        }

        return new List<State>();
    }

    public void StartChasing(Transform target)
    {
        Debug.Log("Chasing started");
        this.target = target;

        State start = grid.GetStateFromWorldPos(transform.position);
        State goal = grid.GetStateFromWorldPos(target.position);

        path = A_Star(start, goal);
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
            Debug.Log("did not find a path");
        }
    }

    private void StopChase()
    {
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

        if (dist < 0.1f)
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

        shipController.SetTurnPort(false);
        shipController.SetTurnStarboard(false);

        float turn = 2f;

        if (Mathf.Abs(angle) > turn)
        {
            if (angle > 0f)
            {
                shipController.SetTurnPort(true);
            }
            else if (angle < -turn)
            {
                shipController.SetTurnStarboard(true);
            }

            shipController.SetAccelerate(true);
            shipController.SetDecelerate(false);
        }
    }

    private void ReplanIfNeeded()
    {
        replan_timer += Time.deltaTime;

        if (replan_timer < replan_interval)
        {
            return;
        }

        replan_timer = 0;

        State curr_goal = grid.GetStateFromWorldPos(target.position);

        // State old
        State prev_goal = path[path.Count - 1];

        if (prev_goal != curr_goal)
        {
            State start = grid.GetStateFromWorldPos(transform.position);
            path = A_Star(start, curr_goal);
            path_index = 0;
        }
    }


}

