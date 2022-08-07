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
        public bool isAiming = false; // Is the player allowed to aim
        public bool isSandy = false; // Is the ball in sand
        public bool isGrassy = false; // Is the ball in grass

        [Header("Camera Data")]
        private Camera mainCamera;
        private Plane mouseProjectionPlane;
        private Vector3 mouseWorldPosition;

        [Header("Hitting")]
        private Vector3 hittingWorldPosition;
        [SerializeField] private float maxHitStrength = 1f;
        [SerializeField] private Gradient hittingStrengthColors;

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
            mainCamera = Camera.main;
        }

        private void Update()
        {
            // Draw the force line when the ball is resting and aiming
            if (isResting && isAiming)
            {
                // Get the world position of the mouse
                mouseWorldPosition = GetMouseWorldPosition();

                // Only update if the mouse world position is not 'null' (the mouse position returns negative infinity for all values when it should be null)
                if (!float.IsNegativeInfinity(mouseWorldPosition.x))
                {
                    // Clamp the mouse position to the hit strength and store it as the hitting world position
                    hittingWorldPosition = ClampPointToRadius(mouseWorldPosition, transform.position, maxHitStrength);
                    
                    // Get the hit strength from the hitting position
                    float currentHitStrength = Vector3.Distance(transform.position, hittingWorldPosition);

                    // Draw the force line
                    DrawForceLine(hittingWorldPosition, currentHitStrength / maxHitStrength);
                }
            }

            // If the ball is not aiming, turn off the line renderer
            if (!isAiming)
            {
                forceLine.enabled = false;
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
                    isAiming = true;
                    break;
                default:
                    Debug.Log("???");
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            // If the ball is resting
            if (isResting)
            {
                // Draw the terrain normal
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, terrainNormal * 0.25f);
                
                // If the ball is aiming
                if (isAiming)
                {
                    // Draw the mouse plane
                    Vector3 mousePlaneSize = new Vector3(2f, 0f, 2f);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(transform.position, mousePlaneSize);
                    Gizmos.color = new Color(0f, 1f, 1f, 0.125f);
                    Gizmos.DrawCube(transform.position, mousePlaneSize);
                
                    // Draw mouse position
                    if (!float.IsNegativeInfinity(mouseWorldPosition.x))
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawSphere(hittingWorldPosition, 0.05f);
                    
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(mouseWorldPosition, 0.0625f);
                    }
                }
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
            // Return to last hit
            transform.position = lastHitPosition;
            transform.eulerAngles = lastHitRotation;
            // Set aiming to true
            isAiming = true;
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
            mouseProjectionPlane = new Plane(terrainNormal, transform.position);
            float distance;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (mouseProjectionPlane.Raycast(ray, out distance))
            {
                return ray.GetPoint(distance);
            }
            else
            {
                return new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            }
        }
        
        // Draws the force line
        private void DrawForceLine(Vector3 worldPoint, float strength)
        {
            // Create the force line positions
            Vector3[] forcePositions = new Vector3[] { worldPoint, transform.position };
            
            // Set the force line positions
            forceLine.SetPositions(forcePositions);
            forceLine.enabled = true;
            
            // Set the color of the force line
            Color strengthColor = hittingStrengthColors.Evaluate(strength);
            forceLine.material.SetColor("_Tint", strengthColor);
            
            // Set the speed of the force line
            forceLine.material.SetFloat("_Speed", strength * -2f);
        }
        
        // Clamps a point to a radius given a center
        private Vector3 ClampPointToRadius(Vector3 point, Vector3 center, float radius)
        {
            // Get the distance to the center point
            float distanceToCenter = Vector3.Distance(point, center);
 
            //If the distance is greater than the radius the point must be clamped
            if (distanceToCenter > radius)
            {
                Vector3 fromCenterToPoint = point - center;
                fromCenterToPoint *= radius / distanceToCenter;
                point = center + fromCenterToPoint;
            }

            return point;
        }
    }
}
