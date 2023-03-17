using Common.Curve.Mover;
using UnityEngine;

namespace Gameplay.Visual.BambooStick
{
    public class BambooStickMover : BezierTimelineClipMove
    {
        [SerializeField] private float finishDistance = 0f;

        protected override bool ShouldTriggerFinish(float t, float displacement, float curveLength)
        {
            return t >= 1f || displacement + finishDistance >= curveLength;
        }
    }
}