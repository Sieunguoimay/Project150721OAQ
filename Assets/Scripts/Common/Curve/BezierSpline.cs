using System;
using System.Collections.Generic;
using Curve;
using UnityEngine;

namespace Common.Curve
{
    public class BezierSpline
    {
        private readonly Vector3[] _points;

        public BezierSpline(Vector3[] points)
        {
            _points = points;
        }

        public Vector3 GetPosition(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = _points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * SegmentCount;
                i = Mathf.FloorToInt(t);
                t -= i;
                i *= 3;
            }

            Debug.Log(i + " " + t);
            return Bezier.GetPoint(_points[i], _points[i + 1], _points[i + 2], _points[i + 3], t);
        }

        public Vector3 GetVelocity(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = _points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * SegmentCount;
                i = (int) t;
                t -= i;
                i *= 3;
            }

            return Bezier.GetFirstDerivative(_points[i], _points[i + 1], _points[i + 2],
                _points[i + 3], t);
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        public int SegmentCount => (_points.Length - 1) / 3;
        public int PointCount => _points.Length;

        public Vector3[] Points => _points;

        public Vector3 GetPoint(int index)
        {
            return _points[index];
        }
    }
}