using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    public interface ITile : IPieceContainer
    {
        float Size { get; }
        IEnumerable<ISelectionAdaptor> GetSelectionAdaptors();
        Vector3 GetPositionInFilledCircle(int index, bool local = false, float space = 0.15f);
        Transform Transform { get; }
        PieceType TargetPieceType { get; }
    }

    [SelectionBase]
    public class Tile : MonoBehaviour, ITile
    {
        [SerializeField] private float size;
        [SerializeField] private PieceType targetPieceType;

        public float Size => size;
        private readonly List<Piece.Piece> _heldPieces = new();
        public IReadOnlyList<Piece.Piece> HeldPieces => _heldPieces;

        public void AddPiece(Piece.Piece piece)
        {
            _heldPieces.Add(piece);
        }

        public void RemoveLast()
        {
            if (_heldPieces.Count > 0)
            {
                _heldPieces.RemoveAt(_heldPieces.Count - 1);
            }
        }

        public void Sort(Comparison<Piece.Piece> comparison)
        {
            _heldPieces.Sort(comparison);
        }

        public void Clear()
        {
            _heldPieces.Clear();
        }

        private const int MaxPiecesSupported = 50;
        private Vector2Int[] _reservedPoints;
        private List<Piece.Piece> _heldPieces1;

        public PieceType TargetPieceType => targetPieceType;

        public IEnumerable<ISelectionAdaptor> GetSelectionAdaptors() =>
            HeldPieces.Where(p => p is Citizen)
                .Select(p => new CitizenToTileSelectorAdaptor(p as Citizen));

        public void RuntimeSetup()
        {
            _reservedPoints = ReservePositionsInFilledCircle(MaxPiecesSupported);
        }

        public Vector3 GetPositionInFilledCircle(int index, bool local = false, float space = 0.15f)
        {
            if (index >= _reservedPoints.Length)
            {
                _reservedPoints = ReservePositionsInFilledCircle(_reservedPoints.Length + MaxPiecesSupported);
            }

            var pos = new Vector3(_reservedPoints[index].x, 0, _reservedPoints[index].y) * space;
            return local ? pos : transform.TransformPoint(pos);
        }

        public Transform Transform => transform;

        private static Vector2Int[] ReservePositionsInFilledCircle(int num)
        {
            var r = 1;
            var n = 0;
            var points = new List<Vector2Int>();
            while (n < num)
            {
                n = 0;
                points.Clear();
                for (var x = -r; x <= r; x++)
                {
                    for (var y = -r; y <= r; y++)
                    {
                        if (x * x + y * y > r * r) continue;

                        points.Add(new Vector2Int(x, y));
                        n++;
                    }
                }

                r++;
            }

            points.Sort((a, b) =>
            {
                var da = a.x * a.x + a.y * a.y;
                var db = b.x * b.x + b.y * b.y;
                return (da == db ? 0 : (da < db ? -1 : 1));
            });

            return points.ToArray();
        }
    }

    public interface ISelectionAdaptor
    {
        void OnTileSelected();
        void OnTileDeselected(bool success);
    }
}