using System;
using System.Collections.Generic;
using UnityEngine;

public struct Pair
{
    public int Row;
    public int Col;

    public Pair(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public override string ToString() => $"({Row},{Col})";
}

public class AStar
{
    // grid[row, col] :
    //   -1 = blocked tile
    //   >0 = walkable with movement cost
    public static List<Pair> FindPath(int[,] grid, Pair start, Pair goal)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // Check coordinates are inside bounds
        if (!IsValid(start.Row, start.Col, rows, cols) ||
            !IsValid(goal.Row, goal.Col, rows, cols))
        {
            Debug.LogWarning("A*: Start or goal is outside grid bounds.");
            return null;
        }

        // Check start and goal are walkable
        if (!IsWalkable(grid, start.Row, start.Col) ||
            !IsWalkable(grid, goal.Row, goal.Col))
        {
            Debug.LogWarning("A*: Start or goal is blocked.");
            return null;
        }

        var open     = new PriorityQueue<Pair>();
        var cameFrom = new Dictionary<Pair, Pair>();
        var gScore   = new Dictionary<Pair, float>();
        var fScore   = new Dictionary<Pair, float>();
        var closed   = new HashSet<Pair>();

        Pair startP = start;
        Pair goalP  = goal;

        gScore[startP] = 0f;
        fScore[startP] = Heuristic(startP, goalP);

        open.Enqueue(startP, fScore[startP]);

        while (open.Count > 0)
        {
            // Node with lowest f-score
            var current = open.Dequeue();

            // Goal reached
            if (current.Row == goalP.Row && current.Col == goalP.Col)
            {
                Debug.Log("A*: Goal reached. Reconstructing path...");
                return ReconstructPath(cameFrom, current);
            }

            closed.Add(current);

            // Explore 4-direction neighbors
            foreach (var neighbor in GetNeighbors(current, rows, cols))
            {
                if (!IsWalkable(grid, neighbor.Row, neighbor.Col))
                    continue;

                if (closed.Contains(neighbor))
                    continue;

                float tentativeG =
                    GetScore(gScore, current)
                    + grid[neighbor.Row, neighbor.Col];  // movement cost from grid

                // First time we see this neighbor OR we found a better path
                if (tentativeG < GetScore(gScore, neighbor))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor]   = tentativeG;

                    float f = tentativeG + Heuristic(neighbor, goalP);
                    fScore[neighbor] = f;

                    open.Enqueue(neighbor, f);
                }
            }
        }

        // No path exists
        Debug.LogWarning("A*: No valid path found.");
        return null;
    }

    private static float GetScore(Dictionary<Pair, float> dict, Pair p)
    {
        return dict.TryGetValue(p, out float value) ? value : float.PositiveInfinity;
    }

    private static bool IsValid(int r, int c, int rows, int cols)
    {
        return r >= 0 && r < rows && c >= 0 && c < cols;
    }

    private static bool IsWalkable(int[,] grid, int r, int c)
    {
        return grid[r, c] != -1;
    }

    private static IEnumerable<Pair> GetNeighbors(Pair p, int rows, int cols)
    {
        // 4-direction movement (no diagonals)
        int r = p.Row;
        int c = p.Col;

        if (r > 0)        yield return new Pair(r - 1, c);
        if (r < rows - 1) yield return new Pair(r + 1, c);
        if (c > 0)        yield return new Pair(r, c - 1);
        if (c < cols - 1) yield return new Pair(r, c + 1);
    }

    private static float Heuristic(Pair a, Pair b)
    {
        // Manhattan distance heuristic
        return Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);
    }

    private static List<Pair> ReconstructPath(Dictionary<Pair, Pair> cameFrom, Pair current)
    {
        var path = new List<Pair> { current };

        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}

// Minimal priority queue used by A*
public class PriorityQueue<T>
{
    private readonly List<(T item, float priority)> data = new();

    public int Count => data.Count;

    public void Enqueue(T item, float priority)
    {
        data.Add((item, priority));
        data.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    public T Dequeue()
    {
        var item = data[0].item;
        data.RemoveAt(0);
        return item;
    }
}

