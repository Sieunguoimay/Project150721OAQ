using Curve;
using UnityEngine;

namespace Common.Curve
{
    public static class BezierSplineHelper
    {
        public static BezierSpline CreateSplineSmoothPath(Vector3[] points)
        {
            var splinePoints = new Vector3[points.Length * 3 - 2];
            for (var i = 0; i < points.Length - 2; i++)
            {
                var a = points[i] - points[i + 1];
                var b = points[i + 2] - points[i + 1];

                var sum = (a + b).normalized;
                var cross = Vector3.Cross(a, b).normalized;
                var dir = Vector3.Cross(sum, cross);

                var minMag = Mathf.Min(a.magnitude, b.magnitude);
                var p2 = points[i + 1] + dir * minMag / 2f;

                splinePoints[i * 3] = points[i];
                splinePoints[i * 3 + 1] = i == 0 ? (p2 - points[i]).normalized * a.magnitude / 2f + points[i] : Mirror(splinePoints[i * 3 - 1], points[i]);
                splinePoints[i * 3 + 2] = p2;
            }

            var lastIndex = points.Length - 2;
            var lastP1 = Mirror(splinePoints[lastIndex * 3 - 1], points[lastIndex]);

            splinePoints[lastIndex * 3] = points[lastIndex];
            splinePoints[lastIndex * 3 + 1] = lastP1;
            splinePoints[lastIndex * 3 + 2] = (lastP1 - points[^1]).normalized * Vector3.Distance(points[lastIndex], points[^1]) / 2f + points[^1];
            splinePoints[lastIndex * 3 + 3] = points[^1];

            return new BezierSpline(splinePoints);
        }

        private static Vector3 Mirror(Vector3 point, Vector3 mirrorPoint)
        {
            return 2f * mirrorPoint - point;
        }
    }
}