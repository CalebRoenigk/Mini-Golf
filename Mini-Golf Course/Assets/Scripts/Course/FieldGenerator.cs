using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
// using Dice;
using Course.Field;

namespace Course
{
    public class FieldGenerator : MonoBehaviour
    {
        [Header("Level")]
        [Range(0,100)]
        public int level = 10;
        private int lastLevel = 10;
        [Range(0,10000)]
        public int seed = 100;
        private int lastSeed = 100;

        [Header("Modifiers")]
        [Range(0f,1f)]
        public float obstacleChance;
        [Range(0f,1f)]
        public float landmarkChance;
        [Range(0f,1f)]
        public float decoChance;
        private float lastDecoChance = 0f;

        [Header("Playfield")]
        [SerializeField] private Playfield playfield;
        [SerializeField] private Transform terrainParent;
        [SerializeField] private Transform trackParent;
        
        [Header("Data")]
        [SerializeField] private List<FieldTileData> terrainTileData = new List<FieldTileData>();
        [SerializeField] private List<FieldTileData> trackTileData = new List<FieldTileData>();
        [SerializeField] private List<DecoTileData> decoTileData = new List<DecoTileData>();

        [Header("Game")]
        [SerializeField] private GameObject fieldTilePrefab;
        [SerializeField] private GameObject decoTilePrefab;
        private List<GameObject> gameTiles = new List<GameObject>();
        // [SerializeField] private Ball ball;

        [Header("Gizmos")]
        [SerializeField] private bool showDebug = false;

        void Start()
        {
            // CreateLevel();
        }

        void Update()
        {
            if (lastLevel != level || lastSeed != seed || lastDecoChance != decoChance)
            {
                DestroyLevel();
                CreateLevel();
            }

            lastLevel = level;
            lastSeed = seed;
            lastDecoChance = decoChance;
        }
        
        private void OnDrawGizmos()
        {
            if (showDebug)
            {
                if (playfield != null)
                {
                    // Draw bounds
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(GridToWorld(new Vector3Int((int)Mathf.Floor(playfield.bounds.center.x), (int)Mathf.Floor(playfield.bounds.center.y), (int)Mathf.Floor(playfield.bounds.center.z))), GridToWorld(playfield.bounds.size));
                    
                    // Draw terrain
                    foreach (FieldTile terrain in playfield.terrain)
                    {
                        Gizmos.color = Color.magenta;
                        switch (terrain.tileType)
                        {
                            case FieldTileType.None:
                                Gizmos.color = Color.magenta;
                                Gizmos.DrawWireCube(GridToWorld(terrain.position), new Vector3(1f, 0.5f, 1f));
                                break;
                            default:
                                Quaternion quaternion = new Quaternion();
                                quaternion.eulerAngles = new Vector3(-90f, terrain.rotation, 0f);
                                Mesh terrainMesh = terrainTileData.Find(t => t.fieldTileType == terrain.tileType).mesh;
                                Gizmos.color = Color.magenta;
                                Gizmos.DrawMesh(terrainMesh, GridToWorld(terrain.position), quaternion, Vector3.one);
                                Gizmos.color = Color.red;
                                Gizmos.DrawWireMesh(terrainMesh, GridToWorld(terrain.position), quaternion, Vector3.one);
                                if (terrain.modifiers.Count > 0)
                                {
                                    Gizmos.color = Color.black;
                                    Gizmos.DrawSphere(GridToWorld(terrain.position), 0.25f);
                                }
                                break;
                        }
                    }
                    
                    // Draw Track
                    foreach (FieldTile track in playfield.track)
                    {
                        switch (track.tileType)
                        {
                            case FieldTileType.None:
                                Gizmos.color = Color.blue;
                                if (track.position.z == 1)
                                {
                                    Gizmos.color = Color.green;
                                }
                                // Gizmos.DrawCube(GridToWorld(track.position), new Vector3(1f, 0.5f, 1f));
                                Gizmos.color = Color.cyan;
                                if (track.position.z == 1)
                                {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawWireCube(GridToWorld(track.position), new Vector3(1f, 0.5f, 1f));
                                break;
                            default:
                                Quaternion quaternion = new Quaternion();
                                quaternion.eulerAngles = new Vector3(-90f, track.rotation, 0f);
                                Mesh trackMesh = trackTileData.Find(t => t.fieldTileType == track.tileType).mesh;
                                Gizmos.color = Color.blue;
                                if (track.tileType == FieldTileType.End)
                                {
                                    Gizmos.color = Color.green;
                                }
                                Gizmos.DrawMesh(trackMesh, GridToWorld(track.position), quaternion, Vector3.one);
                                Gizmos.color = Color.cyan;
                                if (track.tileType == FieldTileType.End)
                                {
                                    Gizmos.color = Color.yellow;
                                }
                                Gizmos.DrawWireMesh(trackMesh, GridToWorld(track.position), quaternion, Vector3.one);
                                break;
                        }
                    }
                }
            }
        }

        // Generates and Instantiates the field
        private void CreateLevel()
        {
            playfield = GeneratePlayfield();
            InstantiatePlayfield();
            // InstantiateDeco();

            // ball.transform.position = playfield.spawn;
        }
        
        // Creates the objects for the game world
        private void InstantiatePlayfield()
        {
            // Create the terrain
            foreach (FieldTile terrainTile in playfield.terrain)
            {
                GameTile terrainObject = Instantiate(fieldTilePrefab, GridToWorld(terrainTile.position), Quaternion.identity, terrainParent).GetComponent<GameTile>();
                terrainObject.SetTile(terrainTile, terrainTileData.Find(t => t.fieldTileType == terrainTile.tileType).mesh);

                foreach (TileModifier modifier in terrainTile.modifiers)
                {
                    DecoTile decoObject = Instantiate(decoTilePrefab, GridToWorld(terrainTile.position), Quaternion.identity, terrainObject.transform).GetComponent<DecoTile>();
                    DecoTileData data = decoTileData.Find(t => t.tileModifer == modifier);
                    decoObject.SetTile(terrainTile, data.mesh, data.material);

                    gameTiles.Add(decoObject.gameObject);
                }

                gameTiles.Add(terrainObject.gameObject);
            }
            
            // Create the track
            foreach (FieldTile trackTile in playfield.track)
            {
                GameTile trackObject = Instantiate(fieldTilePrefab, GridToWorld(trackTile.position), Quaternion.identity, trackParent).GetComponent<GameTile>();
                trackObject.SetTile(trackTile, trackTileData.Find(t => t.fieldTileType == trackTile.tileType).mesh);
                
                gameTiles.Add(trackObject.gameObject);
            }
        }
        
        // Creates deco objects for the level
        private void InstantiateDeco()
        {
            // foreach (Vector3Int decoPoint in playfield.deco)
            // {
            //     GameObject deco = Instantiate(decoPrefabs[0], GridToWorld(decoPoint), Quaternion.identity);
            //     deco.transform.eulerAngles = new Vector3(0f, 0f, 0f);
            //     
            //     gameTiles.Add(deco);
            // }
        }
        
        // Removes the level tiles
        private void DestroyLevel()
        {
            foreach (GameObject tile in gameTiles)
            {
                Destroy(tile, 0f);
            }
        }

        // Generate a playfield
        private Playfield GeneratePlayfield()
        {
            return new Playfield(seed, level);
        }

        // Swaps the z and y in a passed Vector3Int and returns it as a Vector3
        private Vector3 GridToWorld(Vector3Int position)
        {
            return new Vector3(position.x, position.z * 0.5f, position.y);
        }
    }
}
