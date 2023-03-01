using System;
using UnityEngine;

namespace Gameplay.Helpers
{
    [CreateAssetMenu(menuName = "Helpers/GridLocator")]
    public class GridLocator : BaseGenericDependencyInversionScriptableObject<GridLocator>
    {
        [SerializeField] private float cellSize;

        public Vector3 GetPositionAtCellIndex(Transform transform, int index)
        {
            return transform.TransformPoint(GetPositionAtCellIndex(index));
        }
        public Vector3 GetPositionAtCellIndex(int index)
        {
            var pos2D = GridLocationCalculate.GetPositionAtCellIndex(index);
            var localPos = new Vector3(pos2D.x, 0, pos2D.y) * cellSize;
            return localPos;
        }
    }
}