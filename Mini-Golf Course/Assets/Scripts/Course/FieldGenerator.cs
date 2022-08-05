using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// using Dice;
using Course.Field;

namespace Course
{
    public class FieldGenerator : MonoBehaviour
    {
        [Header("Level")]
        [Range(0,100)]
        public int level = 10;
        private int lastLevel = 10;
        [Range(0,10000)]
        public int seed = 100;
        private int lastSeed = 100;

        [Header("Modifiers")]
        [Range(0f,1f)]
        public float obstacleChance;
        [Range(0f,1f)]
        public float landmarkChance;
        [Range(0f,1f)]
        public float decoChance;
        private float lastDecoChance = 0f;

        [Header("Playfield")]
        [SerializeField] private Playfield playfield;

        [Header("Game")]
        [SerializeField] private GameObject gameTilePrefab;
        [SerializeField] private List<GameObject> decoPrefabs = new List<GameObject>();
        private List<GameObject> gameTiles = new List<GameObject>();
        // [SerializeField] private Ball ball;
        
        [Header("Gizmos")]
        [SerializeField] private List<Mesh> terrainMeshes = new List<Mesh>();

        void Start()
        {
            // CreateLevel();
        }

        void Update()
        {
            if (lastLevel != level || lastSeed != seed || lastDecoChance != decoChance)
            {
                DestroyLevel();
                CreateLevel();
            }

            lastLevel = level;
            lastSeed = seed;
            lastDecoChance = decoChance;
        }
        
        private void OnDrawGizmos()
        {
            // // Create a random for generation
            // System.Random rand = new System.Random(seed + level);
            //
            // // Get the start
            // Vector3Int start = Vector3Int.zero;
            //
            // // Determine the end as a point on a circle of 'level' radius
            // float randomAngle = rand.Next(0, 360);
            // Vector3Int end = new Vector3Int((int)Mathf.Floor(Mathf.Cos(Mathf.Deg2Rad * randomAngle)* level), (int)Mathf.Floor(Mathf.Sin(Mathf.Deg2Rad * randomAngle) * level), 0);
            //
            // // Create a 'level' number of obstacles that the pathfinding must work around
            // BoundsInt fieldBounds = new BoundsInt(Vector3Int.zero, Vector3Int.one);
            // Vector3Int min = new Vector3Int((int)Mathf.Min(start.x, end.x), (int)Mathf.Min(start.y, end.y), 0);
            // Vector3Int max = new Vector3Int((int)Mathf.Max(start.x, end.x) + 5, (int)Mathf.Max(start.y, end.y) + 5, 0);
            // fieldBounds.SetMinMax(min, max);
            // List<Vector3Int> obstacles = CreateRandomObstacles(fieldBounds, start, end, level, seed);
            //
            // Gizmos.color = Color.cyan;
            // Gizmos.DrawWireCube(new Vector3(fieldBounds.center.x, fieldBounds.center.z * 0.5f, fieldBounds.center.y), GridToWorld(fieldBounds.size));
            //
            // Gizmos.color = new Color(0f, 1f, 0f, .5f);
            // Gizmos.DrawCube(GridToWorld(start), Vector3.one);
            // Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            // Gizmos.DrawCube(GridToWorld(end), Vector3.one);
            //
            // foreach (Vector3Int obstacle in obstacles)
            // {
            //     Gizmos.color = Color.red;
            //     Gizmos.DrawWireCube(GridToWorld(obstacle), Vector3.one);
            // }
            // Vector3Int lastCell = start;
            //
            if (playfield != null)
            {
                // Draw field
                // foreach (Vector3Int cell in playfield.field)
                // {
                //     Gizmos.color = Color.blue;
                //     Gizmos.DrawLine(GridToWorld(cell), GridToWorld(lastCell));
                //     lastCell = cell;
                //     Gizmos.DrawWireCube(GridToWorld(cell), new Vector3(1f, 0.5f, 1f));
                // }
                
                // Draw bounds
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(GridToWorld(new Vector3Int((int)Mathf.Floor(playfield.bounds.center.x), (int)Mathf.Floor(playfield.bounds.center.y), (int)Mathf.Floor(playfield.bounds.center.z))), GridToWorld(playfield.bounds.size));
                
                // Draw terrain
                foreach (TerrainTile terrain in playfield.terrain)
                {
                    Gizmos.color = Color.magenta;
                    switch (terrain.terrainType)
                    {
                        case TerrainType.None:
                            Gizmos.DrawWireCube(GridToWorld(terrain.position), new Vector3(1f, 0.5f, 1f));
                            break;
                        default:
                            Quaternion quaternion = new Quaternion();
                            quaternion.eulerAngles = new Vector3(-90f, terrain.rotation, 0f);
                            Gizmos.DrawMesh(terrainMeshes[((int)terrain.terrainType) - 1], GridToWorld(terrain.position), quaternion, Vector3.one);
                            break;
                    }
                }
            }
        }
        
