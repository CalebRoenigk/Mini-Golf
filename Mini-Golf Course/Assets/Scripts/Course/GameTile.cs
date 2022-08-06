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
        [SerializeField] private Renderer renderer;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshCollider meshCollider;

        // Set the tile up
        public void SetTile(FieldTile tile, Mesh tileMesh, Material material)
        {
            fieldTile = tile;
            meshFilter.mesh = tileMesh;
            meshCollider.sharedMesh = tileMesh;
            renderer.material = material;
            transform.eulerAngles = new Vector3(-90f, fieldTile.rotation, 0f);
        }
        
        // Set the tile up with a secondary material
        public void SetTile(FieldTile tile, Mesh tileMesh, Material material, Material secondaryMaterial)
        {
            fieldTile = tile;
            meshFilter.mesh = tileMesh;
            meshCollider.sharedMesh = tileMesh;
            Material[] materials = new Material[2];
            materials[0] = material;
            materials[1] = secondaryMaterial;
            renderer.materials = materials;
            transform.eulerAngles = new Vector3(-90f, fieldTile.rotation, 0f);
        }
        
        // Sets the speed of the material at index passed
        public void SetMaterialVector(int materialIndex, string propertyName, Vector4 vector)
        {
            renderer.materials[materialIndex].SetVector(propertyName, vector);
        }
    }
}
