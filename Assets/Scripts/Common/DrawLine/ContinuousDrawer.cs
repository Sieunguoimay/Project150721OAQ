using System.Collections.Generic;
using System.Linq;
using Common.Algorithm;
using UnityEngine;

namespace Common.DrawLine
{
    public class ContinuousDrawer : MonoBehaviour
    {
        [SerializeField] private DrawingPen pen;

        [ContextMenu("Test")]
        private void Test()
        {
            var edges = new[]
            {
                (0, 7), (0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6), (6, 7), (1, 6), (2, 5)
            };

            var contour = GenerateContour(edges);
            var str = "";
            for (var i = 0; i < contour.Length; i++)
            {
                str += $"({contour[i].Item1} {contour[i].Item2}), ";
            }

            Debug.Log(str);
        }

        public static (int, int)[] GenerateContour((int, int)[] edges)
        {
            var n = edges.Length;
            var matrix = GenerateWeightMatrix(edges);
            var solution = new TravelingSalesman().Solve(matrix, n);
            var contour = new (int, int)[solution.Length];
            for (var i = 0; i < solution.Length; i++)
            {
                contour[i] = edges[solution[i]];
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
                        matrix[i][j] = connected > 0 ? edges.Count(e => e != edges[i] && e.Item1 == edges[i].Item1 || e.Item1 == edges[i].Item2 || e.Item2 == edges[i].Item1 || e.Item2 == edges[i].Item2) : int.MaxValue;
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