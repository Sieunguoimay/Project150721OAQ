using Common.Curve.Mover;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.BambooStick
{
    public class BambooStickSpace : MonoBehaviour
    {
        public BezierTimelineClipMove mover;
        public PlayableDirector timeline;
        public Transform start;
        public BambooStickPathPlan pathPlan;
    }
}