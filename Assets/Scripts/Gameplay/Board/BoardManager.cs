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
            Board.SetTiles(CreateBoard(tileGroupNum, tilesPerGroup).Select(t => t as IPieceHolder).ToArray());
        }

        private IEnumerable<Tile> CreateBoard(int groupNum, int tilesPerGroup)
        {
            var tiles = new Tile[groupNum * tilesPerGroup + groupNum];
            var length = tilesPerGroup * citizenTilePrefab.Size;
            var polygon = CreatePolygon(groupNum, length);
            for (var i = 0; i < polygon.Length; i++)
            {
                var mandarinTile = Instantiate(mandarinTilePrefab, transform);
                mandarinTile.transform.SetPositionAndRotation(ToVector3(polygon[i]), Quaternion.identity);
                tiles[i * tilesPerGroup] = mandarinTile;
                Debug.Log(i * tilesPerGroup);
                var p0 = polygon[i];
                var p1 = polygon[(i + 1) % polygon.Length];
                var dir = (p1 - p0).normalized;
                for (var j = 0; j < tilesPerGroup; j++)
                {
                    var pj = p0 + (j + 0.5f) * citizenTilePrefab.Size * dir;
                    var citizenTile = Instantiate(citizenTilePrefab, transform);
                    citizenTile.transform.SetPositionAndRotation(ToVector3(pj), Quaternion.identity);
                    tiles[i * tilesPerGroup + j+1] = citizenTile;
                    Debug.Log(i * tilesPerGroup + j+1);
                }
            }

            return tiles;
        }

        private static Vector3 ToVector3(Vector2 v) => new Vector3(v.x, 0, v.y);

        [Serializable]
        private class BoardMetaData
        {
            public int groupNum;
            public int tilesPerGroup;
            public GameObject prefab;
        }

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

        // private Vector3[] _testArray;
        //
        // [ContextMenu("Test")]
        // public void Test()
        // {
        //     var arr = CreatePolygon(5, 1);
        //     _testArray = new Vector3[arr.Length];
        //     for (var i = 0; i < arr.Length; i++)
        //     {
        //         _testArray[i] = Vector3.zero;
        //         _testArray[i].x = arr[i].x;
        //         _testArray[i].z = arr[i].y;
        //     }
        // }

        // private void OnDrawGizmos()
        // {
        //     if (_testArray == null) return;
        //
        //     var point = _testArray[0];
        //     for (var i = 1; i < _testArray.Length; i++)
        //     {
        //         Gizmos.DrawLine(point, _testArray[i]);
        //         point = _testArray[i];
        //     }
        //
        //     Gizmos.DrawLine(point, _testArray[0]);
        // }
    }
}