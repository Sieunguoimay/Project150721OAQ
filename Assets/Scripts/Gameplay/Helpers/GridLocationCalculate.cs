using UnityEngine;

namespace Gameplay.Helpers
{
    public static class GridLocationCalculate
    {
        public static Vector2Int GetPositionAtCellIndex(int globalCellIndex)
        {
            if (globalCellIndex < 0) return Vector2Int.zero;

            var ringIndex = Mathf.CeilToInt((Mathf.Sqrt(2 * (globalCellIndex + 1) - 1) - 1) / 2);
            if (ringIndex == 0) return Vector2Int.zero;

            var localCellIndex = 0;
            if (globalCellIndex > 0)
            {
                var r = ringIndex - 1;
                var numCellsInDisc = 1 + 2 * (r * r + r);
                localCellIndex = globalCellIndex - numCellsInDisc;
            }

            var edgeIndex = localCellIndex % 4;
            var localCellIndexInEdge = Mathf.FloorToInt(localCellIndex / 4f) % ringIndex;
            var oscillatingIntegerSequence =
                (int) ((localCellIndexInEdge + 1f) / 2f * Mathf.Pow(-1f, localCellIndexInEdge));
            localCellIndexInEdge = Mathf.FloorToInt((ringIndex + 1) / 2f) + oscillatingIntegerSequence;

            return EdgeOrigins[edgeIndex] * ringIndex + EdgeDirections[edgeIndex] * localCellIndexInEdge;
        }

        private static readonly Vector2Int[] EdgeOrigins =
            {new(1, 0), new(0, 1), new(-1, 0), new(0, -1)};

        private static readonly Vector2Int[] EdgeDirections =
            {new(-1, 1), new(-1, -1), new(1, -1), new(1, 1)};
    }
}