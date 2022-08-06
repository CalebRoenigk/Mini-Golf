using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Course.Field;

namespace Course
{
    public class GameTile : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private FieldTile fieldTile;
        
        [Header("Runtime")]
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshCollider meshCollider;

        // Set the tile up
        public void SetTile(FieldTile tile, Mesh tileMesh)
        {
            fieldTile = tile;
            meshFilter.mesh = tileMesh;
            meshCollider.sharedMesh = tileMesh;
            transform.eulerAngles = new Vector3(-90f, fieldTile.rotation, 0f);
        }
    }
}
