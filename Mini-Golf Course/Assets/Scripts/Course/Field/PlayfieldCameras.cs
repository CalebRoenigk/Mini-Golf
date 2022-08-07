using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Course.Field
{
    public class PlayfieldCameras
    {
        // Camera 1
        public CinemachineSmoothPath overheadOrbitTrack;

        // Camera 2
        public CinemachineSmoothPath truckPlayfieldTrack;

        // Camera 3
        public CinemachineSmoothPath fallToStartTrack;
        
        // Player Cam
        public Vector3Int playerCamStart;

        public PlayfieldCameras(CinemachineSmoothPath overheadOrbitTrack, CinemachineSmoothPath truckPlayfieldTrack, CinemachineSmoothPath fallToStartTrack, Vector3Int playerCamStart)
        {
            this.overheadOrbitTrack = overheadOrbitTrack;
            this.truckPlayfieldTrack = truckPlayfieldTrack;
            this.fallToStartTrack = fallToStartTrack;
            this.playerCamStart = playerCamStart;
        }
    }
}
