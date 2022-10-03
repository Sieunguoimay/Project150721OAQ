using System;
using Common.Curve;
using UnityEngine;
using UnityEngine.UIElements;

namespace Curve
{
    public class BezierSplineCreator : MonoBehaviour
    {
        [SerializeField] private Vector3[] controlPoints;
        [SerializeField] private BezierPointMode[] modes;
        [SerializeField] private bool closed;

        private BezierSpline _spline;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _spline = new BezierSpline(controlPoints, modes, closed);
        }

        public void Reset()
        {
            controlPoints = new[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                new Vector3(4f, 0f, 0f),
            };
            modes = new[] {BezierPointMode.Free, BezierPointMode.Free};
            _spline = new BezierSpline(controlPoints, modes, closed);
        }

        public void AddSegment()
        {
            var point = controlPoints[^1];
            Array.Resize(ref controlPoints, controlPoints.Length + 3);
            point.x += 1f;
            controlPoints[^3] = point;
            point.x += 1f;
            controlPoints[^2] = point;
            point.x += 1f;
            controlPoints[^1] = point;

            Array.Resize(ref modes, modes.Length + 1);
            modes[^1] = modes[^2];
            EnforceMode(controlPoints.Length - 4);
            if (!closed)
            {
                _spline = new BezierSpline(controlPoints, modes, closed);
                return;
            }

            controlPoints[^1] = controlPoints[0];
            modes[^1] = modes[0];
            EnforceMode(0);
            _spline = new BezierSpline(controlPoints, modes, closed);
        }

#endif
        public Vector3 GetPoint(float t)
        {
            return transform.TransformPoint(_spline.GetPoint(t));
        }

        public Vector3 GetVelocity(float t)
        {
            return transform.TransformVector(_spline.GetVelocity(t));
        }

        public Vector3 GetControlPoint(int index) => _spline.ControlPoints[index];

        public void SetControlPoint(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                var delta = point - controlPoints[index];
                if (closed)
                {
                    if (index == 0)
                    {
                        controlPoints[1] += delta;
                        controlPoints[^2] += delta;
                        controlPoints[^1] = point;
                    }
                    else if (index == controlPoints.Length - 1)
                    {
                        controlPoints[index - 1] += delta;
                        controlPoints[1] += delta;
                        controlPoints[0] = point;
                    }
                    else
                    {
                        controlPoints[index - 1] += delta;
                        controlPoints[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        controlPoints[index - 1] += delta;
                    }

                    if (index < controlPoints.Length - 1)
                    {
                        controlPoints[index + 1] += delta;
                    }
                }
            }

            controlPoints[index] = point;
            EnforceMode(index);
        }

        public BezierPointMode GetControlPointMode(int index)
        {
            return modes[(index + 1) / 3];
        }

        public void SetControlPointMode(int index, BezierPointMode mode)
        {
            var modeIndex = (index + 1) / 3;
            modes[modeIndex] = mode;
            if (closed)
            {
                if (modeIndex == 0)
                {
                    modes[^1] = mode;
                }
                else if (modeIndex == modes.Length - 1)
                {
                    modes[0] = mode;
                }
            }

            EnforceMode(index);
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
                    fixedIndex = controlPoints.Length - 2;
                }

                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= controlPoints.Length)
                {
                    enforcedIndex = 1;
                }
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= controlPoints.Length)
                {
                    fixedIndex = 1;
                }

                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = controlPoints.Length - 2;
                }
            }

            var middle = controlPoints[middleIndex];
            var enforcedTangent = middle - controlPoints[fixedIndex];
            if (mode == BezierPointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, controlPoints[enforcedIndex]);
            }

            controlPoints[enforcedIndex] = middle + enforcedTangent;
        }

        public int SegmentCount => (controlPoints.Length - 1) / 3;
        public int ControlPointCount => controlPoints.Length;

        public bool Closed
        {
            get => closed;
            set
            {
                closed = value;

                if (!value) return;

                modes[^1] = modes[0];
                SetControlPoint(0, controlPoints[0]);
            }
        }

        public Vector3[] ControlPoints => controlPoints;

        // [ContextMenu("Test")]
        // private void Test()
        // {
        //     var spline = BezierSplineHelper.CreateSplineSmoothPath(new[] {Vector3.zero, Vector3.right, Vector3.up});
        //     for (var i = 0; i < spline.ControlPointCount; i++)
        //     {
        //         Debug.Log(spline.GetControlPoint(i));
        //         controlPoints[i] = spline.GetControlPoint(i);
        //     }
        // }

        // public void SetPoints(Vector3[] ps)
        // {
        //     controlPoints = ps;
        //
        //     modes = new BezierPointMode[controlPoints.Length];
        //     for (var i = 0; i < modes.Length; i++)
        //         modes[i] = BezierPointMode.Aligned;
        // }
    }
}