using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Course.Field;

namespace Course
{
    public class DecoTile : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private FieldTile fieldTile;
        
        [Header("Runtime")]
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Renderer renderer;

        // Set the tile up
        public void SetTile(FieldTile tile, Mesh tileMesh, Material material)
        {
            fieldTile = tile;
            meshFilter.mesh = tileMesh;
            renderer.material = material;
            transform.eulerAngles = new Vector3(0f, fieldTile.rotation, 0f);
        }
    }
}