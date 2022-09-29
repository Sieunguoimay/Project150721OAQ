using System;
using Curve;
using UnityEngine;

namespace Common.Curve
{
    public class BezierSplineDisplay : MonoBehaviour
    {
        private BezierSpline _spline;

        public void Display(BezierSpline spline)
        {
            _spline = spline;
        }

        private void OnDrawGizmos()
        {
            if (_spline == null) return;

            for (var i = 0f; i < 1f; i += 0.01f)
            {
                var p = transform.TransformPoint(_spline.GetPosition(i));
                Gizmos.DrawLine(p + Vector3.left * 0.03f, p + Vector3.right * 0.03f);
                Gizmos.DrawLine(p + Vector3.up * 0.03f, p + Vector3.down * 0.03f);
                Gizmos.DrawLine(p + Vector3.forward * 0.03f, p + Vector3.back * 0.03f);
            }
        }

        [SerializeField] private BezierSplineMono splineMono;

        [ContextMenu("Test")]
        private void Test()
        {
            Display(new BezierSpline(splineMono.Points));
        }
        [ContextMenu("Test2")]
        private void Test2()
        {
            splineMono.SetPoints(_spline.Points);
        }
    }
}