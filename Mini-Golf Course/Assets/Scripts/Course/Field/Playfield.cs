using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Course;

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
        public Dictionary<Vector3Int, TerrainType> terrain = new Dictionary<Vector3Int, TerrainType>();

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
            // Then cast the field down towards the terrain
            // Then calculate the tiles
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

            // Create a base terrain at the bottom of the terrain height
            int zMin = bounds.zMin - fieldSettings.terrainHeight;
            List<Vector3Int> terrainFlags = new List<Vector3Int>();
            for (int z = zMin; z < 0; z++)
            {
                float terrainThreshold = ((float)(z - zMin)) / Mathf.Abs(zMin - 1);

                for (int x = bounds.xMin - fieldSettings.terrainMargin; x < bounds.xMax + 1 + fieldSettings.terrainMargin; x++)
                {
                    for (int y = bounds.yMin - fieldSettings.terrainMargin; y < bounds.yMax + 1 + fieldSettings.terrainMargin; y++)
                    {
                        float terrainSample = Mathf.PerlinNoise((x * fieldSettings.terrainScale) + baseNoiseOffset,(y * fieldSettings.terrainScale) + baseNoiseOffset);
                        if (z == zMin || terrainSample >= terrainThreshold)
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
            foreach (Vector3Int terrainPosition in terrainFlags)
            {
                bool[,,] neighbors = GetTerrainNeighbors(terrainFlags, terrainPosition);
                
                // If there is not a tile above, this terrain exists
                if (!neighbors[1, 1, 2])
                {
                    terrain.Add(terrainPosition, TerrainType.Flat);
                }
            }
        }
        
        // Returns a cube array of direct neighbors for the given terrain tile and flags list
        private bool[,,] GetTerrainNeighbors(List<Vector3Int> terrainFlags, Vector3Int terrainPosition)
        {
            bool[,,] neighbors = new bool[3, 3, 3];

            // Check all neighboring positions for terrain
            for (int z = 0; z < 3; z++)
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        Vector3Int offsetPosition = new Vector3Int(-1 + x, -1 + y, -1 + z);
                        Vector3Int neighborPosition = terrainPosition + offsetPosition;
                        
                        // First check if the neighbor position is out of bounds
                        if (!bounds.Contains(neighborPosition))
                        {
                            neighbors[x, y, z] = false;
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

        // // OLD
        // // Generate the deco locations
        // private void GenerateDeco(int seed)
        // {
        //     // Create a random for generation
        //     System.Random rand = new System.Random(seed);
        //     
        //     // Get a random set of course points
        //     List<Vector3Int> randomPoints = new List<Vector3Int>();
        //     
        //     foreach (FieldTile tile in tiles)
        //     {
        //         if (tile.IsModable())
        //         {
        //             if ((float)rand.NextDouble() > 1f - modifierSettings.decoChance)
        //             {
        //                 Vector3Int randomPoint = tile.tilePosition;
        //             
        //                 // Get a random direction
        //                 List<Vector3Int> directions = new List<Vector3Int>() { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        //                 Vector3Int direction = directions[rand.Next(0, directions.Count - 1)];
        //
        //                 randomPoint = direction + randomPoint;
        //             
        //                 if (!field.Contains(randomPoint) && !randomPoints.Contains(randomPoint))
        //                 {
        //                     if (!GetNeighborDirections(randomPoint).Contains(TileDirection.Empty))
        //                     {
        //                         randomPoints.Add(randomPoint);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //
        //     deco = randomPoints;
        // }
        //
        // // Generates the tiles for the playfield
        // public void GenerateTiles(int seed)
        // {
        //     // Create a random for generation
        //     System.Random rand = new System.Random(seed);
        //
        //     // Create all the field tiles
        //     List<Vector3Int> uncreatedTiles = new List<Vector3Int>();
        //     uncreatedTiles.AddRange(field);
        //     uncreatedTiles.Reverse();
        //     List<Vector3Int> createdTiles = new List<Vector3Int>();
        //
        //     // Create the tiles of the course
        //     while (uncreatedTiles.Count > 0)
        //     {
        //         // Get the tile position
        //         Vector3Int tilePosition = uncreatedTiles[0];
        //         
        //         // Create the tile
        //         FieldTile tile = new FieldTile(this, tilePosition, GetTileDirection(tilePosition));
        //         
        //         // Store the tile in the tiles list
        //         tiles.Add(tile);
        //         
        //         // Move the tile position from the uncreated tiles to the created tiles list
        //         uncreatedTiles.Remove(tilePosition);
        //         createdTiles.Add(tilePosition);
        //     }
        //     
        //     // Get the children of each child
        //     foreach (Vector3Int tilePosition in createdTiles)
        //     {
        //         // Collect all the children
        //         List<Vector3Int> childrenPositions = CollectChildren(tilePosition);
        //         
        //         // Create a list of all children field tiles
        //         List<FieldTile> children = new List<FieldTile>();
        //         foreach (Vector3Int childPosition in childrenPositions)
        //         {
        //             if (tiles.FindIndex(t => t.tilePosition == childPosition) != -1)
        //             {
        //                 // The child exists
        //                 FieldTile childTile = tiles.Find(c => c.tilePosition == childPosition);
        //                 children.Add(childTile);
        //             }
        //         }
        //         
        //         // Get the tile
        //         FieldTile tile = tiles.Find(c => c.tilePosition == tilePosition);
        //         
        //         // Add the children to the tile
        //         foreach (FieldTile child in children)
        //         {
        //             tile.Connect(child);
        //         }
        //
        //         // Determine modifiers for the tile
        //         List<TileModifier> modifiers = new List<TileModifier>();
        //         
        //         // If this tile is the last tile, set it to the hole
        //         if (tilePosition == end)
        //         {
        //             modifiers.Add(TileModifier.Hole);
        //         }
        //         
        //         // Only allow landmarks on straight pieces
        //         if (tile.IsStraight())
        //         {
        //             if ((float)rand.NextDouble() > 1f - modifierSettings.landmarkChance)
        //             { 
        //                 // Landmark
        //                 List<TileModifier> landmarks = new List<TileModifier>() { TileModifier.Pillars, TileModifier.Windmill, TileModifier.Hill };
        //                 modifiers.Add(landmarks[rand.Next(0, landmarks.Count - 1)]);
        //                 // if ((float)rand.NextDouble() > 1f - modifierSettings.archChance)
        //                 // {
        //                 //     // Arch
        //                 //     modifiers.Add(TileModifier.Arch);
        //                 // }
        //                 // else if((float)rand.NextDouble() > 1f - modifierSettings.windmillChance)
        //                 // {
        //                 //     // Windmill
        //                 //     modifiers.Add(TileModifier.Windmill);
        //                 // }
        //                 // else if((float)rand.NextDouble() > 1f - modifierSettings.hillChance)
        //                 // {
        //                 //     // Hill
        //                 //     modifiers.Add(TileModifier.Hill);
        //                 // }
        //             }
        //         }
        //         
        //         // Only allow more modifiers on tiles that dont have modifiers already
        //         if (modifiers.Count == 0)
        //         {
        //             if (tile.IsModable())
        //             {
        //                 if ((float)rand.NextDouble() > 1f - modifierSettings.obstacleChance)
        //                 {
        //                     // Obstacle
        //                     if ((float)rand.NextDouble() > 1f - modifierSettings.rockChance)
        //                     {
        //                         // Rock - can be combined with water, grass, and sand
        //                         modifiers.Add(TileModifier.Rock);
        //                     }
        //                     if ((float)rand.NextDouble() > 1f - modifierSettings.waterChance)
        //                     {
        //                         // Water - can be combined with rock
        //                         if (!modifiers.Contains(TileModifier.Grass) && !modifiers.Contains(TileModifier.Sand))
        //                         {
        //                             modifiers.Add(TileModifier.Water);
        //                         }
        //                     }
        //                     if ((float)rand.NextDouble() > 1f - modifierSettings.grassChance)
        //                     {
        //                         // Grass - can be combined with rock
        //                         if (!modifiers.Contains(TileModifier.Water) && !modifiers.Contains(TileModifier.Sand))
        //                         {
        //                             modifiers.Add(TileModifier.Grass);
        //                         }
        //                     }
        //                     if ((float)rand.NextDouble() > 1f - modifierSettings.sandChance)
        //                     {
        //                         // Sand - can be combined with rock
        //                         if (!modifiers.Contains(TileModifier.Water) && !modifiers.Contains(TileModifier.Grass))
        //                         {
        //                             modifiers.Add(TileModifier.Sand);
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //
        //         // Add the modifiers to the tile
        //         foreach (TileModifier modifier in modifiers)
        //         {
        //             tile.AddModifier(modifier);
        //         }
        //     }
        // }
        //
        // // Returns a tile direction given the tile position in the field
        // private TileDirection GetTileDirection(Vector3Int tilePosition)
        // {
        //     // Create a list of matches
        //     // Depth
        //     bool forwardMatch = FieldPositionExists(tilePosition + Vector3Int.forward);
        //     bool backMatch = FieldPositionExists(tilePosition + Vector3Int.back);
        //     // Horizontal
        //     bool leftMatch = FieldPositionExists(tilePosition + Vector3Int.left);
        //     bool rightMatch = FieldPositionExists(tilePosition + Vector3Int.right);
        //     // Vertical
        //     bool upMatch = FieldPositionExists(tilePosition + Vector3Int.up);
        //     bool downMatch = FieldPositionExists(tilePosition + Vector3Int.down);
        //     
        //     List<bool> depthMatches = new List<bool>() {forwardMatch, backMatch}; // Forward, Back
        //     List<bool> horizontalMatches = new List<bool>() {leftMatch, rightMatch}; // Left, Right
        //     List<bool> verticalMatches = new List<bool>() {upMatch, downMatch}; // Up, Down
        //     List<bool> planeMatches = new List<bool>() {leftMatch, rightMatch, upMatch, downMatch}; // All 2D matches (Left, Right, Up, Down)
        //
        //     // Determine the tile from the number of matches
        //     // If there is a match below, always return empty
        //     if (backMatch)
        //     {
        //         return TileDirection.Empty;
        //     }
        //     
        //     int planeMatchCount = planeMatches.Where(m => m).Count();
        //     switch (planeMatchCount)
        //     {
        //         case 0:
        //             // Solo
        //             return TileDirection.Solo;
        //         case 1:
        //             // Endpoint or Slope
        //             // Determine the direction
        //             if (leftMatch || rightMatch)
        //             {
        //                 // Horizontal
        //                 if (leftMatch)
        //                 {
        //                     // Left Match
        //                     if (forwardMatch)
        //                     {
        //                         // Slope Left
        //                         return TileDirection.HorizontalSlopeLeft;
        //                     }
        //                     else
        //                     {
        //                         // Left
        //                         return TileDirection.HorizontalLeft;
        //                     }
        //                 }
        //                 else
        //                 {
        //                     // Right Match
        //                     if (forwardMatch)
        //                     {
        //                         // Slope Right
        //                         return TileDirection.HorizontalSlopeRight;
        //                     }
        //                     else
        //                     {
        //                         // Right
        //                         return TileDirection.HorizontalRight;
        //                     }
        //                 }
        //             }
        //             else
        //             {
        //                 // Vertical
        //                 if (upMatch)
        //                 {
        //                     // Up Match
        //                     if (forwardMatch)
        //                     {
        //                         // Slope Up
        //                         return TileDirection.VerticalSlopeUp;
        //                     }
        //                     else
        //                     {
        //                         // Up
        //                         return TileDirection.VerticalUp;
        //                     }
        //                 }
        //                 else
        //                 {
        //                     // Down Match
        //                     if (forwardMatch)
        //                     {
        //                         // Slope Down
        //                         return TileDirection.VerticalSlopeDown;
        //                     }
        //                     else
        //                     {
        //                         // Down
        //                         return TileDirection.VerticalDown;
        //                     }
        //                 }
        //             }
        //         case 2:
        //             // Straight or Corner
        //             // Straight
        //             if (horizontalMatches.Where(m => m).Count() == 2 || verticalMatches.Where(m => m).Count() == 2)
        //             {
        //                 if (horizontalMatches.Where(m => m).Count() == 2)
        //                 {
        //                     // Horizontal Straight
        //                     return TileDirection.Horizontal;
        //                 }
        //                 else
        //                 {
        //                     // Vertical Straight
        //                     return TileDirection.Vertical;
        //                 }
        //             }
        //             else
        //             {
        //                 // Corner
        //                 if (upMatch && leftMatch)
        //                 {
        //                     return TileDirection.CornerUpLeft;
        //                 }
        //                 else if (upMatch && rightMatch)
        //                 {
        //                     return TileDirection.CornerUpRight;
        //                 }
        //                 else if (downMatch && leftMatch)
        //                 {
        //                     return TileDirection.CornerDownLeft;
        //                 }
        //                 else if (downMatch && rightMatch)
        //                 {
        //                     return TileDirection.CornerDownRight;
        //                 }
        //             }
        //             break;
        //         case 3:
        //             // Tee
        //             if (leftMatch && upMatch && rightMatch)
        //             {
        //                 // Left Up Right
        //                 return TileDirection.TeeDown;
        //             }
        //             else if (upMatch && rightMatch && downMatch)
        //             {
        //                 // Up Right Down
        //                 return TileDirection.TeeLeft;
        //             }
        //             else if (rightMatch && downMatch && leftMatch)
        //             {
        //                 // Right Down Left
        //                 return TileDirection.TeeUp;
        //             }
        //             else if(downMatch && leftMatch && upMatch)
        //             {
        //                 // Down Left Up
        //                 return TileDirection.TeeRight;
        //             }
        //             break;
        //         case 4:
        //             // Four Way
        //             return TileDirection.FourWay;
        //         default:
        //             return TileDirection.Empty;
        //     }
        //
        //     return TileDirection.Empty;
        // }
        //
        // // Returns a bool for a field position existing in this field
        // private bool FieldPositionExists(Vector3Int fieldPosition)
        // {
        //     return field.Contains(fieldPosition);
        // }
        //
        // // Returns a list of adjacent points in the field to the current requested point
        // private List<Vector3Int> CollectChildren(Vector3Int parentLocation)
        // {
        //     // Adjacent locations
        //     List<Vector3Int> adjacentDirections = new List<Vector3Int>() { Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down, Vector3Int.forward, Vector3Int.back };
        //     List<Vector3Int> adjacentPositions = new List<Vector3Int>();
        //     for (int i = 0; i < adjacentDirections.Count; i++)
        //     {
        //         adjacentPositions.Add(adjacentDirections[i] + parentLocation);
        //     }
        //
        //     List<Vector3Int> childPositions = new List<Vector3Int>();
        //     foreach (Vector3Int adjacentPosition in adjacentPositions)
        //     {
        //         if (field.Contains(adjacentPosition))
        //         {
        //             childPositions.Add(adjacentPosition);
        //         }
        //     }
        //
        //     return childPositions;
        // }
        //
        // // Returns a list of tile directions of the neighboring tiles
        // private List<TileDirection> GetNeighborDirections(Vector3Int position)
        // {
        //     List<Vector3Int> directions = new List<Vector3Int>() { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down };
        //     List<TileDirection> tileDirections = new List<TileDirection>();
        //     
        //     // Look at each direction
        //     foreach (Vector3Int neighbor in directions)
        //     {
        //         if (tiles.FindIndex(t => t.tilePosition == neighbor + position) != -1)
        //         {
        //             tileDirections.Add(tiles.Find(t => t.tilePosition == neighbor + position).tileDirection);
        //         }
        //     }
        //
        //     return tileDirections;
        // }
        //
        // // Swaps the z and y in a passed Vector3Int and returns it as a Vector3
        // private Vector3 GridToWorld(Vector3Int position)
        // {
        //     return new Vector3(position.x, position.z * 0.5f, position.y);
        // }
    }
}
