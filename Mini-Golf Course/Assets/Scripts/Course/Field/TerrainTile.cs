using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Course.Field
{
    public class TerrainTile
    {
        public Vector3Int position;
        public TerrainType terrainType;
        public int rotation;
        
        public TerrainTile()
        {
            
        }

        public TerrainTile(Vector3Int position)
        {
            this.position = position;
            this.terrainType = TerrainType.None;
            this.rotation = 0;
        }
        
        public TerrainTile(Vector3Int position, TerrainType terrainType)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.rotation = 0;
        }
        
        public TerrainTile(Vector3Int position, TerrainType terrainType, int rotation)
        {
            this.position = position;
            this.terrainType = terrainType;
            this.rotation = rotation;
        }
    }
}
