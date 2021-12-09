using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Curve
{
    public class BezierSpline : MonoBehaviour
    {
        public Vector3[] points;
        public BezierPointMode[] modes;
        public bool closed;

        public float CurveLength { get; private set; }
#if UNITY_EDITOR
        public void Reset()
        {
            points = new[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                new Vector3(4f, 0f, 0f),
            };
            modes = new[] {BezierPointMode.Free, BezierPointMode.Free};
        }

        public void AddSegment()
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
            EnforceMode(points.Length - 4);
            if (closed)
            {
                points[points.Length - 1] = points[0];
                modes[modes.Length - 1] = modes[0];
                EnforceMode(0);
            }
        }

#endif
        public void UpdateCurveLength()
        {
            CurveLength = CalculateSplineLength();
        }

        public Vector3 GetPosition(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * SegmentCount;
                i = (int) t;
                t -= i;
                i *= 3;
            }

            return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2],
                points[i + 3], t));
        }

        public Vector3 GetVelocity(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * SegmentCount;
                i = (int) t;
                t -= i;
                i *= 3;
            }

            return transform.TransformVector(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2],
                points[i + 3], t));
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        public Vector3 GetPoint(int index)
        {
            return points[index];
        }

        public int GetIndexOfPointOnSegment(int segmentIndex, int pointIndex)
        {
            return segmentIndex * 3 + pointIndex;
        }

        public void SetPoint(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                var delta = point - points[index];
                if (closed)
                {
                    if (index == 0)
                    {
                        points[1] += delta;
                        points[points.Length - 2] += delta;
                        points[points.Length - 1] = point;
                    }
                    else if (index == points.Length - 1)
                    {
                        points[index - 1] += delta;
                        points[1] += delta;
                        points[0] = point;
                    }
                    else
                    {
                        points[index - 1] += delta;
                        points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        points[index - 1] += delta;
                    }

                    if (index < points.Length - 1)
                    {
                        points[index + 1] += delta;
                    }
                }
            }

            points[index] = point;
            EnforceMode(index);
            UpdateCurveLength();
        }

        public BezierPointMode GetPointMode(int index)
        {
            return modes[(index + 1) / 3];
        }

        public void SetPointMode(int index, BezierPointMode mode)
        {
            var modeIndex = (index + 1) / 3;
            modes[modeIndex] = mode;
            if (closed)
            {
                if (modeIndex == 0)
                {
                    modes[modes.Length - 1] = mode;
                }
                else if (modeIndex == modes.Length - 1)
                {
                    modes[0] = mode;
                }
            }

            EnforceMode(index);
            UpdateCurveLength();
        }

        public float CalculateSplineLength()
        {
            float length = 0f;
            var n = SegmentCount;
            for (int i = 0; i < n; i++)
            {
                var p0 = GetPoint(GetIndexOfPointOnSegment(i, 0));
                var p1 = GetPoint(GetIndexOfPointOnSegment(i, 1));
                var p2 = GetPoint(GetIndexOfPointOnSegment(i, 2));
                var p3 = GetPoint(GetIndexOfPointOnSegment(i, 3));
                length += Bezier.GetCurveLength(p0, p1, p2, p3);
            }

            return length;
        }

        private void EnforceMode(int index)
        {
            int modeIndex = (index + 1) / 3;
            var mode = modes[modeIndex];
            if (mode == BezierPointMode.Free || !closed && (modeIndex == 0 || modeIndex == modes.Length - 1))
            {
                return;
            }

            int middleIndex = modeIndex * 3;
            int fixedIndex, enforcedIndex;
            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                if (fixedIndex < 0)
                {
                    fixedIndex = points.Length - 2;
                }

                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= points.Length)
                {
                    enforcedIndex = 1;
                }
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= points.Length)
                {
                    fixedIndex = 1;
                }

                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = points.Length - 2;
                }
            }

            var middle = points[middleIndex];
            var enforcedTangent = middle - points[fixedIndex];
            if (mode == BezierPointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
            }

            points[enforcedIndex] = middle + enforcedTangent;
        }

        public int SegmentCount => (points.Length - 1) / 3;
        public int PointCount => points.Length;

        public bool Closed
        {
            get => closed;
            set
            {
                closed = value;
                if (value)
                {
                    modes[modes.Length - 1] = modes[0];
                    SetPoint(0, points[0]);
                }
            }
        }
    }
}