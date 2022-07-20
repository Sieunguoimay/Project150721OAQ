using Curve;
using UnityEngine;

namespace Common
{
    public interface ISplineModifier
    {
        void Setup(BezierSpline spline);
        void Modify(Vector3 p0, Vector3 p1);
    }
}