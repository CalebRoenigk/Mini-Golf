using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Course.Field
{
    public class TerrainTile
    {
        public Vector3Int position;
        public FieldTileType terrainType;
        public int rotation;
        
        public TerrainTile()
        {
            
        }

        public TerrainTile(Vector3Int position)
        {
            this.position = position;
            this.terrainType = FieldTileType.None;
            this.rotation = 0;
        }
        
        public TerrainTile(Vector3Int position, FieldTileType terrainType)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.rotation = 0;
        }
        
        public TerrainTile(Vector3Int position, FieldTileType terrainType, int rotation)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.rotation = rotation;
        }
    }
}
