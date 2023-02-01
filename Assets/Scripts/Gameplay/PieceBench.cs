using System;
using System.Collections.Generic;
using Gameplay.Board;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay
{
    public class PieceBench : MonoBehaviour, IPieceContainer
    {
        private readonly List<Piece.Piece> _pieces = new();

        private float _spacing;
        private int _perRow;

        public void SetArrangement(float spacing, int perRow)
        {
            _spacing = spacing;
            _perRow = perRow;
        }

        public void GetPosAndRot(int index, out Vector3 pos, out Quaternion rot)
        {
            var t = transform;
            var rotation1 = t.rotation;
            var dirX = rotation1 * Vector3.right;
            var dirY = rotation1 * Vector3.forward;
            var x = index % _perRow;
            var y = index / _perRow;
            var offsetX = _spacing * x;
            var offsetY = _spacing * y;
            pos = t.position + dirX * offsetX + dirY * offsetY;
            rot = rotation1;
        }

        public IReadOnlyList<Piece.Piece> HeldPieces => _pieces;

        public void AddPiece(Piece.Piece piece)
        {
            _pieces.Add(piece);
        }

        public void RemoveLast()
        {
            if (_pieces.Count > 0)
            {
                _pieces.RemoveAt(_pieces.Count - 1);
            }
        }

        public void Sort(Comparison<Piece.Piece> comparison)
        {
            _pieces.Sort(comparison);
        }

        public void Clear()
        {
            _pieces.Clear();
        }
    }
}