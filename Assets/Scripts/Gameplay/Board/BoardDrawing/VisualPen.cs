using System;
using Common.DrawLine;
using UnityEngine;

namespace Gameplay.Board.BoardDrawing
{
    public class VisualPen : MonoBehaviour
    {
        [SerializeField] private DrawingPen pen;
        [SerializeField] private Transform penBall;
        [SerializeField] private float height = 5f;
        [SerializeField] private float scale = 0.5f;

        private Vector2[] _clampPolygon;

        private void OnEnable()
        {
            pen.OnDraw += OnDraw;
        }

        private void OnDisable()
        {
            pen.OnDraw -= OnDraw;
        }

        public void Draw(Vector2[] points, (int, int)[] contour, Vector2[] clampPolygon)
        {
            _clampPolygon = clampPolygon;
            pen.Draw(points, contour);
        }

        private void OnDraw(Vector3 point)
        {
            var root = Vector3.up * (height + transform.position.y);
            var dir = (point - root).normalized;
            var l = height * point.magnitude / (height + transform.position.y);
            var offset = Mathf.Sqrt(l * l + height * height);

            var pos = root + dir * offset;
            var center = new Vector3(0, transform.position.y, 0);
            var line = pos - center;
            var clamped = ClampPoint(center, dir.normalized);
            transform.position = clamped;

            dir = (clamped - point).normalized;

            penBall.position = point;
            penBall.rotation = Quaternion.LookRotation(dir);
        }

        private Vector3 ClampPoint(Vector3 center, Vector3 dir)
        {
            for (var i = 0; i < _clampPolygon.Length; i++)
            {
                var p1 = _clampPolygon[i] * scale;
                var p2 = _clampPolygon[i] * scale;
            }

            return Vector3.zero;//center + dir * radius;
        }
    }
}