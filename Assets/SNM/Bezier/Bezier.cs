using UnityEngine;

namespace SNM.Bezier
{
    public static class Bezier
    {
        private static double ComputeBinominal(int n, int k)
        {
            var value = 1.0;
            for (var i = 1; i <= k; i++)
            {
                value *= (n + 1 - i) / (double) i;
            }

            if (n == k)
            {
                value = 1;
            }

            return value;
        }

        public static Vector3 ComputeBezierCurve3D(Vector3[] points, float t)
        {
            var outputPoint = Vector3.zero;

            var n = points.Length - 1;

            for (var i = 0; i <= n; i++)
            {
                var x = ((float) ComputeBinominal(n, i))
                        * Mathf.Pow(1f - t, n - i)
                        * Mathf.Pow(t, i)
                        * points[i];
                outputPoint += x;
            }

            return outputPoint;
        }
    }
}