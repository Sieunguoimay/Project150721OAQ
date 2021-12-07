﻿using System;
using UnityEngine;

namespace Curve
{
    public class BezierSpline : MonoBehaviour
    {
        [SerializeField] private Vector3[] points;
        [SerializeField] private BezierControlPointMode[] modes;

        public void Reset()
        {
            points = new Vector3[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                new Vector3(4f, 0f, 0f),
            };
            modes = new[] {BezierControlPointMode.Free, BezierControlPointMode.Free};
        }

        public Vector3 GetPoint(float t)
        {
            var i = GetIndex(ref t);
            return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2],
                points[i + 3], t));
        }

        public Vector3 GetVelocity(float t)
        {
            var i = GetIndex(ref t);
            return transform.TransformVector(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2],
                points[i + 3], t));
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        public void AddCurve()
        {
            var point = points[points.Length - 1];
            Array.Resize(ref points, points.Length + 3);
            point.x += 1f;
            points[points.Length - 3] = point;
            point.x += 1f;
            points[points.Length - 2] = point;
            point.x += 1f;
            points[points.Length - 1] = point;

            Array.Resize(ref modes, modes.Length + 1);
            modes[modes.Length - 1] = modes[modes.Length - 2];
        }

        public int CurveCount => (points.Length - 1) / 3;

        private int GetIndex(ref float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                i = (int) t;
                t -= i;
                i *= 3;
            }

            return i;
        }

        public int ControlPointCount => points.Length;

        public Vector3 GetControlPoint(int index)
        {
            return points[index];
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            points[index] = point;
            EnforceMode(index);
        }

        public BezierControlPointMode GetControlPointMode(int index)
        {
            return modes[(index + 1) / 3];
        }

        public void SetControlPointMode(int index, BezierControlPointMode mode)
        {
            modes[(index + 1) / 3] = mode;
            EnforceMode(index);
        }

        private void EnforceMode(int index)
        {
            int modeIndex = (index + 1) / 3;
            var mode = modes[modeIndex];
            if (mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex == modes.Length - 1)
            {
                return;
            }

            int middleIndex = modeIndex * 3;
            int fixedIndex, enforcedIndex;
            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                enforcedIndex = middleIndex + 1;
            }
            else
            {
                fixedIndex = middleIndex + 1;
                enforcedIndex = middleIndex - 1;
            }

            var middle = points[middleIndex];
            var enforcedTangent = middle - points[fixedIndex];
            if (mode == BezierControlPointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
            }

            points[enforcedIndex] = middle + enforcedTangent;
        }
    }
}