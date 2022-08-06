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
        public List<FieldTile> river = new List<FieldTile>();
        public List<FieldTile> track = new List<FieldTile>();
        public List<FieldTile> support = new List<FieldTile>();
        
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
            fieldSettings = new FieldSettings(this, 0.075f, 0.25f, 0.1f, 0.2f, 0.1f);
            
            // Generate the field and the bounds of the level
            List<Vector3Int> field = GenerateField();

            // Generate Terrain
            GenerateTerrain();
            
            // Then Deco will be generated
            AddTerrainDeco();

            // Then cast the field down towards the terrain
            field = CastTrackToTerrain(field);
            
            // Make sure the last field tile is on the same z as the previous
            Vector3Int lastPosition = field[field.Count - 1];
            lastPosition.z = field[field.Count - 2].z;
            field[field.Count - 1] = lastPosition;

            // Then calculate the track tiles
            GenerateTrack(field);

            // Manually clean the track
            ManualTrackClean();
            
            // Remove deco below any track
            CleanDeco();

            // Store the end
            end = track[track.Count - 1].position;
            
            // Add a river to the the terrain
            GenerateRiver();
            
            // Add the hole modifier to the end tile
            track[track.Count - 1].modifiers.Add(TileModifier.Hole);

            // Remove flat terrain under the hole
            ClearUnderHole();
            
            // Generate the supports
            GenerateSupports();
        }

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
                    if ((float)rand.NextDouble() > 1 - fieldSettings.terrainRockChance)
                    {
                        terrain[i].AddModifier(TileModifier.Rock);
                        continue;
                    }
                    
                    if ((float)rand.NextDouble() > 1 - fieldSettings.terrainBushChance)
                    {
                        terrain[i].AddModifier(TileModifier.Bush);
                        continue;
                    }
                    
                    if ((float)rand.NextDouble() > 1 - fieldSettings.terrainTreeChance)
                    {
                        terrain[i].AddModifier(TileModifier.Tree);
                        continue;
                    }
                    
                    // List<TileModifier> decoModifiers = new List<TileModifier>() { TileModifier.Bush, TileModifier.Tree, TileModifier.Rock };
                    //
                    // terrain[i].AddModifier(decoModifiers[rand.Next(0, decoModifiers.Count)]);
                }
            }
        }
        
        // Casts the field positions to be directly above the terrain
        private List<Vector3Int> CastTrackToTerrain(List<Vector3Int> field)
        {
            // Generate the track as it stands
            GenerateTrack(field);
            
            // Iterate down the track, for each track tile there can be a z level it is at
            // That z level can change only when the track is straight
            Vector3Int startPosition = field[0];
            int currentZ = GetTerrainZ(startPosition) + 1;
            List<Vector3Int> trackCasted = new List<Vector3Int>();
            for (int i = 0; i < track.Count; i++)
            {
                if (track[i].tileType == FieldTileType.Straight)
                {
                    // currentZ = GetTerrainZ(track[i].position) + 1;
                    // Create the search area
                    Vector3Int minSearch = track[i].position - Vector3Int.one;
                    Vector3Int maxSearch = track[i].position + Vector3Int.one;
                    BoundsInt search = new BoundsInt();
                    search.SetMinMax(minSearch, maxSearch);
                    search.zMin = track[i].position.z;
                    search.zMax = track[i].position.z + 1;
                    
                    // Search for the max z
                    currentZ = GetMaxTerrainZ(track[i].position, search) + 1;
                }

                Vector3Int castPosition = track[i].position;
                castPosition.z = currentZ;
                trackCasted.Add(castPosition);
            }
            
            // Clear the created track
            track.Clear();

            return trackCasted;
        }

        // Returns the z of the terrain given a position
        private int GetTerrainZ(Vector3Int position)
        {
            int terrainTileIndex = terrain.FindIndex(t => t.position.x == position.x && t.position.y == position.y);
            if (terrainTileIndex != -1)
            {
                if (terrain[terrainTileIndex].tileType == FieldTileType.Flat)
                {
                    return terrain[terrainTileIndex].position.z - 1;
                }
                return terrain[terrainTileIndex].position.z;
            }

            return position.z;
        }
        
        // Returns the max z of the terrain given a position within a given radius
        private int GetMaxTerrainZ(Vector3Int position, BoundsInt search)
        {
            List<int> terrainSamples = new List<int>();
            // Iterate over the terrain samples
            foreach (Vector3Int searchPosition in search.allPositionsWithin)
            {
                terrainSamples.Add(GetTerrainZ(searchPosition));
            }
            
            // Get the max sample height
            return terrainSamples.Max(t => t);
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
            if (IsTrackEnd(neighborDirections))
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
            int negXIndex = neighborDirections.FindIndex(d => d.x == -1 && d.y == 0);
            int posXIndex = neighborDirections.FindIndex(d => d.x == 1 && d.y == 0);
            int negYIndex = neighborDirections.FindIndex(d => d.y == -1 && d.x == 0);
            int posYIndex = neighborDirections.FindIndex(d => d.y == 1 && d.x == 0);
            
            bool negX = negXIndex != -1;
            bool posX = posXIndex != -1;
            bool negY = negYIndex != -1;
            bool posY = posYIndex != -1;
            
            // End rotation
            if (IsTrackEnd(neighborDirections))
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
            
            // Slope Rotation
            bool isSloping = false;
            if (negXIndex != -1 && posXIndex != -1)
            {
                // X- X+
                if (neighborDirections[negXIndex].z == 1)
                {
                    // X-
                    return 90;
                    isSloping = true;
                }
                if (neighborDirections[posXIndex].z == 1)
                {
                    // X+
                    return -90;
                    isSloping = true;
                }
            }
            if (negYIndex != -1 && posYIndex != -1)
            {
                // Y- Y+
                if (neighborDirections[negYIndex].z == 1)
                {
                    // Y-
                    return 0;
                    isSloping = true;
                }
                if (neighborDirections[posYIndex].z == 1)
                {
                    // Y+
                    return 180;
                    isSloping = true;
                }
            }

            // Straight rotation
            if (IsTrackStraight(neighborDirections) && !isSloping)
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
        private bool IsTrackEnd(List<Vector3Int> neighborDirections)
        {
            // Only a single neighbor
            if (neighborDirections.Count == 1)
            {
                return true;
            }
            
            bool negX = neighborDirections.FindIndex(d => d.x == -1 && d.y == 0) != -1;
            bool posX = neighborDirections.FindIndex(d => d.x == 1 && d.y == 0) != -1;
            bool negY = neighborDirections.FindIndex(d => d.y == -1 && d.x == 0) != -1;
            bool posY = neighborDirections.FindIndex(d => d.y == 1 && d.x == 0) != -1;

            List<bool> directNeighbors = new List<bool>() { negX, posX, negY, posY };
            int countDirect = 0;

            foreach (bool directNeighbor in directNeighbors)
            {
                if (directNeighbor)
                {
                    countDirect++;
                }
            }

            if (countDirect == 1)
            {
                return true;
            }

            return false;
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
            int negXIndex = neighborDirections.FindIndex(d => d.x == -1 && d.y == 0);
            int posXIndex = neighborDirections.FindIndex(d => d.x == 1 && d.y == 0);
            int negYIndex = neighborDirections.FindIndex(d => d.y == -1 && d.x == 0);
            int posYIndex = neighborDirections.FindIndex(d => d.y == 1 && d.x == 0);
            
            if (negXIndex != -1 && posXIndex != -1)
            {
                // X- X+
                if (neighborDirections[negXIndex].z == 1)
                {
                    // X-
                    return true;
                }
                if (neighborDirections[posXIndex].z == 1)
                {
                    // X+
                    return true;
                }
            }
            if (negYIndex != -1 && posYIndex != -1)
            {
                // Y- Y+
                if (neighborDirections[negYIndex].z == 1)
                {
                    // Y-
                    return true;
                }
                if (neighborDirections[posYIndex].z == 1)
                {
                    // Y+
                    return true;
                }
            }

            return false;
        }

        // Manually adjusts the track where needed
        private void ManualTrackClean()
        {
            // Check over the entire track
            for (int i = 1; i < track.Count - 1; i++)
            {
                FieldTile previousTile = track[i - 1];
                FieldTile currentTile = track[i];
                FieldTile nextTile = track[i + 1];

                // Fixes random slopes placed in between two flat straights
                if ((previousTile.tileType == FieldTileType.Straight && nextTile.tileType == FieldTileType.Straight) && (previousTile.position.z == nextTile.position.z) && (previousTile.rotation == nextTile.rotation))
                {
                    if (currentTile.tileType != FieldTileType.Straight)
                    {
                        currentTile.position.z = previousTile.position.z;
                        currentTile.tileType = FieldTileType.Straight;
                    }
                }
            }
        }

        // Removes any deco modifers directly below any track
        private void CleanDeco()
        {
            // Iterate over each track
            foreach (FieldTile trackTile in track)
            {
                int terrainIndex = terrain.FindIndex(t => t.position.x == trackTile.position.x && t.position.y == trackTile.position.y);
                
                if (terrainIndex != -1)
                {
                    terrain[terrainIndex].modifiers.Clear();
                }
            }
        }
        
        // Generates a river on the terrain
        private void GenerateRiver()
        {
            // Create a random for generation
            System.Random rand = new System.Random(seed + level);
            
            // Get a bunch of obstacles
            List<Vector3Int> obstacles = CreateRandomObstacles();
            
            // Get a random start and end that are roughly perpendicular to the start and end
            Vector3Int towardsEnd = end - start;
            // Determine which distance is larger, X or Y
            bool perpendicularOnY = (int)Mathf.Max((int)Mathf.Abs(towardsEnd.x), (int)Mathf.Abs(towardsEnd.y)) != (int)Mathf.Abs(towardsEnd.y);
            
            // Get the start and end points
            Vector3Int startRiver = Vector3Int.zero;
            Vector3Int endRiver = Vector3Int.zero;
            
            // Calculate the center of the track
            Vector3Int trackCenter = new Vector3Int((int)Mathf.Floor((start.x + end.x) / 2f), (int)Mathf.Floor((start.y + end.y) / 2f), 0);
            int startDistanceFromCenterOnAxis = 0;

            if (perpendicularOnY)
            {
                // Get points on Y ends
                startRiver = new Vector3Int((int)Mathf.Floor(rand.Next(bounds.xMin, bounds.xMax - 1)), bounds.yMin, 1);
                endRiver = new Vector3Int(0, bounds.yMax - 1, 1);
                
                // Determine how far away the start is from the center
                startDistanceFromCenterOnAxis = startRiver.x - trackCenter.x;
                
                // Mirror the end to the other side
                endRiver.x = trackCenter.x - startDistanceFromCenterOnAxis;
            }
            else
            {
                // Get points on X ends
                startRiver = new Vector3Int(bounds.xMin, (int)Mathf.Floor(rand.Next(bounds.yMin, bounds.yMax - 1)), 1);
                endRiver = new Vector3Int(bounds.xMax - 1, 0, 1);
                
                // Determine how far away the start is from the center
                startDistanceFromCenterOnAxis = startRiver.y - trackCenter.y;
                
                // Mirror the end to the other side
                endRiver.y = trackCenter.y - startDistanceFromCenterOnAxis;
            }

            startRiver.z = 0;
            endRiver.z = 0;
            
            // Get 3 points between the start and end
            List<Vector3Int> subGoals = new List<Vector3Int>();
            int subGoalCount = 3;

            for (int i = 0; i < subGoalCount; i++)
            {
                Vector3 subGoal = Vector3.Lerp(startRiver, endRiver, ((float)i + 1f) / ((float)subGoalCount + 1f));
                Vector3Int subGoalSnapped = new Vector3Int((int)Mathf.Floor(subGoal.x), (int)Mathf.Floor(subGoal.y), 0);

                if (!subGoals.Contains(subGoalSnapped))
                {
                    subGoals.Add(subGoalSnapped);
                }
            }
            
            // Find the closest obstacle to each point
            List<Vector3Int> subGoalsSelected = new List<Vector3Int>();
            foreach (Vector3Int subGoal in subGoals)
            {
                Vector3Int subGoalSelected = GetClosestPointToPoint(subGoal, obstacles);
                
                if (!subGoalsSelected.Contains(subGoalSelected))
                {
                    subGoalsSelected.Add(subGoalSelected);
                }
            }
            
            // Add the end river to the subgoals
            if (!subGoalsSelected.Contains(endRiver))
            {
                subGoalsSelected.Add(endRiver);
            }
            
            // Create a series of sub-paths with the sub-goals
            // For each segment of the path, find the subpath
            List<Vector3Int> riverPath = new List<Vector3Int>();
            Vector3Int currentStart = startRiver;
            for (int i = 0; i < subGoalsSelected.Count; i++)
            {
                // Get the current end
                Vector3Int currentEnd = subGoalsSelected[i];

                // Get the current path from current start to current end
                List<Vector3Int> currentPath = FindPath(currentStart, currentEnd, obstacles);
                
                // Store the new start
                currentStart = currentPath[currentPath.Count - 1];
                
                // Remove the end from the current path
                currentPath.RemoveRange(currentPath.Count - 1, 1);
                
                // Add the current path to the main path
                riverPath.AddRange(currentPath);
            }
            
            // For each river path position, find the terrain tile below
            foreach (Vector3Int riverPosition in riverPath)
            {
                // Get the index for the terrain below
                int terrainIndex = terrain.FindIndex(t => t.position.x == riverPosition.x && t.position.y == riverPosition.y);

                // If the terrain was found
                if (terrainIndex != -1)
                {
                    // Clear all other modifiers from the terrain tile
                    terrain[terrainIndex]
                    
                    // Add the water modifier to the terrain tile
                    
                }
                
            }

            foreach (Vector3Int riverPosition in riverPath)
            {
                river.Add(new FieldTile(riverPosition, FieldTileType.None, 0));
            }
            
            foreach (Vector3Int obstacle in obstacles)
            {
                river.Add(new FieldTile(obstacle, FieldTileType.Flat, 0));
            }
            
            river.Add(new FieldTile(trackCenter, FieldTileType.Elevated, 0));
        }
        
        // Returns the closest point from a list to a given point 
        private Vector3Int GetClosestPointToPoint(Vector3Int point, List<Vector3Int> searchPoints)
        {
            Vector3Int returnPoint = searchPoints[0];
            float distanceToPoint = Vector3.Distance(point, returnPoint);
            
            // Iterate over each point in the list and return the closest point
            foreach (Vector3Int searchPoint in searchPoints)
            {
                float searchPointDistance = Vector3.Distance(point, searchPoint);
                if (distanceToPoint > searchPointDistance)
                {
                    distanceToPoint = searchPointDistance;
                    returnPoint = searchPoint;
                }

                // If the point is identical to the reference point, just end the method early
                if (searchPoint == point)
                {
                    return searchPoint;
                }
            }

            return returnPoint;
        }
        
        // Clears any flat terrain under the playfield hole
        private void ClearUnderHole()
        {
            int terrainIndex = terrain.FindIndex(t => t.position == end);   
            if (terrainIndex != -1)
            {
                terrain.Remove(terrain[terrainIndex]);
            }
        }
        
        // Generates the supports for the track
        private void GenerateSupports()
        {
            // Iterate over each track tile
            foreach (FieldTile trackTile in track)
            {
                // Get the distance to the ground
                int distanceToGround = GetGroundDistance(trackTile);
                // Check if the tile is sloping
                bool isSloping = trackTile.tileType == FieldTileType.Slope;
                
                // Get the type of terrain under the track (returns -1 if the tile isn't flat because all other terrain tiles actually exist at distance to ground, not distance to ground - 1)
                int terrainTypeBelow = terrain.FindIndex(t => t.position == (trackTile.position - new Vector3Int(0, 0, distanceToGround - 1)));

                if (distanceToGround == 1 && terrainTypeBelow != -1 && trackTile.tileType != FieldTileType.Slope)
                {
                    // The terrain is touching the track, do nothing
                    continue;
                }
                else
                {
                    // Create the list of supports for this tile
                    List<FieldTile> supports = new List<FieldTile>();
                    
                    // Create a base tile for the supports
                    FieldTile supportBase = new FieldTile(trackTile.position, isSloping ? FieldTileType.Slope : FieldTileType.Flat, trackTile.rotation);
                    supportBase.AddModifier(TileModifier.Support);
                    supportBase.AddModifier(TileModifier.Base);
                    
                    // Add the base support to the list of supports
                    supports.Add(supportBase);
                    
                    // Add the special sloping support if the tile is sloping
                    if (isSloping)
                    {
                        FieldTile supportSlope = new FieldTile(trackTile.position, FieldTileType.Slope, trackTile.rotation);
                        supportSlope.AddModifier(TileModifier.Support);
                        
                        // Add the sloping support to the list of supports
                        supports.Add(supportSlope);
                    }
                    
                    // If the terrain tile is flat, subtract one from the distance to ground
                    if (terrainTypeBelow != -1)
                    {
                        distanceToGround--;
                    }

                    // Iterate down towards the terrain, adding a support for each cell
                    for (int i = 0; i < distanceToGround; i++)
                    {
                        // Get the support position
                        Vector3Int supportPosition = trackTile.position - new Vector3Int(0, 0, i);
                        
                        // Create the support tile
                        FieldTile supportTile = new FieldTile(supportPosition, FieldTileType.Flat, trackTile.rotation);
                        supportTile.AddModifier(TileModifier.Support);
                        
                        // Add the support tile to the list of supports
                        supports.Add(supportTile);
                    }
                    
                    // Add the list of supports to the main support list
                    support.AddRange(supports);
                }
            }
        }
        
        // Returns the distance between a tile and the terrain underneath
        private int GetGroundDistance(FieldTile tile)
        {
            // Get the height of the terrain
            Vector3Int flattenedTilePosition = tile.position;
            flattenedTilePosition.z = 99;
            
            int terrainHeight = GetTerrainZ(flattenedTilePosition);
            if (terrainHeight == 99)
            {
                // The track is off the map, set the distance to the bounds
                return bounds.size.z;
            }
            else
            {
                return tile.position.z - terrainHeight;
            }
        }
        
    }
}