        // Generates and Instantiates the field
        private void CreateLevel()
        {
            playfield = GeneratePlayfield();
            // InstantiatePlayfield();
            // InstantiateDeco();

            // ball.transform.position = playfield.spawn;
        }
        
        // Creates deco objects for the level
        private void InstantiateDeco()
        {
            foreach (Vector3Int decoPoint in playfield.deco)
            {
                GameObject deco = Instantiate(decoPrefabs[0], GridToWorld(decoPoint), Quaternion.identity);
                deco.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                
                gameTiles.Add(deco);
            }
        }
        
        // Removes the level tiles
        private void DestroyLevel()
        {
            foreach (GameObject tile in gameTiles)
            {
                Destroy(tile, 0f);
            }
        }

        // Generate a playfield
        private Playfield GeneratePlayfield()
        {
            return new Playfield(seed, level);
        }

        // // Generate a field
        // // TODO: REFACTOR ALL THIS TERRAIN GEN TO BE WITHIN THE PLAYFIELD
        // private Playfield GeneratePlayfield(int level, int seed, bool withArms = false)
        // {
        //     // Create a random for generation
        //     System.Random rand = new System.Random(seed + level);
        //     
        //     // Get the start
        //     Vector3Int start = Vector3Int.zero;
        //     
        //     // Determine the end as a point on a circle of 'level' radius
        //     float randomAngle = rand.Next(0, 360);
        //     Vector3Int end = new Vector3Int((int)Mathf.Floor(Mathf.Cos(Mathf.Deg2Rad * randomAngle)* level), (int)Mathf.Floor(Mathf.Sin(Mathf.Deg2Rad * randomAngle) * level), 0);
        //     
        //     // Create a 'level' number of obstacles that the pathfinding must work around
        //     BoundsInt fieldBounds = new BoundsInt(Vector3Int.zero, Vector3Int.one);
        //     Vector3Int min = new Vector3Int((int)Mathf.Min(start.x, end.x), (int)Mathf.Min(start.y, end.y), 0);
        //     Vector3Int max = new Vector3Int((int)Mathf.Max(start.x, end.x) + 5, (int)Mathf.Max(start.y, end.y) + 5, 1);
        //     fieldBounds.SetMinMax(min, max);
        //     List<Vector3Int> obstacles = CreateRandomObstacles(fieldBounds, start, end, level, seed);
        //
        //     // Get the total count of subdivisions
        //     int subdivisions = (int)Mathf.Floor(0.075f * level) + 1;
        //     int maxOffset = 5;
        //     
        //     // For each segment of the path, find the subpath
        //     List<Vector3Int> path = new List<Vector3Int>();
        //     Vector3Int currentStart = start;
        //     float subdivisionInterval = 1f / subdivisions;
        //     for (int i = 0; i < subdivisions; i++)
        //     {
        //         // Get the current end
        //         Vector3 subPoint = Vector3.Lerp(start, end, subdivisionInterval * (i + 1));
        //         Vector3Int currentEnd = new Vector3Int((int)Mathf.Floor(subPoint.x), (int)Mathf.Floor(subPoint.y), 0);
        //         
        //         // Offset the current end
        //         Vector3Int randomOffset = new Vector3Int(rand.Next(-maxOffset, maxOffset), rand.Next(-maxOffset, maxOffset));
        //         currentEnd += randomOffset;
        //         currentEnd = ClampWithinBounds(fieldBounds, currentEnd);
        //
        //         // Get the current path from current start to current end
        //         List<Vector3Int> currentPath = FindPath(currentStart, currentEnd, obstacles, fieldBounds);
        //         currentPath.RemoveRange(currentPath.Count - 1, 1);
        //         
        //         // Add the current path to the main path and store the new current start
        //         currentStart = currentEnd;
        //         path.AddRange(currentPath);
        //     }
        //     
        //     // Create the terrain
        //     List<Vector3Int> terrain = CreateTerrain(seed, 5, fieldBounds);
        //
        //     // Modify the z height of the course
        //     List<Vector3Int> modifiedPath = ModifyHeight(path, seed, level);
        //     
        //     // Store the modified path somewhere where we can add arms if needed
        //     List<Vector3Int> fieldPath = modifiedPath;
        //     
        //     // Arms
        //     if (withArms)
        //     {
        //         // Generate extra arms
        //         Dictionary<List<int>, List<Vector3Int>> arms = GetArms(path, obstacles, fieldBounds, level, seed);
        //         
        //         // Map the arms to the new heights
        //         Dictionary<List<Vector3Int>, List<Vector3Int>> modifiedArms = MapArmsToHeight(path, modifiedPath, arms);
        //         
        //         // Add the arms to the modified path
        //         foreach (KeyValuePair<List<Vector3Int>, List<Vector3Int>> arm in modifiedArms)
        //         {
        //             fieldPath.AddRange(arm.Value);
        //         }
        //     }
        //
        //     return new Playfield(modifiedPath, modifiedPath[0], modifiedPath[modifiedPath.Count - 1], terrain, seed, new ModifierSettings(obstacleChance, landmarkChance, decoChance));
        // }
        //
        // // Creates the terrain for the playfield
        // private List<Vector3Int> CreateTerrain(int seed, int height, BoundsInt fieldBounds)
        // {
        //     // Create the perlin noise values used for the terrain
        //     float baseNoiseOffset = (float)seed + 0.5f;
        //     float baseNoiseScale = 0.125f;
        //
        //     // Create a base terrain at the bottom of the terrain height
        //     int zMin = fieldBounds.zMin - height;
        //     // bool[,,] terrain = new bool[fieldBounds.size.x, fieldBounds.size.y, height];
        //     List<Vector3Int> terrain = new List<Vector3Int>();
        //     for (int z = zMin; z < 0; z++)
        //     {
        //         float terrainThreshold = ((float)(z - zMin)) / Mathf.Abs(zMin - 1);
        //
        //         for (int x = fieldBounds.xMin; x < fieldBounds.xMax + 1; x++)
        //         {
        //             for (int y = fieldBounds.yMin; y < fieldBounds.yMax + 1; y++)
        //             {
        //                 float terrainSample = Mathf.PerlinNoise((x * baseNoiseScale) + baseNoiseOffset,(y * baseNoiseScale) + baseNoiseOffset);
        //                 if (z == zMin || terrainSample >= terrainThreshold)
        //                 {
        //                     terrain.Add(new Vector3Int(x,y,z));
        //                 }
        //             }
        //         }
        //     }
        //     
        //     // Hollow out any unneeded terrain
        //     return terrain;
        //
        //     // Store the remaining terrain and convert it to terrain directions
        //     // TODO: CODE HERE
        //     // TODO: BREAK OUT TERRAIN INTO ITS OWN SUBNAMESPACE OF COURSE
        // }
        //
        // // Clamps a position to one within a bounds
        // private Vector3Int ClampWithinBounds(BoundsInt bounds, Vector3Int position)
        // {
        //     if (position.x < bounds.xMin)
        //     {
        //         position.x = bounds.xMin;
        //     }
        //     if (position.x > bounds.xMax)
        //     {
        //         position.x = bounds.xMax;
        //     }
        //     
        //     if (position.y < bounds.yMin)
        //     {
        //         position.y = bounds.yMin;
        //     }
        //     if (position.y > bounds.yMax)
        //     {
        //         position.y = bounds.yMax;
        //     }
        //
        //     return position;
        // }
        //
        // // Modifies the height of the level path
        // private List<Vector3Int> ModifyHeight(List<Vector3Int> path, int level, int seed)
        // {
        //     // Create a random for generation
        //     System.Random rand = new System.Random(seed + level);
        //     
        //     // Chance of change
        //     float zChangeChance = 0.2f;
        //     int zCooldown = 3;
        //     
        //     // Iterate over the level path and create some depth
        //     List<Vector3Int> pointPath = new List<Vector3Int>();
        //     int currentZ = 0;
        //     int currentCooldown = 0;
        //     for (int i = 0; i < path.Count; i++)
        //     {
        //         currentCooldown--;
        //         
        //         Vector3Int point = path[i];
        //         // Don't change height on the first or last path point
        //         if (i > 0 && i < path.Count - 1)
        //         {
        //             // Don't change height on path points that arent straight on either end
        //             Vector3Int currentToLast = point - path[i - 1];
        //             Vector3Int nextToCurrent = path[i + 1] - point;
        //             if (nextToCurrent == currentToLast)
        //             {
        //                 if ((float)rand.NextDouble() > 1f - zChangeChance && currentCooldown <= 0)
        //                 {
        //                     // Add a point to the path before the z has changed (the next point added will end up empty)
        //                     point.z = currentZ;
        //                     pointPath.Add(point);
        //             
        //                     if ((float)rand.NextDouble() >= 0.5f)
        //                     {
        //                         currentZ += 1;
        //                     }
        //                     else
        //                     {
        //                         currentZ -= 1;
        //                     }
        //
        //                     currentCooldown = zCooldown;
        //                 }
        //             }
        //         }
        //
        //         // Move the point to the current z
        //         point.z = currentZ;
        //         pointPath.Add(point);
        //     }
        //
        //     return pointPath;
        // }
        //
        // // Modifies the height of the arms along the level path
        // private Dictionary<List<Vector3Int>, List<Vector3Int>> MapArmsToHeight(List<Vector3Int> path, List<Vector3Int> modifiedPath, Dictionary<List<int>, List<Vector3Int>> arms)
        // {
        //     Dictionary<List<Vector3Int>, List<Vector3Int>> modifiedArms = new Dictionary<List<Vector3Int>, List<Vector3Int>>();
        //     
        //     // Iterate over the dictonary and get each arm
        //     foreach (KeyValuePair<List<int>, List<Vector3Int>> arm in arms)
        //     {
        //         // Get the start and end heights in the new path
        //         int startIndex = arm.Key[0];
        //         int endIndex = arm.Key[1];
        //
        //         Vector3Int start = modifiedPath.ElementAt(startIndex);
        //         Vector3Int end = modifiedPath.ElementAt(endIndex);
        //         
        //         // Get the height difference
        //         int heightDifference = end.z - start.z;
        //         // Get the total number of cells the arm contains
        //         int armLength = arm.Value.Count;
        //         // Determine the rate of change of z height
        //         int rateOfChange = (int)Mathf.Floor(armLength / ((int)Mathf.Abs(heightDifference) + 1));
        //         
        //         // Iterate over the arm, every rate of change, changing the z height
        //         int zDirection = 0;
        //         if (heightDifference != 0)
        //         {
        //             zDirection = heightDifference / (int)Mathf.Abs(heightDifference);
        //         }
        //         int change = 0;
        //         int zHeight = start.z;
        //         List<Vector3Int> newArm = new List<Vector3Int>();
        //         for (int i = 0; i < armLength; i++)
        //         {
        //             Vector3Int armFlat = arm.Value[i];
        //             if (change >= rateOfChange)
        //             {
        //                 change = 0;
        //                 zHeight += zDirection;
        //             }
        //
        //             Vector3Int armProjected = armFlat;
        //             armProjected.z = zHeight;
        //             
        //             newArm.Add(armProjected);
        //             change++;
        //         }
        //
        //
        //         List<Vector3Int> startEnd = new List<Vector3Int>() { start, end };
        //         modifiedArms.Add(startEnd, newArm);
        //     }
        //
        //     return modifiedArms;
        // }
        //
        // // Returns a set of arms
        // private Dictionary<List<int>, List<Vector3Int>> GetArms(List<Vector3Int> path, List<Vector3Int> obstacles, BoundsInt fieldBounds, int level, int seed)
        // {
        //     // Determine how many arms should be created
        //     int armCount = (int)Mathf.Floor(0.075f * level);
        //     
        //     // Create a random for generation
        //     System.Random rand = new System.Random(seed + level);
        //     
        //     // Points to choose from
        //     List<Vector3Int> armPointsPossible = new List<Vector3Int>();
        //     armPointsPossible.AddRange(path);
        //     armPointsPossible.Remove(armPointsPossible[0]);
        //     armPointsPossible.Remove(armPointsPossible[armPointsPossible.Count - 1]);
        //     List<Vector3Int> elbowPointsPossible = obstacles;
        //
        //     // Return data store
        //     Dictionary<List<int>, List<Vector3Int>> arms = new Dictionary<List<int>, List<Vector3Int>>();
        //     
        //     for (int a = 0; a < armCount; a++)
        //     {
        //         // For each arm, get a start and end point along the path
        //         Vector3Int start = armPointsPossible.ElementAt(rand.Next(0, armPointsPossible.Count - 1));
        //         int startIndex = path.FindIndex(p => p.Equals(start));
        //         armPointsPossible.Remove(start);
        //         Vector3Int end = armPointsPossible.ElementAt(rand.Next(0, armPointsPossible.Count - 1));
        //         int endIndex = path.FindIndex(p => p.Equals(end));
        //         armPointsPossible.Remove(end);
        //         // Get a random obstacle point as the arm middle ground
        //         Vector3Int elbow = elbowPointsPossible.ElementAt(rand.Next(0, elbowPointsPossible.Count - 1));
        //         elbowPointsPossible.Remove(elbow);
        //
        //         // Pathfind from the start to end thru the elbow
        //         List<Vector3Int> startToElbow = FindPath(start, elbow, obstacles, fieldBounds);
        //         List<Vector3Int> elbowToEnd = FindPath(elbow, end, obstacles, fieldBounds);
        //         elbowToEnd.Remove(elbowToEnd[0]);
        //         startToElbow.AddRange(elbowToEnd);
        //         List<Vector3Int> arm = startToElbow;
        //         
        //         // Arm start and end
        //         List<int> armStartEnd = new List<int>() {startIndex, endIndex};
        //
        //         // Add the arm to the data store
        //         arms.Add(armStartEnd, arm);
        //     }
        //
        //     return arms;
        // }
        //
        // // Returns a list of random points within a bounds
        // private List<Vector3Int> CreateRandomObstacles(BoundsInt bounds, Vector3Int start, Vector3Int end, int count, int seed)
        // {
        //     // Create a random for generation
        //     System.Random rand = new System.Random(seed);
        //     
        //     //Obstacles
        //     List<Vector3Int> obstacles = new List<Vector3Int>();
        //
        //     int maxIterations = count * 4;
        //     int iterations = 0;
        //     while (iterations < maxIterations)
        //     {
        //         if (obstacles.Count >= count)
        //         {
        //             break;
        //         }
        //         
        //         // Create random point within the bounds
        //         Vector3Int randomPoint = new Vector3Int(rand.Next(bounds.xMin, bounds.xMax), rand.Next(bounds.yMin, bounds.yMax), 0);
        //
        //         // Test if that point is start, end, and in the list of points, if not, store it
        //         if (!randomPoint.Equals(start) && !randomPoint.Equals(end) && !obstacles.Contains(randomPoint))
        //         {
        //             obstacles.Add(randomPoint);
        //         }
        //
        //         iterations++;
        //     }
        //
        //     return obstacles;
        // }
        //
        // // Pathfinds from a start to an end
        // private List<Vector3Int> FindPath(Vector3Int start, Vector3Int end, List<Vector3Int> obstacles, BoundsInt bounds)
        // {
        //     // Create a pathfinding grid
        //     PathGrid pathGrid = new PathGrid(bounds, obstacles);
        //
        //     PathNode startNode = pathGrid.GetNode(start);
        //     PathNode endNode = pathGrid.GetNode(end);
        //     List<PathNode> openList = new List<PathNode>() {startNode};
        //     List<PathNode> closedList = new List<PathNode>();
        //     
        //     // Costs
        //     int moveStraightCost = 10;
        //     int moveDiagonalCost = 100;
        //     
        //     // Calculate the f cost for each cell
        //     for (int x = pathGrid.bounds.xMin; x < pathGrid.bounds.xMax; x++)
        //     {
        //         for (int y = pathGrid.bounds.yMin; y < pathGrid.bounds.yMax; y++)
        //         {
        //             PathNode pathNode = pathGrid.GetNode(new Vector3Int(x, y, 0));
        //
        //             pathNode.gCost = int.MaxValue;
        //             pathNode.CalculateFCost();
        //             pathNode.cameFromNode = null;
        //         }
        //     }
        //
        //     // Starting Data
        //     startNode.gCost = 0;
        //     startNode.hCost = CalculateDistanceCost(startNode, endNode, moveStraightCost, moveDiagonalCost);
        //     startNode.CalculateFCost();
        //
        //     while (openList.Count > 0)
        //     {
        //         PathNode currentNode = GetLowestFCost(openList);
        //
        //         if (currentNode == endNode)
        //         {
        //             // Reached final node
        //             return CalculatePath(endNode);
        //         }
        //
        //         openList.Remove(currentNode);
        //         closedList.Add(currentNode);
        //
        //         List<PathNode> neighbors = GetNeighbors(currentNode);
        //         foreach (PathNode neighbor in neighbors)
        //         {
        //             if(closedList.Contains(neighbor)) continue;
        //
        //             int tenativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbor, moveStraightCost, moveDiagonalCost);
        //             if (tenativeGCost < neighbor.gCost)
        //             {
        //                 neighbor.cameFromNode = currentNode;
        //                 neighbor.gCost = tenativeGCost;
        //                 neighbor.hCost = CalculateDistanceCost(neighbor, endNode, moveStraightCost, moveDiagonalCost);
        //                 neighbor.CalculateFCost();
        //
        //                 if (!openList.Contains(neighbor))
        //                 {
        //                     openList.Add(neighbor);
        //                 }
        //             }
        //         }
        //     }
        //     
        //     // Out of nodes on the open list
        //     return null;
        // }
        //
        // // Returns a list of neighboring path nodes
        // private List<PathNode> GetNeighbors(PathNode node)
        // {
        //     List<PathNode> neighborList = new List<PathNode>();
        //     PathGrid grid = node.pathGrid;
        //
        //     // Left
        //     if (grid.Contains(node.position + Vector3Int.left))
        //     {
        //         neighborList.Add(grid.GetNode(node.position + Vector3Int.left));
        //     }
        //     // Right
        //     if (grid.Contains(node.position + Vector3Int.right))
        //     {
        //         neighborList.Add(grid.GetNode(node.position + Vector3Int.right));
        //     }
        //     // Up
        //     if (grid.Contains(node.position + Vector3Int.up))
        //     {
        //         neighborList.Add(grid.GetNode(node.position + Vector3Int.up));
        //     }
        //     // Down
        //     if (grid.Contains(node.position + Vector3Int.down))
        //     {
        //         neighborList.Add(grid.GetNode(node.position + Vector3Int.down));
        //     }
        //
        //     return neighborList;
        // }
        //
        // // Calculates a path towards a node
        // private List<Vector3Int> CalculatePath(PathNode endNode)
        // {
        //     List<PathNode> path = new List<PathNode>();
        //     path.Add(endNode);
        //
        //     PathNode currentNode = endNode;
        //     while (currentNode.cameFromNode != null)
        //     {
        //         path.Add(currentNode.cameFromNode);
        //         currentNode = currentNode.cameFromNode;
        //     }
        //
        //     path.Reverse();
        //
        //     List<Vector3Int> pathPoints = new List<Vector3Int>();
        //     foreach (PathNode point in path)
        //     {
        //         pathPoints.Add(point.position);
        //     }
        //         
        //     return pathPoints;
        // }
        //
        // // Calculates the distance cost between two nodes
        // private int CalculateDistanceCost(PathNode a, PathNode b, int moveStraightCost, int moveDiagonalCost)
        // {
        //     int xDistance = Mathf.Abs(a.position.x - b.position.x);
        //     int yDistance = Mathf.Abs(a.position.y - b.position.y);
        //     int remaining = Mathf.Abs(xDistance - yDistance);
        //
        //     return moveDiagonalCost * Mathf.Min(xDistance, yDistance) + moveStraightCost * remaining;
        // }
        //
        // // Returns the lowest f-cost path node from a list of pathnodes
        // private PathNode GetLowestFCost(List<PathNode> pathNodeList)
        // {
        //     PathNode lowestFCostNode = pathNodeList[0];
        //     for (int i = 0; i < pathNodeList.Count; i++)
        //     {
        //         if (pathNodeList[i].fCost < lowestFCostNode.fCost)
        //         {
        //             lowestFCostNode = pathNodeList[i];
        //         }
        //     }
        //
        //     return lowestFCostNode;
        // }
        //
        // // Places field tiles in the game world
        // private void InstantiatePlayfield()
        // {
        //     // Iterate over all the tiles in the playfield
        //     foreach (FieldTile tile in playfield.tiles)
        //     {
        //         GameTile gameTile = Instantiate(gameTilePrefab, Vector3.zero, Quaternion.identity).GetComponent<GameTile>();
        //         gameTile.SetTile(tile);
        //         gameTiles.Add(gameTile.gameObject);
        //     }
        // }
        
        // Swaps the z and y in a passed Vector3Int and returns it as a Vector3
        private Vector3 GridToWorld(Vector3Int position)
        {
            return new Vector3(position.x, position.z * 0.5f, position.y);
        }
    }
}
