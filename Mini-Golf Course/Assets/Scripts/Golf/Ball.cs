using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Golf
{
    public class Ball : MonoBehaviour
    {
        [Header("Runtime")]
        [SerializeField] private Rigidbody rigidbody;
        
        [Header("General")]
        public bool isResting = false; // Is the physics object deactivated
        public bool isSandy = false;
        public bool isGrassy = false;

        [Header("Gamestate")]
        public Vector3 lastHitPosition;
        public Vector3 lastHitRotation;

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            isResting = rigidbody.IsSleeping();
        }

        private void OnCollisionStay(Collision collisionInfo)
        {
            switch (collisionInfo.gameObject.tag)
            {
                case "Terrain":
                    // Reset to the last position
                    ResetHit(0.25f);
                    break;
                case "Track":
                    // Store last hit as this position
                    StoreLastHit();
                    break;
                default:
                    Debug.Log("???");
                    break;
            }
        }
        
        // Resets the hit with an optional delay
        private void ResetHit(float delay = 1f)
        {
            StartCoroutine(SpawnBallAtLastHit(delay));
        }
        
        // Coroutine that places the ball to the last hit location after a delay
        private IEnumerator SpawnBallAtLastHit(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            transform.position = lastHitPosition;
            transform.eulerAngles = lastHitRotation;
        }
        
        // Stores the last hit as the current transform
        private void StoreLastHit()
        {
            lastHitPosition = transform.position;
            lastHitRotation = transform.eulerAngles;
        }
    }
}
