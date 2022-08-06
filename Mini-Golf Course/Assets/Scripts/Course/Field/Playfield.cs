using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Course;
using UnityEditor.SceneManagement;

namespace Course.Field
{
    [System.Serializable]
    public class Playfield
    {
        // General Props
        public int seed;
        public int level;
        
        // Playfield Props
        public FieldSettings fieldSettings;
        public BoundsInt bounds;
        
        // Playfield Items
        public Vector3Int start;
        public Vector3Int end;
        
        // Playfield Tiles
        public List<FieldTile> terrain = new List<FieldTile>();
        public List<FieldTile> track = new List<FieldTile>();

        // OLD
        
        
        // public List<Vector3Int> field = new List<Vector3Int>();
        public List<FieldTile> tiles = new List<FieldTile>();
        // public List<Vector3Int> terrain = new List<Vector3Int>();
        public List<Vector3Int> deco = new List<Vector3Int>();
        

        public Playfield()
        {
            
        }

        public Playfield(int seed, int level)
        {
            // Generate Playfield using seed and level
            // Store the seed and level in the playfield
            this.seed = seed;
            this.level = level;
            
            // Store a new field settings object
            fieldSettings = new FieldSettings(this, 0.125f, 0.25f);
            
            // Generate the field and the bounds of the level
            List<Vector3Int> field = GenerateField();

            // Generate Terrain
            GenerateTerrain();
            
            // Then Deco will be generated
            AddTerrainDeco();

            // Then cast the field down towards the terrain
            field = CastTrackToTerrain(field);
            
            // Then calculate the track tiles
            GenerateTrack(field);
            
            // Remove deco below any track
            CleanDeco();

            // Store the tiles
            // Need to create a start and end Vector3Int

            // Remove any deco that is occupied by tiles
        }
        
        // public Playfield(List<Vector3Int> field, Vector3Int start, Vector3Int end, List<Vector3Int> terrain, int seed, ModifierSettings modifierSettings)
        // {
        //     this.field = field;
        //     this.start = start;
        //     this.spawn = GridToWorld(start) + new Vector3(0f, 0.25f, 0f);
        //     this.end = end;
        //     this.terrain = terrain;
        //     this.modifierSettings = modifierSettings;
        //     
        //     GenerateTiles(seed);
        //     GenerateDeco(seed);
        // }
        
        // Returns the field locations for the playfield
        private List<Vector3Int> GenerateField()
        {
            // Create a random for generation
            System.Random rand = new System.Random(seed + level);
            
            // Store the start
            start = Vector3Int.zero;
            
            // Determine the end as a point on a circle of 'level' radius
            float randomAngle = rand.Next(0, 360);
            end = new Vector3Int((int)Mathf.Floor(Mathf.Cos(Mathf.Deg2Rad * randomAngle)* level), (int)Mathf.Floor(Mathf.Sin(Mathf.Deg2Rad * randomAngle) * level), 0);
            
            // Create a 'level' number of obstacles that the pathfinding must work around
            bounds = new BoundsInt(Vector3Int.zero, Vector3Int.one);
            Vector3Int min = new Vector3Int((int)Mathf.Min(start.x, end.x), (int)Mathf.Min(start.y, end.y), 0);
            Vector3Int max = new Vector3Int((int)Mathf.Max(start.x, end.x) + 5, (int)Mathf.Max(start.y, end.y) + 5, 1);
            bounds.SetMinMax(min, max);
            List<Vector3Int> obstacles = CreateRandomObstacles();
            
            // Get the total count of subdivisions of the field path
            int subdivisions = (int)Mathf.Floor(0.075f * level) + 1;
            int maxOffset = 5;
            
            // For each segment of the path, find the subpath
            List<Vector3Int> path = new List<Vector3Int>();
            Vector3Int currentStart = start;
            float subdivisionInterval = 1f / subdivisions;
            for (int i = 0; i < subdivisions; i++)
            {
                // Get the current end
                Vector3 subPoint = Vector3.Lerp(start, end, subdivisionInterval * (i + 1));
                Vector3Int currentEnd = new Vector3Int((int)Mathf.Floor(subPoint.x), (int)Mathf.Floor(subPoint.y), 0);
                
                // Offset the current end
                Vector3Int randomOffset = new Vector3Int(rand.Next(-maxOffset, maxOffset), rand.Next(-maxOffset, maxOffset));
                currentEnd += randomOffset;
                currentEnd = ClampWithinBounds(currentEnd);

                // Get the current path from current start to current end
                List<Vector3Int> currentPath = FindPath(currentStart, currentEnd, obstacles);
                currentPath.RemoveRange(currentPath.Count - 1, 1);
                
                // Add the current path to the main path and store the new current start
                currentStart = currentEnd;
                path.AddRange(currentPath);
            }

            // Return the field path
            return path;
        }
        
