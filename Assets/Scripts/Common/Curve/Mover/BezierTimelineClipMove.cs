using System;
using Timeline;
using UnityEngine;
using UnityEngine.Events;
// using UnityEngine.Playables;

namespace Common.Curve.Mover
{
    public class BezierTimelineClipMove : ABezierMover, ITimeControlExtended
    {
        // [SerializeField] private BezierSplineMono bezierSpline;
        [SerializeField, Min(0f)] private float distance;
        [SerializeField] private UnityEvent onFinished;

        // private BezierSplineWithDistance _splineWithDistance;
        // private float _displacement;
        private float _initialDisplacement;

        [field: NonSerialized] public bool Finished { get; private set; }

        public float Distance => distance;

        public void SetTime(double time, double duration)
        {
            if (SplineWithDistance == null || Finished) return;

            var p = (float) time / (float) duration;

            var displacement = Mathf.Lerp(_initialDisplacement, _initialDisplacement + distance, p);

            var t = SetToDisplacement(displacement);

            if (!Finished && ShouldTriggerFinish(t, displacement, SplineWithDistance.ArcLength))
            {
                Finished = true;
                onFinished?.Invoke();
            }
        }

        protected virtual bool ShouldTriggerFinish(float t, float displacement, float curveLength)
        {
            return t >= 1f || displacement >= curveLength;
        }

        public void OnControlTimeStart()
        {
            if (SplineWithDistance == null)
            {
                if (bezierSpline == null)
                {
                    Debug.LogError("bezierSplineCreator is null");
                    return;
                }

                SplineWithDistance = new BezierSplineWithDistance(bezierSpline.Spline);
            }

            _initialDisplacement = Displacement;
        }

        public void OnControlTimeStop()
        {
        }

        [ContextMenu("ResetDisplacement")]
        public void ResetDisplacement()
        {
            SetToDisplacement(0f);
            Finished = false;
            SplineWithDistance = null;
        }
    }
}