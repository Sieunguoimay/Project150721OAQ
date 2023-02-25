using Gameplay.Board;
using UnityEngine;

namespace Gameplay.Player
{
    public class PieceBench : MonoPieceContainer
    {
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
    }
}