        // Returns a list of random obstacles for the field generation
        private List<Vector3Int> CreateRandomObstacles()
        {
            // Create a random for generation
            System.Random rand = new System.Random(seed);
            
            //Obstacles
            List<Vector3Int> obstacles = new List<Vector3Int>();

            int maxIterations = level * 4;
            int iterations = 0;
            while (iterations < maxIterations)
            {
                if (obstacles.Count >= level)
                {
                    break;
                }
                
                // Create random point within the bounds
                Vector3Int randomPoint = new Vector3Int(rand.Next(bounds.xMin, bounds.xMax), rand.Next(bounds.yMin, bounds.yMax), 0);

                // Test if that point is start, end, and in the list of points, if not, store it
                if (!randomPoint.Equals(start) && !randomPoint.Equals(end) && !obstacles.Contains(randomPoint))
                {
                    obstacles.Add(randomPoint);
                }

                iterations++;
            }

            return obstacles;
        }
        
        // Clamps a position to one within a bounds
        private Vector3Int ClampWithinBounds(Vector3Int position)
        {
            if (position.x < bounds.xMin)
            {
                position.x = bounds.xMin;
            }
            if (position.x > bounds.xMax)
            {
                position.x = bounds.xMax;
            }
            
            if (position.y < bounds.yMin)
            {
                position.y = bounds.yMin;
            }
            if (position.y > bounds.yMax)
            {
                position.y = bounds.yMax;
            }

            return position;
        }
        
        // Pathfinds from a start to an end
        private List<Vector3Int> FindPath(Vector3Int pathStart, Vector3Int pathEnd, List<Vector3Int> obstacles)
        {
            // Create a pathfinding grid
            PathGrid pathGrid = new PathGrid(bounds, obstacles);

            PathNode startNode = pathGrid.GetNode(pathStart);
            PathNode endNode = pathGrid.GetNode(pathEnd);
            List<PathNode> openList = new List<PathNode>() {startNode};
            List<PathNode> closedList = new List<PathNode>();
            
            // Costs
            int moveStraightCost = 10;
            int moveDiagonalCost = 100;
            
            // Calculate the f cost for each cell
            for (int x = pathGrid.bounds.xMin; x < pathGrid.bounds.xMax; x++)
            {
                for (int y = pathGrid.bounds.yMin; y < pathGrid.bounds.yMax; y++)
                {
                    PathNode pathNode = pathGrid.GetNode(new Vector3Int(x, y, 0));

                    pathNode.gCost = int.MaxValue;
                    pathNode.CalculateFCost();
                    pathNode.cameFromNode = null;
                }
            }

            // Starting Data
            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode, moveStraightCost, moveDiagonalCost);
            startNode.CalculateFCost();

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCost(openList);

                if (currentNode == endNode)
                {
                    // Reached final node
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                List<PathNode> neighbors = GetNeighbors(currentNode);
                foreach (PathNode neighbor in neighbors)
                {
                    if(closedList.Contains(neighbor)) continue;

                    int tenativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbor, moveStraightCost, moveDiagonalCost);
                    if (tenativeGCost < neighbor.gCost)
                    {
                        neighbor.cameFromNode = currentNode;
                        neighbor.gCost = tenativeGCost;
                        neighbor.hCost = CalculateDistanceCost(neighbor, endNode, moveStraightCost, moveDiagonalCost);
                        neighbor.CalculateFCost();

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }
            
            // Out of nodes on the open list
            return null;
        }
        
        // Calculates the distance cost between two nodes
        private int CalculateDistanceCost(PathNode a, PathNode b, int moveStraightCost, int moveDiagonalCost)
        {
            int xDistance = Mathf.Abs(a.position.x - b.position.x);
            int yDistance = Mathf.Abs(a.position.y - b.position.y);
            int remaining = Mathf.Abs(xDistance - yDistance);

            return moveDiagonalCost * Mathf.Min(xDistance, yDistance) + moveStraightCost * remaining;
        }
        
