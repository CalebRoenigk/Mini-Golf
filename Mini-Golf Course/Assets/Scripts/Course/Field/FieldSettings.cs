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
        
        public FieldSettings(float obstacleChance, float landmarkChance, float decoChance)
        {
            this.obstacleChance = obstacleChance;
            this.rockChance = obstacleChance;
            this.waterChance = obstacleChance;
            this.sandChance = obstacleChance;
            this.grassChance = obstacleChance;
            this.landmarkChance = landmarkChance;
            this.archChance = landmarkChance;
            this.windmillChance = landmarkChance;
            this.hillChance = landmarkChance;
            this.decoChance = decoChance;
        }
        
        public FieldSettings(float obstacleChance, float rockChance, float waterChance, float sandChance, float grassChance, float landmarkChance, float archChance, float windmillChance, float hillChance, float decoChance)
        {
            this.obstacleChance = obstacleChance;
            this.rockChance = rockChance;
            this.waterChance = waterChance;
            this.sandChance = sandChance;
            this.grassChance = grassChance;
            this.landmarkChance = landmarkChance;
            this.archChance = archChance;
            this.windmillChance = windmillChance;
            this.hillChance = hillChance;
            this.decoChance = decoChance;
        }
    }
}
