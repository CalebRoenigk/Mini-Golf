using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Course;
using Golf;
using Cinemachine;
using UnityEngine.Playables;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game")]
        public GameState gameState;
        public int level = 1;
        public int seed;
        [SerializeField] private bool overrideSeed;
        [SerializeField] private int overrideSeedValue;

        [Header("Camera")]
        [SerializeField] private PlayableDirector cameraDirector;
        [SerializeField] private CinemachineTargetGroup ballTargetGroup;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private CinemachineVirtualCamera playerVirtualCamera;
        [SerializeField] private Transform centerTarget;
        [SerializeField] private Transform endTarget;
        [SerializeField] private CinemachineVirtualCamera fallCamera;

        [Header("Ball")]
        [SerializeField] private GameObject ballPrefab;

        // TODO: Add code here

        // Game should have different states: Intro, Playing, Win, Outro
        // On intro, ask camera manager to talk to field generator and get all data needed for the camera intro
        // Once playing, ask camera manager to set priority of follow cam to highest
        // On win, ask camera manager to do Win animation
        // On outro, ask field generator to make new course

        private void OnEnable()
        {
            FieldGenerator.playfieldGenerated += SpawnBall;
            Ball.ballInHole += WinLevel;
        }
        
        private void Start()
        {
            // Get a new seed for the game
            seed = UnityEngine.Random.Range(int.MinValue, Int32.MaxValue);
            if (overrideSeed)
            {
                seed = overrideSeedValue;
            }
            
            // Start the game
            StartLevel(level);
        }
        
        // Starts the level passed
        private void StartLevel(int lvl)
        {
            FieldGenerator.instance.CreateLevel(lvl, seed);
        }
        
        // Spawns the ball
        private void SpawnBall()
        {
            Instantiate(ballPrefab, FieldGenerator.instance.GetSpawnPoint(), Quaternion.identity);
            gameState = GameState.Playing;
            
            // Set up the cameras
            SetupCameras();
        }
        
        // Sets up the ball camera
        private void SetupCameras()
        {
            // Set up basic player camera information
            CinemachineTargetGroup.Target[] ballTargets = new CinemachineTargetGroup.Target[2];
            ballTargets[0].target = Ball.instance.transform;
            ballTargets[0].radius = 4f;
            ballTargets[0].weight = 90f;
            ballTargets[1].target = endTarget;
            ballTargets[1].radius = 1f;
            ballTargets[1].weight = 10f;
        
            ballTargetGroup.m_Targets = ballTargets;
            
            playerVirtualCamera.m_Follow = Ball.instance.transform;

            playerCamera.position = FieldGenerator.instance.GetPlayerCameraSpawnPoint();
            
            // Move overhead camera orbit group to center of level
            // Offset the camera by the level smallest dim
            // Local rotate the camera parent to be offset 45
            // Start the forever spinning
            
            // Tracking Camera lerps between two points
            
            // Put final camera above start a bit back
            fallCamera.transform.position = FieldGenerator.instance.GetFallCameraPosition();

            // Set up the center target
            centerTarget.position = FieldGenerator.instance.GetCenterTargetPosition();
            
            // Set up the hole target
            endTarget.position = FieldGenerator.instance.endHole.position;

            // Play the intro
            // cameraDirector.Play();
        }
        
        // Wins the level TODO: WIN LEVEL
        private void WinLevel()
        {
            // DO SOME SHIT
        }
    }
}
