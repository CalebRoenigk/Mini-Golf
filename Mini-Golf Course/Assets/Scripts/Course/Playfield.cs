using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Course
{
    [System.Serializable]
    public class Playfield
    {
        public Vector3Int start;
        public Vector3Int end;
        public Vector3 spawn;
        public List<Vector3Int> field = new List<Vector3Int>();
        public List<FieldTile> tiles = new List<FieldTile>();
        public List<Vector3Int> terrain = new List<Vector3Int>();
        public List<Vector3Int> deco = new List<Vector3Int>();
        public ModifierSettings modifierSettings;

        public Playfield()
        {
            // Generate Playfield using seed and level
            // Store the seed and level in the playfield
            // Need to create a start and end Vector3Int
            // Spawn can be removed as it is half tile offset of the start
            // Field can be removed, this will be an internal list passed around to get the tiles
            // Terrain will be generated first
            // Then Deco will be generated
            // Then the field will be generated
            // Then cast the field down towards the terrain
            // Then calculate the tiles
            // Store the tiles
            // Remove any deco that is occupied by tiles
        }
        
        public Playfield(List<Vector3Int> field, Vector3Int start, Vector3Int end, List<Vector3Int> terrain, int seed, ModifierSettings modifierSettings)
        {
            this.field = field;
            this.start = start;
            this.spawn = GridToWorld(start) + new Vector3(0f, 0.25f, 0f);
            this.end = end;
            this.terrain = terrain;
            this.modifierSettings = modifierSettings;
            
            GenerateTiles(seed);
            GenerateDeco(seed);
        }
        
        // Generate the deco locations
        private void GenerateDeco(int seed)
        {
            // Create a random for generation
            System.Random rand = new System.Random(seed);
            
            // Get a random set of course points
            List<Vector3Int> randomPoints = new List<Vector3Int>();
            
            foreach (FieldTile tile in tiles)
            {
                if (tile.IsModable())
                {
                    if ((float)rand.NextDouble() > 1f - modifierSettings.decoChance)
                    {
                        Vector3Int randomPoint = tile.tilePosition;
                    
                        // Get a random direction
                        List<Vector3Int> directions = new List<Vector3Int>() { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
                        Vector3Int direction = directions[rand.Next(0, directions.Count - 1)];

                        randomPoint = direction + randomPoint;
                    
                        if (!field.Contains(randomPoint) && !randomPoints.Contains(randomPoint))
                        {
                            if (!GetNeighborDirections(randomPoint).Contains(TileDirection.Empty))
                            {
                                randomPoints.Add(randomPoint);
                            }
                        }
                    }
                }
            }

            deco = randomPoints;
        }
        
        // Generates the tiles for the playfield
        public void GenerateTiles(int seed)
        {
            // Create a random for generation
            System.Random rand = new System.Random(seed);

            // Create all the field tiles
            List<Vector3Int> uncreatedTiles = new List<Vector3Int>();
            uncreatedTiles.AddRange(field);
            uncreatedTiles.Reverse();
            List<Vector3Int> createdTiles = new List<Vector3Int>();

            // Create the tiles of the course
            while (uncreatedTiles.Count > 0)
            {
                // Get the tile position
                Vector3Int tilePosition = uncreatedTiles[0];
                
                // Create the tile
                FieldTile tile = new FieldTile(this, tilePosition, GetTileDirection(tilePosition));
                
                // Store the tile in the tiles list
                tiles.Add(tile);
                
                // Move the tile position from the uncreated tiles to the created tiles list
                uncreatedTiles.Remove(tilePosition);
                createdTiles.Add(tilePosition);
            }
            
            // Get the children of each child
            foreach (Vector3Int tilePosition in createdTiles)
            {
                // Collect all the children
                List<Vector3Int> childrenPositions = CollectChildren(tilePosition);
                
                // Create a list of all children field tiles
                List<FieldTile> children = new List<FieldTile>();
                foreach (Vector3Int childPosition in childrenPositions)
                {
                    if (tiles.FindIndex(t => t.tilePosition == childPosition) != -1)
                    {
                        // The child exists
                        FieldTile childTile = tiles.Find(c => c.tilePosition == childPosition);
                        children.Add(childTile);
                    }
                }
                
                // Get the tile
                FieldTile tile = tiles.Find(c => c.tilePosition == tilePosition);
                
                // Add the children to the tile
                foreach (FieldTile child in children)
                {
                    tile.Connect(child);
                }

                // Determine modifiers for the tile
                List<TileModifier> modifiers = new List<TileModifier>();
                
                // If this tile is the last tile, set it to the hole
                if (tilePosition == end)
                {
                    modifiers.Add(TileModifier.Hole);
                }
                
                // Only allow landmarks on straight pieces
                if (tile.IsStraight())
                {
                    if ((float)rand.NextDouble() > 1f - modifierSettings.landmarkChance)
                    { 
                        // Landmark
                        List<TileModifier> landmarks = new List<TileModifier>() { TileModifier.Pillars, TileModifier.Windmill, TileModifier.Hill };
                        modifiers.Add(landmarks[rand.Next(0, landmarks.Count - 1)]);
                        // if ((float)rand.NextDouble() > 1f - modifierSettings.archChance)
                        // {
                        //     // Arch
                        //     modifiers.Add(TileModifier.Arch);
                        // }
                        // else if((float)rand.NextDouble() > 1f - modifierSettings.windmillChance)
                        // {
                        //     // Windmill
                        //     modifiers.Add(TileModifier.Windmill);
                        // }
                        // else if((float)rand.NextDouble() > 1f - modifierSettings.hillChance)
                        // {
                        //     // Hill
                        //     modifiers.Add(TileModifier.Hill);
                        // }
                    }
                }
                
                // Only allow more modifiers on tiles that dont have modifiers already
                if (modifiers.Count == 0)
                {
                    if (tile.IsModable())
                    {
                        if ((float)rand.NextDouble() > 1f - modifierSettings.obstacleChance)
                        {
                            // Obstacle
                            if ((float)rand.NextDouble() > 1f - modifierSettings.rockChance)
                            {
                                // Rock - can be combined with water, grass, and sand
                                modifiers.Add(TileModifier.Rock);
                            }
                            if ((float)rand.NextDouble() > 1f - modifierSettings.waterChance)
                            {
                                // Water - can be combined with rock
                                if (!modifiers.Contains(TileModifier.Grass) && !modifiers.Contains(TileModifier.Sand))
                                {
                                    modifiers.Add(TileModifier.Water);
                                }
                            }
                            if ((float)rand.NextDouble() > 1f - modifierSettings.grassChance)
                            {
                                // Grass - can be combined with rock
                                if (!modifiers.Contains(TileModifier.Water) && !modifiers.Contains(TileModifier.Sand))
                                {
                                    modifiers.Add(TileModifier.Grass);
                                }
                            }
                            if ((float)rand.NextDouble() > 1f - modifierSettings.sandChance)
                            {
                                // Sand - can be combined with rock
                                if (!modifiers.Contains(TileModifier.Water) && !modifiers.Contains(TileModifier.Grass))
                                {
                                    modifiers.Add(TileModifier.Sand);
                                }
                            }
                        }
                    }
                }

                // Add the modifiers to the tile
                foreach (TileModifier modifier in modifiers)
                {
                    tile.AddModifier(modifier);
                }
            }
        }

        // Returns a tile direction given the tile position in the field
        private TileDirection GetTileDirection(Vector3Int tilePosition)
        {
            // Create a list of matches
            // Depth
            bool forwardMatch = FieldPositionExists(tilePosition + Vector3Int.forward);
            bool backMatch = FieldPositionExists(tilePosition + Vector3Int.back);
            // Horizontal
            bool leftMatch = FieldPositionExists(tilePosition + Vector3Int.left);
            bool rightMatch = FieldPositionExists(tilePosition + Vector3Int.right);
            // Vertical
            bool upMatch = FieldPositionExists(tilePosition + Vector3Int.up);
            bool downMatch = FieldPositionExists(tilePosition + Vector3Int.down);
            
            List<bool> depthMatches = new List<bool>() {forwardMatch, backMatch}; // Forward, Back
            List<bool> horizontalMatches = new List<bool>() {leftMatch, rightMatch}; // Left, Right
            List<bool> verticalMatches = new List<bool>() {upMatch, downMatch}; // Up, Down
            List<bool> planeMatches = new List<bool>() {leftMatch, rightMatch, upMatch, downMatch}; // All 2D matches (Left, Right, Up, Down)

            // Determine the tile from the number of matches
            // If there is a match below, always return empty
            if (backMatch)
            {
                return TileDirection.Empty;
            }
            
            int planeMatchCount = planeMatches.Where(m => m).Count();
            switch (planeMatchCount)
            {
                case 0:
                    // Solo
                    return TileDirection.Solo;
                case 1:
                    // Endpoint or Slope
                    // Determine the direction
                    if (leftMatch || rightMatch)
                    {
                        // Horizontal
                        if (leftMatch)
                        {
                            // Left Match
                            if (forwardMatch)
                            {
                                // Slope Left
                                return TileDirection.HorizontalSlopeLeft;
                            }
                            else
                            {
                                // Left
                                return TileDirection.HorizontalLeft;
                            }
                        }
                        else
                        {
                            // Right Match
                            if (forwardMatch)
                            {
                                // Slope Right
                                return TileDirection.HorizontalSlopeRight;
                            }
                            else
                            {
                                // Right
                                return TileDirection.HorizontalRight;
                            }
                        }
                    }
                    else
                    {
                        // Vertical
                        if (upMatch)
                        {
                            // Up Match
                            if (forwardMatch)
                            {
                                // Slope Up
                                return TileDirection.VerticalSlopeUp;
                            }
                            else
                            {
                                // Up
                                return TileDirection.VerticalUp;
                            }
                        }
                        else
                        {
                            // Down Match
                            if (forwardMatch)
                            {
                                // Slope Down
                                return TileDirection.VerticalSlopeDown;
                            }
                            else
                            {
                                // Down
                                return TileDirection.VerticalDown;
                            }
                        }
                    }
                case 2:
                    // Straight or Corner
                    // Straight
                    if (horizontalMatches.Where(m => m).Count() == 2 || verticalMatches.Where(m => m).Count() == 2)
                    {
                        if (horizontalMatches.Where(m => m).Count() == 2)
                        {
                            // Horizontal Straight
                            return TileDirection.Horizontal;
                        }
                        else
                        {
                            // Vertical Straight
                            return TileDirection.Vertical;
                        }
                    }
                    else
                    {
                        // Corner
                        if (upMatch && leftMatch)
                        {
                            return TileDirection.CornerUpLeft;
                        }
                        else if (upMatch && rightMatch)
                        {
                            return TileDirection.CornerUpRight;
                        }
                        else if (downMatch && leftMatch)
                        {
                            return TileDirection.CornerDownLeft;
                        }
                        else if (downMatch && rightMatch)
                        {
                            return TileDirection.CornerDownRight;
                        }
                    }
                    break;
                case 3:
                    // Tee
                    if (leftMatch && upMatch && rightMatch)
                    {
                        // Left Up Right
                        return TileDirection.TeeDown;
                    }
                    else if (upMatch && rightMatch && downMatch)
                    {
                        // Up Right Down
                        return TileDirection.TeeLeft;
                    }
                    else if (rightMatch && downMatch && leftMatch)
                    {
                        // Right Down Left
                        return TileDirection.TeeUp;
                    }
                    else if(downMatch && leftMatch && upMatch)
                    {
                        // Down Left Up
                        return TileDirection.TeeRight;
                    }
                    break;
                case 4:
                    // Four Way
                    return TileDirection.FourWay;
                default:
                    return TileDirection.Empty;
            }

            return TileDirection.Empty;
        }

        // Returns a bool for a field position existing in this field
        private bool FieldPositionExists(Vector3Int fieldPosition)
        {
            return field.Contains(fieldPosition);
        }
        
        // Returns a list of adjacent points in the field to the current requested point
        private List<Vector3Int> CollectChildren(Vector3Int parentLocation)
        {
            // Adjacent locations
            List<Vector3Int> adjacentDirections = new List<Vector3Int>() { Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down, Vector3Int.forward, Vector3Int.back };
            List<Vector3Int> adjacentPositions = new List<Vector3Int>();
            for (int i = 0; i < adjacentDirections.Count; i++)
            {
                adjacentPositions.Add(adjacentDirections[i] + parentLocation);
            }

            List<Vector3Int> childPositions = new List<Vector3Int>();
            foreach (Vector3Int adjacentPosition in adjacentPositions)
            {
                if (field.Contains(adjacentPosition))
                {
                    childPositions.Add(adjacentPosition);
                }
            }

            return childPositions;
        }
        
        // Returns a list of tile directions of the neighboring tiles
        private List<TileDirection> GetNeighborDirections(Vector3Int position)
        {
            List<Vector3Int> directions = new List<Vector3Int>() { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down };
            List<TileDirection> tileDirections = new List<TileDirection>();
            
            // Look at each direction
            foreach (Vector3Int neighbor in directions)
            {
                if (tiles.FindIndex(t => t.tilePosition == neighbor + position) != -1)
                {
                    tileDirections.Add(tiles.Find(t => t.tilePosition == neighbor + position).tileDirection);
                }
            }

            return tileDirections;
        }
        
        // Swaps the z and y in a passed Vector3Int and returns it as a Vector3
        private Vector3 GridToWorld(Vector3Int position)
        {
            return new Vector3(position.x, position.z * 0.5f, position.y);
        }
    }
}
