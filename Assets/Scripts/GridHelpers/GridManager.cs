using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;


public class GridManager : MonoBehaviour
{

    public Tilemap land_tilemap;

    public LayerMask island_layer;
    public State?[,] grid;
    int cell_size = 1;


    private BoundsInt bounds;
    private int width;
    private int height;
    private int offsetX;
    private int offsetY;

    void Awake()
    {
        // makes a grid that doesn't include the islands

        var bounds = land_tilemap.cellBounds;
        width = bounds.size.x;
        height = bounds.size.y;

        Debug.Log($"Grid size: {width} x {height}");


        offsetX = bounds.xMin;
        offsetY = bounds.yMin;

        grid = new State?[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cell = new Vector3Int(x + offsetX, y + offsetY, 0);

                Vector2 world_pos = land_tilemap.GetCellCenterWorld(cell);

                bool blocked = Physics2D.OverlapCircle(world_pos, cell_size * 0.4f, island_layer); // 0.4 to check just a little less than half
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
                State? is_state = grid[x, y];
                if (is_state == null)
                {
                    continue; // island
                }

                State state = is_state.Value;
                state.neighbours = new List<State>();

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dirs[i, 0];
                    int ny = y + dirs[i, 1];

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) // bounds
                    {
                        continue;
                    }

                    State? neighbour = grid[nx, ny];
                    if (neighbour != null)
                    {
                        state.neighbours.Add(neighbour.Value);
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

        return grid[gx, gy].Value;
    }

    public UnityEngine.Vector3 GetWorldPosFromState(State s)
    {
        Vector3Int cell = new Vector3Int(s.x + offsetX, s.y + offsetY, 0);

        return land_tilemap.GetCellCenterWorld(cell);
    }
}