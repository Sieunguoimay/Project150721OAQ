using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.Algorithm
{
    public class TravelingSalesman
    {
        private int _n;

        private class Node
        {
            public List<(int, int)> Path;
            public int[][] MatrixReduced;
            public int Cost;
            public int Vertex;
            public int Level;
        }

        public List<(int, int)> Solve(int[][] adjacentMatrix, int n)
        {
            _n = n;
            var priorityQueue = new List<Node>();
            var path = new List<(int, int)>();
            var root = NewNode(adjacentMatrix, path, 0, -1, 0);
            root.Cost = CostCalculation(root.MatrixReduced);
            Enqueue(priorityQueue, root);

            while (priorityQueue.Count > 0)
            {
                var min = priorityQueue.First();
                priorityQueue.RemoveAt(0);

                var i = min.Vertex;

                if (min.Level == _n - 1)
                {
                    min.Path.Add((i, 0));
                    return min.Path;
                }

                for (var j = 0; j < _n; j++)
                {
                    if (min.MatrixReduced[i][j] != int.MaxValue)
                    {
                        var child = NewNode(min.MatrixReduced, min.Path, min.Level + 1, i, j);
                        child.Cost = min.Cost + min.MatrixReduced[i][j] + CostCalculation(child.MatrixReduced);
                        Enqueue(priorityQueue, child);
                    }
                }
            }

            return null;
        }

        private int CostCalculation(IReadOnlyList<int[]> matrixReduced)
        {
            var cost = 0;
            var row = new int[_n];
            ReduceRow(matrixReduced, row);

            var col = new int[_n];
            ReduceCol(matrixReduced, col);

            for (var i = 0; i < _n; i++)
            {
                cost += (row[i] != int.MaxValue) ? row[i] : 0;
                cost += (col[i] != int.MaxValue) ? col[i] : 0;
            }

            return cost;
        }

        private void ReduceRow(IReadOnlyList<int[]> matrixReduced, int[] row)
        {
            Array.Fill(row, int.MaxValue, 0, _n);
            for (var i = 0; i < _n; i++)
            {
                for (var j = 0; j < _n; j++)
                {
                    if (matrixReduced[i][j] < row[i])
                    {
                        row[i] = matrixReduced[i][j];
                    }
                }
            }

            for (var i = 0; i < _n; i++)
            {
                for (var j = 0; j < _n; j++)
                {
                    if (matrixReduced[i][j] != int.MaxValue && row[i] != int.MaxValue)
                    {
                        matrixReduced[i][j] -= row[i];
                    }
                }
            }
        }

        private void ReduceCol(IReadOnlyList<int[]> matrixReduced, int[] col)
        {
            Array.Fill(col, int.MaxValue, 0, _n);
            for (var i = 0; i < _n; i++)
            {
                for (var j = 0; j < _n; j++)
                {
                    if (matrixReduced[i][j] < col[j])
                    {
                        col[j] = matrixReduced[i][j];
                    }
                }
            }

            for (var i = 0; i < _n; i++)
            {
                for (var j = 0; j < _n; j++)
                {
                    if (matrixReduced[i][j] != int.MaxValue && col[j] != int.MaxValue)
                    {
                        matrixReduced[i][j] -= col[j];
                    }
                }
            }
        }

        private Node NewNode(IReadOnlyList<int[]> matrixParent, IEnumerable<(int, int)> path, int level, int i, int j)
        {
            var node = new Node {Path = new List<(int, int)>(path)};

            if (level != 0)
            {
                node.Path.Add((i, j));
            }

            node.MatrixReduced = new int[_n][];

            for (var a = 0; a < _n; a++)
            {
                node.MatrixReduced[a] = new int[_n];
                for (var b = 0; b < _n; b++)
                {
                    node.MatrixReduced[a][b] = matrixParent[a][b];
                }
            }

            for (var k = 0; level != 0 && k < _n; k++)
            {
                node.MatrixReduced[i][k] = int.MaxValue;
                node.MatrixReduced[k][j] = int.MaxValue;
            }

            node.MatrixReduced[j][0] = int.MaxValue;
            node.Level = level;
            node.Vertex = j;
            return node;
        }

        private static void Enqueue(List<Node> queue, Node node)
        {
            queue.Add(node);
            queue.Sort((a, b) => a.Cost > b.Cost ? 1 : -1);
        }
    }
}