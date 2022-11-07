using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public class BoardManager : MonoInjectable<BoardManager>
    {
        [SerializeField] private MandarinTile mandarinTilePrefab;
        [SerializeField] private Tile citizenTilePrefab;
        [field: System.NonSerialized] public Board Board { get; } = new();
        [field: System.NonSerialized] public Tile[] SpawnedTiles { get; private set; }

        public void ResetAll()
        {
            foreach (var t in Board.Tiles)
            {
                t.Pieces.Clear();
            }
        }

        public void CreateBoard(int groupNum, int tilesPerGroup)
        {
            var length = tilesPerGroup * citizenTilePrefab.Size;
            var polygon = CreatePolygon(groupNum, length);

            if (Application.isPlaying)
            {
                SpawnedTiles = new Tile[polygon.Length * (tilesPerGroup + 1)];
                var tileGroups = new Board.TileGroup[groupNum];
                for (var i = 0; i < polygon.Length; i++)
                {
                    var cornerPos = polygon[i];

                    SpawnedTiles[i * (tilesPerGroup + 1)] = SpawnMandarinTile(tileGroups, i, tilesPerGroup,
                        ToVector3(cornerPos + cornerPos.normalized * mandarinTilePrefab.Size / 2f),
                        Quaternion.LookRotation(ToVector3(cornerPos)));

                    var p0 = polygon[i];
                    var p1 = polygon[(i + 1) % polygon.Length];
                    var dir = (p1 - p0).normalized;
                    var normal = new Vector2(dir.y, -dir.x); //clockwise 90

                    for (var j = 0; j < tilesPerGroup; j++)
                    {
                        var pj = p0 + (j + 0.5f) * citizenTilePrefab.Size * dir;
                        SpawnedTiles[i * (tilesPerGroup + 1) + j] = SpawnCitizenTile(tileGroups, i, j,
                            ToVector3(pj + normal * citizenTilePrefab.Size / 2f),
                            Quaternion.LookRotation(ToVector3(normal)));
                    }
                }

                Board.SetGroups(tileGroups);
            }

            Board.SetMetadata(new Board.BoardMetadata
            {
                Polygon = polygon,
                TilesPerGroup = tilesPerGroup,
                TileSize = citizenTilePrefab.Size
            });
        }

        private Tile SpawnMandarinTile(IList<Board.TileGroup> tileGroups, int i, int tilesPerGroup, Vector3 position,
            Quaternion rotation)
        {
            var mandarinTile = Instantiate(mandarinTilePrefab, transform);
            mandarinTile.Setup();
            mandarinTile.transform.SetPositionAndRotation(position, rotation);

            tileGroups[i] = new Board.TileGroup
                {MandarinTile = mandarinTile, Tiles = new Tile[tilesPerGroup]};
            return mandarinTile;
        }

        private Tile SpawnCitizenTile(IList<Board.TileGroup> tileGroups, int i, int j, Vector3 position,
            Quaternion rotation)
        {
            var citizenTile = Instantiate(citizenTilePrefab, transform);
            citizenTile.Setup();
            citizenTile.transform.SetPositionAndRotation(position, rotation);
            tileGroups[i].Tiles[j] = citizenTile;
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