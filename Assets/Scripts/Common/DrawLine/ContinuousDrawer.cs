using System.Collections.Generic;
using System.Linq;
using Common.Algorithm;
using UnityEngine;

namespace Common.DrawLine
{
    public class ContinuousDrawer : MonoBehaviour
    {
        [SerializeField] private DrawingPen pen;

        public void Draw(Vector2[] points, (int, int)[] edges)
        {
            var contour = GenerateContour(edges);
            var connectedContour = new List<(int, int)>();
            var str = "";

            for (var i = 0; i < contour.Length; i++)
            {
                str += $"({contour[i].Item1} {contour[i].Item2}), ";
            }

            Debug.Log(str);

            for (var i = 0; i < contour.Length; i++)
            {
                connectedContour.Add(contour[i]);

                var item2 = contour[i].Item2;

                var nextItem1 = contour[(i + 1) % contour.Length].Item1;
                if (item2 == nextItem1)
                {
                }
                else
                {
                    var nextItem2 = contour[(i + 1) % contour.Length].Item2;
                    if (item2 == nextItem2)
                    {
                        contour[(i + 1) % contour.Length].Item1 = nextItem2;
                        contour[(i + 1) % contour.Length].Item2 = nextItem1;
                    }
                    else
                    {
                        contour[i].Item2 = contour[i].Item1;
                        contour[i].Item1 = item2;
                        i--;
                    }
                }
            }

            str = "";
            for (var i = 0; i < connectedContour.Count; i++)
            {
                str += $"({connectedContour[i].Item1} {connectedContour[i].Item2}), ";
            }

            Debug.Log(str);

            if (Application.isPlaying)
            {
                pen.Draw(points, connectedContour.ToArray());
            }
        }

        [ContextMenu("Test")]
        private void Test()
        {
            var edges = new[]
            {
                (0, 7), (0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6), (6, 7), (1, 6), (2, 5)
            };
            var points = new[]
            {
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(2, 1), new Vector2(3, 1),
                new Vector2(3, 0), new Vector2(2, 0), new Vector2(1, 0), new Vector2(0, 0),
            };
            Draw(points, edges);
        }

        [ContextMenu("Test2")]
        private void Test2()
        {
            var edges = new[]
            {
                (0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 0),
            };
            var points = new[]
            {
                new Vector2(0, 0), new Vector2(0, 3), new Vector2(1, 3), new Vector2(2, 2),
                new Vector2(2, 1), new Vector2(1, 0),
            };
            Draw(points, edges);
        }

        [ContextMenu("Test3")]
        private void Test3()
        {
            var sentence = new[]
            {
                (new[]
                {
                    (1, 2), (0, 1), (2, 3), (3, 4), (4, 0), (2, 4),
                }, new[]
                {
                    new Vector2(0, 0),
                    new Vector2(6, 0),
                    new Vector2(6, 5),
                    new Vector2(3, 8),
                    new Vector2(0, 5),
                }),
            };
            DrawAll(sentence, 0);
        }

        private void DrawAll(IList<((int, int)[], Vector2[])> sentence, int index)
        {
            if (index >= sentence.Count) return;

            Draw(sentence[index].Item2, sentence[index].Item1);

            void Done()
            {
                DrawAll(sentence, index + 1);
                pen.ActivityQueue.Done -= Done;
            }

            pen.ActivityQueue.Done += Done;
        }

        public static (int, int)[] GenerateContour((int, int)[] edges)
        {
            var n = edges.Length;
            var matrix = GenerateWeightMatrix(edges);
            var solution = new TravelingSalesman().Solve(matrix, n);
            var contour = new (int, int)[solution.Count];
            for (var i = 0; i < solution.Count; i++)
            {
                contour[i] = edges[solution[i].Item1];
            }

            return contour;
        }

        private static int[][] GenerateWeightMatrix(IList<(int, int)> edges)
        {
            var n = edges.Count;
            var matrix = new int[n][];
            var str = "   0 1 2 3 4 5 6 7\n";
            for (var i = 0; i < n; i++)
            {
                matrix[i] = new int[n];
                str += i + ": ";
                for (var j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        matrix[i][j] = int.MaxValue;
                    }
                    else
                    {
                        var connected = 0;
                        if (edges[i].Item1 == edges[j].Item1) connected++;
                        if (edges[i].Item1 == edges[j].Item2) connected++;
                        if (edges[i].Item2 == edges[j].Item1) connected++;
                        if (edges[i].Item2 == edges[j].Item2) connected++;
                        matrix[i][j] = connected > 0
                            ? edges.Count(e =>
                                e != edges[i] && e.Item1 == edges[i].Item1 || e.Item1 == edges[i].Item2 ||
                                e.Item2 == edges[i].Item1 || e.Item2 == edges[i].Item2)
                            : int.MaxValue;
                    }

                    str += (matrix[i][j] == int.MaxValue ? "X" : $"{matrix[i][j]}") + " ";
                }

                str += "\n";
            }

            // Debug.Log(str);

            return matrix;
        }
    }
}