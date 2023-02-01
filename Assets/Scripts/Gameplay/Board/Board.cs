using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.Board
{
    public class Board
    {
        public ITile[] Tiles { get; }
        public BoardSide[] Sides { get; }
        public BoardMetadata Metadata { get; }

        public Board(BoardSide[] sides, ITile[] tiles, BoardMetadata metadata)
        {
            Sides = sides;
            Tiles = tiles;
            Metadata = metadata;
        }

        public ITile GetSuccessTile(ITile tile, bool forward)
        {
            var index = BoardTraveller.MoveNext(Array.IndexOf(Tiles, tile), Tiles.Length, forward);
            return Tiles[index];
        }

        public class BoardSide
        {
            public ITile MandarinTile;
            public ITile[] CitizenTiles;
        }

        public class BoardMetadata
        {
            public float TileSize;
            public int NumCitizenTilesPerSide;
            public Vector2[] Polygon;
        }
    }

    public static class BoardCreator
    {
        public static Board CreateBoard(int numSides, int numTilesPerSide, Tile mandarinTilePrefab, Tile citizenTilePrefab, Transform parent)
        {
            var length = numTilesPerSide * citizenTilePrefab.Size;
            var polygon = CreatePolygon(numSides, length);

            var spawnedTiles = new ITile[numSides * (numTilesPerSide + 1)];
            var tileGroups = new Board.BoardSide[numSides];
            for (var i = 0; i < polygon.Length; i++)
            {
                var cornerPos = polygon[i];
                var worldPos = parent.TransformPoint(ToVector3(cornerPos + cornerPos.normalized * mandarinTilePrefab.Size / 2f));
                var worldRot = parent.rotation * Quaternion.LookRotation(ToVector3(cornerPos));

                var mandarinTile = SpawnTile(mandarinTilePrefab, worldPos, worldRot, parent);
                tileGroups[i] = new Board.BoardSide {MandarinTile = mandarinTile, CitizenTiles = new ITile[numTilesPerSide]};
                spawnedTiles[i * (numTilesPerSide + 1)] = mandarinTile;

                var p0 = polygon[i];
                var p1 = polygon[(i + 1) % polygon.Length];
                var dir = (p1 - p0).normalized;
                var normal = new Vector2(dir.y, -dir.x); //clockwise 90

                for (var j = 0; j < numTilesPerSide; j++)
                {
                    var pj = p0 + (j + 0.5f) * citizenTilePrefab.Size * dir;
                    worldPos = parent.TransformPoint(ToVector3(pj + normal * citizenTilePrefab.Size / 2f));
                    worldRot = parent.rotation * Quaternion.LookRotation(ToVector3(normal));

                    var citizenTile = SpawnTile(citizenTilePrefab, worldPos, worldRot, parent);
                    spawnedTiles[i * (numTilesPerSide + 1) + j + 1] = citizenTile;
                    tileGroups[i].CitizenTiles[j] = citizenTile;
                }
            }

            return new Board
            (
                tileGroups,
                spawnedTiles,
                new Board.BoardMetadata
                {
                    Polygon = polygon,
                    NumCitizenTilesPerSide = numTilesPerSide,
                    TileSize = citizenTilePrefab.Size
                }
            );
        }

        private static Tile SpawnTile(Tile citizenTilePrefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            var citizenTile = Object.Instantiate(citizenTilePrefab, parent);
            citizenTile.RuntimeSetup();
            citizenTile.transform.SetPositionAndRotation(position, rotation);
            return citizenTile;
        }

        private static Vector3 ToVector3(Vector2 v) => new(v.x, 0, v.y);

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