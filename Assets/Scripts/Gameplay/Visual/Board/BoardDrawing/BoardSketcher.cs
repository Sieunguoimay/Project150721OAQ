﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common.Algorithm;
using Common.DrawLine;
using Framework.DependencyInversion;
using UnityEngine;

namespace Gameplay.Visual.Board.BoardDrawing
{
    public class BoardSketcher : MonoBehaviour
    {
        [SerializeField] private VisualPen[] pens;
        [SerializeField] private DrawingSurface[] surfaces;

        private Vector2[] _points;
        private (int, int)[] _contour;

        public void Sketch(BoardVisual boardVisual)
        {
            GenerateSketch(boardVisual.Metadata, out var points, out var edges);
            var contour = ConnectContour(edges);

            _points = points;
            _contour = contour;
            PenUsageNum = boardVisual.Metadata.Polygon.Count;
        }

        public void DeleteDrawing()
        {
            _points = null;
            _contour = null;
            foreach (var pen in pens)
            {
                pen.ResetAll();
            }

            foreach (var surface in surfaces)
            {
                surface.ResetAll();
            }
        }

        public void StartDrawing(float initialSpeed)
        {
            for (var i = 0; i < PenUsageNum; i++)
            {
                StartPenDrawing(i, initialSpeed);
            }
        }

        public void StartPenDrawing(int index, float initialSpeed)
        {
            var n = _contour.Length / PenUsageNum;
            pens[index].Draw(_points, _contour, index * n, n, surfaces[index], "Board", initialSpeed);
        }

        public IReadOnlyList<Vector2> Points => _points;

        [field: System.NonSerialized] public int PenUsageNum { get; private set; }

        public VisualPen[] Pens => pens;

        public DrawingSurface[] Surfaces => surfaces;

        private static void GenerateSketch(BoardMetadata boardMetadata, out Vector2[] points, out (int, int)[] edges)
        {
            var polygon = boardMetadata.Polygon;
            var tilesPerGroup = boardMetadata.NumCitizenTilesPerSide;
            var tileSize = boardMetadata.TileSize;
            var pointNum = ((tilesPerGroup + 1) * 2 + 2) * polygon.Count;
            var edgeNum = (tilesPerGroup * 2 + tilesPerGroup + 4) * polygon.Count;

            points = new Vector2[pointNum];
            edges = new (int, int)[edgeNum];

            var pCount = 0;
            var eCount = 0;
            for (var i = 0; i < polygon.Count; i++)
            {
                var point1 = polygon[i];
                var point2 = polygon[(i + 1) % polygon.Count];
                var dir = (point2 - point1).normalized;
                var normal = new Vector2(dir.y, -dir.x); //clockwise 90

                var mandarinDir = point1.normalized;
                var mandarinNormal = new Vector2(mandarinDir.y, -mandarinDir.x);
                var mandarinPoint1 = point1 + mandarinDir * tileSize + mandarinNormal * (tileSize * .5f);
                var mandarinPoint2 = point1 + mandarinDir * tileSize - mandarinNormal * (tileSize * .5f);

                points[pCount] = mandarinPoint1;
                points[pCount + 1] = mandarinPoint2;

                edges[eCount++] = (pCount, pCount + 1);
                edges[eCount++] = (pCount + 1, pCount + 2 * tilesPerGroup + 2 + 1);
                edges[eCount++] = (pCount + 2 * tilesPerGroup + 2 + 1, pCount + 2);

                pCount += 2;

                edges[eCount + tilesPerGroup] = (pCount + tilesPerGroup, pCount + tilesPerGroup + 1);
                var count = 0;
                for (var j = 0; j < tilesPerGroup + 1; j++)
                {
                    var onEdgePoint = point1 + dir * (tileSize * j);
                    var offEdgePoint = point1 + dir * (tileSize * j) + normal * tileSize;
                    points[pCount + j] = onEdgePoint;
                    points[pCount + 2 * (tilesPerGroup + 1) - j - 1] = offEdgePoint;

                    if (j < tilesPerGroup)
                    {
                        edges[eCount + j] = (pCount + j, pCount + j + 1);
                        edges[eCount + tilesPerGroup + 1 + j] = (pCount + tilesPerGroup + 1 + j,
                            pCount + tilesPerGroup + 1 + j + 1);
                    }

                    if (j < tilesPerGroup)
                    {
                        if (count % 2 == 1)
                        {
                            edges[eCount + 2 * tilesPerGroup + j] =
                                (pCount + 2 * (tilesPerGroup + 1) - j - 1, pCount + j);
                        }
                        else
                        {
                            edges[eCount + 2 * tilesPerGroup + j] =
                                (pCount + j, pCount + 2 * (tilesPerGroup + 1) - j - 1);
                        }

                        count++;
                    }
                }

                pCount += (tilesPerGroup + 1) * 2;
                eCount += tilesPerGroup * 3;
                edges[eCount++] = (pCount - tilesPerGroup - 1, (pCount) % pointNum);
            }

            // var str = points.Aggregate("", (current, p) => current + (p + ", "));
            // Debug.Log(str);
            //
            // str = edges.Aggregate("", (current, p) => current + (p + ", "));
            // Debug.Log(str);
        }

        private static (int, int)[] ConnectContour(IList<(int, int)> contour)
        {
            var connectedContour = new List<(int, int)>();
            // var str = "";
            //
            // for (var i = 0; i < contour.Count; i++)
            // {
            //     str += $"({contour[i].Item1} {contour[i].Item2}), ";
            // }

            // Debug.Log(str);
            for (var i = 0; i < contour.Count; i++)
            {
                connectedContour.Add(contour[i]);

                if (i == contour.Count - 1) break;

                var item2 = contour[i].Item2;

                var nextItem1 = contour[i + 1].Item1;
                if (item2 == nextItem1)
                {
                }
                else
                {
                    connectedContour.Add((item2, nextItem1));
                }
            }

            // str = "";
            // for (var i = 0; i < connectedContour.Count; i++)
            // {
            //     str += $"({connectedContour[i].Item1} {connectedContour[i].Item2}), ";
            // }

            // Debug.Log(str);

            return connectedContour.ToArray();
        }
#if UNITY_EDITOR

        [ContextMenu("Test AntColonyOptimization")]
        private void Test()
        {
            AntColonyOptimization.Test();
        }
#endif
    }
}