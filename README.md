# Assignment 6 TileMap


## Author:
Raphael Coeffic - 337614747

Yanai Levy - 215011537


## Play the Game  
Itch.io Link: https://genesiswarfare.itch.io/assignment-6

## The scene
We work on the scene Assets/Scenes/5-homework/a-chase

## How to Play:  

How to play: Click on the map to move, the player walks automatically using A* algorithm.You cannot move onto mountains unless you have the goat. You cannot move onto water unless you have the boat. Pick up the goat or the boat to unlock those tiles. To mine, pick up the pickaxe, then press: A = left, W = up, D = right, X = down, S = your tile. If the chosen tile is a mountain, it turns into grass. Click again on the map to continue moving.


## Features Added for the Assignment  

### Part A

Boat: When the player picks up the boat, it disappears from the map. After obtaining it, the player is allowed to move on water tiles. 
Every time the player moves on water, the boat visually reappears under the player (following him).

Goat: When the player picks up the goat, it disappears from the map. After obtaining it, the player is allowed to move on mountain tiles.
Every time the player walks on a mountain tile, the goat automatically appears with the player, indicating that mountains are now traversable.

### Part B

In this part, I replaced the BFS navigation algorithm with A* (A-star) implementation that supports weighted costs per tile type.
Instead of treating every tile as cost 1 (like BFS), A* now computes the shortest path based on real movement cost.
Each terrain has a different weight, defined in TileCostConfig:
Grass = fast movement
Hills or bushes = higher cost
Water or mountains = blocked unless the player has the required item
Mountain mining = only possible after picking up the pickaxe
All movement parameters(for all tiles), tile speed (weight), accessibility rules, and required items are fully configurable directly inside Unity.
This is done using the TileCostManager object in the scene, which has the TileCostConfig script attached to it.
By editing this object in the Inspector, we can freely adjust tile costs (movement speed), set which tiles require the goat, boat and control all terrain accessibility without modifying any code. A* uses these weights to choose the optimal path, so the player will automatically prefer cheaper and faster terrain.

**Unity Implementation:**

The A* algorithm is located in:
Assets/Scripts/6-Astar/AStar.cs
It works directly with the tilemap, reading each tile's cost through TileCostConfig.

**Console and Unit Tests:**

We also implemented a separate console version of A*, including unit tests, inside the folder Astar-console/
The console version verifies: correct pathfinding on weighted grids, impossible paths return null, lower-cost routes are preferred even if they are longer in number of steps, obstacles (-1) are treated as non-walkable. This ensures the algorithm works correctly independently of Unity.


