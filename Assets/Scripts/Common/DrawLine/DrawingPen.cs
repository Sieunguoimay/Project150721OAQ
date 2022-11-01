using System;
using Common.Algorithm;
using Common.Curve;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using Gameplay.Board.BoardDrawing;
using Gameplay.Piece.Activities;
using SNM.Easings;
using UnityEngine;

namespace Common.DrawLine
{
    public interface IDrawingPenHandler
    {
        void OnDraw(Vector3 point, float progress);
        void OnDone();
    }

    public class DrawingPen : MonoBehaviour
    {
        [SerializeField] private DrawingSurface drawingSurface;
        [SerializeField, Min(0.05f)] private float lineThickness = 0.1f;
        [SerializeField, Min(0.05f)] private float minDistance = 0.1f;
        [SerializeField, Min(0.1f)] private float initialSpeed = 1f;
        [SerializeField, Min(0.1f)] private float speed = 1f;

        [field: System.NonSerialized] public ActivityQueue ActivityQueue { get; } = new();
        private IDrawingSurface _targetSurface;

        private struct DrawUnit
        {
            public float Start;
            public float Length;
            public int Index;
        }

        public void SetupDrawActivity(Vector2[] points, (int, int)[] contour, IDrawingSurface surface, string inkName, float initialSpeed)
        {
            SetupDrawActivity(points, contour, 0, contour.Length, surface, inkName, null, initialSpeed);
        }

        public void SetupDrawActivity(Vector2[] points, (int, int)[] contour, int contourStartIndex, int contourLength, IDrawingSurface surface,
            string inkName, IDrawingPenHandler handler, float initialSpeed)
        {
            _targetSurface = surface ?? drawingSurface;

            ActivityQueue.Add(new ActivityCallback(() =>
            {
                var point = points[contour[contourStartIndex].Item1];
                _targetSurface.DrawBegin(point);
                handler.OnDraw(_targetSurface.Get3DPoint(point), 0f);
            }));

            var n = Mathf.Min(contour.Length, contourStartIndex + contourLength);
            if (contourLength > n)
            {
                Debug.LogError("Given length is Out of bound" + contourLength + " > " + n);
            }

            var totalLength = 0f;

            var drawUnits = new DrawUnit[n - contourStartIndex];

            for (var i = contourStartIndex; i < n; i++)
            {
                var point1 = points[contour[i].Item1];
                var point2 = points[contour[i].Item2];

                var drawUnit = new DrawUnit
                {
                    Length = Vector3.Distance(point1, point2),
                    Start = totalLength,
                    Index = i
                };
                totalLength += drawUnit.Length;
                drawUnits[i - contourStartIndex] = drawUnit;
            }

            ActivityQueue.Add(new ActivityDrawing(this, drawUnits, points, contour, handler, totalLength, initialSpeed));
            ActivityQueue.Add(new ActivityCallback(() =>
            {
                _targetSurface.DryInk(inkName);
                handler?.OnDone();
            }));
        }

        public void DrawWithConstantSegmentDuration(Vector2[] points, (int, int)[] contour, int contourStartIndex,
            int contourLength, string inkName, IDrawingPenHandler handler)
        {
            ActivityQueue.Add(new ActivityCallback(() =>
            {
                var point = points[contour[contourStartIndex].Item1];
                _targetSurface.DrawBegin(point);
                handler.OnDraw(point, 0f);
            }));

            var n = Mathf.Min(contour.Length, contourStartIndex + contourLength);
            if (contourLength > n)
            {
                Debug.LogError("Given length is Out of bound" + contourLength + " > " + n);
            }

            var totalLength = (n - contourStartIndex) * 1f;
            var duration = totalLength / speed;

            var activity = new ActivityTimer(duration, t =>
            {
                var index = Mathf.FloorToInt(t * speed) + contourStartIndex;
                if (index >= contour.Length) return;

                var point1 = points[contour[index].Item1];
                var point2 = points[contour[index].Item2];
                var d = 1f / speed;

                var point = Vector2.Lerp(point1, point2, Mathf.Min(1f, (t - d * (index - contourStartIndex)) / d));
                _targetSurface.Draw(point, lineThickness, minDistance);

                handler.OnDraw(_targetSurface.Get3DPoint(point), t / duration);
            });
            ActivityQueue.Add(activity);
            ActivityQueue.Add(new Lambda(() =>
            {
                _targetSurface.DryInk(inkName);
                handler?.OnDone();
            }, () => true));
            ActivityQueue.Begin();
        }

