using System;
using System.Collections.Generic;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay
{
    public class PieceBench
    {
        public List<Piece.Piece> Pieces { get; } = new();

        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly float _spacing;
        private readonly int _perRow;

        public PieceBench(Vector3 position, Quaternion rotation, float spacing, int perRow)
        {
            _position = position;
            _rotation = rotation;
            _spacing = spacing;
            _perRow = perRow;
        }

        public PosAndRot GetPosAndRot(int index)
        {
            var dirX = _rotation * Vector3.right;
            var dirY = _rotation * Vector3.forward;
            var x = index % _perRow;
            var y = index / _perRow;
            var offsetX = _spacing * x;
            var offsetY = _spacing * y;
            return new PosAndRot(_position + dirX * offsetX + dirY * offsetY, _rotation);
        }
    }
}