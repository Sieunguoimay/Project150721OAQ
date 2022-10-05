using Timeline;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Curve.Mover
{
    public class BezierTimelineClipMove : MonoBehaviour, ITimeControlExtended
    {
        [SerializeField] private BezierSplineCreator bezierSplineCreator;
        [SerializeField, Min(0f)] private float distance;
        [SerializeField] private UnityEvent onFinished;

        private BezierSplineWithDistance _splineWithDistance;
        private float _displacement;
        private float _initialDisplacement;
        private bool _finished;

        public void SetTime(double time, double duration)
        {
            if (_splineWithDistance == null || _finished) return;

            var p = (float) time / (float) duration;

            _displacement = Mathf.Lerp(_initialDisplacement, _initialDisplacement + distance, p);

            var t = _splineWithDistance.GetTAtDistance(_displacement);

            Transform tr;
            var globalPosition = (tr = bezierSplineCreator.transform).TransformPoint(_splineWithDistance.Spline.GetPoint(t));
            var globalRotation = tr.rotation * Quaternion.LookRotation(_splineWithDistance.Spline.GetVelocity(t));

            transform.position = globalPosition;
            transform.rotation = globalRotation;

            if (!_finished && (t >= 1f || _displacement >= _splineWithDistance.ArcLength))
            {
                _finished = true;
                onFinished?.Invoke();
            }
        }

        public void OnControlTimeStart()
        {
            if (_splineWithDistance == null)
            {
                if (bezierSplineCreator == null)
                {
                    Debug.LogError("bezierSplineCreator is null");
                    return;
                }

                _splineWithDistance = new BezierSplineWithDistance(bezierSplineCreator.Spline);
            }

            _initialDisplacement = _displacement;
        }

        public void OnControlTimeStop()
        {
        }

        [ContextMenu("ResetDisplacement")]
        public void ResetDisplacement()
        {
            _displacement = 0f;
            _finished = false;
        }
    }
}