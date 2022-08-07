using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Golf
{
    public class Ball : MonoBehaviour
    {
        [Header("Singleton")]
        public static Ball instance;
        
        [Header("Runtime")]
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private LineRenderer forceLine;
        
        [Header("General")]
        public bool isResting = false; // Is the physics object deactivated
        public bool isSandy = false;
        public bool isGrassy = false;

        [Header("Gamestate")]
        public Vector3 lastHitPosition;
        public Vector3 lastHitRotation;
        public Vector3 terrainNormal;
        
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            // Store the default components
            rigidbody = GetComponent<Rigidbody>();
            forceLine = transform.GetChild(0).GetComponent<LineRenderer>();
        }

        private void Update()
        {
            // Draw the force line when the ball is resting
            if (isResting)
            {
                // Get the world position of the mouse
                Vector3 mouseWorldPosition = GetMouseWorldPosition();

                // Return if the mouse world position is actually negative infinity (the mouse position returned 'null')
                if (float.IsNegativeInfinity(mouseWorldPosition.x))
                {
                    return;
                }

                // Draw the force line
                DrawForceLine(mouseWorldPosition);
            }
            
        }
        
        private void FixedUpdate()
        {
            isResting = rigidbody.IsSleeping();
        }

        private void OnCollisionStay(Collision collisionInfo)
        {
            // Get the contacts
            ContactPoint[] contacts = new ContactPoint[collisionInfo.contactCount];
            collisionInfo.GetContacts(contacts);
            
            // Get the lowest point in the contacts
            ContactPoint[] contactsSorted = contacts.OrderBy(c => c.point.y).ToArray();
            ContactPoint lowestContact = contactsSorted[0];
            
            // Get the normal of the lowest contact and store it as the new terrain normal
            terrainNormal = lowestContact.normal;

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

        private void OnDrawGizmos()
        {
            // Draw the terrain normal when the ball is resting
            if (isResting)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, terrainNormal * 0.25f);
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
            // Store last hit
            lastHitPosition = transform.position;
            lastHitRotation = transform.eulerAngles;
        }
        
        // Gets the mouse from the camera view
        private Vector3 GetMouseWorldPosition()
        {
            Plane mousePlane = new Plane(Vector3.up, 0);
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (mousePlane.Raycast(ray, out distance))
            {
                return ray.GetPoint(distance);
            }
            else
            {
                return new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            }
        }
        
        // Draws the force line
        private void DrawForceLine(Vector3 worldPoint)
        {
            // Create the force line positions
            Vector3[] forcePositions = new Vector3[] { transform.position, worldPoint };
            
            // Set the force line positions
            forceLine.SetPositions(forcePositions);
            forceLine.enabled = true;
        }
    }
}
