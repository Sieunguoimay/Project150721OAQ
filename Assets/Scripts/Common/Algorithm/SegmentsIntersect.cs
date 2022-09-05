using UnityEngine;

namespace Common.Algorithm
{
    public static class SegmentsIntersect
    {
        private const float Eps = 1E-9f;

        private class Line
        {
            public float A, B, C;

            public Line(Vector2 p, Vector2 q)
            {
                A = p.y - q.y;
                B = q.x - p.x;
                C = -A * p.x - B * p.y;
                Norm();
            }

            private void Norm()
            {
                var z = Mathf.Sqrt(A * A + B * B);
                if (Mathf.Abs(z) > Eps)
                {
                    A /= z;
                    B /= z;
                    C /= z;
                }
            }

            public float Dist(Vector2 p)
            {
                return A * p.x + B * p.y + C;
            }
        };

        private static float Det(float a, float b, float c, float d)
        {
            return a * d - b * c;
        }

        private static bool Between(float l, float r, float x)
        {
            return Mathf.Min(l, r) <= x + Eps && x <= Mathf.Max(l, r) + Eps;
        }

        private static bool intersect_1d(float a, float b, float c, float d)
        {
            if (a > b)
                Swap(ref a, ref b);
            if (c > d)
                Swap(ref c, ref d);
            return Mathf.Max(a, c) <= Mathf.Min(b, d) + Eps;
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        private static bool LessThan(Vector2 a, Vector2 p)
        {
            return a.x < p.x - Eps || (Mathf.Abs(a.x - p.x) < Eps && a.y < p.y - Eps);
        }

        public static bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 left, out Vector2 right)
        {
            left = a;
            right = b;

            if (!intersect_1d(a.x, b.x, c.x, d.x) || !intersect_1d(a.y, b.y, c.y, d.y))
            {
                return false;
            }

            var m = new Line(a, b);
            var n = new Line(c, d);
            var zn = Det(m.A, m.B, n.A, n.B);
            if (Mathf.Abs(zn) < Eps)
            {
                if (Mathf.Abs(m.Dist(c)) > Eps || Mathf.Abs(n.Dist(a)) > Eps)
                    return false;
                if (LessThan(b, a))
                    Swap(ref a, ref b);
                if (LessThan(d, c))
                    Swap(ref c, ref d);
                left = Vector2.Max(a, c);
                right = Vector2.Min(b, d);
                return true;
            }
            else
            {
                left.x = right.x = -Det(m.C, m.B, n.C, n.B) / zn;
                left.y = right.y = -Det(m.A, m.C, n.A, n.C) / zn;
                return Between(a.x, b.x, left.x) && Between(a.y, b.y, left.y) &&
                       Between(c.x, d.x, left.x) && Between(c.y, d.y, left.y);
            }
        }
    }
}