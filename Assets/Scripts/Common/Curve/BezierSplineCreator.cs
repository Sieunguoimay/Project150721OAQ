using System;
using Common.Curve;
using UnityEngine;
using UnityEngine.UIElements;

namespace Curve
{
    public class BezierSplineCreator : MonoBehaviour
    {
        [SerializeField] private Vector3[] controlPoints;
        [SerializeField] private BezierPointMode[] modes;
        [SerializeField] private bool closed;

        private BezierSplineModifiable _splineModifiable;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _splineModifiable = new BezierSplineModifiable(controlPoints, modes, closed);
        }

        public void Reset()
        {
            controlPoints = new[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                new Vector3(4f, 0f, 0f),
            };
            modes = new[] {BezierPointMode.Free, BezierPointMode.Free};
            _splineModifiable = new BezierSplineModifiable(controlPoints, modes, closed);
        }

        public void AddSegment()
        {
            var point = controlPoints[^1];
            Array.Resize(ref controlPoints, controlPoints.Length + 3);
            point.x += 1f;
            controlPoints[^3] = point;
            point.x += 1f;
            controlPoints[^2] = point;
            point.x += 1f;
            controlPoints[^1] = point;

            Array.Resize(ref modes, modes.Length + 1);
            modes[^1] = modes[^2];

            if (!closed)
            {
                _splineModifiable = new BezierSplineModifiable(controlPoints, modes, closed);
                return;
            }

            controlPoints[^1] = controlPoints[0];
            modes[^1] = modes[0];

            _splineModifiable = new BezierSplineModifiable(controlPoints, modes, closed);
        }

#endif
        public Vector3 GetPoint(float t) => Spline.GetPoint(t);
        public Vector3 GetVelocity(float t) => Spline.GetVelocity(t);
        public Vector3 GetLocalControlPoint(int index) => Spline.ControlPoints[index];
        public void SetLocalControlPoint(int index, Vector3 point) => _splineModifiable.SetControlPoint(index, point);
        public BezierPointMode GetControlPointMode(int index) => _splineModifiable.GetControlPointMode(index);
        public void SetControlPointMode(int index, BezierPointMode mode) => _splineModifiable.SetControlPointMode(index, mode);
        public int SegmentCount => (_splineModifiable.Spline.ControlPoints.Length - 1) / 3;
        public int ControlPointCount => Spline.ControlPoints.Length;
        public bool Closed => _splineModifiable.Closed;
        public BezierSpline Spline => _splineModifiable.Spline;
        public void SetClosed(bool close)
        {
            closed = close;
            _splineModifiable.SetClosed(close);
        }
    }
}