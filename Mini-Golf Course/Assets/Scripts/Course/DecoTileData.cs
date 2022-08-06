using System.Collections;
using System.Collections.Generic;
using Course.Field;
using UnityEngine;

namespace Course
{
    [System.Serializable]
    public struct DecoTileData
    {
        public TileModifier tileModifer;
        public Mesh mesh;
        public Material material;
    }
}