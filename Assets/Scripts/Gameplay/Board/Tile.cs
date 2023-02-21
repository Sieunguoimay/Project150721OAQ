using UnityEngine;

namespace Gameplay.Board
{
    public interface ITile : IPieceContainer
    {
        int TileIndex { get; }
        float Size { get; }
        Vector3 GetGridPosition(int index, bool local = false);
        Transform Transform { get; }
    }

    [SelectionBase]
    public class Tile : MonoPieceContainer, ITile
    {
        [SerializeField, Min(0f)] private float size;
        [SerializeField, Min(0f)] private float cellSize = .15f;

        public int TileIndex { get; private set; }
        public float Size => size;

        public void SetIndex(int tileIndex)
        {
            TileIndex = tileIndex;
        }

        public virtual Vector3 GetGridPosition(int index, bool local = false)
        {
            var pos2D = GridNeighborLocator.GetPositionAtCellIndex(index);
            var localPos = new Vector3(pos2D.x * cellSize, 0, pos2D.y * cellSize);
            return local ? localPos : transform.TransformPoint(localPos);
        }

        public Transform Transform => transform;

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