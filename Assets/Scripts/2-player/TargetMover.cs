using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * This component moves its object towards a given target position.
 * Uses A* on a cost grid built from the Tilemap + TileCostConfig.
 * The time between steps depends on the cost of the next tile.
 */
public class TargetMover : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap = null;

    [Header("Movement")]
    [Tooltip("Base speed on tiles of cost = 1 (in meters/grid units per second).")]
    [SerializeField] private float baseSpeed = 2f;

    [Header("Tile cost configuration")]
    [Tooltip("Configuration that defines movement cost per tile.")]
    [SerializeField] private TileCostConfig tileCostConfig;

    [Tooltip("The target position in world coordinates")]
    [SerializeField] private Vector3 targetInWorld;

    [Tooltip("The target position in grid coordinates")]
    [SerializeField] private Vector3Int targetInGrid;

    // Current path to the target, in grid coordinates.
    private List<Vector3Int> currentPathInGrid = null;

    protected bool atTarget = true;

    private float timeBetweenSteps;

    // Bounds and size of the tilemap, used to build the A* grid.
    private BoundsInt mapBounds;
    private int gridRows;
    private int gridCols;

    public void SetTarget(Vector3 newTargetInWorld)
    {
        if (targetInWorld != newTargetInWorld)
        {
            targetInWorld = newTargetInWorld;
            targetInGrid = tilemap.WorldToCell(targetInWorld);
            atTarget = false;
            currentPathInGrid = null;
        }
    }

    public Vector3 GetTarget()
    {
        return targetInWorld;
    }

    [System.Obsolete]
    protected virtual void Start()
    {
        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();

        if (tileCostConfig == null)
            tileCostConfig = FindObjectOfType<TileCostConfig>();

        // Cache tilemap bounds and size for A*
        mapBounds = tilemap.cellBounds;
        gridCols = mapBounds.size.x;
        gridRows = mapBounds.size.y;

        // On a tile of cost 1, use baseSpeed
        timeBetweenSteps = 1f / baseSpeed;

        MoveTowardsTheTarget();
    }

    async void MoveTowardsTheTarget()
    {
        for (; ; )
        {
            await Awaitable.WaitForSecondsAsync(timeBetweenSteps);
            if (enabled && !atTarget)
                MakeOneStepTowardsTheTarget();
        }
    }

    private void MakeOneStepTowardsTheTarget()
    {
        if (tilemap == null || tileCostConfig == null)
            return;

        Vector3Int startCell = tilemap.WorldToCell(transform.position);

        // Recompute path if needed (new target or we moved)
        if (currentPathInGrid == null ||
            currentPathInGrid.Count == 0 ||
            currentPathInGrid[0] != startCell)
        {
            currentPathInGrid = ComputePathAStar(startCell, targetInGrid);

            if (currentPathInGrid == null || currentPathInGrid.Count == 0)
            {
                Debug.LogWarning($"A*: no path found between {startCell} and {targetInGrid}");
                atTarget = true;
                return;
            }
        }

        // Path only contains current cell -> we are at the target
        if (currentPathInGrid.Count <= 1)
        {
            atTarget = true;
            return;
        }

        // Remove current cell, move to next one
        currentPathInGrid.RemoveAt(0);
        Vector3Int nextCell = currentPathInGrid[0];

        // Get tile cost for the next cell
        float cost = tileCostConfig.GetCost(nextCell);
        if (float.IsInfinity(cost) || cost <= 0f)
            cost = 1f;

        // Effective speed = baseSpeed / cost
        float effectiveSpeed = baseSpeed / cost;
        timeBetweenSteps = 1f / effectiveSpeed;

        // Move object to the next tile center
        transform.position = tilemap.GetCellCenterWorld(nextCell);
    }

    private List<Vector3Int> ComputePathAStar(Vector3Int startCell, Vector3Int goalCell)
    {
        // If start or goal outside tilemap bounds -> no path
        if (!IsInsideBounds(startCell) || !IsInsideBounds(goalCell))
            return null;

        int[,] costGrid = BuildCostGrid();

        Pair start = CellToPair(startCell);
        Pair goal = CellToPair(goalCell);

        var pathPairs = AStar.FindPath(costGrid, start, goal);
        if (pathPairs == null || pathPairs.Count == 0)
            return null;

        // convert Pair (row,col) back to Unity cell coordinates
        var pathCells = new List<Vector3Int>();
        foreach (var p in pathPairs)
        {
            Vector3Int cell = PairToCell(p);
            pathCells.Add(cell);
        }

        return pathCells;
    }

    private int[,] BuildCostGrid()
    {
        int[,] grid = new int[gridRows, gridCols];

        for (int y = 0; y < gridRows; y++)
        {
            for (int x = 0; x < gridCols; x++)
            {
                Vector3Int cell = new Vector3Int(mapBounds.xMin + x, mapBounds.yMin + y, 0);

                // not walkable -> blocked
                if (!tileCostConfig.IsWalkable(cell))
                {
                    grid[y, x] = -1;
                }
                else
                {
                    // get float cost from config (1, 1.5, 2...)
                    float cost = tileCostConfig.GetCost(cell);
                    if (cost <= 0f || float.IsInfinity(cost))
                        cost = 1f;

                    // multiply by 10 to keep decimal precision
                    int intCost = Mathf.RoundToInt(cost * 10f);
                    if (intCost < 1) intCost = 1;

                    grid[y, x] = intCost;
                }
            }
        }

        return grid;
    }

    private bool IsInsideBounds(Vector3Int cell)
    {
        return mapBounds.Contains(cell);
    }

    private Pair CellToPair(Vector3Int cell)
    {
        int row = cell.y - mapBounds.yMin; // row = Y
        int col = cell.x - mapBounds.xMin; // col = X
        return new Pair(row, col);
    }

    private Vector3Int PairToCell(Pair p)
    {
        int x = mapBounds.xMin + p.Col;
        int y = mapBounds.yMin + p.Row;
        return new Vector3Int(x, y, 0);
    }
}
