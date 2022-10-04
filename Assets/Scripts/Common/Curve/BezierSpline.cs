using System;
using System.Collections.Generic;
using Curve;
using UnityEngine;

namespace Common.Curve
{
    public class BezierSpline
    {
        public Vector3[] ControlPoints { get; }

        public BezierSpline(Vector3[] controlPoints)
        {
            ControlPoints = controlPoints;
        }

        public Vector3 GetPoint(float t)
        {
            var i = MapTToIndex(t, out var segmentT);
            return Bezier.GetPoint(ControlPoints[i], ControlPoints[i + 1], ControlPoints[i + 2], ControlPoints[i + 3], segmentT);
        }

        public Vector3 GetVelocity(float t)
        {
            var i = MapTToIndex(t, out var segmentT);
            return Bezier.GetFirstDerivative(ControlPoints[i], ControlPoints[i + 1], ControlPoints[i + 2],
                ControlPoints[i + 3], segmentT);
        }

        private int MapTToIndex(float t, out float segmentT)
        {
            int i;

            if (t >= 1f)
            {
                t = 1f;
                i = ControlPoints.Length - 4;
            }
            else
            {
                var segmentCount = (ControlPoints.Length - 1) / 3;
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