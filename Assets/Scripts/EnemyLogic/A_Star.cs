using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using System.Collections;



public static class A_star
{

    private static List<State> ReconstructPath(Dictionary<State, State> cameFrom, State current)
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



    public static List<State> Search(State start, State goal)
    {
        // https://en.wikipedia.org/wiki/A*_search_algorithm
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

                if (g == int.MaxValue)
                {
                    continue;
                }

                int tentative_g_score = g + 1;

                int neighbour_g = g_score.ContainsKey(neighbour) ? g_score[neighbour] : int.MaxValue;

                if (tentative_g_score < neighbour_g)
                {
                    came_from[neighbour] = current;
                    g_score[neighbour] = tentative_g_score;
                    f_score[neighbour] = tentative_g_score + Heuristic.h(neighbour, goal);

                    if (!open_set.Contains(neighbour))
                    {
                        open_set.Add(neighbour);
                    }
                }
            }
        }

        return new List<State>();
    }
}
