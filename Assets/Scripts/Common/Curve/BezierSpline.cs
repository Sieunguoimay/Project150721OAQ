using System;
using System.Collections.Generic;
using Curve;
using UnityEngine;

namespace Common.Curve
{
    public class BezierSpline
    {
        private readonly Vector3[] _controlPoints;
        private readonly BezierPointMode[] _modes;

        public IReadOnlyList<Vector3> ControlPoints => _controlPoints;
        public bool Closed { get; private set; }

        public BezierSpline(Vector3[] controlPoints, BezierPointMode[] modes, bool closed)
        {
            _controlPoints = controlPoints;
            _modes = modes;
            Closed = closed;
        }

        public Vector3 GetPoint(float t)
        {
            var i = MapTToIndex(t, out var segmentT);
            return Bezier.GetPoint(_controlPoints[i], _controlPoints[i + 1], _controlPoints[i + 2], _controlPoints[i + 3], segmentT);
        }

        public Vector3 GetVelocity(float t)
        {
            var i = MapTToIndex(t, out var segmentT);
            return Bezier.GetFirstDerivative(_controlPoints[i], _controlPoints[i + 1], _controlPoints[i + 2],
                _controlPoints[i + 3], segmentT);
        }

        private int MapTToIndex(float t, out float segmentT)
        {
            int i;

            if (t >= 1f)
            {
                t = 1f;
                i = _controlPoints.Length - 4;
            }
            else
            {
                var segmentCount = (_controlPoints.Length - 1) / 3;
                t = Mathf.Clamp01(t) * segmentCount;
                i = Mathf.FloorToInt(t);
                t -= i;
                i *= 3;
            }

            segmentT = t;
            return i;
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                var delta = point - _controlPoints[index];
                if (Closed)
                {
                    if (index == 0)
                    {
                        _controlPoints[1] += delta;
                        _controlPoints[^2] += delta;
                        _controlPoints[^1] = point;
                    }
                    else if (index == _controlPoints.Length - 1)
                    {
                        _controlPoints[index - 1] += delta;
                        _controlPoints[1] += delta;
                        _controlPoints[0] = point;
                    }
                    else
                    {
                        _controlPoints[index - 1] += delta;
                        _controlPoints[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        _controlPoints[index - 1] += delta;
                    }

                    if (index < _controlPoints.Length - 1)
                    {
                        _controlPoints[index + 1] += delta;
                    }
                }
            }

            _controlPoints[index] = point;
            EnforceMode(index);
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

            EnforceMode(index);
        }

        private void EnforceMode(int index)
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
                    fixedIndex = _controlPoints.Length - 2;
                }

                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= _controlPoints.Length)
                {
                    enforcedIndex = 1;
                }
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= _controlPoints.Length)
                {
                    fixedIndex = 1;
                }

                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = _controlPoints.Length - 2;
                }
            }

            var middle = _controlPoints[middleIndex];
            var enforcedTangent = middle - _controlPoints[fixedIndex];
            if (mode == BezierPointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, _controlPoints[enforcedIndex]);
            }

            _controlPoints[enforcedIndex] = middle + enforcedTangent;
        }

        public void SetClosed(bool closed)
        {
            Closed = closed;

            if (!closed) return;

            _modes[^1] = _modes[0];
            SetControlPoint(0, _controlPoints[0]);
        }
    }
}