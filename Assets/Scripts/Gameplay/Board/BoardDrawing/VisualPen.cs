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

        private float _radius;
        private BezierSpline _spline;

        public void Draw(Vector2[] points, (int, int)[] contour, float radius, string inkName)
        {
            _radius = radius;

            pen.Draw(points, contour, inkName);
        }

        public void Draw(Vector2[] points, (int, int)[] contour, string inkName)
        {
            pen.Draw(points, contour, inkName);
        }

        public void Draw(Vector2[] points, (int, int)[] contour, int contourStartIndex, int contourLength, string inkName)
        {
            var points3D = new Vector3[contourLength + 1];
            for (var i = contourStartIndex; i < contourStartIndex + contourLength; i++)
            {
                points3D[i - contourStartIndex] =
                    new Vector3(points[contour[i].Item1].x, 0, points[contour[i].Item1].y);
            }

            points3D[^1] = new Vector3(points[contour[contourStartIndex + contourLength - 1].Item2].x, 0,
                points[contour[contourStartIndex + contourLength - 1].Item2].y);

            _spline = BezierSplineUtility.CreateSplineSmoothPath(points3D);
            pen.Draw(points, contour, contourStartIndex, contourLength, inkName, this);
            // pen.DrawWithConstantSegmentDuration(points, contour, contourStartIndex, contourLength, inkName, this);
            // pen.DrawWithSpline(_spline, inkName, this);
        }

        public void OnDraw(Vector3 point, float progress)
        {
            var p = _spline.GetPoint(progress);
            var p3 = new Vector3(p.x, transform.position.y, p.z);

            transform.position = p3;
            penBall.position = point;
            penBall.rotation = Quaternion.LookRotation(p3 - point);
        }

        public void OnDone()
        {
        }

        private Vector2 ProjectOnCircle(Vector2 drawPoint)
        {
            return (drawPoint - Vector2.zero).normalized * (_radius * scale);
        }

        // private Vector2 ProjectOnPolygon(Vector2 drawPoint)
        // {
        //     var rootPos = transform.position;
        //     var root = new Vector2(rootPos.x, rootPos.z);
        //     var minDistance = float.MaxValue;
        //     var point = Vector2.zero;
        //     for (var i = 0; i < _clampPolygon.Length; i++)
        //     {
        //         var p1 = _clampPolygon[i] * scale;
        //         var p2 = _clampPolygon[(i + 1) % _clampPolygon.Length] * scale;
        //         var projectedPoint = ProjectionOfPointOnSegment(drawPoint, p1, p2);
        //         var sqrDistance = (projectedPoint - drawPoint).sqrMagnitude;
        //         var sqrDistanceToRoot = (projectedPoint - root).sqrMagnitude;
        //         sqrDistance += sqrDistanceToRoot;
        //         if (sqrDistance < minDistance)
        //         {
        //             minDistance = sqrDistance;
        //             point = projectedPoint;
        //         }
        //     }
        //
        //     return point;
        // }
        //
        // private static Vector2 ProjectionOfPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
        // {
        //     var aToPoint = point - a;
        //     var aTob = b - a;
        //
        //     if (aTob.sqrMagnitude > 0.001f)
        //     {
        //         var dot = Vector2.Dot(aTob, aToPoint);
        //         var magAToBSqr = aTob.sqrMagnitude;
        //         if (dot >= magAToBSqr)
        //         {
        //             return b;
        //         }
        //
        //         if (dot <= 0)
        //         {
        //             return a;
        //         }
        //
        //         return aTob * dot / magAToBSqr + a;
        //     }
        //
        //     return Vector2.zero;
        // }
    }
}