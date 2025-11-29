using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;


public class GridManager : MonoBehaviour
{

    public Tilemap land_tilemap;

    public LayerMask island_layer;
    public State[,] grid;
    int cell_size = 1;


    private BoundsInt bounds;
    private int width;
    private int height;
    private int offsetX;
    private int offsetY;

    void Awake()
    {
        if (land_tilemap == null)
        {
            Debug.LogError("GridManager: land_tilemap is NOT assigned!");
            return;
        }
        // makes a grid that doesn't include the islands

        var bounds = land_tilemap.cellBounds;
        width = bounds.size.x;
        height = bounds.size.y;

        Debug.Log($"Grid size: {width} x {height}");


        offsetX = bounds.xMin;
        offsetY = bounds.yMin;

        grid = new State[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cell = new Vector3Int(x + offsetX, y + offsetY, 0);

                bool blocked = land_tilemap.HasTile(cell);


                if (!blocked)
                {
                    grid[x, y] = new State(x, y);
                }
            }

        }

        int[,] dirs = new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                State state = grid[x, y];
                if (state == null)
                {
                    Debug.LogError($"Null state at {x},{y}");

                    continue; // island
                }

                // state.neighbours.Clear();
                // if (state.neighbours == null)
                // {
                //     state.neighbours = new List<State>();
                // }
                // else
                // {
                //     state.neighbours.Clear();
                // }
                Debug.Log("state: " + state);
                state.neighbours = new List<State>();


                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dirs[i, 0];
                    int ny = y + dirs[i, 1];

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) // bounds
                    {
                        continue;
                    }

                    State neighbour = grid[nx, ny];
                    if (neighbour != null)
                    {
                        state.neighbours.Add(neighbour);
                    }
                }
                grid[x, y] = state;
            }
        }
    }
    public State GetStateFromWorldPos(UnityEngine.Vector3 world_pos)
    {
        Vector3Int cell = land_tilemap.WorldToCell(world_pos);

        int gx = cell.x - offsetX;
        int gy = cell.y - offsetY;

        if (gx < 0 || gx >= width || gy < 0 || gy >= height)
        {
            return null;
        }
        return grid[gx, gy];
    }

    public UnityEngine.Vector3 GetWorldPosFromState(State s)
    {
        Vector3Int cell = new Vector3Int(s.x + offsetX, s.y + offsetY, 0);

        return land_tilemap.GetCellCenterWorld(cell);
    }
}