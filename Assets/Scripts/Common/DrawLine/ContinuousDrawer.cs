using System.Collections.Generic;
using System.Linq;
using Common.Algorithm;
using UnityEngine;

namespace Common.DrawLine
{
    public class ContinuousDrawer : MonoBehaviour
    {
        [SerializeField] private DrawingPen pen;

        public void Draw(Vector2[] points, (int, int)[] edges, bool shouldConnect = false)
        {
            // var contour = SwapEdges(GenerateContour(points, (edges)));
            // if (shouldConnect)
            // {
            //     contour = ConnectContour(contour);
            // }

            if (Application.isPlaying)
            {
                pen.Draw(points, edges);
            }
        }

        public void DrawAll(IList<((int, int)[], Vector2[])> sentence, int index = 0)
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

        public static (int, int)[] GenerateContour(Vector2[] points, (int, int)[] edges)
        {
            var n = edges.Length;
            var matrix = GenerateWeightMatrix(points, edges);
            // var solution = new TravelingSalesmanBranchAndBound().Solve(matrix, n);
            var solution = AntColonyOptimization.Solve(matrix, n, 6);
            var contour = new (int, int)[solution.Length];
            for (var i = 0; i < solution.Length; i++)
            {
                contour[i] = edges[solution[i]];
            }

            return contour;
        }

        private static (int, int)[] ConnectContour((int, int)[] contour)
        {
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

                if (i == contour.Length - 1) break;

                var item2 = contour[i].Item2;

                var nextItem1 = contour[i + 1].Item1;
                if (item2 == nextItem1)
                {
                }
                else
                {
                    var nextItem2 = contour[i + 1].Item2;
                    if (item2 == nextItem2)
                    {
                        contour[i + 1].Item1 = nextItem2;
                        contour[i + 1].Item2 = nextItem1;
                    }
                    else
                    {
                        var shouldSwap = true;
                        for (var j = 0; j < connectedContour.Count; j++)
                        {
                            if (connectedContour[j].Item1 == contour[i].Item2 && connectedContour[j].Item2 == contour[i].Item1)
                            {
                                shouldSwap = false;
                            }
                        }

                        if (shouldSwap)
                        {
                            contour[i].Item2 = contour[i].Item1;
                            contour[i].Item1 = item2;
                            i--;
                        }
                    }
                }
            }

            str = "";
            for (var i = 0; i < connectedContour.Count; i++)
            {
                str += $"({connectedContour[i].Item1} {connectedContour[i].Item2}), ";
            }

            Debug.Log(str);

            return connectedContour.ToArray();
        }

        private static (int, int)[] SwapEdges((int, int)[] contour)
        {
            for (var i = 0; i < contour.Length; i++)
            {
                var item1 = contour[i].Item1;
                var item2 = contour[i].Item2;

                var nextIndex = Mod(i + 1, contour.Length);
                var nextItem1 = contour[nextIndex].Item1;
                var nextItem2 = contour[nextIndex].Item2;

                var prevIndex = Mod(i - 1, contour.Length);
                var prevItem1 = contour[prevIndex].Item1;
                var prevItem2 = contour[prevIndex].Item2;

                if (item1 == prevItem2 || item1 == prevItem1 || item2 == nextItem1 || item2 == nextItem2) continue;
                if (item1 == nextItem1 || item1 == nextItem2 || item2 == prevItem1 || item2 == prevItem2)
                {
                    contour[i].Item1 = item2;
                    contour[i].Item2 = item1;
                }
            }

            return contour;

            static int Mod(int x, int m)
            {
                var r = x % m;
                return r < 0 ? r + m : r;
            }
        }

        private static int[][] GenerateWeightMatrix(Vector2[] points, IList<(int, int)> edges)
        {
            var n = edges.Count;
            var matrix = new int[n][];
            var str = "";
            for (var i = 0; i < n; i++)
            {
                matrix[i] = new int[n];
                str += i + "\t";
                for (var j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        matrix[i][j] = int.MaxValue;
                    }
                    else
                    {
                        var connected =
                            edges[i].Item1 == edges[j].Item1 ||
                            edges[i].Item1 == edges[j].Item2 ||
                            edges[i].Item2 == edges[j].Item1 ||
                            edges[i].Item2 == edges[j].Item2;
                        matrix[i][j] = connected ? (int) Mathf.Max(1f, (Vector2.Distance(points[edges[i].Item2], points[edges[j].Item1]) * 10f)) : int.MaxValue;
                    }

                    str += (matrix[i][j] == int.MaxValue ? "-" : $"{matrix[i][j]}") + " ";
                }

                str += "\n";
            }

            Debug.Log(str);

            return matrix;
        }

#if UNITY_EDITOR

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
                    (2, 4), (2, 3), (4, 0), (1, 2), (3, 4), (0, 1),
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
#endif
    }
}