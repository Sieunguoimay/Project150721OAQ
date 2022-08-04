using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SNM
{

    public static class Math
    {
        public static Vector3 Projection(Vector3 v, Vector3 up)
        {
            var n = Vector3.Cross(v, up);
            var tangent = Vector3.Cross(n, up);
            return Vector3.Dot(v, tangent) * tangent;
        }

        public static float CalculateJumpInitialVelocity(float s, float a, float t)
        {
            return (s - 0.5f * a * t * t) / t;
        }

        public static Vector3 MotionEquation(Vector3 initialPos, Vector3 initialVel, Vector3 initialAcc, float t)
        {
            return initialPos + initialVel * t + initialAcc * (0.5f * t * t);
        }
        public static float MotionEquation(float initialPos, float initialVel, float initialAcc, float t)
        {
            return initialPos + initialVel * t + initialAcc * (0.5f * t * t);
        }
        public static IEnumerable<Vector2Int> BresenhamCircleAlgorithm(int xc, int yc, int r)
        {
            var x = 0;
            var y = r;
            var d = 3 - 2 * r;
            var pixels = new List<Vector2Int>[8];
            for (var i = 0; i < 8; i++)
            {
                pixels[i] = new List<Vector2Int>();
            }

            BresenhamDrawCircle(xc, yc, x, y, pixels);
            while (y >= x)
            {
                x++;
                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                {
                    d = d + 4 * x + 6;
                }

                BresenhamDrawCircle(xc, yc, x, y, pixels);
            }

            var combinedPixels = new List<Vector2Int>();
            foreach (var p in pixels)
            {
                combinedPixels.AddRange(p);
            }

            return combinedPixels.Distinct().ToList();
        }

        private static void BresenhamDrawCircle(int xc, int yc, int x, int y, List<Vector2Int>[] pixels)
        {
            pixels[0].Add(new Vector2Int(xc + x, yc + y));
            pixels[1].Add(new Vector2Int(xc - x, yc + y));
            pixels[2].Add(new Vector2Int(xc + x, yc - y));
            pixels[3].Add(new Vector2Int(xc - x, yc - y));
            pixels[4].Add(new Vector2Int(xc + y, yc + x));
            pixels[5].Add(new Vector2Int(xc - y, yc + x));
            pixels[6].Add(new Vector2Int(xc + y, yc - x));
            pixels[7].Add(new Vector2Int(xc - y, yc - x));
        }

        public static Vector3 ClampMagnitude(Vector3 a, float mag)
        {
            if (a.sqrMagnitude > mag * mag)
            {
                a = a.normalized * mag;
            }

            return a;
        }

        public static Vector2 CubicBezier(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            var cx = 3 * (p1.x - p0.x);
            var cy = 3 * (p1.y - p0.y);
            var bx = 3 * (p2.x - p1.x) - cx;
            var by = 3 * (p2.y - p1.y) - cy;
            var ax = p3.x - p0.x - cx - bx;
            var ay = p3.y - p0.y - cy - by;
            var cube = t * t * t;
            var square = t * t;

            var resX = (ax * cube) + (bx * square) + (cx * t) + p0.x;
            var resY = (ay * cube) + (by * square) + (cy * t) + p0.y;

            return new Vector2(resX, resY);
        }

        public static float CubicBezier(float t, float p0, float p1, float p2, float p3)
        {
            var cx = 3 * (p1 - p0);
            var bx = 3 * (p2 - p1) - cx;
            var ax = p3 - p0 - cx - bx;
            var Cube = t * t * t;
            var Square = t * t;

            var resX = (ax * Cube) + (bx * Square) + (cx * t) + p0;

            return resX;
        }
    }
}