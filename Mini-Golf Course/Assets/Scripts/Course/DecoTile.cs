using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Course.Field;

namespace Course
{
    public class DecoTile : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private TileModifier tileModifier;

        [Header("Runtime")]
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Renderer renderer;

        // Set the tile up
        public void SetTile(TileModifier modifier, Color color, Mesh tileMesh, Material material)
        {
            tileModifier = modifier;
            meshFilter.mesh = tileMesh;

            renderer.material = material;
            renderer.material.SetColor("_BaseColor", color);
            transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
        }
    }
}