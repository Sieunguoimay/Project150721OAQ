using System;
using System.Linq;
using Common.Curve;
using Curve;
using UnityEngine;
using UnityEngine.Events;

namespace Common
{
    public class BezierMover : MonoBehaviour
    {
        [SerializeField] private BezierSplineCreator initialPath;
        [SerializeField, Min(0.01f)] private float speed = 1f;
        [SerializeField] private bool loop;

        private BezierSplineWithDistance _splineWithDistance;
        private bool _moving;
        private float _displacement;
        private BezierSpline Spline => initialPath.Spline;

        private void Awake()
        {
            _splineWithDistance = new BezierSplineWithDistance(Spline);
        }

        [ContextMenu("Move")]
        private void Move() => Move(0f);

        public void Move(float startDisplacement)
        {
            _moving = true;
            _displacement = startDisplacement;
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            _moving = false;
        }

        private void Update()
        {
            if (loop && _displacement >= _splineWithDistance.ArcLength)
            {
                _displacement -= _splineWithDistance.ArcLength;
            }

            if (!_moving || Spline == null || _displacement >= _splineWithDistance.ArcLength) return;

            _displacement += Time.deltaTime * speed;

            var t = _splineWithDistance.GetTAtDistance(_displacement);
            transform.position = _splineWithDistance.Spline.GetPoint(t);
            transform.rotation = Quaternion.LookRotation(_splineWithDistance.Spline.GetVelocity(t));
        }
    }
}