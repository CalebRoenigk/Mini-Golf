using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Cinemachine;
// using Dice;
using Course.Field;
using Golf;

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
        [SerializeField] private List<FieldTileData> supportTileData = new List<FieldTileData>();
        [SerializeField] private List<DecoTileData> decoTileData = new List<DecoTileData>();
        [SerializeField] private Material trackMaterial;
        [SerializeField] private Material supportMaterial;
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private Material waterMaterial;

        [Header("Camera")]
        [SerializeField] private CinemachineTargetGroup ballTargetGroup;

        [Header("Game")]
        [SerializeField] private GameObject fieldTilePrefab;
        [SerializeField] private GameObject decoTilePrefab;
        [SerializeField] private GameObject holePrefab;
        [SerializeField] private Transform endHole;
        private List<GameObject> gameTiles = new List<GameObject>();
        // [SerializeField] private Ball ball;
        private Vector3 trackOffsetFromTerrain = new Vector3(0f, 0.03f, 0f); // The amount of extra Y offset used to keep the ball from hitting the terrain when colliding with the track normally

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
                    
                    // Draw River
                    foreach (FieldTile river in playfield.river)
                    {
                        Gizmos.color = Color.blue;
                        if (river.tileType == FieldTileType.None)
                        {
                            Gizmos.color = Color.cyan;
                        }
                        Gizmos.DrawCube(GridToWorld(river.position), new Vector3(1f, 0.5f, 1f));
                    }
                }
            }
        }

        // Generates and Instantiates the field
        private void CreateLevel()
        {
            playfield = GeneratePlayfield();
            InstantiatePlayfield();
            
            // Set the target of the ball camera
            SetupCamera();

            // ball.transform.position = playfield.spawn;
        }
        
        // Creates the objects for the game world
        private void InstantiatePlayfield()
        {
            // Create the terrain
            foreach (FieldTile terrainTile in playfield.terrain)
            {
                GameTile terrainObject = Instantiate(fieldTilePrefab, GridToWorld(terrainTile.position), Quaternion.identity, terrainParent).GetComponent<GameTile>();
                FieldTileData terrainData = terrainTileData.Find(t => t.fieldTileType == terrainTile.tileType);
                Mesh terrainMesh = terrainData.mesh;
                Material secondaryMaterial = terrainMaterial;
                string materialVectorID = "";
                Vector4 materialVector = Vector4.zero;
                bool hasSecondaryMaterial = false;
                bool setMaterialVector = false;
                terrainObject.gameObject.name = "Terrain" + terrainTile.position.ToString();

                foreach (TileModifier modifier in terrainTile.modifiers)
                {
                    if (modifier != TileModifier.Water)
                    {
                        DecoTile decoObject = Instantiate(decoTilePrefab, GridToWorld(terrainTile.position), Quaternion.identity, terrainObject.transform).GetComponent<DecoTile>();
                        DecoTileData data = decoTileData.Find(t => t.tileModifer == modifier);
                        Color color = data.colors[(int)Mathf.Floor(Random.Range(0, data.colors.Count))];
                        Mesh mesh = data.meshes[(int)Mathf.Floor(Random.Range(0, data.meshes.Count))];
                        decoObject.SetTile(modifier, color, mesh, data.material);

                        decoObject.gameObject.tag = "Decoration";
                        decoObject.gameObject.name = modifier.ToString();
                        gameTiles.Add(decoObject.gameObject);
                    }
                    else
                    {
                        // River Placement
                        terrainMesh = terrainData.fieldTileModiferData.Find(m => m.replacmentModifier == TileModifier.Water).mesh;
                        secondaryMaterial = terrainData.fieldTileModiferData.Find(m => m.replacmentModifier == TileModifier.Water).secondaryMaterial;
                        materialVector = playfield.GetRiverDirection(terrainTile.position);
                        if (terrainTile.rotation == 90 && terrainTile.tileType == FieldTileType.Flat)
                        {
                            materialVector.y = 1;
                        }

                        if ((terrainTile.rotation == 180 || terrainTile.rotation == 90) && terrainTile.tileType == FieldTileType.Bend)
                        {
                            materialVector.x = -1;
                            materialVector.y = 1;
                        }
                        
                        if ((terrainTile.rotation == 180 || terrainTile.rotation == -90) && terrainTile.tileType == FieldTileType.Slope)
                        {
                            materialVector.y = 1;
                        }

                        materialVector *= 0.5f;
                        
                        materialVectorID = "_RiverSpeed";
                        hasSecondaryMaterial = true;
                        setMaterialVector = true;
                        terrainObject.gameObject.name = "River" + terrainTile.position.ToString();
                    }
                }

                if (hasSecondaryMaterial)
                {
                    terrainObject.SetTile(terrainTile, terrainMesh, terrainMaterial, secondaryMaterial);
                }
                else
                {
                    terrainObject.SetTile(terrainTile, terrainMesh, terrainMaterial);
                }

                if (setMaterialVector)
                {
                    terrainObject.SetMaterialVector(1, materialVectorID,materialVector);
                }
                
                
                terrainObject.gameObject.tag = "Terrain";
                gameTiles.Add(terrainObject.gameObject);
            }
            
            // Create the track
            foreach (FieldTile trackTile in playfield.track)
            {
                GameTile trackObject = Instantiate(fieldTilePrefab, GridToWorld(trackTile.position) + trackOffsetFromTerrain, Quaternion.identity, trackParent).GetComponent<GameTile>();
                FieldTileData data = trackTileData.Find(t => t.fieldTileType == trackTile.tileType);
                Mesh mesh = data.mesh;
                foreach (FieldTileModifierData tileModifier in data.fieldTileModiferData)
                {
                    if (trackTile.modifiers.Contains(tileModifier.replacmentModifier))
                    {
                        mesh = tileModifier.mesh;
                    }
                }
                trackObject.SetTile(trackTile, mesh, trackMaterial);
                
                trackObject.gameObject.tag = "Track";
                trackObject.gameObject.name = "Track" + trackTile.position.ToString();
                gameTiles.Add(trackObject.gameObject);

                if (trackTile.modifiers.Contains(TileModifier.Hole))
                {
                    endHole = Instantiate(holePrefab, GridToWorld(trackTile.position) + trackOffsetFromTerrain, Quaternion.identity, trackParent).transform;
                }
            }
            
            // Create the supports
            foreach (FieldTile supportTile in playfield.support)
            {
                GameTile supportObject = Instantiate(fieldTilePrefab, GridToWorld(supportTile.position) + trackOffsetFromTerrain, Quaternion.identity, trackParent).GetComponent<GameTile>();
                FieldTileData data = supportTileData.Find(t => t.fieldTileType == supportTile.tileType);
                Mesh mesh = data.mesh;
                foreach (FieldTileModifierData tileModifier in data.fieldTileModiferData)
                {
                    if (supportTile.modifiers.Contains(tileModifier.replacmentModifier))
                    {
                        mesh = tileModifier.mesh;
                    }
                }
                supportObject.SetTile(supportTile, mesh, supportMaterial);
                
                supportObject.gameObject.tag = "Decoration";
                supportObject.gameObject.name = "Support" + supportTile.position.ToString();
                gameTiles.Add(supportObject.gameObject);
            }
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
        
        // Sets up the ball camera
        private void SetupCamera()
        {
            CinemachineTargetGroup.Target[] ballTargets = new CinemachineTargetGroup.Target[2];
            ballTargets[0].target = Ball.instance.transform;
            ballTargets[0].radius = 4f;
            ballTargets[0].weight = 90f;
            ballTargets[1].target = endHole;
            ballTargets[1].radius = 1f;
            ballTargets[1].weight = 10f;

            ballTargetGroup.m_Targets = ballTargets;
        }
    }
}
