using System;
using Common.Algorithm;
using Common.Curve;
using CommonActivities;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using Gameplay.Board.BoardDrawing;
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
        [SerializeField, Min(0.1f)] private float speed = 1f;

        public ActivityQueue ActivityQueue { get; } = new();

        private struct DrawUnit
        {
            public float Start;
            public float Duration;
            public int Index;
        }

        public void Draw(Vector2[] points, (int, int)[] contour, string inkName)
        {
            Draw(points, contour, 0, contour.Length, inkName, null);
        }

        public void Draw(Vector2[] points, (int, int)[] contour, int contourStartIndex, int contourLength,
            string inkName, IDrawingPenHandler handler)
        {
            ActivityQueue.Add(new Lambda(() =>
            {
                var point = points[contour[contourStartIndex].Item1];
                drawingSurface.DrawBegin(point);
                handler.OnDraw(point, 0f);
            }, () => true));

            var n = Mathf.Min(contour.Length, contourStartIndex + contourLength);
            if (contourLength > n)
            {
                Debug.LogError("Given length is Out of bound" + contourLength + " > " + n);
            }

            var duration = 0f;

            var times = new DrawUnit[n - contourStartIndex];

            for (var i = contourStartIndex; i < n; i++)
            {
                var point1 = points[contour[i].Item1];
                var point2 = points[contour[i].Item2];

                var drawUnit = new DrawUnit
                {
                    Duration = Vector3.Distance(point1, point2) / speed,
                    Start = duration,
                    Index = i
                };
                duration += drawUnit.Duration;
                times[i - contourStartIndex] = drawUnit;
            }

            var activity = new Timer(duration, t =>
            {
                var time = times[0];
                for (var i = 0; i < times.Length; i++)
                {
                    if (t >= times[i].Start && t < times[i].Start + times[i].Duration)
                    {
                        time = times[i];
                        break;
                    }

                    if (i == times.Length - 1)
                    {
                        time = times[i];
                    }
                }

                var point1 = points[contour[time.Index].Item1];
                var point2 = points[contour[time.Index].Item2];

                var point = Vector2.Lerp(point1, point2, Mathf.Min(1f, (t - time.Start) / time.Duration));
                drawingSurface.Draw(point, lineThickness, minDistance);

                handler.OnDraw(drawingSurface.transform.TransformPoint(new Vector3(point.x, 0, point.y)), t / duration);
            });
            ActivityQueue.Add(activity);
            ActivityQueue.Add(new Lambda(() =>
            {
                drawingSurface.DryInk(inkName);
                handler?.OnDone();
            }, () => true));
            ActivityQueue.Begin();
        }

        public void DrawWithConstantSegmentDuration(Vector2[] points, (int, int)[] contour, int contourStartIndex, int contourLength,
            string inkName, IDrawingPenHandler handler)
        {
            ActivityQueue.Add(new Lambda(() =>
            {
                var point = points[contour[contourStartIndex].Item1];
                drawingSurface.DrawBegin(point);
                handler.OnDraw(point, 0f);
            }, () => true));

            var n = Mathf.Min(contour.Length, contourStartIndex + contourLength);
            if (contourLength > n)
            {
                Debug.LogError("Given length is Out of bound" + contourLength + " > " + n);
            }

            var totalLength = (n - contourStartIndex) * 1f;
            var duration = totalLength / speed;

            var activity = new Timer(duration, t =>
            {
                var index = Mathf.FloorToInt(t * speed) + contourStartIndex;
                if (index >= contour.Length) return;

                var point1 = points[contour[index].Item1];
                var point2 = points[contour[index].Item2];
                var d = 1f / speed;

                var point = Vector2.Lerp(point1, point2, Mathf.Min(1f, (t - d * (index - contourStartIndex)) / d));
                drawingSurface.Draw(point, lineThickness, minDistance);

                handler.OnDraw(drawingSurface.transform.TransformPoint(new Vector3(point.x, 0, point.y)), t / duration);
            });
            ActivityQueue.Add(activity);
            ActivityQueue.Add(new Lambda(() =>
            {
                drawingSurface.DryInk(inkName);
                handler?.OnDone();
            }, () => true));
            ActivityQueue.Begin();
        }

        public void DrawWithSpline(BezierSpline spline, string inkName, IDrawingPenHandler handler)
        {
            ActivityQueue.Add(new Lambda(() =>
            {
                var p = spline.ControlPoints[0];
                drawingSurface.DrawBegin(p);
                handler.OnDraw(p, 0f);
            }, () => true));

            var duration = 5f;

            var activity = new Timer(duration, t =>
            {
                var point3D = spline.GetPoint(t / duration);
                var point = new Vector2(point3D.x, point3D.z);
                drawingSurface.Draw(point, lineThickness, minDistance);

                handler.OnDraw(drawingSurface.transform.TransformPoint(new Vector3(point.x, 0, point.y)), t / duration);
            });
            ActivityQueue.Add(activity);
            ActivityQueue.Add(new Lambda(() =>
            {
                drawingSurface.DryInk(inkName);
                handler?.OnDone();
            }, () => true));
            ActivityQueue.Begin();
        }

        private void Update()
        {
            ActivityQueue.Update(Time.deltaTime);
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

            Draw(points, contour, "Test");
        }
#endif
    }
}