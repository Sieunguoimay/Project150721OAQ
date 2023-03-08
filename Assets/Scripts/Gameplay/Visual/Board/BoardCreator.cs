using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Visual.Board
{
    public class BoardCreator : BaseGenericDependencyInversionUnit<BoardCreator>
    {
        [SerializeField] private MandarinTile mandarinTilePrefab;
        [SerializeField] private CitizenTile citizenTilePrefab;
        [SerializeField] private float tileSize;

        private IReadOnlyList<Vector2> _polygon;
        private int _numTilesPerSide;

        public static void DeleteBoard(BoardVisual boardVisual)
        {
            foreach (var tile in boardVisual.Tiles)
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
            AppendIndexToTiles(allTiles);
            boardVisual.SetReferences(boardSides, allTiles, boardMetadata);

            _polygon = null;
        }

        private BoardSideVisual[] SpawnBoardSides(Transform parent)
        {
            var boardSides = new BoardSideVisual[_polygon.Count];
            var tileSpawner = new TileSpawner(citizenTilePrefab, mandarinTilePrefab, parent, tileSize);

            for (var i = 0; i < _polygon.Count; i++)
            {
                tileSpawner.SetCornersPos(_polygon[i], _polygon[(i + 1) % _polygon.Count]);

                var mandarinTile = tileSpawner.SpawnMandarinTile();

                var citizenTiles = tileSpawner.SpawnCitizenTiles(_numTilesPerSide);

                boardSides[i] = CreateBoardSide(mandarinTile, citizenTiles);
            }

            return boardSides;
        }

        private static BoardSideVisual CreateBoardSide(MandarinTile mandarinTile, IReadOnlyList<CitizenTile> citizenTiles)
        {
            return new() {MandarinTile = mandarinTile, CitizenTiles = citizenTiles};
        }

        private static Tile[] CreateAllTilesArray(IEnumerable<BoardSideVisual> boardSides)
        {
            return boardSides.SelectMany(s => new Tile[] {s.MandarinTile}.Concat(s.CitizenTiles)).ToArray();
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

        private static void AppendIndexToTiles(IReadOnlyList<Tile> allTiles)
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
        private readonly CitizenTile _citizenTilePrefab;
        private readonly MandarinTile _mandarinTilePrefab;
        private readonly Transform _parent;

        public TileSpawner(CitizenTile citizenTilePrefab, MandarinTile mandarinTilePrefab, Transform parent, float tileSize)
        {
            _citizenTilePrefab = citizenTilePrefab;
            _mandarinTilePrefab = mandarinTilePrefab;
            _parent = parent;
            _tileSize = tileSize;
        }

        public void SetCornersPos(Vector2 cornerPos, Vector2 nextCornerPos)
        {
            _cornerPos = cornerPos;
            _nextCornerPos = nextCornerPos;
        }

        public MandarinTile SpawnMandarinTile()
        {
            var mandarinTile = UnityEngine.Object.Instantiate(_mandarinTilePrefab, _parent);
            UpdateMandarinTilePosition(mandarinTile.transform);
            return mandarinTile;
        }

        public CitizenTile[] SpawnCitizenTiles(int numTiles)
        {
            var dir = (_nextCornerPos - _cornerPos).normalized;
            var normal = new Vector2(dir.y, -dir.x); //clockwise 90

            var citizenTiles = new CitizenTile[numTiles];

            for (var i = 0; i < numTiles; i++)
            {
                var citizenTile = UnityEngine.Object.Instantiate(_citizenTilePrefab, _parent);

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

        private void UpdateCitizenTilePosition(Transform tileTransform, Vector2 cornerPos, Vector2 verticalOffset, Vector2 horizontalOffset)
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