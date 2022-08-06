using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Course.Field
{
    [System.Serializable]
    public class FieldSettings
    {
        // Playfield
        public Playfield playfield;
        
        // Terrain
        public float terrainScale;
        public int terrainHeight;
        public int terrainMargin;
        public float decoChance;
        public float terrainRockChance;
        public float terrainBushChance;
        public float terrainTreeChance;

        // Obstacles
        public float obstacleChance;
        public float rockChance;
        public float waterChance;
        public float sandChance;
        public float grassChance;
        
        // Landmarks
        public float landmarkChance;
        public float archChance;
        public float windmillChance;
        public float hillChance;

        public FieldSettings()
        {
            this.obstacleChance = 0f;
            this.rockChance = 0f;
            this.waterChance = 0f;
            this.sandChance = 0f;
            this.grassChance = 0f;
            this.landmarkChance = 0f;
            this.archChance = 0f;
            this.windmillChance = 0f;
            this.hillChance = 0f;
            this.decoChance = 0f;
        }
        
        public FieldSettings(Playfield playfield, float terrainScale, float decoChance)
        {
            this.playfield = playfield;

            this.terrainScale = terrainScale;
            this.terrainHeight = 5;
            this.terrainMargin = 5;
            this.decoChance = decoChance;
            
            this.obstacleChance = 0f;
            this.rockChance = 0f;
            this.waterChance = 0f;
            this.sandChance = 0f;
            this.grassChance = 0f;
            this.landmarkChance = 0f;
            this.archChance = 0f;
            this.windmillChance = 0f;
            this.hillChance = 0f;
        }
        
        public FieldSettings(Playfield playfield, float terrainScale, float decoChance, float terrainRockChance = -1f, float terrainBushChance = -1f, float terrainTreeChance = -1f)
        {
            this.playfield = playfield;

            this.terrainScale = terrainScale;
            this.terrainHeight = 5;
            this.terrainMargin = 5;
            this.decoChance = decoChance;
            this.terrainRockChance = terrainRockChance == -1f ? decoChance : terrainRockChance;
            this.terrainBushChance = terrainBushChance == -1f ? decoChance : terrainBushChance;
            this.terrainTreeChance = terrainTreeChance == -1f ? decoChance : terrainTreeChance;
            
            this.obstacleChance = 0f;
            this.rockChance = 0f;
            this.waterChance = 0f;
            this.sandChance = 0f;
            this.grassChance = 0f;
            this.landmarkChance = 0f;
            this.archChance = 0f;
            this.windmillChance = 0f;
            this.hillChance = 0f;
        }
    }
}
