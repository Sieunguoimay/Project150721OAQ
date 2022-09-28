using System;
using System.Collections.Generic;
using Curve;
using UnityEngine;

namespace Common.Curve
{
    [Serializable]
    public class BezierSpline
    {
        public Vector3[] _points;
        private float _length;

        public BezierSpline(Vector3[] points)
        {
            _points = points;
            // _length = ArcLength(1);
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
                i = (int) t;
                t -= i;
                i *= 3;
            }

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

        public float Length => _length;

        public Vector3 GetPoint(int index)
        {
            return _points[index];
        }

        // private float TangentMagnitude(float t) => GetVelocity(t).magnitude;
        // public float ArcLength(float t) => Integrate(TangentMagnitude, 0, t);
        //
        // public float Parameter(float length)
        // {
        //     var t = length / _length;
        //     var lowerBound = 0f;
        //     var upperBound = 1f;
        //     for (var i = 0; i < 100; ++i)
        //     {
        //         var f = ArcLength(t) - length;
        //         if (Mathf.Abs(f) < 0.01f) break;
        //         var derivative = TangentMagnitude(t);
        //         var candidateT = t - f / derivative;
        //         if (f > 0)
        //         {
        //             upperBound = t;
        //             if (candidateT <= 0) t = (upperBound + lowerBound) / 2;
        //             else t = candidateT;
        //         }
        //         else
        //         {
        //             lowerBound = t;
        //             if (candidateT >= 1) t = (upperBound + lowerBound) / 2;
        //             else t = candidateT;
        //         }
        //     }
        //
        //     return t;
        // }

        // private static readonly (float, float)[] CubicQuadrature = {(-0.7745966F, 0.5555556F), (0, 0.8888889F), (0.7745966F, 0.5555556F)};
        //
        // public static float Integrate(Func<float, float> f, in float lowerBound, in float upperBound)
        // {
        //     var sum = 0f;
        //     foreach (var (arg, weight) in CubicQuadrature)
        //     {
        //         var t = Mathf.Lerp(lowerBound, upperBound, Mathf.InverseLerp(-1, 1, arg));
        //         sum += weight * f(t);
        //     }
        //
        //     return sum * (upperBound - lowerBound) / 2;
        // }
    }
}