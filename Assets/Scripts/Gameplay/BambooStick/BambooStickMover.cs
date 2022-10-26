using Common.Curve.Mover;
using UnityEngine;

namespace Gameplay.BambooStick
{
    public class BambooStickMover : BezierTimelineClipMove
    {
        [SerializeField] private float finishDistance = 0f;

        protected override bool ShouldTriggerFinish(float t, float displacement, float curveLength)
        {
            if (t >= 1f || (displacement + finishDistance) >= curveLength)
            {
                return true;
            }

            return false;
        }
    }
}