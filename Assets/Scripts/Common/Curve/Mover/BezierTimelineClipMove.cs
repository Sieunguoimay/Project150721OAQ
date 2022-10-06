using System;
using Timeline;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Common.Curve.Mover
{
    public class BezierTimelineClipMove : MonoBehaviour, ITimeControlExtended
    {
        [SerializeField] private BezierSplineMono bezierSpline;
        [SerializeField, Min(0f)] private float distance;
        [SerializeField] private UnityEvent onFinished;

        private BezierSplineWithDistance _splineWithDistance;
        private float _displacement;
        private float _initialDisplacement;

        [field: NonSerialized] public bool Finished { get; private set; }

        public void SetTime(double time, double duration)
        {
            if (_splineWithDistance == null || Finished) return;

            var p = (float) time / (float) duration;

            _displacement = Mathf.Lerp(_initialDisplacement, _initialDisplacement + distance, p);

            var t = _splineWithDistance.GetTAtDistance(_displacement);

            Transform tr;
            var globalPosition = (tr = bezierSpline.transform).TransformPoint(_splineWithDistance.Spline.GetPoint(t));
            var globalRotation = tr.rotation * Quaternion.LookRotation(_splineWithDistance.Spline.GetVelocity(t));

            transform.position = globalPosition;
            transform.rotation = globalRotation;

            if (!Finished && (t >= 1f || _displacement >= _splineWithDistance.ArcLength))
            {
                Finished = true;
                onFinished?.Invoke();
            }
        }

        public void OnControlTimeStart()
        {
            if (_splineWithDistance == null)
            {
                if (bezierSpline == null)
                {
                    Debug.LogError("bezierSplineCreator is null");
                    return;
                }

                _splineWithDistance = new BezierSplineWithDistance(bezierSpline.Spline);
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
            Finished = false;
            _splineWithDistance = null;
        }
    }
}