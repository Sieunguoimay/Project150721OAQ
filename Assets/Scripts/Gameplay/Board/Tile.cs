using System.Linq;
using UnityEngine;

namespace Gameplay.Board
{

    [SelectionBase]
    public class Tile : MonoPieceContainer
    {
        [SerializeField, Min(0f)] private float size;
        [SerializeField, Min(0f)] private float cellSize = .15f;

        public int TileIndex { get; private set; }
        public float Size => size;

        public void SetIndex(int tileIndex)
        {
            TileIndex = tileIndex;
        }

        public virtual Vector3 GetPositionAtGridCellIndex(int index, bool local = false)
        {
            var pos2D = GridNeighborLocator.GetPositionAtCellIndex(index);
            var localPos = new Vector3(pos2D.x * cellSize, 0, pos2D.y * cellSize);
            return local ? localPos : transform.TransformPoint(localPos);
        }

        public Transform Transform => transform;
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public int GetPiecesCount()
        {
            return HeldPieces.Count;
        }

#if UNITY_EDITOR
        [SerializeField] private bool drawGizmos;
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up * .05f, new Vector3(size, 0.1f, size));
        }
#endif
    }
}