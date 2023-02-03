using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Board
{
    public interface ITile : IPieceContainer
    {
        float Size { get; }
        Vector3 GetPositionInFilledCircle(int index, bool local = false);
        Transform Transform { get; }
    }

    [SelectionBase]
    public class Tile : MonoPieceContainer, ITile
    {
        [SerializeField] private float size;

        public float Size => size;

        private const int MaxPiecesSupported = 50;
        private Vector2Int[] _reservedPoints;

        public void RuntimeSetup()
        {
            _reservedPoints = ReservePositionsInFilledCircle(MaxPiecesSupported);
        }

        public virtual Vector3 GetPositionInFilledCircle(int index, bool local = false)
        {
            if (index >= _reservedPoints.Length)
            {
                _reservedPoints = ReservePositionsInFilledCircle(_reservedPoints.Length + MaxPiecesSupported);
            }

            var pos = new Vector3(_reservedPoints[index].x, 0, _reservedPoints[index].y) * .15f;
            return local ? pos : transform.TransformPoint(pos);
        }

        public Transform Transform => transform;

        private Vector2Int GetNextPosition()
        {
            
            return Vector2Int.one;
        }
            
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
                return da == db ? 0 : da < db ? -1 : 1;
            });

            return points.ToArray();
        }
    }

}