        // Returns the lowest f-cost path node from a list of pathnodes
        private PathNode GetLowestFCost(List<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList[0];
            for (int i = 0; i < pathNodeList.Count; i++)
            {
                if (pathNodeList[i].fCost < lowestFCostNode.fCost)
                {
                    lowestFCostNode = pathNodeList[i];
                }
            }

            return lowestFCostNode;
        }
        
        // Calculates a path towards a node
        private List<Vector3Int> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            path.Add(endNode);

            PathNode currentNode = endNode;
            while (currentNode.cameFromNode != null)
            {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }

            path.Reverse();

            List<Vector3Int> pathPoints = new List<Vector3Int>();
            foreach (PathNode point in path)
            {
                pathPoints.Add(point.position);
            }
                
            return pathPoints;
        }
        
        // Returns a list of neighboring path nodes
        private List<PathNode> GetNeighbors(PathNode node)
        {
            List<PathNode> neighborList = new List<PathNode>();
            PathGrid grid = node.pathGrid;

            // Left
            if (grid.Contains(node.position + Vector3Int.left))
            {
                neighborList.Add(grid.GetNode(node.position + Vector3Int.left));
            }
            // Right
            if (grid.Contains(node.position + Vector3Int.right))
            {
                neighborList.Add(grid.GetNode(node.position + Vector3Int.right));
            }
            // Up
            if (grid.Contains(node.position + Vector3Int.up))
            {
                neighborList.Add(grid.GetNode(node.position + Vector3Int.up));
            }
            // Down
            if (grid.Contains(node.position + Vector3Int.down))
            {
                neighborList.Add(grid.GetNode(node.position + Vector3Int.down));
            }

            return neighborList;
        }
        
