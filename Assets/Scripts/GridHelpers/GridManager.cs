using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;


public class GridManager : MonoBehaviour
{
    // makes a grid that doesn't include the islands

    public Tilemap land_tilemap;
    public State[,] grid;

    private BoundsInt bounds;
    private int width;
    private int height;
    private int offsetX;
    private int offsetY;

    private static readonly Vector2Int[] dirs =
    {
        new Vector2Int( 1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int( 0, 1),
        new Vector2Int( 0,-1),
    };


    void Awake()
    {
        if (land_tilemap == null)
        {
            Debug.LogError("GridManager: no land_tilemap assigned");
            return;
        }

        bounds = land_tilemap.cellBounds;
        width = bounds.size.x;
        height = bounds.size.y;

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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                State state = grid[x, y];
                if (state is null)
                {
                    continue; // island
                }

                state.neighbours = new List<State>();

                foreach (var d in dirs)
                {
                    int nx = x + d.x;
                    int ny = y + d.y;

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
        Vector3Int cell = new Vector3Int(s.grid_x + offsetX, s.grid_y + offsetY, 0);

        return land_tilemap.GetCellCenterWorld(cell);
    }
}