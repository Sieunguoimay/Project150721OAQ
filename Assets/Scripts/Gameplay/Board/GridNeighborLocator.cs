using UnityEngine;

namespace Gameplay.Board
{
    // public class GridNeighborLocator : MonoBehaviour
    // {
    //     private static int NumCellsOnRing(int r) => r == 0 ? 1 : 4 * r;
    //     private static int NumCellsInDisc(int r) => 1 + 2 * (r * r + r);
    //     private static int LowerBoundRadiusFromNumCells(int s) => Mathf.FloorToInt(RadiusFromNumCells(s));
    //     private static int UpperBoundRadiusFromNumCells(int s) => Mathf.CeilToInt(RadiusFromNumCells(s));
    //     private static float RadiusFromNumCells(int s) => (Mathf.Sqrt(2 * s - 1) - 1) / 2;
    //
    //     private static int RadiusFromCellIndex(int cellIndex)
    //         => UpperBoundRadiusFromNumCells(cellIndex + 1);
    //
    //     private static int LocalCellIndexInRing(int globalCellIndex)
    //     {
    //         if (globalCellIndex == 0) return 0;
    //         return globalCellIndex - NumCellsInDisc(LowerBoundRadiusFromNumCells(globalCellIndex));
    //     }
    //
    //     public static Vector2Int CellIndexToPosition(int globalCellIndex)
    //     {
    //         return LocalCellIndexToPosition(
    //             RadiusFromCellIndex(globalCellIndex),
    //             LocalCellIndexInRing(globalCellIndex));
    //     }
    //
    //
    //     private static Vector2Int LocalCellIndexToPosition(int ringIndex, int localCellIndex)
    //     {
    //         if (ringIndex < 0 || localCellIndex < 0 || localCellIndex >= NumCellsOnRing(ringIndex))
    //         {
    //             Debug.LogError($"Invalid indices ringIndex {ringIndex} localCellIndex {localCellIndex}");
    //             return Vector2Int.zero;
    //         }
    //
    //         if (ringIndex == 0) return Vector2Int.zero;
    //
    //         //var edgeIndex = localCellIndex / ringIndex;
    //         //var localCellIndexInEdge = localCellIndex % ringIndex;
    //
    //         var edgeIndex = localCellIndex % 4;
    //         var localCellIndexInEdge = Mathf.FloorToInt(localCellIndex / 4f) % ringIndex;
    //         localCellIndexInEdge = Mathf.FloorToInt((ringIndex + 1) / 2f) +
    //                                OscillatingIntegerSequence(localCellIndexInEdge);
    //
    //         return LocalEdgeCellIndexToPosition(ringIndex, edgeIndex, localCellIndexInEdge);
    //     }
    //
    //     private static Vector2Int LocalEdgeCellIndexToPosition(int ringIndex, int edgeIndex, int localCellIndexInEdge)
    //     {
    //         switch (edgeIndex)
    //         {
    //             case 0:
    //                 return new Vector2Int(1, 0) * ringIndex + new Vector2Int(-1, 1) * localCellIndexInEdge;
    //             case 1:
    //                 return new Vector2Int(0, 1) * ringIndex + new Vector2Int(-1, -1) * localCellIndexInEdge;
    //             case 2:
    //                 return new Vector2Int(-1, 0) * ringIndex + new Vector2Int(1, -1) * localCellIndexInEdge;
    //             case 3:
    //                 return new Vector2Int(0, -1) * ringIndex + new Vector2Int(1, 1) * localCellIndexInEdge;
    //         }
    //
    //         Debug.LogError($"Invalid edgeIndex {edgeIndex}");
    //         return Vector2Int.zero;
    //     }
    //
    //     private static int OscillatingIntegerSequence(int x)
    //     {
    //         return (int) ((x + 1f) / 2f * Mathf.Pow(-1f, x));
    //     }
    //
    //     [ContextMenu("Next")]
    //     private void Next()
    //     {
    //         numCells++;
    //     }
    //
    //     [SerializeField, Min(0)] private int numCells = 0;
    //     [SerializeField] private bool toggleEdgeCycle = false;
    //
    //     private void OnDrawGizmos()
    //     {
    //         var index = 0;
    //         if (toggleEdgeCycle)
    //             Gizmos.matrix = transform.localToWorldMatrix;
    //         for (var i = 0; i < numCells; i++)
    //         {
    //             var cell = GridNeighborLocator.GetPositionAtCellIndex(i);
    //             var v = index / (float) numCells;
    //             Gizmos.color = new Color(v, v, v, 1);
    //
    //             Gizmos.DrawCube((new Vector3(cell.x, index * .01f, cell.y)),
    //                 new Vector3(1, .1f, 1));
    //
    //             index++;
    //         }
    //     }
    // }

    public static class GridNeighborLocator
    {
        public static Vector2Int GetPositionAtCellIndex(int globalCellIndex)
        {
            if (globalCellIndex < 0) return Vector2Int.zero;

            var radiusFromNumCells = (Mathf.Sqrt(2 * (globalCellIndex + 1) - 1) - 1) / 2;

            var ringIndex = Mathf.CeilToInt(radiusFromNumCells);
            if (ringIndex == 0) return Vector2Int.zero;

            var localCellIndex = 0;
            if (globalCellIndex > 0)
            {
                var radiusFromCellIndex = (Mathf.Sqrt(2 * (globalCellIndex) - 1) - 1) / 2;
                var r = Mathf.FloorToInt(radiusFromCellIndex);
                var numCellsInDisc = 1 + 2 * (r * r + r);
                localCellIndex = globalCellIndex - numCellsInDisc;
            }

            var edgeIndex = localCellIndex % 4;
            var localCellIndexInEdge = Mathf.FloorToInt(localCellIndex / 4f) % ringIndex;
            var oscillatingIntegerSequence =
                (int) ((localCellIndexInEdge + 1f) / 2f * Mathf.Pow(-1f, localCellIndexInEdge));
            localCellIndexInEdge = Mathf.FloorToInt((ringIndex + 1) / 2f) + oscillatingIntegerSequence;

            return LocalEdgeCellIndexToPosition(ringIndex, edgeIndex, localCellIndexInEdge);
        }

        private static readonly Vector2Int[] EdgeDirections =
            {new(1, 0), new(0, 1), new(-1, 0), new(0, -1)};

        private static readonly Vector2Int[] PerpendicularDirections =
            {new(-1, 1), new(-1, -1), new(1, -1), new(1, 1)};

        private static Vector2Int LocalEdgeCellIndexToPosition(int ringIndex, int edgeIndex,
            int localCellIndexInEdge)
        {
            var edgeDirection = EdgeDirections[edgeIndex];
            var perpendicularDirection = PerpendicularDirections[edgeIndex];
            var ringIndexVector = edgeDirection * ringIndex;
            var localCellIndexInEdgeVector = perpendicularDirection * localCellIndexInEdge;

            return ringIndexVector + localCellIndexInEdgeVector;
        }
    }
}