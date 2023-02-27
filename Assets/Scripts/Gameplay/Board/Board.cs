using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.Board
{
    public class BoardSide
    {
        public MandarinTile MandarinTile;
        public IReadOnlyList<CitizenTile> CitizenTiles;
    }

    public class BoardMetadata
    {
        public float TileSize;
        public int NumCitizenTilesPerSide;
        public Vector2[] Polygon;
    }

    public class Board
    {
        public IReadOnlyList<Tile> Tiles { get; }
        public IReadOnlyList<BoardSide> Sides { get; }
        public BoardMetadata Metadata { get; }

        public Board(IReadOnlyList<BoardSide> sides, IReadOnlyList<Tile> tiles, BoardMetadata metadata)
        {
            Sides = sides;
            Tiles = tiles;
            Metadata = metadata;
        }
    }

    public static class BoardCreator
    {
        public static Board CreateBoard(int numSides, int numTilesPerSide,
            CitizenTile citizenTilePrefab, ITileFactory tileFactory)
        {
            var length = numTilesPerSide * citizenTilePrefab.Size;
            var polygon = CreatePolygon(numSides, length);

            var spawnedTiles = new Tile[numSides * (numTilesPerSide + 1)];
            var boardSides = new BoardSide[numSides];
            for (var i = 0; i < numSides; i++)
            {
                var cornerPos = polygon[i];
                var index = i * (numTilesPerSide + 1);
                var mandarinTile = tileFactory.CreateMandarinTile(cornerPos);
                mandarinTile.SetIndex(index);
                spawnedTiles[index] = mandarinTile;

                var nextCornerPos = polygon[(i + 1) % polygon.Length];
                var dir = (nextCornerPos - cornerPos).normalized;
                var normal = new Vector2(dir.y, -dir.x); //clockwise 90

                var citizenTiles = new CitizenTile[numTilesPerSide];
                for (var j = 0; j < numTilesPerSide; j++)
                {
                    index = i * (numTilesPerSide + 1) + j + 1;
                    citizenTiles[j] = tileFactory.CreateCitizenTile(cornerPos, normal * 0.5f, dir * (j + 0.5f));
                    citizenTiles[j].SetIndex(index);
                    spawnedTiles[index] = citizenTiles[j];
                }

                boardSides[i] = new BoardSide {MandarinTile = mandarinTile, CitizenTiles = citizenTiles};
            }

            return new Board
            (
                boardSides,
                spawnedTiles,
                new BoardMetadata
                {
                    Polygon = polygon,
                    NumCitizenTilesPerSide = numTilesPerSide,
                    TileSize = citizenTilePrefab.Size
                }
            );
        }

        public static Vector3 ToVector3(Vector2 v) => new(v.x, 0, v.y);

        private static Vector2[] CreatePolygon(int vertexNum, float edgeLength = 1f)
        {
            if (vertexNum < 2) return new[] {Vector2.zero};
            var alpha = (180f - 360f / vertexNum) / 2f;
            var h = edgeLength / 2f * Mathf.Tan(Mathf.Deg2Rad * alpha);

            var p0 = Vector2.left * edgeLength / 2f - Vector2.up * h;
            var p1 = Vector2.right * edgeLength / 2f - Vector2.up * h;

            var vertices = new Vector2[vertexNum];

            vertices[0] = p0;
            vertices[1] = p1;

            var a = 360f / vertexNum;
            var cosA = Mathf.Cos(Mathf.Deg2Rad * a);
            var sinA = Mathf.Sin(Mathf.Deg2Rad * a);

            for (var i = 2; i < vertexNum; i++)
            {
                vertices[i].x = cosA * vertices[i - 1].x - sinA * vertices[i - 1].y;
                vertices[i].y = sinA * vertices[i - 1].x + cosA * vertices[i - 1].y;
            }

            return vertices;
        }
    }
}