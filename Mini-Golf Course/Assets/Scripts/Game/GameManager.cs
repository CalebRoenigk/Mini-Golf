using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        // TODO: Add code here
        
        // Game should have different states: Intro, Playing, Win, Outro
        // On intro, ask camera manager to talk to field generator and get all data needed for the camera intro
        // Once playing, ask camera manager to set priority of follow cam to highest
        // On win, ask camera manager to do Win animation
        // On outro, ask field generator to make new course
    }
}
