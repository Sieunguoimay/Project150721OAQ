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
        public Transform end;

        public void UpdatePath(Vector3 startPos, Vector3 startForward, Vector3 endPos, Vector3 endForward)
        {
            start.position = startPos;
            start.forward = startForward;
            end.position = endPos;
            end.forward = endForward;
            GetComponentInChildren<BambooStickPathPlan>().PlanPath();
        }
    }
}