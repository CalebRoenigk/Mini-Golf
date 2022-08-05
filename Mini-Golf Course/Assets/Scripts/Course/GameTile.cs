using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Course
{
    public class GameTile : MonoBehaviour
    {
        // [Header("General")]
        // [SerializeField] private List<Mesh> meshes = new List<Mesh>(); // Solo, End, Straight, Slope, Corner, Tee, FourWay
        // [SerializeField] private List<GameObject> props = new List<GameObject>();
        //
        // [Header("Settings")]
        // [SerializeField] private TileDirection tileDirection;
        // [SerializeField] private List<TileModifier> tileModifers = new List<TileModifier>();
        // [SerializeField] private Vector3Int gridPosition;
        // [SerializeField] private MeshFilter meshFilter;
        // [SerializeField] private MeshCollider meshCollider;
        //
        // void Start()
        // {
        //     // meshFilter = gameObject.GetComponent<MeshFilter>();
        //     // meshCollider = gameObject.GetComponent<MeshCollider>();
        // }
        //
        // public void SetTile(FieldTile tile)
        // {
        //     // Store settings data
        //     tileDirection = tile.tileDirection;
        //     tileModifers = tile.tileModifiers;
        //     gridPosition = tile.tilePosition;
        //
        //     // Place the tile at the correct location
        //     transform.position = GridToWorld(tile.tilePosition);
        //     
        //     // Determine the mesh of the tile given the tile direction
        //     Mesh tileMesh = GetTileMesh();
        //     meshFilter.mesh = tileMesh;
        //     meshCollider.sharedMesh = tileMesh;
        //
        //     // Determine the rotation of the tile
        //     Vector3 tileRotation = GetTileRotation();
        //     transform.eulerAngles = tileRotation;
        // }
        //
        // // Swaps the z and y in a passed Vector3Int and returns it as a Vector3
        // private Vector3 GridToWorld(Vector3Int position)
        // {
        //     return new Vector3(position.x, position.z * 0.5f, position.y);
        // }
        //
        // // Returns the tile mesh based on tile direction and modifiers
        // private Mesh GetTileMesh()
        // {
        //     switch (tileDirection)
        //     {
        //         case TileDirection.Empty:
        //         default:
        //             // Empty
        //             return new Mesh();
        //         case TileDirection.Solo:
        //             // Solo
        //             return meshes[0];
        //         case TileDirection.VerticalUp:
        //         case TileDirection.VerticalDown:
        //         case TileDirection.HorizontalLeft:
        //         case TileDirection.HorizontalRight:
        //             if (tileModifers.Contains(TileModifier.Hole))
        //             {
        //                 // Hole
        //                 return meshes[2];
        //             }
        //             // Endpoint
        //             return meshes[1];
        //         case TileDirection.Horizontal:
        //         case TileDirection.Vertical:
        //             if (tileModifers.Contains(TileModifier.Hill))
        //             {
        //                 // Hill
        //                 return meshes[4];
        //             }
        //             if (tileModifers.Contains(TileModifier.Water))
        //             {
        //                 // Water
        //                 return meshes[5];
        //             }
        //             if (tileModifers.Contains(TileModifier.Pillars))
        //             {
        //                 // Pillars
        //                 ApplyPropSettings(0);
        //                 return meshes[3];
        //             }
        //             // Straight
        //             return meshes[3];
        //         case TileDirection.VerticalSlopeUp:
        //         case TileDirection.VerticalSlopeDown:
        //         case TileDirection.HorizontalSlopeLeft:
        //         case TileDirection.HorizontalSlopeRight:
        //             // Slope
        //             return meshes[6];
        //         case TileDirection.CornerUpLeft:
        //         case TileDirection.CornerUpRight:
        //         case TileDirection.CornerDownLeft:
        //         case TileDirection.CornerDownRight:
        //             if (tileModifers.Contains(TileModifier.Water))
        //             {
        //                 // Water
        //                 return meshes[8];
        //             }
        //             //Corner
        //             return meshes[7];
        //         case TileDirection.TeeDown:
        //         case TileDirection.TeeLeft:
        //         case TileDirection.TeeUp:
        //         case TileDirection.TeeRight:
        //             // Tee
        //             return meshes[9];
        //         case TileDirection.FourWay:
        //             // Four Way
        //             return meshes[10];
        //     }
        // }
        //
        // // Returns the rotation of the tile based on direction
        // private Vector3 GetTileRotation()
        // {
        //     // Default Rotation
        //     Vector3 tileRotation = new Vector3(-90f, 0f, 0f);
        //
        //     // Rotations per direction
        //     switch (tileDirection)
        //     {
        //         case TileDirection.Empty:
        //         case TileDirection.Solo:
        //         case TileDirection.FourWay:
        //         case TileDirection.VerticalDown:
        //         case TileDirection.VerticalSlopeUp:
        //         case TileDirection.Vertical:
        //         case TileDirection.CornerDownLeft:
        //         case TileDirection.TeeDown:
        //         default:
        //             tileRotation.z = 0f;
        //             break;
        //         case TileDirection.Horizontal:
        //         case TileDirection.HorizontalLeft:
        //         case TileDirection.HorizontalSlopeRight:
        //         case TileDirection.CornerUpLeft:
        //         case TileDirection.TeeLeft:
        //             tileRotation.z = 90f;
        //             break;
        //         case TileDirection.VerticalUp:
        //         case TileDirection.VerticalSlopeDown:
        //         case TileDirection.CornerUpRight:
        //         case TileDirection.TeeUp:
        //             tileRotation.z = 180f;
        //             break;
        //         case TileDirection.CornerDownRight:
        //         case TileDirection.HorizontalRight:
        //         case TileDirection.HorizontalSlopeLeft:
        //         case TileDirection.TeeRight:
        //             tileRotation.z = -90f;
        //             break;
        //     }
        //     
        //     return tileRotation;
        // }
        //
        // // Spawns a prop
        // private void ApplyPropSettings(int propIndex)
        // {
        //     Quaternion quaternion = new Quaternion();
        //     quaternion.eulerAngles = new Vector3(0f, 0f, 0f);
        //     
        //     GameObject prop = Instantiate(props[propIndex], Vector3.zero, quaternion, transform);
        //     prop.transform.localPosition = Vector3.zero;
        // }
    }
}
