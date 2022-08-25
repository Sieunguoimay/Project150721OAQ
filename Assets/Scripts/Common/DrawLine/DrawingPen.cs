using System;
using Common.Algorithm;
using CommonActivities;
using UnityEngine;

namespace Common.DrawLine
{
    public class DrawingPen : MonoBehaviour
    {
        [SerializeField] private DrawingSurface drawingSurface;

        private readonly ActivityQueue _activityQueue = new();

        public void Draw(Vector2[] points, (int, int)[] contour)
        {
            _activityQueue.Add(new Lambda(() => { drawingSurface.DrawBegin(points[contour[0].Item1]); }, () => true));
            for (var i = 0; i < contour.Length; i++)
            {
                var point1 = points[contour[i].Item1];
                var point2 = points[contour[i].Item2];
                _activityQueue.Add(new Timer(1, t => { drawingSurface.Draw(Vector2.Lerp(point1, point2, t)); }));
            }

            _activityQueue.Begin();
        }

        private void Update()
        {
            _activityQueue.Update(Time.deltaTime);
        }

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
            var tsp = new TravelingSalesman();
            var solution = tsp.Solve(adjacencyMatrix, 5);

            foreach (var item1 in solution)
            {
                Debug.Log((item1));
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
    }
}