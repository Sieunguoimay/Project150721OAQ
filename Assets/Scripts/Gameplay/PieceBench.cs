using System;
using System.Collections.Generic;
using Gameplay.Piece;
using SNM;
using UnityEngine;

namespace Gameplay
{
    public class PieceBench : MonoBehaviour
    {
        public List<Piece.Piece> Pieces { get; } = new();

        private float _spacing;
        private int _perRow;

        public void SetArrangement(float spacing, int perRow)
        {
            _spacing = spacing;
            _perRow = perRow;
        }

        public PosAndRot GetPosAndRot(int index)
        {
            var t = transform;
            var rotation1 = t.rotation;
            var dirX = rotation1 * Vector3.right;
            var dirY = rotation1 * Vector3.forward;
            var x = index % _perRow;
            var y = index / _perRow;
            var offsetX = _spacing * x;
            var offsetY = _spacing * y;
            return new PosAndRot(t.position + dirX * offsetX + dirY * offsetY, rotation1);
        }
    }
}