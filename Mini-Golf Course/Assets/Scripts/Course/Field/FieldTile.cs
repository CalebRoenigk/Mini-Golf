using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Course.Field
{
    public class FieldTile
    {
        public Vector3Int position;
        public FieldTileType tileType;
        public int rotation;
        public List<TileModifier> modifiers = new List<TileModifier>();
        
        public FieldTile()
        {
            
        }

        public FieldTile(Vector3Int position)
        {
            this.position = position;
            this.tileType = FieldTileType.None;
            this.rotation = 0;
        }
        
        public FieldTile(Vector3Int position, FieldTileType tileType)
        {
            this.position = position;
            this.tileType = tileType;
            this.rotation = 0;
        }
        
        public FieldTile(Vector3Int position, FieldTileType tileType, int rotation)
        {
            this.position = position;
            this.tileType = tileType;
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
