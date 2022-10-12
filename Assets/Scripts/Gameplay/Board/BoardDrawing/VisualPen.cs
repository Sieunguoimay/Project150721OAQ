using System;
using Common.Algorithm;
using Common.Curve;
using Common.DrawLine;
using UnityEngine;

namespace Gameplay.Board.BoardDrawing
{
    public class VisualPen : DrawingPen, IDrawingPenHandler
    {
        [SerializeField] private Transform penBall;

        private BezierSplineWithDistance _spline;

        public event Action<VisualPen> Done;

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

            _spline = new BezierSplineWithDistance(BezierSplineUtility.CreateSplineSmoothPath(points3D));

            Draw(points, contour, contourStartIndex, contourLength, inkName, this);
        }

        public void OnDraw(Vector3 point, float progress)
        {
            var p = _spline.GetPointAtDistance(progress * _spline.ArcLength);
            var p3 = new Vector3(p.x, transform.position.y, p.z);

            transform.position = p3;
            if (penBall == null) return;
            penBall.position = point;
            // penBall.rotation = Quaternion.LookRotation(p3 - point);
            penBall.up = (p3 - point).normalized;
        }

        public void OnDone()
        {
            Done?.Invoke(this);
        }

        public void SetPenBall(Transform tr)
        {
            penBall = tr;
        }
    }
}