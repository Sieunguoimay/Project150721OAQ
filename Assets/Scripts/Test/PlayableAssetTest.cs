using System.Collections.Generic;
using Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Test
{
    public class PlayableAssetTest : MonoBehaviour, ITimeControlExtended
    {
        public void SetTime(double time, double duration)
        {
            Debug.Log(time + " " + duration);
        }

        public void OnControlTimeStart()
        {
            Debug.Log("OnControlTimeStart");
        }

        public void OnControlTimeStop()  
        {
            Debug.Log("OnControlTimeStop");
        }
    }
}