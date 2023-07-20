using System;
using System.Collections.Generic;
using System.Linq;
using Framework.DependencyInversion;
using Gameplay.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Visual.Board
{
    public class BoardVisualCreator : MonoBehaviour
    {
        [FormerlySerializedAs("mandarinTilePrefab")] [SerializeField]
        private MandarinTileVisual mandarinTileVisualPrefab;

        [FormerlySerializedAs("citizenTilePrefab")] [SerializeField]
        private CitizenTileVisual citizenTileVisualPrefab;

        [SerializeField] private float tileSize;

        private IReadOnlyList<Vector2> _polygon;
        private int _numTilesPerSide;

        public static void DeleteBoard(BoardVisual boardVisual)
        {
            foreach (var tile in boardVisual.TileVisuals)
            {
                Destroy(tile.gameObject);
            }

            Destroy(boardVisual.gameObject);
        }

        public BoardVisual CreateBoard(int numSides, int tilesPerSide)
        {
            _numTilesPerSide = tilesPerSide;

            var board = CreateBoardGameObject();

            GenerateBoardReferences(numSides, board);

            return board;
        }

        private BoardVisual CreateBoardGameObject()
        {
            var board = new GameObject("Board").AddComponent<BoardVisual>();
            var t = board.transform;
            t.SetParent(transform);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            return board;
        }

        private void GenerateBoardReferences(int numSides, BoardVisual boardVisual)
        {
            _polygon = new BoardPolygonGenerator(numSides, _numTilesPerSide * tileSize).CreatePolygon();

            var boardSides = SpawnBoardSides(boardVisual.transform);
            var boardMetadata = CreateMetadata();
            var allTiles = CreateAllTilesArray(boardSides);
            var pockets = boardSides.Select(s => CreatePieceBench(s, null)).ToArray();
            AppendIndexToTiles(allTiles);
            boardVisual.SetReferences(boardSides, allTiles, pockets, boardMetadata);

            _polygon = null;
        }

        private static PieceBench CreatePieceBench(BoardSideVisual boardSide, Transform transform)
        {
            var pos1 = boardSide.CitizenTiles[0].transform.position;
            var pos2 = boardSide.CitizenTiles[^1].transform.position;
            var diff = pos2 - pos1;
            var pos = pos1 + new Vector3(diff.z, diff.y, -diff.x) * 0.5f;
            var rot = Quaternion.LookRotation(pos1 - pos, Vector3.up);

            var b = new GameObject($"{nameof(PieceBench)}").AddComponent<PieceBench>();
            b.SetArrangement(0.25f, 15);

            var t = b.transform;
            t.SetParent(transform);
            t.position = pos;
            t.rotation = rot;
            return b;
        }

        private BoardSideVisual[] SpawnBoardSides(Transform parent)
        {
            var boardSides = new BoardSideVisual[_polygon.Count];
            var tileSpawner = new TileSpawner(citizenTileVisualPrefab, mandarinTileVisualPrefab, parent, tileSize);

            for (var i = 0; i < _polygon.Count; i++)
            {
                tileSpawner.SetCornersPos(_polygon[i], _polygon[(i + 1) % _polygon.Count]);

                var mandarinTile = tileSpawner.SpawnMandarinTile();

                var citizenTiles = tileSpawner.SpawnCitizenTiles(_numTilesPerSide);

                boardSides[i] = CreateBoardSide(mandarinTile, citizenTiles);
            }

            return boardSides;
        }

        private static BoardSideVisual CreateBoardSide(MandarinTileVisual mandarinTileVisual,
            IReadOnlyList<CitizenTileVisual> citizenTiles)
        {
            return new() {MandarinTileVisual = mandarinTileVisual, CitizenTiles = citizenTiles};
        }

        private static TileVisual[] CreateAllTilesArray(IEnumerable<BoardSideVisual> boardSides)
        {
            return boardSides.SelectMany(s => new TileVisual[] {s.MandarinTileVisual}.Concat(s.CitizenTiles)).ToArray();
        }

        private BoardMetadata CreateMetadata()
        {
            return new()
            {
                Polygon = _polygon,
                NumCitizenTilesPerSide = _numTilesPerSide,
                TileSize = tileSize
            };
        }

        private static void AppendIndexToTiles(IReadOnlyList<TileVisual> allTiles)
        {
            for (var i = 0; i < allTiles.Count; i++)
            {
                allTiles[i].SetIndex(i);
            }
        }
    }

    public class BoardPolygonGenerator
    {
        private readonly int _vertexNum;
        private readonly float _edgeLength;

        public BoardPolygonGenerator(int vertexNum, float edgeLength = 1f)
        {
            _vertexNum = vertexNum;
            _edgeLength = edgeLength;
        }

        public Vector2[] CreatePolygon()
        {
            if (_vertexNum < 2) return new[] {Vector2.zero};
            var alpha = (180f - 360f / _vertexNum) / 2f;
            var h = _edgeLength / 2f * Mathf.Tan(Mathf.Deg2Rad * alpha);

            var p0 = Vector2.left * _edgeLength / 2f - Vector2.up * h;
            var p1 = Vector2.right * _edgeLength / 2f - Vector2.up * h;

            var vertices = new Vector2[_vertexNum];

            vertices[0] = p0;
            vertices[1] = p1;

            var a = 360f / _vertexNum;
            var cosA = Mathf.Cos(Mathf.Deg2Rad * a);
            var sinA = Mathf.Sin(Mathf.Deg2Rad * a);

            for (var i = 2; i < _vertexNum; i++)
            {
                vertices[i].x = cosA * vertices[i - 1].x - sinA * vertices[i - 1].y;
                vertices[i].y = sinA * vertices[i - 1].x + cosA * vertices[i - 1].y;
            }

            return vertices;
        }
    }

    public class TileSpawner
    {
        private Vector2 _cornerPos;
        private Vector2 _nextCornerPos;
        private readonly float _tileSize;
        private readonly CitizenTileVisual _citizenTileVisualPrefab;
        private readonly MandarinTileVisual _mandarinTileVisualPrefab;
        private readonly Transform _parent;

        public TileSpawner(CitizenTileVisual citizenTileVisualPrefab, MandarinTileVisual mandarinTileVisualPrefab,
            Transform parent, float tileSize)
        {
            _citizenTileVisualPrefab = citizenTileVisualPrefab;
            _mandarinTileVisualPrefab = mandarinTileVisualPrefab;
            _parent = parent;
            _tileSize = tileSize;
        }

        public void SetCornersPos(Vector2 cornerPos, Vector2 nextCornerPos)
        {
            _cornerPos = cornerPos;
            _nextCornerPos = nextCornerPos;
        }

        public MandarinTileVisual SpawnMandarinTile()
        {
            var mandarinTile = UnityEngine.Object.Instantiate(_mandarinTileVisualPrefab, _parent);
            UpdateMandarinTilePosition(mandarinTile.transform);
            return mandarinTile;
        }

        public CitizenTileVisual[] SpawnCitizenTiles(int numTiles)
        {
            var dir = (_nextCornerPos - _cornerPos).normalized;
            var normal = new Vector2(dir.y, -dir.x); //clockwise 90

            var citizenTiles = new CitizenTileVisual[numTiles];

            for (var i = 0; i < numTiles; i++)
            {
                var citizenTile = UnityEngine.Object.Instantiate(_citizenTileVisualPrefab, _parent);

                UpdateCitizenTilePosition(citizenTile.transform, _cornerPos, normal * 0.5f, dir * (i + 0.5f));

                citizenTiles[i] = citizenTile;
            }

            return citizenTiles;
        }

        private void UpdateMandarinTilePosition(Transform tileTransform)
        {
            var parent = tileTransform.parent;

            var localPos = _cornerPos + _cornerPos.normalized * _tileSize / 2f;
            var worldPos = parent.TransformPoint(ToVector3(localPos));
            var worldRot = parent.rotation * Quaternion.LookRotation(ToVector3(_cornerPos));

            tileTransform.SetPositionAndRotation(worldPos, worldRot);
        }

        private void UpdateCitizenTilePosition(Transform tileTransform, Vector2 cornerPos, Vector2 verticalOffset,
            Vector2 horizontalOffset)
        {
            var parent = tileTransform.parent;

            var localPos = cornerPos + verticalOffset * _tileSize + horizontalOffset * _tileSize;
            var worldPos = parent.TransformPoint(ToVector3(localPos));
            var worldRot = parent.rotation * Quaternion.LookRotation(ToVector3(verticalOffset));

            tileTransform.SetPositionAndRotation(worldPos, worldRot);
        }

        private static Vector3 ToVector3(Vector2 v) => new(v.x, 0, v.y);
    }
}