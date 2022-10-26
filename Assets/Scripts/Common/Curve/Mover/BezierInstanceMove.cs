using Timeline;
using UnityEngine;

namespace Common.Curve.Mover
{
    public class BezierInstanceMove : ABezierMover, ITimeControlExtended
    {
        [SerializeField] private BezierSplineCreator bezierSplineCreator;

        public void SetTime(double time, double duration)
        {
            var displacement = (float) time / (float) duration * SplineWithDistance.ArcLength;
            SetToDisplacement(displacement);
        }

        public void OnControlTimeStart()
        {
            if (SplineWithDistance == null)
            {
                SplineWithDistance = new BezierSplineWithDistance(bezierSplineCreator.Spline);
            }
        }

        public void OnControlTimeStop()
        {
        }
    }
}