        public void DrawWithSpline(BezierSpline spline, string inkName, IDrawingPenHandler handler)
        {
            ActivityQueue.Add(new ActivityCallback(() =>
            {
                var p = spline.ControlPoints[0];
                _targetSurface.DrawBegin(p);
                handler.OnDraw(p, 0f);
            }));

            var duration = 5f;

            var activity = new ActivityTimer(duration, t =>
            {
                var point3D = spline.GetPoint(t / duration);
                var point = new Vector2(point3D.x, point3D.z);
                _targetSurface.Draw(point, lineThickness, minDistance);

                handler.OnDraw(_targetSurface.Get3DPoint(point), t / duration);
            });
            ActivityQueue.Add(activity);
            ActivityQueue.Add(new ActivityCallback(() =>
            {
                _targetSurface.DryInk(inkName);
                handler?.OnDone();
            }));
            ActivityQueue.Begin();
        }

        private void Update()
        {
            ActivityQueue.Update(Time.deltaTime);
        }

        private class ActivityDrawing : Activity
        {
            private readonly DrawingPen _pen;
            private readonly DrawUnit[] _drawUnits;
            private readonly Vector2[] _points;
            private readonly (int, int)[] _contour;
            private readonly IDrawingPenHandler _handler;
            private readonly float _totalLength;
            private float _distance;
            private float _speed;

            public ActivityDrawing(DrawingPen pen, DrawUnit[] drawUnits, Vector2[] points, (int, int)[] contour,
                IDrawingPenHandler handler, float totalLength, float initialSpeed)
            {
                _pen = pen;
                _drawUnits = drawUnits;
                _points = points;
                _contour = contour;
                _handler = handler;
                _totalLength = totalLength;
                _distance = 0f;
                _speed = initialSpeed;
            }

            public override void Update(float deltaTime)
            {
                if (_speed < _pen.speed)
                {
                    _speed += deltaTime * _pen.speed;
                }

                _distance += deltaTime * _speed;

                var time = _drawUnits[0];
                for (var i = 0; i < _drawUnits.Length; i++)
                {
                    if (_distance >= _drawUnits[i].Start && _distance < _drawUnits[i].Start + _drawUnits[i].Length)
                    {
                        time = _drawUnits[i];
                        break;
                    }

                    if (i == _drawUnits.Length - 1)
                    {
                        time = _drawUnits[i];
                    }
                }

                var point1 = _points[_contour[time.Index].Item1];
                var point2 = _points[_contour[time.Index].Item2];

                var point = Vector2.Lerp(point1, point2, Mathf.Min(1f, (_distance - time.Start) / time.Length));
                var pointWorld = _pen._targetSurface.Get3DPoint(point);

                _pen._targetSurface.Draw(point, _pen.lineThickness, _pen.minDistance);
                _handler.OnDraw(pointWorld, _distance / _totalLength);

                if (_distance >= _totalLength)
                {
                    MarkAsDone();
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test")]
        private void Test()
        {
            var adjacencyMatrix = new[]
            {
                new[] {int.MaxValue, 20, 30, 10, 11},
                new[] {15, int.MaxValue, 16, 4, 2},
                new[] {3, 5, int.MaxValue, 2, 4},
                new[] {19, 6, 18, int.MaxValue, 3},
                new[] {16, 4, 7, 16, int.MaxValue}
            };
            var tsp = new TravelingSalesmanBranchAndBound();
            var solution = tsp.Solve(adjacencyMatrix, 5);

            foreach (var item1 in solution)
            {
                Debug.Log(item1);
            }
        }

        [ContextMenu("TestDraw")]
        private void TestDraw()
        {
            var points = new[]
            {
                new Vector2(-1f, -1f),
                new Vector2(-1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, -1f),
            };
            var contour = new[]
            {
                (0, 1),
                (1, 2),
                (2, 3),
                (3, 0),
                (0, 2),
            };

            SetupDrawActivity(points, contour, null, "Test", initialSpeed);
        }
#endif
    }
}