using System;
using Framework.DependencyInversion;
using UnityEngine;

namespace Gameplay.Helpers
{
    public class GridLocator : SelfBindingDependencyInversionScriptableObject
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