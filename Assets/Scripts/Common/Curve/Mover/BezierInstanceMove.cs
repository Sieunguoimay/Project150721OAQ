using Timeline;
using UnityEngine;

namespace Common.Curve.Mover
{
    public class BezierInstanceMove : MonoBehaviour, ITimeControlExtended
    {
        [SerializeField] private BezierSplineCreator bezierSplineCreator;
        private BezierSplineWithDistance _splineWithDistance;

        public void SetTime(double time, double duration)
        {
            var displacement = (float) time / (float) duration * _splineWithDistance.ArcLength;

            var t = _splineWithDistance.GetTAtDistance(displacement);

            Transform tr;
            var globalPosition = (tr = bezierSplineCreator.transform).TransformPoint(_splineWithDistance.Spline.GetPoint(t));
            var globalRotation = tr.rotation * Quaternion.LookRotation(_splineWithDistance.Spline.GetVelocity(t));

            transform.position = globalPosition;
            transform.rotation = globalRotation;
        }

        public void OnControlTimeStart()
        {
            if (_splineWithDistance == null)
            {
                _splineWithDistance = new BezierSplineWithDistance(bezierSplineCreator.Spline);
            }
        }

        public void OnControlTimeStop()
        {
        }
    }
}