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

        public virtual Vector3 GetPositionInFilledCircle(int index, bool local = false)
        {
            var pos2D = GridNeighborLocator.GetPositionAtCellIndex(index);
            var pos = new Vector3(pos2D.x, 0, pos2D.y) * .15f;
            return local ? pos : transform.TransformPoint(pos);
        }

        public Transform Transform => transform;
        
    }
}