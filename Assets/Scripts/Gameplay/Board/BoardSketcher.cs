using System.Linq;
using Common.Algorithm;
using Common.DrawLine;
using UnityEngine;

namespace Gameplay.Board
{
    public class BoardSketcher : MonoBehaviour
    {
        [SerializeField] private ContinuousDrawer drawer;

        [ContextMenu("Test")]
        private void Test()
        {
            AntColonyOptimization.Test();
        }

        public void Sketch(Board board)
        {
            GenerateSketch(board.Metadata, out var points, out var edges);

            var str = points.Aggregate("", (current, p) => current + (p + ", "));
            Debug.Log(str);

            str = edges.Aggregate("", (current, p) => current + (p + ", "));
            Debug.Log(str);
            drawer.Draw(points, edges);
        }

        private static void GenerateSketch(Board.BoardMetadata boardMetadata, out Vector2[] points, out (int, int)[] edges)
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
                edges[eCount++] = (pCount + 1, pCount + 3);

                pCount += 2;

                for (var j = 0; j < tilesPerGroup + 1; j++)
                {
                    var onEdgePoint = point1 + dir * tileSize * j;
                    var offEdgePoint = point1 + dir * tileSize * j + normal * tileSize;
                    points[pCount] = onEdgePoint;
                    points[pCount + 1] = offEdgePoint;

                    edges[eCount++] = (pCount, pCount + 1);
                    
                    if (j < tilesPerGroup)
                    {
                        edges[eCount++] = (pCount + 1, pCount + 3);
                        edges[eCount++] = (pCount, pCount + 2);
                    }

                    pCount += 2;
                }

                edges[eCount++] = (pCount - 1, (pCount) % pointNum);
            }
        }
    }
}