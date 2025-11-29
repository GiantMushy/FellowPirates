using System.Collections.Generic;
public class State
{
    // Search space states for the enemy chasing pathfinding

    public int grid_x, grid_y;
    public List<State> neighbours = new List<State>();
    public State(int in_x, int in_y)
    {
        grid_x = in_x;
        grid_y = in_y;
    }

    public static bool operator ==(State a, State b)
    {
        if (ReferenceEquals(a, b)) // if literally same object
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.grid_x == b.grid_x && a.grid_y == b.grid_y;
    }

    public static bool operator !=(State a, State b)
    {
        return !(a == b);
    }

    // to be able to use Dictionary with state
    public override bool Equals(object obj)
    {
        if (!(obj is State))
        {
            return false;
        }

        State other = (State)obj;
        return grid_x == other.grid_x && grid_y == other.grid_y;
    }

    public override int GetHashCode()
    {
        return (grid_x, grid_y).GetHashCode();
    }

}