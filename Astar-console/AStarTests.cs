using System;
using System.Collections.Generic;

public static class AStarTests
{
    private static int testsRun = 0;
    private static int testsPassed = 0;

    private static void Assert(bool condition, string message)
    {
        testsRun++;
        if (condition)
        {
            testsPassed++;
            Console.WriteLine("[OK]   " + message);
        }
        else
        {
            Console.WriteLine("[FAIL] " + message);
        }
    }

    private static string PathToString(List<Pair> path)
    {
        if (path == null) return "null";
        return string.Join(" -> ", path);
    }

    // Compute total cost of a path (excluding the start cell)
    private static int ComputePathCost(int[,] grid, List<Pair> path)
    {
        if (path == null || path.Count == 0)
            return int.MaxValue;

        int cost = 0;

        for (int i = 1; i < path.Count; i++)
        {
            var p = path[i];
            cost += grid[p.Row, p.Col];
        }

        return cost;
    }

    // ------------- TESTS -------------

    // Check trivial case: start == goal
    private static void Test_SingleCell_StartEqualsGoal()
    {
        int[,] grid = { { 1 } };
        var start = new Pair(0, 0);
        var goal = new Pair(0, 0);

        var path = AStar.FindPath(grid, start, goal);

        Assert(path != null, "SingleCell: path not null");
        Assert(path!.Count == 1, "SingleCell: path length == 1");
        Assert(path[0].Row == 0 && path[0].Col == 0, "SingleCell: start==goal");
    }

    // Check simple 2x2 movement
    private static void Test_Simple2x2()
    {
        int[,] grid =
        {
            { 1, 1 },
            { 1, 1 }
        };

        var start = new Pair(1, 0);
        var goal = new Pair(1, 1);

        var path = AStar.FindPath(grid, start, goal);

        Assert(path != null, "2x2: path not null");
        Assert(path!.Count == 2, "2x2: path length == 2");
        Assert(path[0].Row == start.Row && path[0].Col == start.Col
               && path[1].Row == goal.Row && path[1].Col == goal.Col,
               "2x2: path is start -> goal");
    }

    // Check that blocked destination returns null
    private static void Test_BlockedDestination()
    {
        int[,] grid =
        {
            { 1, -1 },
            { 1,  1 }
        };

        var start = new Pair(1, 0);
        var goal = new Pair(0, 1);

        var path = AStar.FindPath(grid, start, goal);

        Assert(path == null, "BlockedDestination: path must be null");
    }

    // Check that a full vertical wall prevents any path
    private static void Test_NoPath_WallsBarrier()
    {
        int[,] grid =
        {
            { 1, -1, 1 },
            { 1, -1, 1 },
            { 1, -1, 1 }
        };

        var start = new Pair(0, 0);
        var goal = new Pair(2, 2);

        var path = AStar.FindPath(grid, start, goal);

        Assert(path == null, "NoPath: vertical wall barrier");
    }

    // Check that A* chooses the cheaper weighted path
    private static void Test_CheapestPath_WithWeights()
    {
        int[,] grid =
        {
            {  1,  5,  5,  5,  1 },
            {  1,  1,  1,  1,  1 }
        };

        var start = new Pair(0, 0);
        var goal = new Pair(0, 4);

        var path = AStar.FindPath(grid, start, goal);
        Assert(path != null, "CheapPath: path not null");

        bool usesBottomRow =
            path!.Exists(p => p.Row == 1 && p.Col == 1) &&
            path.Exists(p => p.Row == 1 && p.Col == 3);

        Assert(usesBottomRow,
            "CheapPath: path uses second row. Path = " + PathToString(path));

        int cost = ComputePathCost(grid, path);
        int expectedOptimalCost = 6;

        Assert(cost == expectedOptimalCost,
            $"CheapPath: total cost is optimal. Expected {expectedOptimalCost}, got {cost}");
    }

    // Check 10x10 empty grid path length (Manhattan)
    private static void Test_LargeGrid_NoObstacles()
    {
        int rows = 10;
        int cols = 10;
        int[,] grid = new int[rows, cols];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                grid[r, c] = 1;

        var start = new Pair(0, 0);
        var goal = new Pair(rows - 1, cols - 1);

        var path = AStar.FindPath(grid, start, goal);

        Assert(path != null, "LargeGrid(10x10): path not null");
        int manhattan = (rows - 1) + (cols - 1);
        Assert(path!.Count == manhattan + 1,
               "LargeGrid(10x10): path length == manhattan+1");
    }

