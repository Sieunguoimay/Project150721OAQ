using System.Collections.Generic;
using Common.Algorithm;
using UnityEngine;

namespace Gameplay.Board.BoardDrawing
{
    public class BoardSketcher : InjectableBehaviour<BoardSketcher>
    {
        [SerializeField] private VisualPen[] pens;

        public void Sketch(Board board)
        {
            GenerateSketch(board.Metadata, out var points, out var edges);

            var contour = ConnectContour(edges);
            var n = contour.Length / board.Metadata.Polygon.Length;
            for (var i = 0; i < board.Metadata.Polygon.Length; i++)
            {
                pens[i].Draw(points, contour, i * n, n, "Board");
            }
        }

        private static void GenerateSketch(Board.BoardMetadata boardMetadata, out Vector2[] points,
            out (int, int)[] edges)
        {
            var polygon = boardMetadata.Polygon;
            var tilesPerGroup = boardMetadata.TilesPerGroup;
            var tileSize = boardMetadata.TileSize;
            var pointNum = ((tilesPerGroup + 1) * 2 + 2) * polygon.Length;
            var edgeNum = (tilesPerGroup * 2 + tilesPerGroup + 4) * polygon.Length;

            points = new Vector2[pointNum];
            edges = new (int, int)[edgeNum];

            var pCount = 0;
            var eCount = 0;
            for (var i = 0; i < polygon.Length; i++)
            {
                var point1 = polygon[i];
                var point2 = polygon[(i + 1) % polygon.Length];
                var dir = (point2 - point1).normalized;
                var normal = new Vector2(dir.y, -dir.x); //clockwise 90

                var mandarinDir = point1.normalized;
                var mandarinNormal = new Vector2(mandarinDir.y, -mandarinDir.x);
                var mandarinPoint1 = point1 + mandarinDir * tileSize + mandarinNormal * tileSize * .5f;
                var mandarinPoint2 = point1 + mandarinDir * tileSize - mandarinNormal * tileSize * .5f;

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
                    var onEdgePoint = point1 + dir * tileSize * j;
                    var offEdgePoint = point1 + dir * tileSize * j + normal * tileSize;
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
            var str = "";

            for (var i = 0; i < contour.Count; i++)
            {
                str += $"({contour[i].Item1} {contour[i].Item2}), ";
            }

            Debug.Log(str);
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

            str = "";
            for (var i = 0; i < connectedContour.Count; i++)
            {
                str += $"({connectedContour[i].Item1} {connectedContour[i].Item2}), ";
            }

            Debug.Log(str);

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