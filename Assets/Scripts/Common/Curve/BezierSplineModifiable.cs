using Curve;
using UnityEngine;

namespace Common.Curve
{
    public class BezierSplineModifiable
    {
        private readonly BezierPointMode[] _modes;
        public bool Closed { get; private set; }

        public BezierSpline Spline { get; }

        public BezierSplineModifiable(Vector3[] controlPoints, BezierPointMode[] modes, bool closed)
            : this(new BezierSpline(controlPoints), modes, closed)
        {
        }

        public BezierSplineModifiable(BezierSpline spline, BezierPointMode[] modes, bool closed)
        {
            Spline = spline;
            _modes = modes;
            Closed = closed;
            for (var i = 0; i < Spline.ControlPoints.Length; i++)
            {
                EnforceModeAtControlPoint(i);
            }
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                var delta = point - Spline.ControlPoints[index];
                if (Closed)
                {
                    if (index == 0)
                    {
                        Spline.ControlPoints[1] += delta;
                        Spline.ControlPoints[^2] += delta;
                        Spline.ControlPoints[^1] = point;
                    }
                    else if (index == Spline.ControlPoints.Length - 1)
                    {
                        Spline.ControlPoints[index - 1] += delta;
                        Spline.ControlPoints[1] += delta;
                        Spline.ControlPoints[0] = point;
                    }
                    else
                    {
                        Spline.ControlPoints[index - 1] += delta;
                        Spline.ControlPoints[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        Spline.ControlPoints[index - 1] += delta;
                    }

                    if (index < Spline.ControlPoints.Length - 1)
                    {
                        Spline.ControlPoints[index + 1] += delta;
                    }
                }
            }

            Spline.ControlPoints[index] = point;
            EnforceModeAtControlPoint(index);
        }

        public BezierPointMode GetControlPointMode(int index)
        {
            return _modes[(index + 1) / 3];
        }

        public void SetControlPointMode(int index, BezierPointMode mode)
        {
            var modeIndex = (index + 1) / 3;
            _modes[modeIndex] = mode;
            if (Closed)
            {
                if (modeIndex == 0)
                {
                    _modes[^1] = mode;
                }
                else if (modeIndex == _modes.Length - 1)
                {
                    _modes[0] = mode;
                }
            }

            EnforceModeAtControlPoint(index);
        }

        private void EnforceModeAtControlPoint(int index)
        {
            var modeIndex = (index + 1) / 3;
            var mode = _modes[modeIndex];
            if (mode == BezierPointMode.Free || !Closed && (modeIndex == 0 || modeIndex == _modes.Length - 1))
            {
                return;
            }

            var middleIndex = modeIndex * 3;
            int fixedIndex, enforcedIndex;
            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                if (fixedIndex < 0)
                {
                    fixedIndex = Spline.ControlPoints.Length - 2;
                }

                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= Spline.ControlPoints.Length)
                {
                    enforcedIndex = 1;
                }
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= Spline.ControlPoints.Length)
                {
                    fixedIndex = 1;
                }

                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = Spline.ControlPoints.Length - 2;
                }
            }

            var middle = Spline.ControlPoints[middleIndex];
            var enforcedTangent = middle - Spline.ControlPoints[fixedIndex];
            if (mode == BezierPointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized *
                                  Vector3.Distance(middle, Spline.ControlPoints[enforcedIndex]);
            }

            Spline.ControlPoints[enforcedIndex] = middle + enforcedTangent;
        }

        public void SetClosed(bool closed)
        {
            Closed = closed;

            if (!closed) return;

            _modes[^1] = _modes[0];
            SetControlPoint(0, Spline.ControlPoints[0]);
        }
    }
}