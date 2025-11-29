using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using System.Collections;

public class State
{

    public int x, y;
    public List<State> neighbours = new List<State>();
    public State(int in_x, int in_y)
    {
        x = in_x;
        y = in_y;
        // neighbours = new List<State>();
    }

    public static bool operator ==(State a, State b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        if (a is null || b is null)
        {
            return false;
        }
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