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
        public List<Mesh> meshes;
        public Material material;
        public List<Color> colors;
    }
}