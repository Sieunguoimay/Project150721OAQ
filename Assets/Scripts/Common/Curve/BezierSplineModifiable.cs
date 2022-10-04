using System;
using System.Collections.Generic;
using System.Linq;
using Curve;
using UnityEngine;

namespace Common.Curve
{
    public class BezierSplineModifiable : BezierSpline
    {
        private BezierPointMode[] _modes;
        public bool Closed { get; private set; }
        public IReadOnlyList<BezierPointMode> Modes => _modes;

        //
        // public BezierSplineModifiable(Vector3[] controlPoints, BezierPointMode[] modes, bool closed)
        // {
        //     _modes = modes;
        //     Closed = closed;
        //     Spline = new BezierSpline();
        //     SetControlPoints(controlPoints);
        //     for (var i = 0; i < ControlPoints.Length; i++)
        //     {
        //         EnforceModeAtControlPoint(i);
        //     }
        // }

        public BezierSplineModifiable(BezierPointMode[] modes, bool closed)
        {
            _modes = modes;
            Closed = closed;
        }

        public override void SetControlPoints(Vector3[] controlPoints)
        {
            base.SetControlPoints(controlPoints);

            for (var i = 0; i < ProtectedControlPoints.Length; i++)
            {
                EnforceModeAtControlPoint(i);
            }
        }

        public void AddSegment(int segmentNum)
        {
            var controlPoints = ProtectedControlPoints;
            if (controlPoints.Length < 4)
            {
                controlPoints = new[]
                {
                    new Vector3(1f, 0f, 0f),
                    new Vector3(2f, 0f, 0f),
                    new Vector3(3f, 0f, 0f),
                    new Vector3(4f, 0f, 0f),
                };
                _modes = new[] {BezierPointMode.Free, BezierPointMode.Free};
                segmentNum--;
            }

            if (segmentNum <= 0)
            {
                SetControlPoints(controlPoints);
                return;
            }

            var point = controlPoints[^1];
            var oldLength = controlPoints.Length;
            var oldModeLength = _modes.Length;
            var mode = _modes[oldModeLength - 1];

            Array.Resize(ref controlPoints, oldLength + 3 * segmentNum);
            Array.Resize(ref _modes, _modes.Length + 1 * segmentNum);

            for (var i = 0; i < segmentNum; i++)
            {
                point.x += 1f;
                controlPoints[oldLength + i * 3] = point;
                point.x += 1f;
                controlPoints[oldLength + 1 + i * 3] = point;
                point.x += 1f;
                controlPoints[oldLength + 2 + i * 3] = point;

                _modes[oldModeLength + i * 1] = mode;
            }

            EnforceModeAtControlPoint(oldLength - 1);

            if (Closed)
            {
                controlPoints[^1] = controlPoints[0];
                _modes[^1] = _modes[0];

                EnforceModeAtControlPoint(0);
            }

            SetControlPoints(controlPoints);
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                var delta = point - ProtectedControlPoints[index];
                if (Closed)
                {
                    if (index == 0)
                    {
                        ProtectedControlPoints[1] += delta;
                        ProtectedControlPoints[^2] += delta;
                        ProtectedControlPoints[^1] = point;
                    }
                    else if (index == ProtectedControlPoints.Length - 1)
                    {
                        ProtectedControlPoints[index - 1] += delta;
                        ProtectedControlPoints[1] += delta;
                        ProtectedControlPoints[0] = point;
                    }
                    else
                    {
                        ProtectedControlPoints[index - 1] += delta;
                        ProtectedControlPoints[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        ProtectedControlPoints[index - 1] += delta;
                    }

                    if (index < ProtectedControlPoints.Length - 1)
                    {
                        ProtectedControlPoints[index + 1] += delta;
                    }
                }
            }

            ProtectedControlPoints[index] = point;
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
                    fixedIndex = ProtectedControlPoints.Length - 2;
                }

                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= ProtectedControlPoints.Length)
                {
                    enforcedIndex = 1;
                }
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= ProtectedControlPoints.Length)
                {
                    fixedIndex = 1;
                }

                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = ProtectedControlPoints.Length - 2;
                }
            }

            var middle = ProtectedControlPoints[middleIndex];
            var enforcedTangent = middle - ProtectedControlPoints[fixedIndex];
            if (mode == BezierPointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized *
                                  Vector3.Distance(middle, ProtectedControlPoints[enforcedIndex]);
            }

            ProtectedControlPoints[enforcedIndex] = middle + enforcedTangent;
        }

        public void SetClosed(bool closed)
        {
            Closed = closed;

            if (!closed) return;

            _modes[^1] = _modes[0];
            SetControlPoint(0, ProtectedControlPoints[0]);
        }
    }
}