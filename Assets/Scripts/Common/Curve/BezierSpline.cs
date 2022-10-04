using System;
using System.Collections.Generic;
using Curve;
using UnityEngine;

namespace Common.Curve
{
    public class BezierSpline
    {
        protected Vector3[] ProtectedControlPoints;

        public int SegmentCount => (ControlPoints.Count - 1) / 3;
        public IReadOnlyList<Vector3> ControlPoints => ProtectedControlPoints;
        
        public virtual void SetControlPoints(Vector3[] controlPoints)
        {
            ProtectedControlPoints = controlPoints;
        }

        public Vector3 GetPoint(float t)
        {
            var i = MapTToIndex(t, out var segmentT);
            return Bezier.GetPoint(ProtectedControlPoints[i], ProtectedControlPoints[i + 1], ProtectedControlPoints[i + 2], ProtectedControlPoints[i + 3], segmentT);
        }

        public Vector3 GetVelocity(float t)
        {
            var i = MapTToIndex(t, out var segmentT);
            return Bezier.GetFirstDerivative(ProtectedControlPoints[i], ProtectedControlPoints[i + 1], ProtectedControlPoints[i + 2],
                ProtectedControlPoints[i + 3], segmentT);
        }

        private int MapTToIndex(float t, out float segmentT)
        {
            int i;

            if (t >= 1f)
            {
                t = 1f;
                i = ProtectedControlPoints.Length - 4;
            }
            else
            {
                var segmentCount = (ProtectedControlPoints.Length - 1) / 3;
                t = Mathf.Clamp01(t) * segmentCount;
                i = Mathf.FloorToInt(t);
                t -= i;
                i *= 3;
            }

            segmentT = t;
            return i;
        }
    }
}