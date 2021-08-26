using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SNM
{
    public class Utils
    {
        public static T NewGameObject<T>() where T : MonoBehaviour
        {
            //create a GameObject that should be automatically added to the game scene
            var go = (new GameObject());
            var t = go.AddComponent<T>();
            go.name = t.GetType().Name;
            return t;
        }
    }

    public class Math
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

        public static List<Vector2Int> BresenhamCircleAlgorithm(int xc, int yc, int r)
        {
            int x = 0;
            int y = r;
            int d = 3 - 2 * r;
            List<Vector2Int>[] pixels = new List<Vector2Int>[8];
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
    }
}