    // Check that out-of-bounds start or goal returns null
    private static void Test_OutOfBounds()
    {
        int[,] grid =
        {
            { 1, 1 },
            { 1, 1 }
        };

        var startOut = new Pair(-1, 0);
        var goalIn = new Pair(0, 1);
        Assert(AStar.FindPath(grid, startOut, goalIn) == null,
               "OutOfBounds: start outside grid");

        var startIn = new Pair(0, 0);
        var goalOut = new Pair(2, 2);
        Assert(AStar.FindPath(grid, startIn, goalOut) == null,
               "OutOfBounds: goal outside grid");
    }

    // Check 20x20 empty grid path length
    private static void Test_VeryLargeGrid_20x20_Clear()
    {
        int rows = 20;
        int cols = 20;
        int[,] grid = new int[rows, cols];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                grid[r, c] = 1;

        var start = new Pair(0, 0);
        var goal = new Pair(rows - 1, cols - 1);

        var path = AStar.FindPath(grid, start, goal);

        Assert(path != null, "LargeGrid(20x20): path not null");
        int manhattan = (rows - 1) + (cols - 1);
        Assert(path!.Count == manhattan + 1,
               "LargeGrid(20x20): path length == manhattan+1");
    }

    // Check forced corridor with single gap
    private static void Test_VeryLargeGrid_30x30_WithCorridor()
    {
        int rows = 30;
        int cols = 30;
        int[,] grid = new int[rows, cols];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                grid[r, c] = 1;

        int wallCol = 15;
        int gapRow = rows / 2;

        for (int r = 0; r < rows; r++)
        {
            if (r == gapRow) continue;
            grid[r, wallCol] = -1;
        }

        var start = new Pair(0, 0);
        var goal = new Pair(rows - 1, cols - 1);

        var path = AStar.FindPath(grid, start, goal);

        Assert(path != null, "LargeGrid(30x30 + corridor): path not null");

        bool passesThroughGap = path!.Exists(p => p.Row == gapRow && p.Col == wallCol);
        Assert(passesThroughGap,
               "LargeGrid(30x30 + corridor): path goes through gap");
    }

    // Check custom 10x10 grid with weights and obstacles + optimal cost
    private static void Test_CustomGrid_10x10_WeightedOptimal()
    {
        int[,] grid =
        {
            { 1,  1, -1,  1, -1,  1,  1,  2,  1,  1 },
            { 1,  1, -1, -1, -1,  1,  2,  2,  2,  1 },
            { 1,  1, -1,  1, -1,  1,  1,  1,  2,  1 },
            { 1,  1,  1, -1, -1, -1, -1,  1,  2,  1 },
            { 1, -1,  1,  1,  1,  1,  1,  1,  1,  1 },
            { 1, -1, -1,  1, -1,  1, -1, -1, -1, -1 },
            { 1,  2,  1,  1, -1,  1,  1,  1, -1,  1 },
            { 1, -1, -1,  1, -1,  1, -1,  1, -1,  1 },
            { 1, -1,  1,  1,  1,  1, -1,  1,  1,  1 },
            { 1, -1,  1,  1,  1,  1,  1,  1,  1,  1 }
        };

        var start = new Pair(0, 0);
        var goal = new Pair(9, 9);

        var path = AStar.FindPath(grid, start, goal);

        Assert(path != null, "Custom10x10: path not null");
        if (path == null) return;

        bool wrongCell = false;
        foreach (var p in path)
        {
            if (grid[p.Row, p.Col] == -1)
                wrongCell = true;
        }
        Assert(!wrongCell, "Custom10x10: path avoids all -1 cells");

        int foundCost = ComputePathCost(grid, path);
        int expectedCost = 18;

        Assert(foundCost == expectedCost,
            $"Custom10x10: optimal cost expected {expectedCost}, got {foundCost}");
    }

    // ------------- RUNNER -------------

    public static void RunAll()
    {
        Console.WriteLine("=== A* unit tests ===");

        Test_SingleCell_StartEqualsGoal();
        Test_Simple2x2();
        Test_BlockedDestination();
        Test_NoPath_WallsBarrier();
        Test_CheapestPath_WithWeights();
        Test_LargeGrid_NoObstacles();
        Test_OutOfBounds();
        Test_VeryLargeGrid_20x20_Clear();
        Test_VeryLargeGrid_30x30_WithCorridor();
        Test_CustomGrid_10x10_WeightedOptimal();

        Console.WriteLine();
        Console.WriteLine($"Tests passed: {testsPassed}/{testsRun}");
    }
}
