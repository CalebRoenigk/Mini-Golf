using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Course.Field
{
    public class FieldTile
    {
        public Vector3Int position;
        public FieldTileType terrainType;
        public int rotation;
        public List<TileModifier> modifiers = new List<TileModifier>();
        
        public FieldTile()
        {
            
        }

        public FieldTile(Vector3Int position)
        {
            this.position = position;
            this.terrainType = FieldTileType.None;
            this.rotation = 0;
        }
        
        public FieldTile(Vector3Int position, FieldTileType terrainType)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.rotation = 0;
        }
        
        public FieldTile(Vector3Int position, FieldTileType terrainType, int rotation)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.rotation = rotation;
        }
        
        // Adds a modifier to the tile
        public void AddModifier(TileModifier modifier)
        {
            if (!modifiers.Contains(modifier))
            {
                modifiers.Add(modifier);
            }
        }
        
        // Adds a list of modifiers to the tile
        public void AddModifiers(List<TileModifier> modifierList)
        {
            foreach (TileModifier modifier in modifierList)
            {
                AddModifier(modifier);
            }
        }
    }
}
