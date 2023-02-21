using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameplay.Board
{
    public class BoardSide
    {
        public IMandarinTile MandarinTile;
        public IReadOnlyList<ICitizenTile> CitizenTiles;
    }

    public class BoardMetadata
    {
        public float TileSize;
        public int NumCitizenTilesPerSide;
        public Vector2[] Polygon;
    }

    public class Board
    {
        private readonly ITile[] _tiles;
        private readonly BoardSide[] _sides;
        public IReadOnlyList<ITile> Tiles => _tiles;
        public IReadOnlyList<BoardSide> Sides => _sides;
        public BoardMetadata Metadata { get; }

        public Board(BoardSide[] sides, ITile[] tiles, BoardMetadata metadata)
        {
            _sides = sides;
            _tiles = tiles;
            Metadata = metadata;
        }
    }

    public static class BoardCreator
    {
        public static Board CreateBoard(int numSides, int numTilesPerSide, MandarinTile mandarinTilePrefab, CitizenTile citizenTilePrefab, Transform parent)
        {
            var length = numTilesPerSide * citizenTilePrefab.Size;
            var polygon = CreatePolygon(numSides, length);

            var spawnedTiles = new ITile[numSides * (numTilesPerSide + 1)];
            var tileGroups = new BoardSide[numSides];
            for (var i = 0; i < numSides; i++)
            {
                var cornerPos = polygon[i];
                var worldPos = parent.TransformPoint(ToVector3(cornerPos + cornerPos.normalized * mandarinTilePrefab.Size / 2f));
                var worldRot = parent.rotation * Quaternion.LookRotation(ToVector3(cornerPos));

                var mandarinTile =
                    SpawnTile(spawnedTiles, mandarinTilePrefab, worldPos, worldRot, parent, i * (numTilesPerSide + 1))
                        as IMandarinTile;

                var p0 = polygon[i];
                var p1 = polygon[(i + 1) % polygon.Length];
                var dir = (p1 - p0).normalized;
                var normal = new Vector2(dir.y, -dir.x); //clockwise 90

                var citizenTiles = new ICitizenTile[numTilesPerSide];
                for (var j = 0; j < numTilesPerSide; j++)
                {
                    var pj = p0 + (j + 0.5f) * citizenTilePrefab.Size * dir;
                    worldPos = parent.TransformPoint(ToVector3(pj + normal * citizenTilePrefab.Size / 2f));
                    worldRot = parent.rotation * Quaternion.LookRotation(ToVector3(normal));
                    citizenTiles[j] =
                        SpawnTile(spawnedTiles, citizenTilePrefab, worldPos, worldRot, parent, i * (numTilesPerSide + 1) + j + 1)
                            as ICitizenTile;
                }

                tileGroups[i] = new BoardSide {MandarinTile = mandarinTile, CitizenTiles = citizenTiles};
            }

            return new Board
            (
                tileGroups,
                spawnedTiles,
                new BoardMetadata
                {
                    Polygon = polygon,
                    NumCitizenTilesPerSide = numTilesPerSide,
                    TileSize = citizenTilePrefab.Size
                }
            );
        }

        private static Tile SpawnTile(IList<ITile> container, Tile tilePrefab, Vector3 position, Quaternion rotation, Transform parent, int index)
        {
            var tile = Object.Instantiate(tilePrefab, parent);
            tile.transform.SetPositionAndRotation(position, rotation);
            tile.SetIndex(index);
            container[index] = tile;
            return tile;
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