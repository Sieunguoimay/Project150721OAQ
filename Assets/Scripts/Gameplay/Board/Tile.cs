using System.Collections.Generic;
using System.Linq;
using Gameplay.Piece;
using UnityEngine;

namespace Gameplay.Board
{
    [SelectionBase]
    public class Tile : PieceContainer
    {
        [SerializeField] private float size;
        public float Size => size;

        private const int MaxPiecesSupported = 50;
        private Vector2Int[] _reservedPoints;

        public IEnumerable<ISelectionAdaptor> GetSelectionAdaptors() =>
            Pieces.Where(p => p is Citizen)
                .Select(p => new CitizenToTileSelectorAdaptor(p));

        public void Setup()
        {
            _reservedPoints = ReservePositionsInFilledCircle(MaxPiecesSupported);
        }

        public Vector3 GetPositionInFilledCircle(int index, bool local = false, float space = 0.15f)
        {
            var pos = new Vector3(_reservedPoints[index].x, 0, _reservedPoints[index].y) * space;
            return local ? pos : transform.TransformPoint(pos);
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
                return (da == db ? 0 : (da < db ? -1 : 1));
            });

            return points.ToArray();
        }
    }
}