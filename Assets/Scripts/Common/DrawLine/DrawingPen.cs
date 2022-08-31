using System;
using Common.Algorithm;
using CommonActivities;
using UnityEngine;

namespace Common.DrawLine
{
    public class DrawingPen : MonoBehaviour
    {
        [SerializeField] private DrawingSurface drawingSurface;
        [SerializeField, Min(0.05f)] private float lineThickness = 0.1f;
        [SerializeField, Min(0.05f)] private float minDistance = 0.1f;
        [SerializeField, Min(0.1f)] private float speed = 1f;

        public ActivityQueue ActivityQueue { get; } = new();
        public event Action<Vector3> OnDraw;

        public void Draw(Vector2[] points, (int, int)[] contour)
        {
            ActivityQueue.Add(new Lambda(() =>
            {
                var point = points[contour[0].Item1];
                drawingSurface.DrawBegin(point);
                InvokeOnDraw(point);
            }, () => true));

            for (var i = 0; i < contour.Length; i++)
            {
                var point1 = points[contour[i].Item1];
                var point2 = points[contour[i].Item2];
                var duration = Vector3.Distance(point1, point2) / speed;
                var activity = new Timer(duration, t =>
                {
                    var point = Vector2.Lerp(point1, point2, Mathf.Min(1f, t / duration));
                    drawingSurface.Draw(point, lineThickness, minDistance);
                    InvokeOnDraw(point);
                });
                ActivityQueue.Add(activity);
            }

            ActivityQueue.Add(new Lambda(() => { drawingSurface.DryInk("Board"); }, () => true));
            ActivityQueue.Begin();
        }

        private void Update()
        {
            ActivityQueue.Update(Time.deltaTime);
        }

        protected virtual void InvokeOnDraw(Vector2 point)
        {
            OnDraw?.Invoke(drawingSurface.transform.TransformPoint(new Vector3(point.x, 0, point.y)));
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

            Draw(points, contour);
        }
#endif
    }
}