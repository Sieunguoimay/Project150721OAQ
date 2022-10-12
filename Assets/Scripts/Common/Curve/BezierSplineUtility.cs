using UnityEngine;

namespace Common.Curve
{
    public static class BezierSplineUtility
    {
        public static BezierSpline CreateSplineSmoothPath(Vector3[] points)
        {
            var splinePoints = new Vector3[points.Length * 3 - 2];
            for (var i = 0; i < points.Length - 2; i++)
            {
                if (IsCollinear(points[i], points[i + 1], points[i + 2]))
                {
                    var a = points[i + 1] - points[i];
                    splinePoints[i * 3] = points[i];
                    splinePoints[i * 3 + 1] = points[i] + a / 3f;
                    splinePoints[i * 3 + 2] = points[i + 1] - a / 3f;
                }
                else
                {
                    var a = points[i] - points[i + 1];
                    var b = points[i + 2] - points[i + 1];

                    var sum = (a + b).normalized;
                    var cross = Vector3.Cross(a, b).normalized;
                    var dir = Vector3.Cross(sum, cross);

                    var minMag = Mathf.Min(a.magnitude, b.magnitude);
                    var p2 = points[i + 1] + dir * minMag / 3f;

                    splinePoints[i * 3] = points[i];
                    splinePoints[i * 3 + 1] = i == 0 ? (points[i + 1] - points[i]).normalized * a.magnitude / 3f + points[i] : Mirror(splinePoints[i * 3 - 1], points[i]);
                    splinePoints[i * 3 + 2] = p2;
                }
            }

            var lastIndex = points.Length - 2;
            var lastP1 = Mirror(splinePoints[lastIndex * 3 - 1], points[lastIndex]);

            splinePoints[lastIndex * 3] = points[lastIndex];
            splinePoints[lastIndex * 3 + 1] = lastP1;
            splinePoints[lastIndex * 3 + 2] = (points[^2] - points[^1]).normalized * Vector3.Distance(points[^2], points[^1]) / 3f + points[^1];
            splinePoints[lastIndex * 3 + 3] = points[^1];

            var controlPoint = new BezierSpline();
            controlPoint.SetControlPoints(splinePoints);
            return controlPoint;
        }

        private static bool IsCollinear(Vector3 a, Vector3 b, Vector3 c)
        {
            var distance1 = (a - b).sqrMagnitude;
            var distance2 = (b - c).sqrMagnitude;
            var distance3 = (c - a).sqrMagnitude;
            var area = 4 * distance1 * distance1 - (distance1 + distance2 - distance3) * (distance1 + distance2 - distance3);
            return area == 0f;
        }

        private static Vector3 Mirror(Vector3 point, Vector3 mirrorPoint)
        {
            return 2f * mirrorPoint - point;
        }
    }
}