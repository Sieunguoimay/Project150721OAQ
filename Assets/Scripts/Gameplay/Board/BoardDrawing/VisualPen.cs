using System;
using Common.Algorithm;
using Common.Curve;
using Common.DrawLine;
using Curve;
using UnityEngine;

namespace Gameplay.Board.BoardDrawing
{
    public class VisualPen : MonoBehaviour, IDrawingPenHandler
    {
        [SerializeField] private DrawingPen pen;
        [SerializeField] private Transform penBall;
        [SerializeField] private float scale = 0.5f;
        [SerializeField] private bool smoothRotation = true;

        private float _radius;
        private Vector2[] _clampPolygon;
        private Func<Vector2, Vector2> _projection;
        private BezierSpline _spline;

        public void Draw(Vector2[] points, (int, int)[] contour, float radius, string inkName)
        {
            _radius = radius;
            _projection = ProjectOnCircle;

            pen.Draw(points, contour, inkName);
        }

        public void Draw(Vector2[] points, (int, int)[] contour, Vector2[] clampPolygon, string inkName)
        {
            _clampPolygon = clampPolygon;

            pen.Draw(points, contour, inkName);
        }

        public void Draw(Vector2[] points, (int, int)[] contour, int contourStartIndex, int contourLength,
            Vector2[] clampPolygon, string inkName)
        {
            _clampPolygon = clampPolygon;

            var points3D = new Vector3[contourLength + 1];
            for (var i = contourStartIndex; i < contourStartIndex + contourLength; i++)
            {
                points3D[i - contourStartIndex] = new Vector3(points[contour[i].Item1].x, 0, points[contour[i].Item1].y);
            }

            points3D[^1] = new Vector3(points[contour[contourStartIndex + contourLength - 1].Item2].x, 0, points[contour[contourStartIndex + contourLength - 1].Item2].y);

            _spline = BezierSplineHelper.CreateSplineSmoothPath(points3D);

            pen.Draw(points, contour, contourStartIndex, contourLength, inkName, this);

            Test();
        }

        public void OnDraw(Vector3 point, float progress)
        {
            var p = _spline.GetPosition(_spline.Parameter(progress * _spline.Length));
            transform.position = new Vector3(p.x, transform.position.y, p.z);

            var dir = (transform.position - penBall.position).normalized;
            penBall.rotation = Quaternion.LookRotation(dir);

            penBall.position = point;
        }

        public void OnDone()
        {
        }

        private void Update()
        {
            // if (smoothRotation)
            // {
            //     var dir = (transform.position - penBall.position).normalized;
            //     penBall.rotation = Quaternion.RotateTowards(penBall.rotation, Quaternion.LookRotation(dir),
            //         Time.deltaTime * 180f);
            // }
        }

        private Vector2 ProjectOnCircle(Vector2 drawPoint)
        {
            return (drawPoint - Vector2.zero).normalized * (_radius * scale);
        }

        private Vector2 ProjectOnPolygon(Vector2 drawPoint)
        {
            var rootPos = transform.position;
            var root = new Vector2(rootPos.x, rootPos.z);
            var minDistance = float.MaxValue;
            var point = Vector2.zero;
            for (var i = 0; i < _clampPolygon.Length; i++)
            {
                var p1 = _clampPolygon[i] * scale;
                var p2 = _clampPolygon[(i + 1) % _clampPolygon.Length] * scale;
                var projectedPoint = ProjectionOfPointOnSegment(drawPoint, p1, p2);
                var sqrDistance = (projectedPoint - drawPoint).sqrMagnitude;
                var sqrDistanceToRoot = (projectedPoint - root).sqrMagnitude;
                sqrDistance += sqrDistanceToRoot;
                if (sqrDistance < minDistance)
                {
                    minDistance = sqrDistance;
                    point = projectedPoint;
                }
            }

            return point;
        }

        private static Vector2 ProjectionOfPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var aToPoint = point - a;
            var aTob = b - a;

            if (aTob.sqrMagnitude > 0.001f)
            {
                var dot = Vector2.Dot(aTob, aToPoint);
                var magAToBSqr = aTob.sqrMagnitude;
                if (dot >= magAToBSqr)
                {
                    return b;
                }

                if (dot <= 0)
                {
                    return a;
                }

                return aTob * dot / magAToBSqr + a;
            }

            return Vector2.zero;
        }

        [SerializeField] private BezierSplineMono display;

        private void Test()
        {
            if (display == null) return;
            display.points = _spline._points;
            // Array.Resize(ref display.points,_spline._points.Length);
            // Array.Resize(ref display.modes, _spline.PointCount);
            display.modes = new BezierPointMode[_spline.PointCount];
            for (var i = 0; i < display.modes.Length; i++)
            {
                display.modes[i] = BezierPointMode.Aligned;
            }

            // for (var i = 0; i < _spline._points.Length; i++)
            // {
            //     display.points[i] = _spline._points[i];
            // }
        }
    }
}