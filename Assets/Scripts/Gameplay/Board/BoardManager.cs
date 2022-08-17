using System;
using System.Collections.Generic;
using System.Linq;
using Common.ResolveSystem;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private Tile mandarinTilePrefab;
        [SerializeField] private Tile citizenTilePrefab;
        public Board Board { get; } = new();

        public void SetBoardByTileGroupNum(int tileGroupNum, int tilesPerGroup)
        {
            Board.SetGroups(CreateBoard(tileGroupNum, tilesPerGroup));
        }

        private Board.TileGroup[] CreateBoard(int groupNum, int tilesPerGroup)
        {
            var tileGroups = new Board.TileGroup[groupNum];
            var length = tilesPerGroup * citizenTilePrefab.Size;
            var polygon = CreatePolygon(groupNum, length);
            for (var i = 0; i < polygon.Length; i++)
            {
                var cornerPos = polygon[i];
                var mandarinTile = Instantiate(mandarinTilePrefab, transform);
                mandarinTile.Setup();
                mandarinTile.transform.SetPositionAndRotation(ToVector3(cornerPos + cornerPos.normalized * mandarinTilePrefab.Size / 2f), Quaternion.LookRotation(ToVector3(cornerPos)));

                tileGroups[i] = new Board.TileGroup {MandarinTile = mandarinTile, Tiles = new IPieceHolder[tilesPerGroup]};

                var p0 = polygon[i];
                var p1 = polygon[(i + 1) % polygon.Length];
                var dir = (p1 - p0).normalized;
                var normal = ((p0 + p1) / 2f).normalized;

                for (var j = 0; j < tilesPerGroup; j++)
                {
                    var pj = p0 + (j + 0.5f) * citizenTilePrefab.Size * dir;
                    var citizenTile = Instantiate(citizenTilePrefab, transform);
                    citizenTile.Setup();
                    citizenTile.transform.SetPositionAndRotation(ToVector3(pj + normal * citizenTilePrefab.Size / 2f), Quaternion.LookRotation(ToVector3(normal)));

                    tileGroups[i].Tiles[j] = citizenTile;
                }
            }

            return tileGroups;
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
                Debug.Log(vertices[i]);
            }

            return vertices;
        }
#if UNITY_EDITOR
        [ContextMenu("Test")]
        private void Test() => SetBoardByTileGroupNum(5, 5);
#endif
    }
}