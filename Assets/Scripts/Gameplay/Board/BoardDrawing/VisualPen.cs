using System;
using Common.Algorithm;
using Common.DrawLine;
using UnityEngine;

namespace Gameplay.Board.BoardDrawing
{
    public class VisualPen : MonoBehaviour
    {
        [SerializeField] private DrawingPen pen;
        [SerializeField] private Transform penBall;
        [SerializeField] private float scale = 0.5f;
        [SerializeField] private bool smoothRotation = true;

        private float _radius;
        private Vector2[] _clampPolygon;
        private Func<Vector2, Vector2> _projection;

        private void OnEnable()
        {
            pen.OnDraw += OnDraw;
        }

        private void OnDisable()
        {
            pen.OnDraw -= OnDraw;
        }

        public void Draw(Vector2[] points, (int, int)[] contour, float radius, string inkName)
        {
            _radius = radius;
            _projection = ProjectOnCircle;

            pen.Draw(points, contour, inkName);
        }

        public void Draw(Vector2[] points, (int, int)[] contour, Vector2[] clampPolygon, string inkName)
        {
            _clampPolygon = clampPolygon;
            _projection = ProjectOnPolygon;

            pen.Draw(points, contour, inkName);
        }


        private void OnDraw(Vector3 point)
        {
            if (_projection == null) return;

            var clamped = _projection.Invoke(new Vector2(point.x, point.z));
            transform.position = new Vector3(clamped.x, transform.position.y, clamped.y);
            if (!smoothRotation)
            {
                var dir = (transform.position - penBall.position).normalized;
                penBall.rotation = Quaternion.LookRotation(dir);
            }

            penBall.position = point;
        }

        private void Update()
        {
            if (smoothRotation)
            {
                var dir = (transform.position - penBall.position).normalized;
                penBall.rotation = Quaternion.RotateTowards(penBall.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 180f);
            }
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
    }
}