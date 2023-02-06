using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Board
{
    public interface ITile : IPieceContainer
    {
        float Size { get; }
        Vector3 GetGridPosition(int index, bool local = false);
        Transform Transform { get; }
    }

    [SelectionBase]
    public class Tile : MonoPieceContainer, ITile
    {
        [SerializeField, Min(0f)] private float size;
        [SerializeField, Min(0f)] private float cellSize = .15f;

        public float Size => size;

        public virtual Vector3 GetGridPosition(int index, bool local = false)
        {
            var pos2D = GridNeighborLocator.GetPositionAtCellIndex(index);
            var localPos = new Vector3(pos2D.x * cellSize, 0, pos2D.y * cellSize);
            return local ? localPos : transform.TransformPoint(localPos);
        }

        public Transform Transform => transform;
    }
}