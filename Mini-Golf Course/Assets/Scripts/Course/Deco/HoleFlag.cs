using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Course.Deco
{
    public class HoleFlag : MonoBehaviour
    {
        [Header("Game")]
        [SerializeField] private List<Transform> flagBoneTransforms = new List<Transform>();
        [SerializeField] private float amplitude = 1f;
        [SerializeField] private float frequency = 1f;
        [SerializeField] private float speed = 1f;

        private void Update()
        {
            // Iterate over the bones and wave them
            foreach (Transform flagBone in flagBoneTransforms)
            {
                Vector3 position = flagBone.position;

                float zRotation = amplitude * Mathf.Sin(frequency * (Time.time + (position.x * speed))) + position.y;

                flagBone.localEulerAngles = new Vector3(0f, 0f, zRotation);
            }
        }
    }
}
