using System;
using Curve;
using Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Common.Curve
{
    public class BezierSplineDisplay : MonoBehaviour, ITimeControlExtended
    {
        [SerializeField, Range(0f, 1f)] private float testSlider;
        private BezierSplineWithDistance _spline;

        public void Display(BezierSplineWithDistance spline)
        {
            _spline = spline;
        }

        private void Update()
        {
            if (_spline == null) return;
            if (testSlider < 1f)
            {
                testSlider += Time.deltaTime * 0.1f;
            }
            else
            {
                testSlider -= 1f;
            }
        }

        private void OnDrawGizmos()
        {
            if (_spline == null) return;

            for (var i = 0; i < _spline.Vertices.Count - 1; i++)
            {
                var p1 = transform.TransformPoint(_spline.Vertices[i].Vertex);
                var p2 = transform.TransformPoint(_spline.Vertices[i + 1].Vertex);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(p1, p2);
                Gizmos.color = Color.red;
                DrawCross(p1);
            }

            Gizmos.color = Color.white;

            var testPos = _spline.GetPointAtDistance(testSlider * _spline.ArcLength);
            Gizmos.DrawCube(transform.TransformPoint(testPos), Vector3.one * .2f);
        }

        private static void DrawCross(Vector3 p)
        {
            Gizmos.DrawLine(p + Vector3.left * 0.03f, p + Vector3.right * 0.03f);
            Gizmos.DrawLine(p + Vector3.up * 0.03f, p + Vector3.down * 0.03f);
            Gizmos.DrawLine(p + Vector3.forward * 0.03f, p + Vector3.back * 0.03f);
        }

        [SerializeField] private BezierSplineCreator splineMono;

        [ContextMenu("Test")]
        private void Test()
        {
            Display(new BezierSplineWithDistance(splineMono.Spline));
        }

        public void SetTime(double time, double duration)
        {
            testSlider = (float) time / (float) duration;
        }

        public void OnControlTimeStart()
        {
        }

        public void OnControlTimeStop()
        {
        }
    }
}