        // Generate terrain for the playfield
        private void GenerateTerrain()
        {
            // Create the perlin noise values used for the terrain
            float baseNoiseOffset = (float)seed + 0.5f;
            
            // Expand the bounds to fit the terrain
            bounds.zMin -= fieldSettings.terrainHeight;
            bounds.zMax += 1;
            bounds.xMin -= fieldSettings.terrainMargin;
            bounds.xMax += fieldSettings.terrainMargin;
            bounds.yMin -= fieldSettings.terrainMargin;
            bounds.yMax += fieldSettings.terrainMargin;

            // Create a base terrain at the bottom of the terrain height and build terrain up from there
            List<Vector3Int> terrainFlags = new List<Vector3Int>();
            for (int z = bounds.zMin; z < 0; z++)
            {
                float terrainThreshold = ((float)(z - bounds.zMin)) / Mathf.Abs(bounds.zMin - 1);

                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        float terrainSample = Mathf.PerlinNoise((x * fieldSettings.terrainScale) + baseNoiseOffset,(y * fieldSettings.terrainScale) + baseNoiseOffset);
                        if (z == bounds.zMin || terrainSample >= terrainThreshold)
                        {
                            terrainFlags.Add(new Vector3Int(x,y,z));
                        }
                    }
                }
            }
            
            // Convert terrain flags into a proper terrain dictonary and store it to the playfield
            ConvertTerrainFlagsToTerrain(terrainFlags);
        }
        
        // Converts a list of terrain filled positions to terrain and stores it in the playfield
        private void ConvertTerrainFlagsToTerrain(List<Vector3Int> terrainFlags)
        {
            // Iterate over the flags create the terrain dict
            List<Vector3Int> additiveFlags = new List<Vector3Int>();
            foreach (Vector3Int terrainPosition in terrainFlags)
            {
                bool[,,] neighbors = GetTerrainNeighbors(terrainFlags, terrainPosition);
                
                // If there is not a tile above, this terrain exists
                if (!neighbors[1, 1, 2])
                {
                    // Get the counts of neighbors by Z slice
                    int[] neighborCounts = GetNeighborCountByZSlice(neighbors);
                    FieldTile terrainTile = new FieldTile(terrainPosition, GetTerrainTileType(neighbors, neighborCounts), GetTerrainRotation(neighbors, neighborCounts));
                    if (terrainTile.tileType == FieldTileType.Elevated)
                    {
                        terrainTile.position += Vector3Int.forward;
                        terrainTile.tileType = FieldTileType.Flat;
                        additiveFlags.Add(terrainTile.position);
                    }
                    
                    terrain.Add(terrainTile);
                }
            }
            
            // Clear the terrain tiles
            terrain.Clear();
            
            // Combine the additive flags with the terrain flags
            terrainFlags.AddRange(additiveFlags);
            
            // Recalc the terrain tiles
            foreach (Vector3Int terrainPosition in terrainFlags)
            {
                bool[,,] neighbors = GetTerrainNeighbors(terrainFlags, terrainPosition);
                
                // If there is not a tile above, this terrain exists
                if (!neighbors[1, 1, 2])
                {
                    // Get the counts of neighbors by Z slice
                    int[] neighborCounts = GetNeighborCountByZSlice(neighbors);
                    FieldTile terrainTile = new FieldTile(terrainPosition, GetTerrainTileType(neighbors, neighborCounts), GetTerrainRotation(neighbors, neighborCounts));
                    if (terrainTile.tileType == FieldTileType.Elevated)
                    {
                        terrainTile.position += Vector3Int.forward;
                        terrainTile.tileType = FieldTileType.Flat;
                        additiveFlags.Add(terrainTile.position);
                    }
                    
                    terrain.Add(terrainTile);
                }
            }
        }
        
        // Returns a cube array of direct neighbors for the given terrain tile and flags list
        private bool[,,] GetTerrainNeighbors(List<Vector3Int> terrainFlags, Vector3Int terrainPosition)
        {
            bool[,,] neighbors = new bool[3, 3, 3];

            // Check all neighboring positions for terrain
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        Vector3Int offsetPosition = new Vector3Int(-1 + x, -1 + y, -1 + z);
                        Vector3Int neighborPosition = terrainPosition + offsetPosition;
                        
                        // First check if the neighbor position is out of bounds
                        if (!bounds.Contains(neighborPosition))
                        {
                            neighbors[x, y, z] = z == 1;
                            // Add true if the z is 2 and the next neighbor exists in the list
                            if (z == 2)
                            {
                                List<Vector3Int> neighboringOffsets = new List<Vector3Int>() { Vector3Int.down, Vector3Int.left, Vector3Int.right, Vector3Int.up };
                                bool neighborFlag = false;
                                foreach (Vector3Int neighboringOffset in neighboringOffsets)
                                {
                                    Vector3Int neighborCheck = neighboringOffset + neighborPosition;
                                    if (!neighborFlag)
                                    {
                                        neighborFlag = terrainFlags.Contains(neighborCheck);
                                    }
                                }
                                
                                neighbors[x, y, z] = neighborFlag;
                            }
                            continue;
                        }
                        else
                        {
                            // Is the neighbor position within the terrain flags
                            neighbors[x, y, z] = terrainFlags.Contains(neighborPosition);
                        }
                    }
                }
            }

            return neighbors;
        }
        
        // Returns the terrain type given the neighbors
        private FieldTileType GetTerrainTileType(bool[,,] neighbors, int[] neighborCounts)
        {
            // Get Standard Flats
            if (neighborCounts[2] == 0)
            {
                return FieldTileType.Flat;
            }
            
            // Get the 'Elevated' type
            if (IsTerrainElevated(neighbors, neighborCounts))
            {
                return FieldTileType.Elevated;
            }

            // Get the Inverted Corners
            // Inverted Corners appear when the top neighbor count is at least 3 and they are not all in a straight line
            if (IsTerrainInvertedCorner(neighbors, neighborCounts))
            {
                return FieldTileType.CornerInverse;
            }
            
            // Get the Slopes
            // Slopes only appear when direct top neighbors have neighbors on both sides
            if (IsTerrainSlope(neighbors, neighborCounts))
            {
                return FieldTileType.Slope;
            }
            
            // Get the Corners
            // Corners appear when only 1 neighbor is counted above
            if (neighborCounts[2] == 1)
            {
                return FieldTileType.Corner;
            }
            
            return FieldTileType.None;
        }
        
        // Returns a bool denoting if the terrain is a slope based on its neighbors
        private bool IsTerrainSlope(bool[,,] neighbors, int[] neighborCounts)
        {
            if (neighborCounts[2] <= 3 && neighborCounts[2] > 1)
            {
                if (neighbors[0, 1, 2])
                {
                    // X-
                    if (neighbors[0, 0, 2] || neighbors[0, 2, 2])
                    {
                        // X- Slope
                        return true;
                    }
                }
                if (neighbors[2, 1, 2])
                {
                    // X+
                    if (neighbors[2, 0, 2] || neighbors[2, 2, 2])
                    {
                        // X+ Slope
                        return true;
                    }
                }
                if (neighbors[1, 0, 2])
                {
                    // Y-
                    if (neighbors[0, 0, 2] || neighbors[2, 0, 2])
                    {
                        // Y- Slope
                        return true;
                    }
                }
                if (neighbors[1, 2, 2])
                {
                    // Y+
                    if (neighbors[0, 2, 2] || neighbors[2, 2, 2])
                    {
                        // Y+ Slope
                        return true;
                    }
                } 
            }

            if (neighborCounts[2] == 1)
            {
                if (neighbors[0, 1, 2])
                {
                    // X- Slope
                    return true;
                }
                if (neighbors[2, 1, 2])
                {
                    // X+ Slope
                    return true;
                }
                if (neighbors[1, 0, 2])
                {
                    // Y- Slope
                    return true;
                }
                if (neighbors[1, 2, 2])
                {
                    // Y+ Slope
                    return true;
                }
            }
            
            // Test for slopes where terrain is neighboring on left and right but not center
            if (neighborCounts[2] == 2)
            {
                if (neighbors[0, 0, 2] && neighbors[0, 2, 2])
                {
                    // X- Slope
                    return true;
                }
                if (neighbors[2, 0, 2] && neighbors[2, 2, 2])
                {
                    // X+ Slope
                    return true;
                }
                if (neighbors[0, 0, 2] && neighbors[2, 0, 2])
                {
                    // Y- Slope
                    return true;
                }
                if (neighbors[0, 2, 2] && neighbors[2, 2, 2])
                {
                    // Y+ Slope
                    return true;
                }
            }

            return false;
        }
        
        // Returns a bool denoting if the terrain is an inverted corner based on its neighbors
        private bool IsTerrainInvertedCorner(bool[,,] neighbors, int[] neighborCounts)
        {
            if (neighborCounts[2] > 2)
            {
                if (neighborCounts[2] == 3)
                {
                    if (neighbors[0, 1, 2] && neighbors[1, 0, 2])
                    {
                        // X- and Y-
                        return true;
                    }
                    if (neighbors[2, 1, 2] && neighbors[1, 2, 2])
                    {
                        // X+ and Y+
                        return true;
                    }
                    if (neighbors[0, 1, 2] && neighbors[1, 2, 2])
                    {
                        // X- and Y+
                        return true;
                    }
                    if (neighbors[2, 1, 2] && neighbors[1, 0, 2])
                    {
                        // X+ and Y-
                        return true;
                    }

                    return false;
                }
                else
                {
                    return true;
                }
            }
            
            return false;
        }
        
        // Returns a bool denoting if the terrain is elevated based on its neighbors
        private bool IsTerrainElevated(bool[,,] neighbors, int[] neighborCounts)
        {
            if (neighborCounts[2] < 5)
            {
                return false;
            }
            else
            {
                if (neighbors[0, 1, 2])
                {
                    // X-
                    if (neighbors[1, 0, 2] && neighbors[1, 2, 2])
                    {
                        return true;
                    }
                }
                if (neighbors[2, 1, 2])
                {
                    // X+
                    if (neighbors[1, 0, 2] && neighbors[1, 2, 2])
                    {
                        return true;
                    }
                }
                if (neighbors[1, 0, 2])
                {
                    // Y-
                    if (neighbors[0, 1, 2] && neighbors[2, 1, 2])
                    {
                        return true;
                    }
                }
                if (neighbors[1, 2, 2])
                {
                    // Y+
                    if (neighbors[0, 1, 2] && neighbors[2, 1, 2])
                    {
                        return true;
                    }
                } 
            }

            return false;
        }
        
        // Returns the rotation of the terrain given the neighbors
        private int GetTerrainRotation(bool[,,] neighbors, int[] neighborCounts)
        {
            // Inverted Corner rotation
            if (IsTerrainInvertedCorner(neighbors, neighborCounts))
            {
                if (neighbors[0, 1, 2] && neighbors[1, 0, 2])
                {
                    // X- and Y-
                    return -90;
                }
                if (neighbors[2, 1, 2] && neighbors[1, 2, 2])
                {
                    // X+ and Y+
                    return 90;
                }
                if (neighbors[0, 1, 2] && neighbors[1, 2, 2])
                {
                    // X- and Y+
                    return 0;
                }
                if (neighbors[2, 1, 2] && neighbors[1, 0, 2])
                {
                    // X+ and Y-
                    return 180;
                }
            }
            
            // Slope rotation
            if (IsTerrainSlope(neighbors, neighborCounts))
            {
                if (neighbors[0, 1, 2])
                {
                    // X-
                    if (neighbors[0, 0, 2] || neighbors[0, 2, 2])
                    {
                        // X- Slope
                        return 0;
                    }
                }
                if (neighbors[2, 1, 2])
                {
                    // X+
                    if (neighbors[2, 0, 2] || neighbors[2, 2, 2])
                    {
                        // X+ Slope
                        return 180;
                    }
                }
                if (neighbors[1, 0, 2])
                {
                    // Y-
                    if (neighbors[0, 0, 2] || neighbors[2, 0, 2])
                    {
                        // Y- Slope
                        return -90;
                    }
                }
                if (neighbors[1, 2, 2])
                {
                    // Y+
                    if (neighbors[0, 2, 2] || neighbors[2, 2, 2])
                    {
                        // Y+ Slope
                        return 90;
                    }
                }
                
                if (neighborCounts[2] == 1)
                {
                    if (neighbors[0, 1, 2])
                    {
                        // X- Slope
                        return 0;
                    }
                    if (neighbors[2, 1, 2])
                    {
                        // X+ Slope
                        return 180;
                    }
                    if (neighbors[1, 0, 2])
                    {
                        // Y- Slope
                        return -90;
                    }
                    if (neighbors[1, 2, 2])
                    {
                        // Y+ Slope
                        return 90;
                    }
                }
                
                if (neighborCounts[2] == 2)
                {
                    if (neighbors[0, 0, 2] && neighbors[0, 2, 2])
                    {
                        // X- Slope
                        return 0;
                    }
                    if (neighbors[2, 0, 2] && neighbors[2, 2, 2])
                    {
                        // X+ Slope
                        return 180;
                    }
                    if (neighbors[0, 0, 2] && neighbors[2, 0, 2])
                    {
                        // Y- Slope
                        return -90;
                    }
                    if (neighbors[0, 2, 2] && neighbors[2, 2, 2])
                    {
                        // Y+ Slope
                        return 90;
                    }
                }
            }
            
            // Corner rotation
            if (neighborCounts[2] == 1)
            {
                if (neighbors[0, 0, 2])
                {
                    // X- Y-
                    return -90;
                }
                if (neighbors[2, 0, 2])
                {
                    // X+ Y-
                    return 180;
                }
                if (neighbors[2, 2, 2])
                {
                    // X+ Y+
                    return 90;
                }
                if (neighbors[0, 2, 2])
                {
                    // X- Y+
                    return 0;
                }
            }
            
            // All other rotations
            return 0;
        }
        
        // Returns a count of neighbors at each given slice of the neighbors array (by z)
        private int[] GetNeighborCountByZSlice(bool[,,] neighbors)
        {
            int[] counts = new int[3];
            
            // Iterate over the neighbors and store their counts
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        counts[z] += neighbors[x, y, z] ? 1 : 0;
                    }
                }
            }

            return counts;
        }
        
        // Add decoration to the terrain
        private void AddTerrainDeco()
        {
            // Create a random generator
            System.Random rand = new System.Random(seed + level);
            
            // Iterate over each terrain tile
            for (int i = 0; i < terrain.Count; i++)
            {
                // Determine if the terrain tile should have decoration
                if ((float)rand.NextDouble() > 1 - fieldSettings.decoChance)
                {
                    // Add a random deco to the terrain
                    List<TileModifier> decoModifiers = new List<TileModifier>() { TileModifier.Rock, TileModifier.Tree, TileModifier.Bush };
                    
                    terrain[i].AddModifier(decoModifiers[rand.Next(0, decoModifiers.Count - 1)]);
                }
            }
        }
        
        // Casts the field positions to be directly above the terrain
        private List<Vector3Int> CastTrackToTerrain(List<Vector3Int> field)
        {
            // Iterate over the field, casting each tile to 1 above the terrain at that location
            int currentZ = 0;
            int lastZ = 0;
            List<Vector3Int> fieldCast = new List<Vector3Int>();
            for (int i = 0; i < field.Count; i++)
            {
                // Get the field position
                Vector3Int fieldPosition = field[i];
                
                // Get the terrain z
                int terrainZ = GetTerrainZ(fieldPosition);
                
                if (i == 0)
                {
                    // On the first tile, determine the current Z
                    currentZ = terrainZ + 1;
                    lastZ = currentZ;
                }
                else
                {
                    // Check if the terrain z is more than 1 higher or lower than the current z
                    if ((int)Mathf.Abs((terrainZ + 1) - currentZ) > 1)
                    {
                        // Only increment the step down by 1
                        int terrainZChange = (terrainZ + 1) - currentZ;
                        currentZ += (terrainZChange / (int)Mathf.Abs(terrainZChange));
                    }
                    else
                    { 
                        // Store the new current z
                        currentZ = terrainZ + 1;
                    }
                }

                // // Add an extra position at the last z if the z has changed
                // if (lastZ != currentZ)
                // {
                //     fieldCast.Add(new Vector3Int(fieldPosition.x, fieldPosition.y, lastZ));
                // }
                
                // Store the new field tile to the cast list
                fieldCast.Add(new Vector3Int(fieldPosition.x, fieldPosition.y, currentZ));

                lastZ = currentZ;
            }
            
            return fieldCast;
        }
        
        // Returns the z of the terrain given a position
        private int GetTerrainZ(Vector3Int position)
        {
            FieldTile terrainTile = terrain.Find(t => t.position.x == position.x && t.position.y == position.y);
            if (terrainTile.position != null)
            {
                if (terrainTile.tileType == FieldTileType.Flat)
                {
                    return terrainTile.position.z - 1;
                }
                return terrainTile.position.z;
            }

            return position.z;
        }

        // Generate track for the playfield
        private void GenerateTrack(List<Vector3Int> trackList)
        {
            // Iterate down the track
            foreach (Vector3Int trackPosition in trackList)
            {
                // Get all neighbors for the current track position
                bool[,,] trackNeighbors = GetTrackNeighbors(trackList, trackPosition);
                int totalNeighborCount = GetTrackNeighborCount(trackNeighbors);
                List<Vector3Int> neighborDirections = GetNeighborDirections(trackNeighbors);

                track.Add(new FieldTile(trackPosition, GetTrackTileType(neighborDirections, totalNeighborCount), GetTrackRotation(neighborDirections, totalNeighborCount)));
            }
        }
        
        // Returns a cube array of direct neighbors for the given terrain tile and flags list
        private bool[,,] GetTrackNeighbors(List<Vector3Int> trackPositions, Vector3Int trackPosition)
        {
            bool[,,] neighbors = new bool[3, 3, 3];

            // Check all neighboring positions for terrain
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        Vector3Int offsetPosition = new Vector3Int(-1 + x, -1 + y, -1 + z);
                        Vector3Int neighborPosition = trackPosition + offsetPosition;
                        
                        // Check if the neighbor is in the track list
                        neighbors[x, y, z] = trackPositions.Contains(neighborPosition);
                    }
                }
            }
            
            // Set the source tile to false
            neighbors[1, 1, 1] = false;

            return neighbors;
        }
        
        // Returns a count of all track neighbors
        private int GetTrackNeighborCount(bool[,,] neighbors)
        {
            int count = 0;
            // Iterate over all neighbors
            foreach (bool neighbor in neighbors)
            {
                if (neighbor)
                {
                    count++;
                }
            }

            return count;
        }
        
        // Returns a count of all track neighbors that are on the same plane as the source tile
        private int GetTrackFlatNeighborCount(bool[,,] neighbors)
        {
            int count = 0;
            // Iterate only on the flat plane
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (neighbors[x,y,1])
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        
        // Returns the track type given the neighbors
        private FieldTileType GetTrackTileType(List<Vector3Int> neighborDirections, int totalNeighborCount)
        {
            // Solo point
            if (totalNeighborCount == 0)
            {
                return FieldTileType.Solo;
            }
            
            // End point
            if (totalNeighborCount == 1)
            {
                return FieldTileType.End;
            }
            
            // Slope point
            if (IsTrackSlope(neighborDirections))
            {
                return FieldTileType.Slope;
            }
            
            // Straight point
            if (IsTrackStraight(neighborDirections))
            {
                return FieldTileType.Straight;
            }
            
            // Corner point
            if (IsTrackCorner(neighborDirections))
            {
                return FieldTileType.Corner;
            }
            
            return FieldTileType.None;
        }
        
        // Returns the track rotation given the neighbors
        private int GetTrackRotation(List<Vector3Int> neighborDirections, int totalNeighborCount)
        {
            // Directions
            bool negX = neighborDirections.FindIndex(d => d.x == -1 && d.y == 0) != -1;
            bool posX = neighborDirections.FindIndex(d => d.x == 1 && d.y == 0) != -1;
            bool negY = neighborDirections.FindIndex(d => d.y == -1 && d.x == 0) != -1;
            bool posY = neighborDirections.FindIndex(d => d.y == 1 && d.x == 0) != -1;
            
            // End rotation
            if (totalNeighborCount == 1)
            {
                if (negY)
                {
                    // Y-
                    return 0;
                }
                if (posY)
                {
                    // Y+
                    return 180;
                }
                if (negX)
                {
                    // X-
                    return 90;
                }
                if (posX)
                {
                    // X+
                    return -90;
                }
            }

            // Straight rotation
            if (IsTrackStraight(neighborDirections))
            {
                if (negX && posX)
                {
                    // Straight on X
                    return 90;
                }
                if (negY && posY)
                {
                    // Straight on Y
                    return 0;
                }
            }
            
            // Corner rotation
            if (IsTrackCorner(neighborDirections))
            {
                if (negX && negY)
                {
                    // X- Y-
                    return 0;
                }
                if (negX && posY)
                {
                    // X- Y+
                    return 90; // Right
                }
                if (posX && posY)
                {
                    // X+ Y+
                    return 180;
                }
                if (posX && negY)
                {
                    // X+ Y-
                    return -90; // Right
                }
            }
            
            return 0;
        }
        
        // Returns the directions of the neighbors
        private List<Vector3Int> GetNeighborDirections(bool[,,] neighbors)
        {
            List<Vector3Int> directions = new List<Vector3Int>();
            
            // Iterate over the neighbors
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        if (neighbors[x, y, z])
                        {
                            // Get the direction based on the track position
                            directions.Add(new Vector3Int(-1 + x, -1 + y, -1 + z));
                        }
                    }
                }
            }

            return directions;
        }
        
        // Returns a bool denoting if the track neighbors are straight
        private bool IsTrackStraight(List<Vector3Int> neighborDirections)
        {
            bool negX = neighborDirections.FindIndex(d => d.x == -1 && d.y == 0) != -1;
            bool posX = neighborDirections.FindIndex(d => d.x == 1 && d.y == 0) != -1;
            bool negY = neighborDirections.FindIndex(d => d.y == -1 && d.x == 0) != -1;
            bool posY = neighborDirections.FindIndex(d => d.y == 1 && d.x == 0) != -1;
            
            if (negX && posX)
            {
                // X- X+
                return true;
            }
            if (negY && posY)
            {
                // Y- Y+
                return true;
            }

            return false;
        }
        
        // Returns a bool denoting if the track neighbors are cornered
        private bool IsTrackCorner(List<Vector3Int> neighborDirections)
        {
            bool negX = neighborDirections.FindIndex(d => d.x == -1 && d.y == 0) != -1;
            bool posX = neighborDirections.FindIndex(d => d.x == 1 && d.y == 0) != -1;
            bool negY = neighborDirections.FindIndex(d => d.y == -1 && d.x == 0) != -1;
            bool posY = neighborDirections.FindIndex(d => d.y == 1 && d.x == 0) != -1;
            
            if (negX && negY)
            {
                // X- Y-
                return true;
            }
            if (negX && posY)
            {
                // X- Y+
                return true;
            }
            if (posX && posY)
            {
                // X+ Y+
                return true;
            }
            if (posX && negY)
            {
                // X+ Y-
                return true;
            }
            
            return false;
        }
        
        // Returns a bool denoting if the track neighbors are straight
        private bool IsTrackSlope(List<Vector3Int> neighborDirections)
        {
            bool negX = neighborDirections.FindIndex(d => d.x == -1 && d.y == 0 && d.z != 0) != -1;
            bool posX = neighborDirections.FindIndex(d => d.x == 1 && d.y == 0 && d.z != 0) != -1;
            bool negY = neighborDirections.FindIndex(d => d.y == -1 && d.x == 0 && d.z != 0) != -1;
            bool posY = neighborDirections.FindIndex(d => d.y == 1 && d.x == 0 && d.z != 0) != -1;
            
            if (negX && posX)
            {
                // X- X+
                return true;
            }
            if (negY && posY)
            {
                // Y- Y+
                return true;
            }

            return false;
        }
        
        // Removes any deco modifers directly below any track
        private void CleanDeco()
        {
            // Iterate over each track
            foreach (FieldTile trackTile in track)
            {
                Vector3Int tileBelowPosition = trackTile.position;
                int terrainIndex = terrain.FindIndex(t => t.position == tileBelowPosition);
                
                if (terrainIndex != -1)
                {
                    terrain[terrainIndex].modifiers.Clear();
                }
            }
        }
    }
}
